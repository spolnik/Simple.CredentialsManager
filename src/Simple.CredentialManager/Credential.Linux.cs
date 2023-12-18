
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using secrets.DBus;
using Tmds.DBus;

namespace Simple.CredentialManager;


[SupportedOSPlatform("Linux")]
public class LinuxCredential : ICredential
{
    private Connection connection;
    internal struct SecretService
    {
        public IService Service;
        public ObjectPath Session;
        public ObjectPath? TargetCollection = null;

        public SecretService() { }
    }

    SecretService secretService;
    private bool disposedValue;


    public string Description { get; set; }
    // should not contain any special chars or spaces
    public string Collection { get; set; }

    public DateTime LastWriteTime { get; set; }

    public DateTime LastWriteTimeUtc { get; set; }

    public string Password { 
        get {
            return SecureStringHelper.CreateString(password);
        } 
        set {
            password = SecureStringHelper.CreateSecureString(value);
        }
    }
    private SecureString password { get; set; }
    public SecureString SecurePassword { 
        get {
            return password;
        }
        set {
            password = value;          
        }
    }
    public string Username { get; set; }

    public LinuxCredential()
    {
        Collection = "default";
        init();
    }
    public LinuxCredential(string username) : this(username, null)
    { }
    public LinuxCredential(string username, string password) : this(username, password, null)
    { }
    public LinuxCredential(string username, SecureString password, string description, string collection = "default")
        : this(username, SecureStringHelper.CreateString(password), description, collection)
    { }
    public LinuxCredential(string username, string password, string description, string collection = "default")
    {
        Username = username;
        Password = password;
        Description = description;
        Collection = collection;

        init();
    }

    private void init()
    {
        connection = Connection.Session;
        var service = connection.CreateProxy<IService>("org.freedesktop.secrets", "/org/freedesktop/secrets");
        var session = service.OpenSessionAsync("plain", "").Result.result;

        secretService = new SecretService()
        {
            Service = service,
            Session = session
        };
        if (Collection is null) Collection = "";

        var collections = getAllCollections();
        foreach (var collection in collections)
        {
            if (collection.ToString().EndsWith(Collection))
            {
                secretService.TargetCollection = collection;
                break;
            }
        }

        if (secretService.TargetCollection is null)
            secretService.TargetCollection = createCollection().Result;

        if (secretService.TargetCollection is null)
            throw new ArgumentNullException(nameof(secretService.TargetCollection));

        List<ObjectPath> objectsToUnlock = new() {
            (ObjectPath) secretService.TargetCollection
        };

        _ = secretService.Service.UnlockAsync(objectsToUnlock.ToArray()).Result;
    }
    /// <summary>
    /// The Secret Service API uses return paramaters with multiple meanings
    /// For example if CreateCollection is used, the result has to objects.
    ///     ObjectPath collection - The new collection object, or '/' if prompting is necessary.
    ///     ObjectPath prompt     - A prompt object if prompting is necessary, or '/' if no prompt was needed.
    /// This method is therefore using the latter object to make a prompt, if necessary.
    /// Caution: This prompt is made by the OS not the application itself!
    /// </summary>
    /// <param name="prompt">A prompt object if prompting is necessary, or '/' if no prompt was needed.</param>
    /// <returns>
    /// </returns>
    private async Task<(bool, ObjectPath?)> doPrompt(ObjectPath prompt)
    {
        bool done = false;
        ObjectPath? collection = null;

        // as per documentation, if the prompt object equals '/' no prompt is needed.
        if (prompt == "/") return (done, collection);

        var promptService = connection.CreateProxy<IPrompt>("org.freedesktop.secrets", prompt);
        await promptService.PromptAsync(Collection);

        do
        {
            using var promptCompleted = await promptService.WatchCompletedAsync(e =>
            {
                if (e.dismissed) throw new Exception("Keyring: User has dismissed the keyring's prompt!");
                done = true;
                collection = (ObjectPath?)e.result;

            }, (Exception ex) =>
            {
                throw ex;
            });
        } while (!done);

        return (done, collection);
    }

    private async Task<ObjectPath?> createCollection()
    {
        var properties = new Dictionary<string, object>
        {
            { "org.freedesktop.Secret.Collection.Label", Collection }
        };

        var result = await secretService.Service.CreateCollectionAsync(properties, "");
        var promptResult = await doPrompt(result.prompt);
        return promptResult.Item2;
    }

    private IEnumerable<ObjectPath> getAllCollections()
    {
        var properties = secretService.Service.GetAllAsync().Result;

        foreach (var collection in properties.Collections)
            yield return collection;
    }

    public bool Delete()
    {
        var attributes = new Dictionary<string, string> {
            { "Username", Username },
            { "xdg:schema", $"org.freedesktop.{Collection}.Secret" }
        };

        var collectionProxy = connection.CreateProxy<ICollection>("org.freedesktop.secrets", (ObjectPath)secretService.TargetCollection);
        var result = collectionProxy.SearchItemsAsync(attributes).Result;

        if (result.Length is 0) return false;

        var itemProxy = connection.CreateProxy<IItem>("org.freedesktop.secrets", result[0]);
        var deleteResult = itemProxy.DeleteAsync().Result;
        if (deleteResult != "/") {
            var promptResult = doPrompt(deleteResult).Result; 
            return promptResult.Item1;
        }
        return true;
    }

    public bool Exists()
    {
        var attributes = new Dictionary<string, string> {
            { "Username", Username },
            { "xdg:schema", $"org.freedesktop.{Collection}.Secret" }
        };

        var collectionProxy = connection.CreateProxy<ICollection>("org.freedesktop.secrets", (ObjectPath)secretService.TargetCollection);
        var result = collectionProxy.SearchItemsAsync(attributes).Result;

        return result.Length > 0;
    }

    public bool Load()
    {
        var attributes = new Dictionary<string, string> {
            { "Username", Username },
            { "xdg:schema", $"org.freedesktop.{Collection}.Secret" }
        };

        var collectionProxy = connection.CreateProxy<ICollection>("org.freedesktop.secrets", (ObjectPath)secretService.TargetCollection);
        var result = collectionProxy.SearchItemsAsync(attributes).Result;

        if (result.Length is 0) return false;

        var itemProxy = connection.CreateProxy<IItem>("org.freedesktop.secrets", result[0]);
        Description = itemProxy.GetAsync<string>("Label").Result;
        var secret = itemProxy.GetSecretAsync(secretService.Session).Result;   //todo: might be locked
        Password = Encoding.Default.GetString(secret.Item3);
        LastWriteTime = DateTimeOffset.FromUnixTimeSeconds((long)itemProxy.GetModifiedAsync().Result).DateTime;
        var itemAttributes = itemProxy.GetAttributesAsync().Result;

        return true;
    }

    public bool Save()
    {
        var replace = true; // replace credential, if it already exists.  
        var properties = new Dictionary<string, object> {
            { "org.freedesktop.Secret.Item.Label", Description},
            {
                "org.freedesktop.Secret.Item.Attributes",
                new Dictionary<string, string> {
                    { "Username", Username },
                    { "xdg:schema", $"org.freedesktop.{Collection}.Secret" }
                }
            }
        };

        var collectionProxy = connection.CreateProxy<ICollection>("org.freedesktop.secrets", (ObjectPath)secretService.TargetCollection);
        //todo: collection might be locked
        var addResult = collectionProxy.CreateItemAsync(
            properties,
            (
                secretService.Session,
                Array.Empty<byte>(),
                Encoding.Default.GetBytes(Password),
                ""
            ),
            replace).Result;

        if (addResult.item != "/")
        {
            return true;
        }

        var promptResult = doPrompt(addResult.prompt).Result;
        return promptResult.Item1;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                connection.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    public IEnumerable<LinuxCredential> LoadAll()
    {
        var collectionProxy = connection.CreateProxy<ICollection>("org.freedesktop.secrets", (ObjectPath)secretService.TargetCollection);
        var result = collectionProxy.GetAllAsync().Result;

        foreach (var credential in result.Items)
        {
            var itemProxy = connection.CreateProxy<IItem>("org.freedesktop.secrets", credential);
            var description = itemProxy.GetAsync<string>("Label").Result;
            var secret = itemProxy.GetSecretAsync(secretService.Session).Result;
            var password = Encoding.Default.GetString(secret.Item3);
            var lastWrite = DateTimeOffset.FromUnixTimeSeconds((long)itemProxy.GetModifiedAsync().Result).DateTime;
            var username = itemProxy.GetAttributesAsync().Result["Username"];

            //todo: make Collection different?
            yield return new LinuxCredential(username, password, description, Collection)
            {
                LastWriteTime = lastWrite
            };
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~CredentialLinux()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }
    public override string ToString()
    {
        return string.Format($"Username: {Username}, Password: {Password}, Collection: {Collection}, LastWriteTime: {LastWriteTime}");
    }
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}