using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Tmds.DBus;

[assembly: InternalsVisibleTo(Tmds.DBus.Connection.DynamicAssemblyName)]
namespace secrets.DBus
{
    [SupportedOSPlatform("Linux")]
    [DBusInterface("org.gnome.keyring.InternalUnsupportedGuiltRiddenInterface")]
    interface IInternalUnsupportedGuiltRiddenInterface : IDBusObject
    {
        Task ChangeWithMasterPasswordAsync(ObjectPath Collection, (ObjectPath, byte[], byte[], string) Original, (ObjectPath, byte[], byte[], string) Master);
        Task<ObjectPath> ChangeWithPromptAsync(ObjectPath Collection);
        Task<ObjectPath> CreateWithMasterPasswordAsync(IDictionary<string, object> Attributes, (ObjectPath, byte[], byte[], string) Master);
        Task UnlockWithMasterPasswordAsync(ObjectPath Collection, (ObjectPath, byte[], byte[], string) Master);
    }

    [SupportedOSPlatform("Linux")]
    [DBusInterface("org.freedesktop.Secret.Service")]
    interface IService : IDBusObject
    {
        Task<(object output, ObjectPath result)> OpenSessionAsync(string Algorithm, object Input);
        Task<(ObjectPath collection, ObjectPath prompt)> CreateCollectionAsync(IDictionary<string, object> Properties, string Alias);
        Task<(ObjectPath[] unlocked, ObjectPath[] locked)> SearchItemsAsync(IDictionary<string, string> Attributes);
        Task<(ObjectPath[] unlocked, ObjectPath prompt)> UnlockAsync(ObjectPath[] Objects);
        Task<(ObjectPath[] locked, ObjectPath prompt)> LockAsync(ObjectPath[] Objects);
        Task LockServiceAsync();
        Task<ObjectPath> ChangeLockAsync(ObjectPath Collection);
        Task<IDictionary<ObjectPath, (ObjectPath, byte[], byte[], string)>> GetSecretsAsync(ObjectPath[] Items, ObjectPath Session);
        Task<ObjectPath> ReadAliasAsync(string Name);
        Task SetAliasAsync(string Name, ObjectPath Collection);
        Task<IDisposable> WatchCollectionCreatedAsync(Action<ObjectPath> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchCollectionDeletedAsync(Action<ObjectPath> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchCollectionChangedAsync(Action<ObjectPath> handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<ServiceProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [SupportedOSPlatform("Linux")]
    [Dictionary]
    class ServiceProperties
    {
        private ObjectPath[] _Collections = default(ObjectPath[]);
        public ObjectPath[] Collections
        {
            get
            {
                return _Collections;
            }

            set
            {
                _Collections = (value);
            }
        }
    }

    [SupportedOSPlatform("Linux")]
    static class ServiceExtensions
    {
        public static Task<ObjectPath[]> GetCollectionsAsync(this IService o) => o.GetAsync<ObjectPath[]>("Collections");
    }

    [SupportedOSPlatform("Linux")]
    [DBusInterface("org.freedesktop.Secret.Collection")]
    interface ICollection : IDBusObject
    {
        Task<ObjectPath> DeleteAsync();
        Task<ObjectPath[]> SearchItemsAsync(IDictionary<string, string> Attributes);
        Task<(ObjectPath item, ObjectPath prompt)> CreateItemAsync(IDictionary<string, object> Properties, (ObjectPath, byte[], byte[], string) Secret, bool Replace);
        Task<IDisposable> WatchItemCreatedAsync(Action<ObjectPath> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchItemDeletedAsync(Action<ObjectPath> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchItemChangedAsync(Action<ObjectPath> handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<CollectionProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [SupportedOSPlatform("Linux")]
    [Dictionary]
    class CollectionProperties
    {
        private ObjectPath[] _Items = default(ObjectPath[]);
        public ObjectPath[] Items
        {
            get
            {
                return _Items;
            }

            set
            {
                _Items = (value);
            }
        }

        private string _Label = default(string);
        public string Label
        {
            get
            {
                return _Label;
            }

            set
            {
                _Label = (value);
            }
        }

        private bool _Locked = default(bool);
        public bool Locked
        {
            get
            {
                return _Locked;
            }

            set
            {
                _Locked = (value);
            }
        }

        private ulong _Created = default(ulong);
        public ulong Created
        {
            get
            {
                return _Created;
            }

            set
            {
                _Created = (value);
            }
        }

        private ulong _Modified = default(ulong);
        public ulong Modified
        {
            get
            {
                return _Modified;
            }

            set
            {
                _Modified = (value);
            }
        }
    }

    [SupportedOSPlatform("Linux")]
    static class CollectionExtensions
    {
        public static Task<ObjectPath[]> GetItemsAsync(this ICollection o) => o.GetAsync<ObjectPath[]>("Items");
        public static Task<string> GetLabelAsync(this ICollection o) => o.GetAsync<string>("Label");
        public static Task<bool> GetLockedAsync(this ICollection o) => o.GetAsync<bool>("Locked");
        public static Task<ulong> GetCreatedAsync(this ICollection o) => o.GetAsync<ulong>("Created");
        public static Task<ulong> GetModifiedAsync(this ICollection o) => o.GetAsync<ulong>("Modified");
        public static Task SetLabelAsync(this ICollection o, string val) => o.SetAsync("Label", val);
    }

    [DBusInterface("org.freedesktop.Secret.Item")]
    [SupportedOSPlatform("Linux")]
    interface IItem : IDBusObject
    {
        Task<ObjectPath> DeleteAsync();
        Task<(ObjectPath secret, byte[], byte[], string)> GetSecretAsync(ObjectPath Session);
        Task SetSecretAsync((ObjectPath, byte[], byte[], string) Secret);
        Task<T> GetAsync<T>(string prop);
        Task<ItemProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    [SupportedOSPlatform("Linux")]
    class ItemProperties
    {
        private bool _Locked = default(bool);
        public bool Locked
        {
            get
            {
                return _Locked;
            }

            set
            {
                _Locked = (value);
            }
        }

        private IDictionary<string, string> _Attributes = default(IDictionary<string, string>);
        public IDictionary<string, string> Attributes
        {
            get
            {
                return _Attributes;
            }

            set
            {
                _Attributes = (value);
            }
        }

        private string _Label = default(string);
        public string Label
        {
            get
            {
                return _Label;
            }

            set
            {
                _Label = (value);
            }
        }

        private string _Type = default(string);
        public string Type
        {
            get
            {
                return _Type;
            }

            set
            {
                _Type = (value);
            }
        }

        private ulong _Created = default(ulong);
        public ulong Created
        {
            get
            {
                return _Created;
            }

            set
            {
                _Created = (value);
            }
        }

        private ulong _Modified = default(ulong);
        public ulong Modified
        {
            get
            {
                return _Modified;
            }

            set
            {
                _Modified = (value);
            }
        }
    }

    [SupportedOSPlatform("Linux")]
    static class ItemExtensions
    {
        public static Task<bool> GetLockedAsync(this IItem o) => o.GetAsync<bool>("Locked");
        public static Task<IDictionary<string, string>> GetAttributesAsync(this IItem o) => o.GetAsync<IDictionary<string, string>>("Attributes");
        public static Task<string> GetLabelAsync(this IItem o) => o.GetAsync<string>("Label");
        public static Task<string> GetTypeAsync(this IItem o) => o.GetAsync<string>("Type");
        public static Task<ulong> GetCreatedAsync(this IItem o) => o.GetAsync<ulong>("Created");
        public static Task<ulong> GetModifiedAsync(this IItem o) => o.GetAsync<ulong>("Modified");
        public static Task SetAttributesAsync(this IItem o, IDictionary<string, string> val) => o.SetAsync("Attributes", val);
        public static Task SetLabelAsync(this IItem o, string val) => o.SetAsync("Label", val);
        public static Task SetTypeAsync(this IItem o, string val) => o.SetAsync("Type", val);
    }

    [DBusInterface("org.freedesktop.Secret.Session")]
    [SupportedOSPlatform("Linux")]
    interface ISession : IDBusObject
    {
        Task CloseAsync();
    }

    [DBusInterface("org.freedesktop.Secret.Prompt")]
    [SupportedOSPlatform("Linux")]
    interface IPrompt : IDBusObject
    {
        Task PromptAsync(string WindowId);
        Task DismissAsync();
        Task<IDisposable> WatchCompletedAsync(Action<(bool dismissed, object result)> handler, Action<Exception> onError = null);
    }

    [DBusInterface("org.freedesktop.impl.portal.Secret")]
    [SupportedOSPlatform("Linux")]
    interface ISecret : IDBusObject
    {
        Task<(uint response, IDictionary<string, object> results)> RetrieveSecretAsync(ObjectPath Handle, string AppId, CloseSafeHandle Fd, IDictionary<string, object> Options);
        Task<T> GetAsync<T>(string prop);
        Task<SecretProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    [SupportedOSPlatform("Linux")]
    class SecretProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    [SupportedOSPlatform("Linux")]
    static class SecretExtensions
    {
        public static Task<uint> GetVersionAsync(this ISecret o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.gnome.keyring.Daemon")]
    [SupportedOSPlatform("Linux")]
    interface IDaemon : IDBusObject
    {
        Task<IDictionary<string, string>> GetEnvironmentAsync();
        Task<string> GetControlDirectoryAsync();
    }
}