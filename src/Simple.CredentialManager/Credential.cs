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
    { }
}