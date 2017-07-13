using GalaSoft.MvvmLight.Command;
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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ICareAutoUpdateClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            MinCommand = new RelayCommand(() => App.Current.MainWindow.WindowState = WindowState.Minimized, () => true);
            CloseCommand = new RelayCommand(() => App.Current.Shutdown(), () => true);
        }

        protected override void OnLoad()
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

                foreach (var p in pargs)
                {
                    Logger.Main.Info($"CommandLineArgs:{p}");
                }
                if (pargs.Length < 2 || pargs[1] == null)
                {
                    AppendText($"启动参数错误");
                    //ShutDownApp();
                    return;
                }

                try
                {
                    _message = new Message
                    {
                        LocalUrl = pargs[1].Split('|')[0].TrimEnd('\\'),
                        UpdateUrl = pargs[1].Split('|')[1],
                        PublicLibUrl = pargs[1].Split('|')[2]
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
                    AppendText($"启动参数错误");
                    //ShutDownApp();
                    return;
                }

                AppendText("正在更新中...");
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
                //ShutDownApp();
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
            //AppendText("公共库更新下载完成...");
            if (arg2.Error != null)
            {
                //todo 异常处理
                AppendText($"公共库下载异常:{arg2.Error.Message}");
                Logger.Main.Error($"公共库下载异常:{arg2.Error.Message} {arg2.Error.InnerException}");
                return;
            }

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
            //AppendText("公共库更新成功...");
            Do();
        }

        private void MainPackage_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //todo 下载文件完成后解压更新
            //AppendText("主程序更新下载完成...");
            if (e.Error != null)
            {
                //todo 异常处理
                AppendText($"主程序下载异常:{e.Error.Message}");
                Logger.Main.Error($"主程序下载异常:{e.Error.Message} {e.Error.InnerException}");
                return;
            }

            if (!ZipHelper.UnZipPath(FrameworkConst.MainPackagePath,
                _message.LocalUrl))
            {
                AppendText("更新失败,原因:解压覆盖失败...");
            }

            MainPackageDownload = true;
            Do();
        }

        private void AppendText(string msg)
        {
            Logger.Main.Info(msg);
            TipMessage = $"{msg}";

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
                //ShutDownApp();
            }
        }

        private void StartMainProcess()
        {
            try
            {
                var exeName = Path.Combine(_message.LocalUrl, "IDCube.exe");
                var exeDir = Path.Combine(_message.LocalUrl, "IDCube");

                ProcessProvide.StartProcess(exeName, exeDir);
                AppendText("更新完成!");
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

        private void GetProgressPercentageMin()
        {
            if (!MainPackageDownload && !PublibPackageDownload)
            {
                //todo 同时下载时
                ProgressPercentageMin = Math.Min(MainPackageProgressPercentage, PublibPackageProgressPercentage);
            }
            else if (!MainPackageDownload && PublibPackageDownload)
            {
                //todo x主程序
                ProgressPercentageMin = MainPackageProgressPercentage;
            }
            else
            {
                ProgressPercentageMin = PublibPackageProgressPercentage;
            }
        }

        #region MyRegion

        #region Image

        public ImageSource _logoImage = ResourceEngine.GetImageSource("logo.png");

        public ImageSource LogoImage
        {
            get { return _logoImage; }
            set
            {
                _logoImage = value;

                OnPropertyChanged();
            }
        }

        public ImageSource _closeImage = ResourceEngine.GetImageSource("icon_cancel.png");

        public ImageSource CloseImage
        {
            get { return _closeImage; }
            set
            {
                _closeImage = value;

                OnPropertyChanged();
            }
        }

        public ImageSource _minImage = ResourceEngine.GetImageSource("icon_small.png");

        public ImageSource MinImage
        {
            get { return _minImage; }
            set
            {
                _minImage = value;

                OnPropertyChanged();
            }
        }

        public ImageSource _backgroundImage = ResourceEngine.GetImageSource("bg.png");

        public ImageSource BackgroundImage
        {
            get { return _backgroundImage; }
            set
            {
                _backgroundImage = value;

                OnPropertyChanged();
            }
        }

        public ImageSource _mainLogo = ResourceEngine.GetImageSource("MainLogo.jpg");

        public ImageSource MainLogo
        {
            get { return _mainLogo; }
            set
            {
                _mainLogo = value;

                OnPropertyChanged();
            }
        }

        #endregion Image

        #region download

        private bool _mainPackageDownload = false;

        public bool MainPackageDownload
        {
            get { return _mainPackageDownload; }
            set
            {
                _mainPackageDownload = value;

                OnPropertyChanged();
            }
        }

        private bool _publibPackageDownload = false;

        public bool PublibPackageDownload
        {
            get { return _publibPackageDownload; }
            set
            {
                _publibPackageDownload = value;

                OnPropertyChanged();
            }
        }

        private int _mainPackageProgressPercentage;

        public int MainPackageProgressPercentage
        {
            get { return _mainPackageProgressPercentage; }
            set
            {
                _mainPackageProgressPercentage = value;
                GetProgressPercentageMin();

                OnPropertyChanged();
            }
        }

        private int _publibPackageProgressPercentage;

        public int PublibPackageProgressPercentage
        {
            get { return _publibPackageProgressPercentage; }
            set
            {
                _publibPackageProgressPercentage = value;

                GetProgressPercentageMin();
                OnPropertyChanged();
            }
        }

        private int _progressPercentageMin;

        public int ProgressPercentageMin
        {
            get { return _progressPercentageMin; }
            set
            {
                _progressPercentageMin = value;
                OnPropertyChanged();
            }
        }

        private string _tipMessage;

        public string TipMessage
        {
            get { return _tipMessage; }
            set
            {
                _tipMessage = value;

                OnPropertyChanged();
            }
        }

        #endregion download

        private ICommand _minCommand;

        public ICommand MinCommand
        {
            get { return _minCommand; }
            set
            {
                _minCommand = value;
                OnPropertyChanged();
            }
        }

        private ICommand _closeCommand;

        public ICommand CloseCommand
        {
            get { return _closeCommand; }
            set
            {
                _closeCommand = value;
                OnPropertyChanged();
            }
        }

        #endregion MyRegion
    }
}