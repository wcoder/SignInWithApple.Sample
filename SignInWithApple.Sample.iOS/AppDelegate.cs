using Foundation;
using SignInWithApple.Sample.iOS.Services;
using SignInWithApple.Sample.iOS.Services.SignInWithApple;
using SignInWithApple.Sample.iOS.ViewControllers;
using UIKit;

namespace SignInWithApple.Sample.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window { get; set; }
        
        internal static ISignInWithAppleService AuthService { get; private set; }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // For the purpose of this demo app, store the userIdentifier in the keychain.
            var sessionManager = new KeychainSessionManager("com.xamarin.AddingTheSignInWithAppleFlowToYourApp");

            AuthService = new SignInWithAppleService(sessionManager, () => Window);
            AuthService.GetCredentialState(
                credentialNotFound: ShowLoginPage);

            return true;
        }

        private void ShowLoginPage()
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

