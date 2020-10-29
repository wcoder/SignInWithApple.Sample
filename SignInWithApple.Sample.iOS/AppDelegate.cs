using Foundation;
using SignInWithApple.Sample.iOS.Services;
using SignInWithApple.Sample.iOS.ViewControllers;
using UIKit;

namespace SignInWithApple.Sample.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window { get; set; }
        
        internal static SampleSignInWithAppleService AuthService { get; private set; }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            AuthService = new SampleSignInWithAppleService(() => Window);
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

