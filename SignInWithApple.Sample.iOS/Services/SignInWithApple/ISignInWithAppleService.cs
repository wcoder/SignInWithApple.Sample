using System;

namespace SignInWithApple.Sample.iOS.Services.SignInWithApple
{
    public interface ISignInWithAppleService
    {
        event EventHandler<AppleIdCredential> CompletedWithAppleId;
        event EventHandler<PasswordCredential> CompletedWithPassword;

        string CurrentUserIdentifier { get; }

        /// <summary>
        ///     Handler the credential state for the given user.
        /// </summary>
        /// <param name="authorized">The Apple ID credential is valid handler.</param>
        /// <param name="credentialRevoked">The Apple ID credential is revoked handler.</param>
        /// <param name="credentialNotFound">No credential was found, so show the sign-in UI.</param>
        void GetCredentialState(
            Action authorized = null,
            Action credentialRevoked = null,
            Action credentialNotFound = null);

        void SignIn();
        void SignUp();

        /// <summary>
        ///     Prompts the user if an existing iCloud Keychain credential or Apple ID credential is found.
        /// </summary>
        void PerformExistingAccountSetupFlows();
    }
}