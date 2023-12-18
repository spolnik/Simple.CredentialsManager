using System;
using System.Security;

namespace Simple.CredentialManager
{
    public interface ICredential : IDisposable
    {
        string Description { get; set; }
        DateTime LastWriteTime { get; set;}
        DateTime LastWriteTimeUtc { get; set;}
        string Password { get; set; }
        string Username { get; set; }

        bool Delete();
        bool Exists();
        bool Load();
        bool Save();
        string ToString();
    }
}