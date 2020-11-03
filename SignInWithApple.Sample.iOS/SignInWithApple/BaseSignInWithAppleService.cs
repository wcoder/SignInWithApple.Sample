using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AuthenticationServices;
using Foundation;
using UIKit;

namespace SignInWithApple.Sample.iOS.SignInWithApple
{
    /// <summary>
    ///     The base class to integrate with Sign-In with Apple.
    /// </summary>
    public abstract class BaseSignInWithAppleService : IDisposable
    {
        private readonly ASAuthorizationAppleIdProvider _appleIdProvider;
        private readonly CustomDelegate _authControllerDelegate;
        private readonly CustomPresentationContextProvider _presentationProvider;

        private IDisposable? _credentialRevokedObserver;

        protected BaseSignInWithAppleService(Func<UIWindow> windowProvider)
        {
            _appleIdProvider = new ASAuthorizationAppleIdProvider();

            _authControllerDelegate = new CustomDelegate();
            _authControllerDelegate.CompletedWithAppleId += DidCompleteAuthWithAppleId;
            _authControllerDelegate.CompletedWithPassword += DidCompleteAuthWithPassword;
            _authControllerDelegate.CompletedWithError += DidCompleteAuthWithError;
            
            _presentationProvider = new CustomPresentationContextProvider(windowProvider);
        }

        public void Dispose()
        {
            _authControllerDelegate.CompletedWithAppleId -= DidCompleteAuthWithAppleId;
            _authControllerDelegate.CompletedWithPassword -= DidCompleteAuthWithPassword;
            _authControllerDelegate.CompletedWithError -= DidCompleteAuthWithError;
            
            _credentialRevokedObserver?.Dispose();
            _credentialRevokedObserver = null;
        }

        public abstract string CurrentUserIdentifier { get; }

        /// <summary>
        ///     Handler the credential state for the given user.
        /// </summary>
        /// <param name="authorized">The Apple ID credential is valid handler.</param>
        /// <param name="credentialRevoked">The Apple ID credential is revoked handler.</param>
        /// <param name="credentialNotFound">No credential was found, so show the sign-in UI.</param>
        public void GetCredentialState(
            Action authorized = null,
            Action credentialRevoked = null,
            Action credentialNotFound = null)
        {
            _appleIdProvider.GetCredentialState(CurrentUserIdentifier, (credentialState, error) =>
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

        public void SignIn(
            bool needEmail = true,
            bool needFullName = true)
        {
            var requestedScopes = new List<ASAuthorizationScope>();
            if (needEmail)
            {
                requestedScopes.Add(ASAuthorizationScope.Email);
            }
            if (needFullName)
            {
                requestedScopes.Add(ASAuthorizationScope.FullName);
            }
            
            var request = _appleIdProvider.CreateRequest();
            request.RequestedScopes = requestedScopes.ToArray();

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

        public abstract void SignUp();
        
        /// <summary>
        ///     Prompts the user if an existing iCloud Keychain credential or Apple ID credential is found.
        /// </summary>
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

        /// <summary>
        ///     Optional method. 
        ///     Add observer when the user revokes the sign-in in Settings app
        /// </summary>
        /// <param name="appleIdStateRevoked">Log out user, change UI etc</param>
        public void AddCredentialRevokedObserver(Action appleIdStateRevoked)
        {
            _credentialRevokedObserver = ASAuthorizationAppleIdProvider.Notifications.ObserveCredentialRevoked(
                (s, args) =>
                {
                    appleIdStateRevoked.Invoke();
                });
        }
        
        /// <summary>
        ///     Register New Account. Will be called once to save details.
        /// </summary>
        /// <param name="appleIdCredential">A credential that results from a successful Apple ID authentication.</param>
        /// <returns></returns>
        protected abstract Task RegisterNewAccount(ASAuthorizationAppleIdCredential appleIdCredential);

        /// <summary>
        ///     Sign-in with an existing account. Without additional user data (email, name).
        /// </summary>
        /// <param name="appleIdCredential">A credential that results from a successful Apple ID authentication.</param>
        /// <returns></returns>
        protected abstract Task SignInWithExistingAccount(ASAuthorizationAppleIdCredential appleIdCredential);
        
        /// <summary>
        ///     Sign in using an existing iCloud Keychain credential.
        /// </summary>
        /// <param name="passwdCredential">A password credential.</param>
        /// <returns></returns>
        protected abstract Task SignInWithUserAndPassword(ASPasswordCredential passwdCredential);

        protected abstract void HandleException(Exception exception);

        private async void DidCompleteAuthWithAppleId(object sender, ASAuthorizationAppleIdCredential e)
        {
            // Apple will only provide you the requested details (Name, Email) on the first authentication.
            if (string.IsNullOrEmpty(e.Email))
            {
                await SignInWithExistingAccount(e);
            }
            else
            {
                await RegisterNewAccount(e);
            }
        }

        private void DidCompleteAuthWithPassword(object sender, ASPasswordCredential e)
        {
            SignInWithUserAndPassword(e);
        }

        private void DidCompleteAuthWithError(object sender, NSError error)
        {
            HandleException(new NSErrorException(error));
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
                // Determine whether the user authenticated via Apple ID or a stored iCloud password.
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
