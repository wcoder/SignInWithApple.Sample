using System;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using WebKit;

namespace SignInWithApple.Sample.iOS.SignInWithApple
{
    public class CustomWebViewSignInWithAppleService
    {
        private readonly CustomAuthModel _model;

        public CustomWebViewSignInWithAppleService()
        {
            _model = new CustomAuthModel();
        }

        public void SingIn()
        {
            var topViewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            var viewController = new CustomAuthViewController(_model);
            
            topViewController?.PresentViewController(viewController, true, null);
        }
     
        // can be ViewModel
        private class CustomAuthModel
        {
            public string Url => "https://.../account/login/external?provider=Apple";

            public bool IsCallbackPath(string path) => path == "/account/login-confirm/callback"
                                                       || path == "/account/login/external/callback";

            public bool IsConfirmEmailPath(string path) => path == "/account/login-confirm";

            public bool IsBusy { get; set; }
            
            public bool IsInitialized { get; set; }

            public void Login(string json)
            {
                // TODO: get access_token from json string
            }
        }
            
        private class CustomAuthViewController : UIViewController
        {
            private readonly CustomAuthModel _model;

            public CustomAuthViewController(CustomAuthModel model)
            {
                _model = model;
            }
            
            public override void ViewDidLoad()
            {
                base.ViewDidLoad();
                
                var webView = new WKWebView(View!.Frame, new WKWebViewConfiguration());
                View.AddSubview(webView);
                
                webView.NavigationDelegate = new LoginWithAppleWebViewDelegate(_model, () => DismissModalViewController(true));
                webView.LoadRequest(new NSUrlRequest(new NSUrl(_model.Url)));
            }
        }
            
        
        private class LoginWithAppleWebViewDelegate : WKNavigationDelegate
        {
            private readonly WeakReference<CustomAuthModel> _modelRef;
            private readonly WeakReference<Action> _closeRef;
            
            private CancellationTokenSource? _cts;

            public LoginWithAppleWebViewDelegate(CustomAuthModel model, Action close)
            {
                _modelRef = new WeakReference<CustomAuthModel>(model);
                _closeRef = new WeakReference<Action>(close);
            }

            public override void DecidePolicy(
                WKWebView webView,
                WKNavigationAction navigationAction,
                Action<WKNavigationActionPolicy> decisionHandler)
            {
                var path = navigationAction.Request.Url.Path;

                if (_modelRef.TryGetTarget(out CustomAuthModel model))
                {
                    if (model.IsCallbackPath(path))
                    {
                        model.IsBusy = true;    
                    }
                    else if (model.IsConfirmEmailPath(path))
                    {
                        model.IsBusy = false;
                    }
                }

                decisionHandler(WKNavigationActionPolicy.Allow);
            }

            public override void DidFailProvisionalNavigation(
                WKWebView webView,
                WKNavigation navigation,
                NSError error)
            {
                // HACK YP: handling when auth was canceled
                _cts = new CancellationTokenSource();
                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(1000, _cts.Token);
                        _cts.Token.ThrowIfCancellationRequested();
                        webView.BeginInvokeOnMainThread(() =>
                        {
                            if (_closeRef.TryGetTarget(out var close))
                            {
                                close.Invoke();
                            }
                        });
                    }
                    catch (OperationCanceledException)
                    {
                        // ignored
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }

            public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
            {
                _cts?.Cancel();
            }

            public override async void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
            {
                if (!_modelRef.TryGetTarget(out CustomAuthModel model))
                {
                    return;
                }
                
                if (!_closeRef.TryGetTarget(out var close))
                {
                    return;
                }
                
                if (!model.IsInitialized)
                {
                    model.IsInitialized = true;
                    model.IsBusy = false;
                }
               
                var url = webView.Url;
                if (url == null)
                {
                    return;
                }

                if (!model.IsCallbackPath(url.Path))
                {
                    return;
                }

                var rawPageBody = await webView.EvaluateJavaScriptAsync("document.documentElement.outerText");
                var content = rawPageBody.ToString();
                    
                RemoveAppleCookies();

                model.Login(content);
                    
                close.Invoke();
            }

            private void RemoveAppleCookies()
            {
                var websiteDataTypes = WKWebsiteDataStore.AllWebsiteDataTypes;
                WKWebsiteDataStore.DefaultDataStore.FetchDataRecordsOfTypes(websiteDataTypes, records =>
                {
                    for (nuint i = 0; i < records.Count; i++)
                    {
                        var record = records.GetItem<WKWebsiteDataRecord>(i);
                        if (record.DisplayName.Contains("apple"))
                        {
                            WKWebsiteDataStore.DefaultDataStore.RemoveDataOfTypes(
                                record.DataTypes,
                                new[] { record },
                                () =>
                                {
                                    Console.WriteLine($"deleted: {record.DisplayName}");
                                });
                        }
                    }
                });
            }
        }
    }
}