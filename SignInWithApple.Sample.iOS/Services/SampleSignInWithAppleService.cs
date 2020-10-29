using System;
using System.Threading.Tasks;
using AuthenticationServices;
using SignInWithApple.Sample.iOS.Models;
using SignInWithApple.Sample.iOS.SignInWithApple;
using UIKit;

namespace SignInWithApple.Sample.iOS.Services
{
    public class SampleSignInWithAppleService : BaseSignInWithAppleService
    {
        private readonly SampleKeychainSessionManager _sessionManager;

        public SampleSignInWithAppleService(Func<UIWindow> windowProvider)
            : base(windowProvider)
        {
            // For the purpose of this demo app, store the userIdentifier in the keychain.
            _sessionManager = new SampleKeychainSessionManager("com.xamarin.AddingTheSignInWithAppleFlowToYourApp");
        }

        public event EventHandler<AppleIdCredential> CompletedWithAppleId;
        public event EventHandler<PasswordCredential> CompletedWithPassword;

        public override string CurrentUserIdentifier => _sessionManager.CurrentUserIdentifier;

        public override void SignUp()
        {
            _sessionManager.DeleteUserIdentifier();
        }

        protected override Task RegisterNewAccount(ASAuthorizationAppleIdCredential appleIdCredential)
        {
            var credential = Map(appleIdCredential);

            _sessionManager.CreateUserIdentifier(credential);
            
            // TODO: Make a call to your service and signify to the caller whether registration succeeded or not.
            
            CompletedWithAppleId?.Invoke(this, credential);
            
            return Task.CompletedTask;
        }

        protected override Task SignInWithExistingAccount(ASAuthorizationAppleIdCredential appleIdCredential)
        {
            var credential = Map(appleIdCredential);

            // You *should* have a fully registered account here.  If you get back an error
            // from your server that the account doesn't exist, you can look in the keychain 
            // for the credentials and rerun setup
            
            // if (WebAPI.login(credential.user, 
            //                  credential.identityToken,
            //                  credential.authorizationCode)) {
            //   ...
            // }

            CompletedWithAppleId?.Invoke(this, credential);
            
            return Task.CompletedTask;
        }

        protected override Task SignInWithUserAndPassword(ASPasswordCredential passwdCredential)
        {
            var credential = new PasswordCredential
            {
                User = passwdCredential.User,
                Password = passwdCredential.Password
            };
            
            // if (WebAPI.login(credential.user, credential.password)) {
            //   ...
            // }
            
            CompletedWithPassword?.Invoke(this, credential);

            return Task.CompletedTask;
        }

        protected override void HandleException(Exception exception)
        {
            Console.WriteLine(exception);
        }

        private AppleIdCredential Map(ASAuthorizationAppleIdCredential e)
        {
            // https://developer.apple.com/documentation/authenticationservices/asauthorizationappleidcredential
            return new AppleIdCredential
            {
                IdentityToken = e.IdentityToken?.ToString(),
                AuthorizationCode = e.AuthorizationCode?.ToString(),
                Email = e.Email,
                FullName = e.FullName?.ToString(),
                GivenName = e.FullName?.GivenName,
                FamilyName = e.FullName?.FamilyName,
                State = e.State,
                User = e.User
            };
        }
    }
}