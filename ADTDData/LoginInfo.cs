using Mihelcic.Net.Visio.Common;
using System;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using System.Security;

namespace Mihelcic.Net.Visio.Data
{
    public class LoginInfo : ViewModelBase
    {
        #region Public Properties

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

        public string FullUserName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(UserDomain))
                    return UserName;
                else return $"{UserDomain}\\{UserName}";
            }
        }

        public string Password
        {
            get { return SecureStringToString(UserPassword); }
        }

        #endregion

        public LoginInfo()
        {
            Validated = false;
        }

        #region Public Methods

        public static bool ValidateCredentials(string dc, string username, SecureString password, string domain)
        {
            bool isValid = false;
            IntPtr passwordBSTR = default;
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, dc))
                {
                    isValid = context.ValidateCredentials(username, SecureStringToString(password), ContextOptions.Negotiate);
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

        #endregion

        #region Private Methods

        private static string SecureStringToString(SecureString value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                // Marshal SecureString to IntPtr (BSTR)
                valuePtr = Marshal.SecureStringToBSTR(value);
                // Marshal IntPtr (BSTR) to string
                return Marshal.PtrToStringBSTR(valuePtr);
            }
            finally
            {
                // Make sure to zero and free the BSTR to minimize time the unencrypted string exists in memory
                if (valuePtr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(valuePtr);
                }
            }
        }

        #endregion
    }
}
