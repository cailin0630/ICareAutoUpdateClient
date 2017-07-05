using System;
using System.IO;

namespace ICareAutoUpdateClient.Common
{
    public class FrameworkConst
    {
        public const string MainPackageName = "MainPackage.7z";

        public const string PubLibPackageName = "PubLibPackage.exe";

        public readonly string MainPackagePath = 
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MainPackageName);

        public readonly string PubLibPackagePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PubLibPackageName);
    }
}