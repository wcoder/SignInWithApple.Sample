using AuthenticationServices;
using Foundation;
using SignInWithApple.Sample.iOS.Utils;
using SignInWithApple.Sample.iOS.ViewControllers;
using UIKit;

namespace SignInWithApple.Sample.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window { get; set; }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            InitAuthorization();

            return true;
        }

        private void InitAuthorization()
        {
            var appleIdProvider = new ASAuthorizationAppleIdProvider();
            var userId = KeychainItem.CurrentUserIdentifier;

            appleIdProvider.GetCredentialState(userId, (credentialState, error) =>
            {
                switch (credentialState)
                {
                    case ASAuthorizationAppleIdProviderCredentialState.Authorized:
                        // The Apple ID credential is valid.
                        break;
                    case ASAuthorizationAppleIdProviderCredentialState.Revoked:
                        // The Apple ID credential is revoked.
                        break;
                    case ASAuthorizationAppleIdProviderCredentialState.NotFound:
                        // No credential was found, so show the sign-in UI.

                        GoToLogin();
                        break;
                }
            });
        }

        private void GoToLogin()
        {
            InvokeOnMainThread(() =>
            {
                var storyboard = UIStoryboard.FromName("Main", null);

                if (!(storyboard.InstantiateViewController(nameof(LoginViewController)) is LoginViewController viewController))
                {
                    return;
                }

                viewController.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
                viewController.ModalInPresentation = true;

                Window?.RootViewController?.PresentViewController(viewController, true, null);
            });
        }
    }
}

