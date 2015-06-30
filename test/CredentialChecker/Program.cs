using System;
using Simple.CredentialManager;

namespace CredentialChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            //WriteSingleCredential();

            WriteAllCredentials();

            Console.ReadLine();
        }

        private static void WriteSingleCredential()
        {
            var credential = new Credential {Target = "Test"};
            credential.Load();

            Console.WriteLine("User name: {0}, Password: {1}", credential.Username, credential.Password);
        }

        private static void WriteAllCredentials()
        {
            var loadAll = Credential.LoadAll();

            foreach (var credential1 in loadAll)
            {
                Console.WriteLine(credential1);
            }
        }
    }
}
