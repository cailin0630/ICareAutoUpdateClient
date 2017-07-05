using System;
using System.IO;

namespace ICareAutoUpdateClient.Common
{
    public class FrameworkConst
    {

        public static string TempFolder= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
        public const string MainPackageName = "MainPackage.7z";

        public const string PubLibPackageName = "PubLibPackage.exe";

        public static string MainPackagePath = 
            Path.Combine(TempFolder, MainPackageName);

        public static string PubLibPackagePath =
            Path.Combine(TempFolder, PubLibPackageName);
    }
}