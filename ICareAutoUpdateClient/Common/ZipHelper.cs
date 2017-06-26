using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using ICareAutoUpdateClient.Log;

namespace ICareAutoUpdateClient.Common
{
    public class ZipHelper
    {
        #region 7z压缩
        /// 压缩文件
        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="fileName">被压缩文件的文件名</param>
        /// <param name="zipFileName">压缩文件名</param>
        /// <returns>返回值为true:成功  false:失败</returns>
        public static bool WinrarZipFile(ArrayList fileName, string zipFileName)
        {
            try
            {
                string zipPara;  //压缩文件的命令行参数
                for (int i = 0; i < fileName.Count; i++)    //获取压缩文件并将其添加到命令行参数中
                {
                    zipPara = " a -r \"" + zipFileName + "\" \"" + fileName[i].ToString().Trim() + "\"";
                    if (!SeventZPrcess(zipPara))
                        return false;
                }
                //启动进程并调用Winrar
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// 压缩文件
        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="fileName">被压缩文件的文件名</param>
        /// <param name="zipFileName">压缩文件名</param>
        /// <returns>返回值为true:成功  false:失败</returns>
        public static bool ZipFileMore(ArrayList fileName, string zipFileName)
        {
            try
            {
                bool result = true;
                for (int i = 0; i <= (fileName.Count - 1) / 10; i++)    //获取压缩文件并将其添加到命令行参数中
                {
                    string zipPara;  //压缩文件的命令行参数
                    for (int j = i * 10; j < i * 10 + 10; j++)
                    {
                        if (j < fileName.Count)
                        {
                            zipPara = " a \"" + zipFileName + "\"    " + " \"" + fileName[j].ToString().Trim() + "\"";
                            if (!SeventZPrcess(zipPara))
                                return false;
                        }
                        else
                        {
                            break;
                        }
                    }

                }
                return true; ;
                //return false;
            }
            catch
            {
                return false;
            }
        }


        /// 解压缩文件(解压后带目录)
        /// <summary>
        /// 解压缩文件（解压后带目录）
        /// </summary>
        /// <param name="zipFileName">压缩文件名</param>
        /// <param name="unzipPath">解压缩后文件的路径</param>
        /// <returns>返回值true:成功  false:失败</returns>
        public static bool UnZipPath(string zipFileName, string unZipPath)
        {
            try
            {
                Logger.Main.Info($"zipFileName:{zipFileName} unZipPath:{unZipPath}");
                var arguments = " x -y \"" + zipFileName + "\" -o\"" + unZipPath + "\"";
                return SeventZPrcess(arguments);
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"解压失败:{ex.Message} {ex.StackTrace}");
                return false;
            }
        }

        public static bool SeventZPrcess(string arguments)
        {
            var winrarPro = new Process
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Minimized,
                    FileName = GetProcessPath(),
                    CreateNoWindow = false,
                    Arguments = arguments
                }
            };
            Logger.Main.Info(GetProcessPath());
            winrarPro.Start();
            winrarPro.WaitForExit();
            if (!winrarPro.HasExited) return true;
            var iExitCode = winrarPro.ExitCode;
            winrarPro.Close();
            return iExitCode == 0 || iExitCode == 1;
        }

        private static string GetProcessPath()
        {
            var in64Bit = (IntPtr.Size == 8);
            return in64Bit ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"7ZIP\64bit\7z.exe") :
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"7ZIP\32bit\7z.exe");
        }
        #endregion
    }
}
