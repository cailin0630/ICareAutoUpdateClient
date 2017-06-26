using Microsoft.Win32;
using System;

namespace ICareAutoUpdateClient.Common
{
    public class PublibProvide
    {
        public static RegistryKey GetPublibRegKey()
        {
            var in64Bit = IntPtr.Size == 8;
            var pubKeyName = in64Bit ? @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{A1AC64C5-AAF3-458A-ADA6-7F2256F8BC3F}_is1"
                : @"Software\Microsoft\Windows\CurrentVersion\Uninstall\{A1AC64C5-AAF3-458A-ADA6-7F2256F8BC3F}_is1";
            return Registry.LocalMachine.OpenSubKey(pubKeyName);
        }

        public static Version GetPublibVersion()
        {
           return GetPublibRegKey().GetValue("DisplayVersion") as Version;
        }

        public static string GetPublibPath()
        {
            return GetPublibRegKey().GetValue("InstallLocation").ToString();
        }
    }
}