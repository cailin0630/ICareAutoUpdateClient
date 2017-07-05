using ICareAutoUpdateClient.Common;
using ICareAutoUpdateClient.Log;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace ICareAutoUpdateClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            Icon = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Image\\MainLogo.jpg")));
            Task.Factory.StartNew(StartUpdate);
            Logger.Main.Info("程序启动完成");
            
        }

        private static Message _message;
        private bool _mainPackageDownload;
        private bool _publibPackageDownload;
       

        private void StartUpdate()
        {
            

            var pargs = Environment.GetCommandLineArgs();
            if (pargs.Length < 4)
            {
                AppendText($"启动参数错误:{pargs[0]}");
                ShutDownApp();
                return;
            }

            try
            {
                _message = new Message
                {
                    LocalUrl = pargs[1].TrimEnd('\\'),
                    UpdateUrl = pargs[2],
                    PublicLibUrl = pargs[3]
                };
                Logger.Main.Info($"获取命令行参数:{JsonConvert.SerializeObject(_message)}");
                if (_message.UpdateUrl == "-")
                {
                    _mainPackageDownload = true;
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        MainStackPanel.Visibility = Visibility.Collapsed;
                    }));
                }
                if (_message.PublicLibUrl == "-")
                {
                    _publibPackageDownload = true;
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        PublicStackPanel.Visibility = Visibility.Collapsed;
                    }));
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"参数解析异常:{ex.Message} {ex.StackTrace}");
                AppendText($"启动参数错误:{pargs[1]}\n{pargs[2]}");
                ShutDownApp();
                return;
            }

            AppendText("开始更新...");
            ProcessProvide.KillProcess("IDCube");

            if (File.Exists(Path.Combine(_message.LocalUrl, FrameworkConst.MainPackageName)))
            {
                File.Delete(Path.Combine(_message.LocalUrl, FrameworkConst.MainPackageName));
            }
            if (File.Exists(Path.Combine(_message.LocalUrl, FrameworkConst.PubLibPackageName)))
            {
                File.Delete(Path.Combine(_message.LocalUrl, FrameworkConst.PubLibPackageName));
            }

            //todo 下载主程序更新包
            if (_message.UpdateUrl != "-")
            {
                var updateUrl = new Uri(_message.UpdateUrl);
                var webclient = new WebProvide
                {
                    DownloadProgressChangedAction = MainPackage_DownloadProgressChanged,
                    DownloadFileCompletedAction = MainPackage_DownloadFileCompleted
                };
                webclient.DownLoad(updateUrl, FrameworkConst.MainPackageName);
            }

            //TODO 下载公共库更新包
            if (_message.PublicLibUrl != "-")
            {
                var pubLibUrl = new Uri(_message.PublicLibUrl);
                var webclient = new WebProvide
                {
                    DownloadProgressChangedAction = PubLibPackage_DownloadProgressChanged,
                    DownloadFileCompletedAction = PubLibPackage_DownloadFileCompleted
                };
               
                webclient.DownLoad(pubLibUrl, FrameworkConst.PubLibPackageName);
            }
        }

        private void PubLibPackage_DownloadProgressChanged(object arg1, DownloadProgressChangedEventArgs arg2)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                PublibProgressBar.Value = arg2.ProgressPercentage;
                PublibTextBlock.Text = $"{arg2.ProgressPercentage}%";
            }));
        }

        private void MainPackage_DownloadProgressChanged(object arg1, DownloadProgressChangedEventArgs arg2)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainProgressBar.Value = arg2.ProgressPercentage;
                MainTextBlock.Text = $"{arg2.ProgressPercentage}%";
            }));
        }

        private void PubLibPackage_DownloadFileCompleted(object arg1, AsyncCompletedEventArgs arg2)
        {
            //todo 下载文件完成后解压更新
            AppendText("公共库更新下载完成...");

            //AppendText(!ZipHelper.UnZipPath(Path.Combine(_message.LocalUrl, WebProvide.PubLibPackageName),
            //    _pubLibExePath)
            //    ? "公共库更新失败,原因:解压覆盖失败..."
            //    : "公共库更新成功,解压覆盖完成...");

            //todo 解压并启动exe
            var pro = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(_message.LocalUrl, FrameworkConst.PubLibPackageName),
                }
            };

            pro.Start();
            pro.WaitForExit();
            _publibPackageDownload = true;
            Do();
        }

        private void MainPackage_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //todo 下载文件完成后解压更新
            AppendText("主程序更新下载完成...");

            AppendText(!ZipHelper.UnZipPath(Path.Combine(_message.LocalUrl, FrameworkConst.MainPackageName),
                _message.LocalUrl)
                ? "主程序更新失败,原因:解压覆盖失败..."
                : "主程序更新成功,解压覆盖完成...");
            _mainPackageDownload = true;
            Do();
        }

        private void AppendText(string msg)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Logger.Main.Info(msg);
                TextBoxInput.Text += $"[{DateTime.Now:yyyy:MM:dd HH:mm:ss}] {msg}\n";
            }));
#if DEBUG
            Thread.Sleep(2000);
#endif
        }

        private void Do()
        {
            lock (WindowStyleProperty)
            {
                if (!_mainPackageDownload || !_publibPackageDownload) return;
                StartMainProcess();
                ShutDownApp();
            }
        }

        private void StartMainProcess()
        {
            var exeName = Path.Combine(_message.LocalUrl, "IDCube.exe");
            var exeDir = Path.Combine(_message.LocalUrl, "IDCube");

            ProcessProvide.StartProcess(exeName, exeDir);
            AppendText("启动目标程序成功...");
        }

        private void ShutDownApp()
        {
            Thread.Sleep(5000);
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Application.Current.Shutdown();
            }));
        }
    }
}