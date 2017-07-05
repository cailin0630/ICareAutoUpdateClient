using System;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace ICareAutoUpdateClient.Common
{
    public class WebProvide
    {
        public Action<object, AsyncCompletedEventArgs> DownloadFileCompletedAction;
        public Action<object, DownloadProgressChangedEventArgs> DownloadProgressChangedAction;

        public void DownLoad(Uri url, string fileName)
        {
            var webClient = new WebClient
            {
                Proxy = { Credentials = CredentialCache.DefaultCredentials },
                Encoding = Encoding.UTF8
            };
            webClient.DownloadFileCompleted += DownloadFileCompleted;
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            webClient.DownloadFileAsync(url, fileName);
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgressChangedAction(sender, e);
        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DownloadFileCompletedAction(sender, e);
        }
    }
}