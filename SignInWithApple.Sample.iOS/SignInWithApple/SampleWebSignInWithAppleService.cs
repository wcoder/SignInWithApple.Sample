using System;
using System.Linq;
using AuthenticationServices;
using Foundation;
using UIKit;

namespace SignInWithApple.Sample.iOS.SignInWithApple
{
    /// <summary>
    /// Original example:
    ///     https://developer.apple.com/documentation/authenticationservices/authenticating_a_user_through_a_web_service
    /// </summary>
    public class SampleWebSignInWithAppleService
    {
        private const string AuthUrl = "https://.../api/account/login/external?provider=Apple";
        private readonly CustomPresentationContextProvider _presentationContext;
        
        public SampleWebSignInWithAppleService(Func<UIWindow> windowProvider)
        {
            _presentationContext = new CustomPresentationContextProvider(windowProvider);
        }

        public void SignIn()
        {
            var url = NSUrl.FromString(AuthUrl);
            var callbackScheme = "exampleauth";
            var session = new ASWebAuthenticationSession(url, callbackScheme, (callbackUrl, error) =>
            {
                if (error != null)
                {
                    // TODO: Handle error
                    return;
                }

                if (callbackUrl == null)
                {
                    // TODO: Handle error
                    return;
                }

                // The callback URL format depends on the provider. For this example:
                //   exampleauth://auth?token=1234
                var queryItems = NSUrlComponents.FromString(callbackUrl.AbsoluteString).QueryItems;
                var token = queryItems?.FirstOrDefault(x => x.Name == "token")?.Value;
                
                // TODO: Check token
            })
            {
                PresentationContextProvider = _presentationContext
            };
            session.Start();
        }

        public virtual void SignUp()
        {
        }

        private class CustomPresentationContextProvider : NSObject, IASWebAuthenticationPresentationContextProviding
        {
            private readonly WeakReference<Func<UIWindow>> _windowProviderRef;

            public CustomPresentationContextProvider(Func<UIWindow> func)
            {
                _windowProviderRef = new WeakReference<Func<UIWindow>>(func);
            }

            public UIWindow GetPresentationAnchor(ASWebAuthenticationSession session)
            {
                return _windowProviderRef.TryGetTarget(out var windowProvider)
                    ? windowProvider.Invoke()
                    : UIApplication.SharedApplication.KeyWindow;
            }
        }
    }
}