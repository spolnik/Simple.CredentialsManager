using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Simple.CredentialManager;

namespace CredentialChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            // WriteAllCredentials();
            AddCredential();
            LoadSingleCredential();
            // WriteAllCredentials();
            // DeleteCredential();
            // WriteAllCredentials();
        }

        private static void LoadSingleCredential()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var cred = new WinCredential("MyUser");
                var result = cred.Load();

                Console.WriteLine("Load single User result: " + result.ToString());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var credential = new LinuxCredential("MyUser");
                var result = credential.Load();

                Console.WriteLine("Load single User result: " + result.ToString());
            }
        }

        private static void AddCredential()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var credential = new WinCredential("MyUser")
                {
                    Target = "Test@testinnng",
                    PersistenceType = PersistenceType.LocalComputer,
                    Description = "Saved by credman",
                    Password = "SuperSecret2322?",
                    Type = CredentialType.Generic
                };

                var saved = credential.Save();

                Console.WriteLine("User name: {0}, Password: {1}, saved: {2}", credential.Username, credential.Password, saved.ToString());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var credential = new LinuxCredential("MyUser", "MySuperSecretPassword", "Description");
                var result = credential.Save();
                Console.WriteLine("Add a user: " + result.ToString());
            }
        }

        private static void WriteAllCredentials()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var loadAll = WinCredential.LoadAll();

                foreach (var credential1 in loadAll)
                {
                    Console.WriteLine(credential1);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                LinuxCredential cred = new LinuxCredential("", "", "");
                foreach (var c in cred.LoadAll())
                {
                    Console.WriteLine(c);
                }
            }
        }

        private static void DeleteCredential() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                var cred = new LinuxCredential("MyUser");
                var credRes = cred.Load();
                if (credRes) {
                    var delRes = cred.Delete();                
                    Console.WriteLine("Delete a user: " + delRes.ToString());
                }
                else {
                    Console.WriteLine("Delete something went wrong.");
                }
                
            } else {
                throw new NotImplementedException();
            }
        }
    }
}
