using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Simple.CredentialManager
{
    /// <summary>
    ///     Adapter to WinCredential for compatibility reasons.
    ///     Before Linux was supported, there was only this credential class. Now there is LinuxCredential and WinCredential.
    /// </summary>
    public class Credential : WinCredential
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WinCredential" /> class.
        /// </summary>
        public Credential(): base(null)
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WinCredential" /> class.
        /// </summary>
        /// <param name="username">The username.</param>
        public Credential(string username)
            : base(username, null)
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WinCredential" /> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public Credential(string username, string password)
            : base(username, password, null)
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WinCredential" /> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="target">The string that contains the name of the credential.</param>
        public Credential(string username, string password, string target)
            : base(username, password, target, CredentialType.Generic)
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WinCredential" /> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="target">The string that contains the name of the credential.</param>
        /// <param name="type">The credential type.</param>
        public Credential(string username, string password, string target, CredentialType type) 
            : base(username, password, target, type)
        { }
    }
}