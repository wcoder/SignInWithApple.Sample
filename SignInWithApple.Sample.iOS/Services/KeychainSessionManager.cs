using System;
using SignInWithApple.Sample.iOS.Services.Keychain;
using SignInWithApple.Sample.iOS.Services.SignInWithApple;

namespace SignInWithApple.Sample.iOS.Services
{
    public class KeychainSessionManager : ISessionManager
    {
        private readonly string _service;

        /// <summary>
        ///     Initializes new instance.
        /// </summary>
        /// <param name="service">Service associated with an InternetPassword.</param>
        public KeychainSessionManager(string service)
        {
            _service = service;
        }
        
        public string CurrentUserIdentifier
        {
            get
            {
                try
                {
                    return GetKeychainItem().ReadItem();
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public void CreateUserIdentifier(AppleIdCredential credential)
        {
            try
            {
                GetKeychainItem().SaveItem(credential.User);
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to save userIdentifier to keychain.");
            }
        }

        public void DeleteUserIdentifier()
        {
            try
            {
                GetKeychainItem().DeleteItem();
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to delete userIdentifier from keychain");
            }
        }

        private KeychainItem GetKeychainItem()
        {
            return new KeychainItem(_service, "userIdentifier");
        }
    }
}