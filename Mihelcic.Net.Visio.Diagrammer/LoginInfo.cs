using Microsoft.ADTD.Data;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using System;
using System.Security;

namespace Microsoft.ADTD.Data
{
    public class LoginInfo : ViewModelBase
    {
        public bool Validated
        {
            get { return GetValue<bool>(nameof(Validated)); }
            set
            {
                SetValue(value, nameof(Validated));
                CurrentUserContext = !value;
            }
        }
        public string UserName
        {
            get { return GetValue<string>(nameof(UserName)); }
            set { SetValue(value, nameof(UserName)); }
        }
        public SecureString UserPassword
        {
            get { return GetValue<SecureString>(nameof(UserPassword)); }
            set { SetValue(value, nameof(UserPassword)); }
        }
        public string UserDomain
        {
            get { return GetValue<string>(nameof(UserDomain)); }
            set { SetValue(value, nameof(UserDomain)); }
        }

        public bool CurrentUserContext
        {
            get { return GetValue<bool>(nameof(CurrentUserContext)); }
            set { SetValue(value, nameof(CurrentUserContext)); }
        }

        public LoginInfo()
        {
            Validated = false;
        }

        public static bool ValidateCredentials(string dc, string username, SecureString password, string domain)
        {
            bool isValid = false;
            IntPtr passwordBSTR = default(IntPtr);
            try
            {
                // Convert SecureString to BSTR and then to a readable string for demonstration purposes
                passwordBSTR = Marshal.SecureStringToBSTR(password);
                string readablePassword = Marshal.PtrToStringBSTR(passwordBSTR);

                using (var context = new PrincipalContext(ContextType.Domain, dc))
                {
                    isValid = context.ValidateCredentials(username, readablePassword, ContextOptions.Negotiate);
                }
            }
            finally
            {
                // Make sure to zero and free BSTR pointer
                if (passwordBSTR != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(passwordBSTR);
                }
            }
            return isValid;
        }
    }
}
