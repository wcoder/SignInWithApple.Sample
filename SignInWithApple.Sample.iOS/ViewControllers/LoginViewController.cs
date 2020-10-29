using System;
using AuthenticationServices;
using SignInWithApple.Sample.iOS.Services;
using SignInWithApple.Sample.iOS.Services.SignInWithApple;
using UIKit;

namespace SignInWithApple.Sample.iOS.ViewControllers
{
    public partial class LoginViewController : UIViewController
    {
        public LoginViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AddSignInWithAppleButton();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            AppDelegate.AuthService.CompletedWithAppleId += DidCompleteAuthWithAppleId;
            AppDelegate.AuthService.CompletedWithPassword += DidCompleteAuthWithPassword;
            
            AppDelegate.AuthService.PerformExistingAccountSetupFlows();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            
            AppDelegate.AuthService.CompletedWithAppleId -= DidCompleteAuthWithAppleId;
            AppDelegate.AuthService.CompletedWithPassword -= DidCompleteAuthWithPassword;
        }

        private void AddSignInWithAppleButton()
        {
            var authorizationButton = new ASAuthorizationAppleIdButton(
                ASAuthorizationAppleIdButtonType.Default,
                ASAuthorizationAppleIdButtonStyle.White);
            authorizationButton.CornerRadius = 20;
            authorizationButton.TouchUpInside += OnTouchUpInsideAppleIdButton;

            loginProviderStackView.AddArrangedSubview(authorizationButton);
        }

        private void OnTouchUpInsideAppleIdButton(object sender, EventArgs e)
        {
            AppDelegate.AuthService.SignIn();
        }

        private void DidCompleteAuthWithAppleId(object sender, AppleIdCredential credential)
        {
            // For the purpose of this demo app, show the Apple ID credential information in the ResultViewController.
            if (!(PresentingViewController is ResultViewController viewController))
            {
                return;
            }

            InvokeOnMainThread(() =>
            {
                viewController.UserIdentifierText = credential.User;
                viewController.GivenNameText = credential.GivenName ?? "";
                viewController.FamilyNameText = credential.FamilyName ?? "";
                viewController.EmailText = credential.Email ?? "";

                DismissViewController(true, null);
            });
        }

        private void DidCompleteAuthWithPassword(object sender, PasswordCredential passwordCredential)
        {
            var username = passwordCredential.User;
            var password = passwordCredential.Password;

            // For the purpose of this demo app, show the password credential as an alert.
            InvokeOnMainThread(() =>
            {
                var message = $"The app has received your selected credential from the keychain. \n\n Username: {username}\n Password: {password}";
                var alertController = UIAlertController.Create("Keychain Credential Received", message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Dismiss", UIAlertActionStyle.Cancel, null));

                PresentViewController(alertController, true, null);
            });
        }
    }
}

