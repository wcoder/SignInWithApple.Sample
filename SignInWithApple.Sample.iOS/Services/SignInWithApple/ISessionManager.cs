namespace SignInWithApple.Sample.iOS.Services.SignInWithApple
{
    public interface ISessionManager
    {
        string CurrentUserIdentifier { get; }

        /// <summary>
        ///     Create an account in your system.
        /// </summary>
        /// <param name="credential"></param>
        void CreateUserIdentifier(AppleIdCredential credential);

        void DeleteUserIdentifier();
    }
}