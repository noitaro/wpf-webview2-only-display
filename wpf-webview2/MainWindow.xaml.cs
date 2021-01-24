using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace wpf_webview2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }

        async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;
            webView.CoreWebView2.WebResourceResponseReceived += CoreWebView2_WebResourceResponseReceived;
            webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            webView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
        }

        private void CoreWebView2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            //Debug.WriteLine($"SourceChanged: {webView.CoreWebView2.Source}");
            textBox.Text = webView.CoreWebView2.Source;
        }

        private async void CoreWebView2_WebResourceResponseReceived(object sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            // URL の末尾が jpg か png のみ、画像を保存する。
            if (e.Request.Uri.EndsWith("jpg") || e.Request.Uri.EndsWith("png"))
            {
                //Debug.WriteLine($"WebResourceResponseReceived: {e.Request.Uri}");

                // フォルダが無ければ作成する。
                if (!File.Exists("img")) Directory.CreateDirectory("img");

                var uri = new Uri(e.Request.Uri);

                // 非同期でレスポンス画像を取得する。
                using (var stream = await e.Response.GetContentAsync())
                {
                    using (var fileStream = new FileStream($"img/{uri.Segments.Last()}", FileMode.Create, FileAccess.Write))
                    {
                        // ストリームをファイルに保存する。
                        stream.CopyTo(fileStream);
                    };
                };
            }
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            // リンク先を新しいウィンドウで開かなくする。
            e.Handled = true;

            // リンク先をメイン画面のWebView2で開く。
            webView.CoreWebView2.Navigate(e.Uri);
        }

        private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            Debug.WriteLine($"NavigationStarting: {e.Uri}");
            var uri = new Uri(e.Uri);

            // 移動先がpixiv以外の場合は、ページ遷移をキャンセルする。
            if (!uri.Host.Contains("pixiv"))
            {
                webView.CoreWebView2.ExecuteScriptAsync($"alert('pixiv以外のページを表示する事は出来ません。')");
                e.Cancel = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // IDを自動入力する.
            webView.CoreWebView2.ExecuteScriptAsync($"document.querySelector('#LoginComponent > form > div.input-field-group > div:nth-child(1) > input[type=text]').value = '★★★ ID ★★★'");
            // パスワードを自動入力する.
            webView.CoreWebView2.ExecuteScriptAsync($"document.querySelector('#LoginComponent > form > div.input-field-group > div:nth-child(2) > input[type=password]').value = '★★★ パスワード ★★★'");
            // ログインをする.
            webView.CoreWebView2.ExecuteScriptAsync($"document.querySelector('#LoginComponent > form > button').click()");
        }
    }
}
