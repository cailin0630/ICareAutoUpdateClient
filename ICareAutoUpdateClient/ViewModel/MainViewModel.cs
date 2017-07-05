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
                Logger.Main.Info("�����������");
                var pargs = Environment.GetCommandLineArgs();
                if (pargs.Length < 4)
                {
                    AppendText($"������������:{pargs[0]}");
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

                    Logger.Main.Info($"��ȡ�����в���:{JsonConvert.SerializeObject(_message)}");
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
                    Logger.Main.Error($"���������쳣:{ex.Message} {ex.StackTrace}");
                    AppendText($"������������:{pargs[1]}\n{pargs[2]}");
                    ShutDownApp();
                    return;
                }

                AppendText("��ʼ����...");
                ProcessProvide.KillProcess("IDCube");
                TryDeleteTempFile();

                //todo ������������°�
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

                //TODO ���ع�������°�
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
                Logger.Main.Error($"�����쳣:{ex.Message} {ex.StackTrace}");
                AppendText($"�����쳣:{ex.Message}");
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
            //todo �����ļ���ɺ��ѹ����
            AppendText("����������������...");

            //AppendText(!ZipHelper.UnZipPath(Path.Combine(_message.LocalUrl, WebProvide.PubLibPackageName),
            //    _pubLibExePath)
            //    ? "���������ʧ��,ԭ��:��ѹ����ʧ��..."
            //    : "��������³ɹ�,��ѹ�������...");

            //todo ��ѹ������exe
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
            AppendText("��������³ɹ�...");
            Do();
        }

        private void MainPackage_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //todo �����ļ���ɺ��ѹ����
            AppendText("����������������...");

            AppendText(!ZipHelper.UnZipPath(FrameworkConst.MainPackagePath,
                _message.LocalUrl)
                ? "���������ʧ��,ԭ��:��ѹ����ʧ��..."
                : "��������³ɹ�,��ѹ�������...");
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
                AppendText("����Ŀ�����ɹ�...");
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"�����������쳣:{ex.Message} {ex.StackTrace}");
                AppendText("����Ŀ������쳣...");
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