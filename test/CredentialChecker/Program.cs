using System;
using Simple.CredentialManager;

namespace CredentialChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            var credential = new Credential {Target = "Test"};
            credential.Load();

            Console.WriteLine("User name: {0}, Password: {1}", credential.Username, credential.Password);
        }
    }
}
