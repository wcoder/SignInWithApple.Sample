using System;
using System.Diagnostics.CodeAnalysis;
using AuthenticationServices;
using Foundation;
using UIKit;

namespace SignInWithApple.Sample.iOS.Services.SignInWithApple
{
    public class SignInWithAppleService : ISignInWithAppleService
    {
        private readonly ISessionManager _sessionManager;
        private readonly ASAuthorizationAppleIdProvider _appleIdProvider;
        private readonly CustomDelegate _authControllerDelegate;
        private readonly CustomPresentationContextProvider _presentationProvider;

        public SignInWithAppleService(
            ISessionManager sessionManager,
            Func<UIWindow> windowProvider)
        {
            _sessionManager = sessionManager;
            _appleIdProvider = new ASAuthorizationAppleIdProvider();

            _authControllerDelegate = new CustomDelegate();
            _authControllerDelegate.CompletedWithAppleId += DidCompleteAuthWithAppleId;
            _authControllerDelegate.CompletedWithPassword += DidCompleteAuthWithPassword;
            _authControllerDelegate.CompletedWithError += DidCompleteAuthWithError;
            
            _presentationProvider = new CustomPresentationContextProvider(windowProvider);
        }

        public event EventHandler<AppleIdCredential> CompletedWithAppleId;

        public event EventHandler<PasswordCredential> CompletedWithPassword;

        public string CurrentUserIdentifier => _sessionManager.CurrentUserIdentifier;

        public void GetCredentialState(
            Action authorized = null,
            Action credentialRevoked = null,
            Action credentialNotFound = null)
        {
            _appleIdProvider.GetCredentialState(_sessionManager.CurrentUserIdentifier, (credentialState, error) =>
            {
                switch (credentialState)
                {
                    case ASAuthorizationAppleIdProviderCredentialState.Authorized:
                        authorized?.Invoke();
                        break;
                    case ASAuthorizationAppleIdProviderCredentialState.Revoked:
                        credentialRevoked?.Invoke();
                        break;
                    case ASAuthorizationAppleIdProviderCredentialState.NotFound:
                        credentialNotFound?.Invoke();
                        break;
                }
            });
        }

        public void SignIn()
        {
            var request = _appleIdProvider.CreateRequest();
            request.RequestedScopes = new[]
            {
                ASAuthorizationScope.Email,
                ASAuthorizationScope.FullName
            };

            // Prepare request for Apple ID.
            ASAuthorizationRequest[] requests = {
                _appleIdProvider.CreateRequest()
            };
            
            // Create an authorization controller with the given requests.
            var authorizationController = new ASAuthorizationController(requests)
            {
                Delegate = _authControllerDelegate,
                PresentationContextProvider = _presentationProvider
            };
            authorizationController.PerformRequests();
        }
        
        public void SignUp()
        {
            _sessionManager.DeleteUserIdentifier();
        }
        
        public void PerformExistingAccountSetupFlows()
        {
            // Prepare requests for both Apple ID and password providers.
            ASAuthorizationRequest[] requests = {
                _appleIdProvider.CreateRequest(),
                new ASAuthorizationPasswordProvider().CreateRequest()
            };

            // Create an authorization controller with the given requests.
            var authorizationController = new ASAuthorizationController(requests)
            {
                Delegate = _authControllerDelegate,
                PresentationContextProvider = _presentationProvider
            };
            authorizationController.PerformRequests();
        }

        private void DidCompleteAuthWithAppleId(object sender, ASAuthorizationAppleIdCredential e)
        {
            // https://developer.apple.com/documentation/authenticationservices/asauthorizationappleidcredential
            var credential = new AppleIdCredential
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
            
            _sessionManager.CreateUserIdentifier(credential);
            
            CompletedWithAppleId?.Invoke(sender, credential);
        }
        
        private void DidCompleteAuthWithPassword(object sender, ASPasswordCredential e)
        {
            // Sign in using an existing iCloud Keychain credential.
            var credential = new PasswordCredential
            {
                User = e.User,
                Password = e.Password
            };

            CompletedWithPassword?.Invoke(sender, credential);
        }
        
        protected virtual void DidCompleteAuthWithError(object sender, NSError error)
        {
            Console.WriteLine(error);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private class CustomDelegate : NSObject, IASAuthorizationControllerDelegate
        {
            public event EventHandler<ASAuthorizationAppleIdCredential> CompletedWithAppleId;
            public event EventHandler<ASPasswordCredential> CompletedWithPassword;
            public event EventHandler<NSError> CompletedWithError;

            [Export("authorizationController:didCompleteWithAuthorization:")]
            public void DidComplete(ASAuthorizationController controller, ASAuthorization authorization)
            {
                if (authorization.GetCredential<ASAuthorizationAppleIdCredential>() is ASAuthorizationAppleIdCredential appleIdCredential)
                {
                    CompletedWithAppleId?.Invoke(controller, appleIdCredential);
                }
                else if (authorization.GetCredential<ASPasswordCredential>() is ASPasswordCredential passwordCredential)
                {
                    CompletedWithPassword?.Invoke(controller, passwordCredential);
                }
            }

            [Export("authorizationController:didCompleteWithError:")]
            public void DidComplete(ASAuthorizationController controller, NSError error)
            {
                CompletedWithError?.Invoke(controller, error);
            }
        }

        private class CustomPresentationContextProvider : NSObject, IASAuthorizationControllerPresentationContextProviding
        {
            private readonly WeakReference<Func<UIWindow>> _windowProviderRef;

            public CustomPresentationContextProvider(Func<UIWindow> func)
            {
                _windowProviderRef = new WeakReference<Func<UIWindow>>(func);
            }

            public UIWindow GetPresentationAnchor(ASAuthorizationController controller)
            {
                return _windowProviderRef.TryGetTarget(out var windowProvider)
                    ? windowProvider.Invoke()
                    : UIApplication.SharedApplication.KeyWindow;
            }
        }
    }
}
