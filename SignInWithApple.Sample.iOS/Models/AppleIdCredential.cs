namespace SignInWithApple.Sample.iOS.Models
{
    public class AppleIdCredential
    {
        // Identifying a User:

        public string IdentityToken { get; set; }
        
        public string AuthorizationCode { get; set; }

        public string State { get; set; }

        public string User { get; set; }

        // Getting Contact Information:
        
        public string FullName { get; set; }

        public string GivenName { get; set; }
        
        public string FamilyName { get; set; }
        
        public string Email { get; set; }
    }
}