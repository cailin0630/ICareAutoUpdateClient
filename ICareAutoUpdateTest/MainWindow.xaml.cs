using System;
using System.Diagnostics;
using System.Windows;

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

            var args =
                $"{@"D:\Code\SVN\IV007_IDcube\branches\v1.0.0.0\Source\Build\Debug"} {@"F:\AutoUpdate.rar"} { @"F:\公共库2.2.1.0623.exe"}";

            var pro = new Process
            {
                StartInfo =
                {
                    FileName = "ICareAutoUpdate\\ICareAutoUpdateClient.exe",

                    Arguments = args
                }
            };

            pro.Start();
        }
    }
}