using System;
using Foundation;
using UIKit;

namespace SignInWithApple.Sample.iOS.ViewControllers
{
    public partial class ResultViewController : UIViewController
    {
        public string UserIdentifierText
        {
            get => userIdentifierLabel.Text;
            set => userIdentifierLabel.Text = value;
        }

        public string GivenNameText
        {
            get => givenNameLabel.Text;
            set => givenNameLabel.Text = value;
        }

        public string FamilyNameText
        {
            get => familyNameLabel.Text;
            set => familyNameLabel.Text = value;
        }

        public string EmailText
        {
            get => emailLabel.Text;
            set => emailLabel.Text = value;
        }

        public ResultViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            userIdentifierLabel.Text = AppDelegate.AuthService.CurrentUserIdentifier;
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

