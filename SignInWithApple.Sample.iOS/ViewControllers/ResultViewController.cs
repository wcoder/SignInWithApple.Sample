using System;
using Foundation;
using SignInWithApple.Sample.iOS.Models;
using UIKit;

namespace SignInWithApple.Sample.iOS.ViewControllers
{
    public partial class ResultViewController : UIViewController
    {
        public ResultViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            userIdentifierLabel.Text = AppDelegate.AuthService.CurrentUserIdentifier;
        }
        
        public void SetInfo(AppleIdCredential credential)
        {
            userIdentifierLabel.Text = credential.User;
            givenNameLabel.Text = credential.GivenName ?? "";
            familyNameLabel.Text = credential.FamilyName ?? "";
            emailLabel.Text = credential.Email ?? "";
        }

        partial void SignOutButtonPressed(NSObject sender)
        {
            AppDelegate.AuthService.SignUp();

            // Clear the user interface.
            userIdentifierLabel.Text = "";
            givenNameLabel.Text = "";
            familyNameLabel.Text = "";
            emailLabel.Text = "";

            // Display the login controller again.
            var storyboard = UIStoryboard.FromName("Main", null);

            if (!(storyboard.InstantiateViewController(nameof(LoginViewController)) is LoginViewController viewController))
            {
                return;
            }

            viewController.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
            viewController.ModalInPresentation = true;
            PresentViewController(viewController, true, null);
        }
    }
}

