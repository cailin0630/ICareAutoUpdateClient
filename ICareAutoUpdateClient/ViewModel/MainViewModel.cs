using GalaSoft.MvvmLight;
using ICareAutoUpdateClient.Common;
using ICareAutoUpdateClient.Log;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ICareAutoUpdateClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Task.Factory.StartNew(StartUpdate);
        }

        private static Message _message;

        private void StartUpdate()
        {
            try
            {
                Logger.Main.Info("程序启动完成");
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
                        MainPackageDownload = true;
                    }
                    if (_message.PublicLibUrl == "-")
                    {
                        PublibPackageDownload = true;
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
                TryDeleteTempFile();

                //todo 下载主程序更新包
                if (_message.UpdateUrl != "-")
                {
                    var updateUrl = new Uri(_message.UpdateUrl);
                    var webclient = new WebProvide
                    {
                        DownloadProgressChangedAction = MainPackage_DownloadProgressChanged,
                        DownloadFileCompletedAction = MainPackage_DownloadFileCompleted
                    };
                    webclient.DownLoad(updateUrl, FrameworkConst.MainPackagePath);
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

                    webclient.DownLoad(pubLibUrl, FrameworkConst.PubLibPackagePath);
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"更新异常:{ex.Message} {ex.StackTrace}");
                AppendText($"更新异常:{ex.Message}");
                ShutDownApp();
                return;
            }
        }

        private void PubLibPackage_DownloadProgressChanged(object arg1, DownloadProgressChangedEventArgs arg2)
        {
            PublibPackageProgressPercentage = arg2.ProgressPercentage;
        }

        private void MainPackage_DownloadProgressChanged(object arg1, DownloadProgressChangedEventArgs arg2)
        {
            MainPackageProgressPercentage = arg2.ProgressPercentage;
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
                    FileName = FrameworkConst.PubLibPackagePath,
                }
            };

            pro.Start();
            pro.WaitForExit();
            PublibPackageDownload = true;
            AppendText("公共库更新成功...");
            Do();
        }

        private void MainPackage_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //todo 下载文件完成后解压更新
            AppendText("主程序更新下载完成...");

            AppendText(!ZipHelper.UnZipPath(FrameworkConst.MainPackagePath,
                _message.LocalUrl)
                ? "主程序更新失败,原因:解压覆盖失败..."
                : "主程序更新成功,解压覆盖完成...");
            MainPackageDownload = true;
            Do();
        }

        private void AppendText(string msg)
        {
            Logger.Main.Info(msg);
            TipMessage += $"[{DateTime.Now:yyyy:MM:dd HH:mm:ss}] {msg}\n";

#if DEBUG
            Thread.Sleep(2000);
#endif
        }

        private void Do()
        {
            lock (new object())
            {
                if (!MainPackageDownload || !PublibPackageDownload) return;
                StartMainProcess();
                ShutDownApp();
            }
        }

        private void StartMainProcess()
        {
            try
            {
                var exeName = Path.Combine(_message.LocalUrl, "IDCube.exe");
                var exeDir = Path.Combine(_message.LocalUrl, "IDCube");

                ProcessProvide.StartProcess(exeName, exeDir);
                AppendText("启动目标程序成功...");
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"启动主程序异常:{ex.Message} {ex.StackTrace}");
                AppendText("启动目标程序异常...");
            }
           
          
        }

        private void ShutDownApp()
        {
            Thread.Sleep(5000);
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                App.Current.Shutdown();
            }));
        }

        private void TryDeleteTempFile()
        {
            try
            {
                if (!Directory.Exists(FrameworkConst.TempFolder))
                    Directory.CreateDirectory(FrameworkConst.TempFolder);
                File.Delete(FrameworkConst.MainPackagePath);
                File.Delete(FrameworkConst.PubLibPackagePath);
            }
            catch
            {
            }
        }

        #region MyRegion

        public string _mainTitle = "ICare.AutoUpdateClient";

        public string MainTitle
        {
            get { return _mainTitle; }
            set
            {
                _mainTitle = value;
                RaisePropertyChanged(nameof(MainTitle));
            }
        }

        public ImageSource _mainLogo = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Image\\MainLogo.jpg")));

        public ImageSource MainLogo
        {
            get { return _mainLogo; }
            set
            {
                _mainLogo = value;
                RaisePropertyChanged(nameof(MainLogo));
            }
        }

        public bool _mainPackageDownload = false;

        public bool MainPackageDownload
        {
            get { return _mainPackageDownload; }
            set
            {
                _mainPackageDownload = value;
                RaisePropertyChanged(nameof(MainPackageDownload));
            }
        }

        public bool _publibPackageDownload = false;

        public bool PublibPackageDownload
        {
            get { return _publibPackageDownload; }
            set
            {
                _publibPackageDownload = value;
                RaisePropertyChanged(nameof(PublibPackageDownload));
            }
        }

        public int _mainPackageProgressPercentage;

        public int MainPackageProgressPercentage
        {
            get { return _mainPackageProgressPercentage; }
            set
            {
                _mainPackageProgressPercentage = value;
                RaisePropertyChanged(nameof(MainPackageProgressPercentage));
            }
        }

        public int _publibPackageProgressPercentage;

        public int PublibPackageProgressPercentage
        {
            get { return _publibPackageProgressPercentage; }
            set
            {
                _publibPackageProgressPercentage = value;
                RaisePropertyChanged(nameof(PublibPackageProgressPercentage));
            }
        }

        public string _tipMessage;

        public string TipMessage
        {
            get { return _tipMessage; }
            set
            {
                _tipMessage = value;
                RaisePropertyChanged(nameof(TipMessage));
            }
        }

        #endregion MyRegion
    }
}