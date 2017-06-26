using System.Diagnostics;
using System.Linq;
using ICareAutoUpdateClient.Log;

namespace ICareAutoUpdateClient.Common
{
    public class ProcessProvide
    {
        public static bool KillProcess(string destProcessName)
        {
            var allProc = Process.GetProcesses();
            foreach (var pro in allProc)
            {
                if (pro.ProcessName == destProcessName)
                    pro.Kill();
            }
            return true;
            //return (from pro in allProc
            //        where pro.ProcessName == destProcessName || pro.ProcessName == $"{destProcessName}.vshost"
            //        select pro.CloseMainWindow()).FirstOrDefault();
        }

        public static bool StartProcess(string fileName, string workingDirectory,string args="")
        {
            var pro = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    WorkingDirectory = workingDirectory,
                    Arguments = args
                }
            };
            return pro.Start();
        }
    }
}