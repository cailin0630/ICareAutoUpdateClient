using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;

namespace ICareAutoUpdateClient.Common
{
    public class WebProvide
    {
        public WebProvide()
        {
            WebClient=new WebClient();
        }

        public static  string MainPackageName = "MainPackage.7z";
        //public static string PubLibPackageName = "PubLibPackage.7z";
        public static string PubLibPackageName = "PubLibPackage.exe";
        private  WebClient WebClient;
        private  string _token = "AutoUpdate";
        public  Action<object,AsyncCompletedEventArgs> DownloadFileCompletedAction;
        public  Action<object, DownloadProgressChangedEventArgs> DownloadProgressChangedAction;


        
        public  void DownLoad(Uri url,string fileName)
        {
            WebClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
            WebClient.DownloadFileCompleted += DownloadFileCompleted;
            WebClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            WebClient.Encoding = Encoding.UTF8;
            WebClient.DownloadFileAsync(url, fileName);
        }

        private  void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgressChangedAction(sender,e);
        }

        private  void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DownloadFileCompletedAction( sender,e);
        }
    }
}
