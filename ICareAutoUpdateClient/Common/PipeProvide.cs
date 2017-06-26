using System.IO.Pipes;
using System.Security.Principal;

namespace ICareAutoUpdateClient.Common
{
    public class PipeProvide
    {
        internal static readonly string ServerKey = "I am the one true server!";
        private static readonly NamedPipeClientStream PipeClient = new NamedPipeClientStream(
            ".",
            "AutoUpdatePipe",
            PipeDirection.InOut,
            PipeOptions.None,
            TokenImpersonationLevel.Impersonation);

        public static string Communication(string writeString)
        {
            PipeClient.Connect(500);
            if (!PipeClient.IsConnected) return null;
            var ss = new StreamString(PipeClient);
            if (ss.ReadString() != ServerKey) return null;
            ss.WriteString(writeString);
            return ss.ReadString();
        }
    }
}
