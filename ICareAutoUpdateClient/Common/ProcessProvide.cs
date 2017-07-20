using System.Diagnostics;

namespace ICareAutoUpdateClient.Common
{
    public class ProcessProvide
    {
        public static void KillProcess(string destProcessName)
        {
            var allProc = Process.GetProcesses();
            foreach (var pro in allProc)
            {
                if (pro.ProcessName == destProcessName)
                    pro.Kill();
            }
           
        }

        public static void StartProcess(string fileName, string workingDirectory, string args = "")
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
            pro.Start();
        }
    }
}