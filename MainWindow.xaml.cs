using CefSharp;
using CefSharp.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using WpfCefSharpExample.Messaging;
using WpfCefSharpExample.Schemes;

namespace WpfCefSharpExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _counter = 0;
        private short _reloadCounter = 0;
        private const string _defaultUrl = "http://localhost:80";
        private string _defaultAuthority;

        public MainWindow()
        {
            _defaultAuthority = new Uri(_defaultUrl).Authority;

            CefSettings settings = new CefSettings();
            settings.RegisterScheme(new CefCustomScheme()
            {
                SchemeName = LocalSchemeHandlerFactory.SchemeName,
                SchemeHandlerFactory = new LocalSchemeHandlerFactory()
            });
            Cef.Initialize(settings);

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddTab();
        }

        private void AddTab()
        {
            _counter++;
            var browser = new ChromiumWebBrowser(_defaultUrl);
            browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
            var tabItem = new TabItem { Header = $"Tab {_counter}" };
            tabItem.Content = browser;
            tabControl.Items.Add(tabItem);
        }

        private void Browser_LoadError(object sender, LoadErrorEventArgs e)
        {
            // On a Cef UI thread so invoke up to the parent thread.
            Application.Current.Dispatcher.Invoke(() => ReloadBrowser());
        }

        private void ReloadBrowser()
        {
            // Retry the page a few times and then redirect to an error page if fails.
            if (_reloadCounter < 3)
            {
                _reloadCounter++;
                Browser.Reload();
            }
            else
            {
                _reloadCounter = 0;
                ShowErrorPage();
            }
        }

        private void ShowErrorPage()
        {
            Browser.Address = "local://Error.html";
        }

        private void Browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldVal = (e.OldValue as bool?) ?? false;
            var newVal = (e.NewValue as bool?) ?? false;
            var browser = sender as ChromiumWebBrowser;

            if (browser != null && newVal && !oldVal)
            {
                var sessionCookie = new Cookie
                {
                    Domain = "localhost",
                    HttpOnly = false,

                    Name = "jvandernoot-user-session",
                    Expires = DateTime.Now.AddYears(10),
                    Secure = true,
                    Value = "Visitor"
                };

                                
                if (browser == Browser)
                {
                    browser.GetCookieManager().SetCookie(_defaultAuthority, sessionCookie);
                }
                
                // Uncomment to show dev tools on the load of the browser
                //browser.ShowDevTools();
            }
        }

        private void Browser_JavascriptMessageReceived(object sender, JavascriptMessageReceivedEventArgs e)
        {
            //Complex objects are initially expresses as IDicionary
            //You can use dynamic to access properties (the IDicionary is an ExpandoObject)
            //dynamic msg = e.Message;
            //Alternatively you can use the built in Model Binder to convert to a custom model
            var msg = e.ConvertMessageTo<BroadcastMessage>();

            Console.WriteLine("Received Message: " + msg.Message);

            // Invoke a JS method into the Browser to broadcast a message to the loaded page
            string script = @"var event = new CustomEvent('messageReceiveEvent', {detail: 'Message from WPF'});
                                document.dispatchEvent(event);";

            Browser.ExecuteScriptAsync(script);
        }
    }
}
