using ICareAutoUpdateClient.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace ICareAutoUpdateTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var exeName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ICareAutoUpdateClient.exe");
            var exeDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ICareAutoUpdateClient");

            var message = new Message
            {
                UpdateUrl = @"F:\AutoUpdate.rar",
                LocalUrl = @"D:\Code\SVN\IV007_IDcube\branches\v1.0.0.0\Source\Build\Debug"
            };
            var jsonStr = JsonConvert.SerializeObject(message);
            //var args = @"D:\Code\SVN\IV007_IDcube\branches\v1.0.0.0\Source\Build\Debug" + " " + @"F:\AutoUpdate.rar" +
            //           " " + @"F:\publibUpdate.rar";
            var args = "11";
            //ProcessProvide.StartProcess(exeName, exeDir, args);

            var pro = new Process
            {
                StartInfo =
                {
                    FileName = exeName,
                    WorkingDirectory = exeDir,
                    Arguments = args
                }
            };
            
            pro.Start();
            pro.WaitForExit();

        }

        private void Pro_Exited(object sender, EventArgs e)
        {
            MessageBox.Show("Pro_Exited ");
        }
    }
}