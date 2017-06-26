using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;

namespace ICareAutoUpdateClient.Log
{
    public class Logger
    {
        static Logger()
        {
            string[] list = { "ICareAutoUpdateClient_Main"};
            var layout = new PatternLayout
            {
                ConversionPattern = "[%date{HH:mm:ss.fff}][%-5level]%message%newline"
            };
            layout.ActivateOptions();
            foreach (var s in list)
            {
                var repo = LogManager.CreateRepository(s);
                var rfAppender = new RollingFileAppender
                {
                    File = "Logs\\" + s + ".log",
                    ImmediateFlush = true,
                    AppendToFile = true,
                    RollingStyle = RollingFileAppender.RollingMode.Date,
                    DatePattern = "_yyyyMMdd",
                    StaticLogFileName = false,
                    PreserveLogFileNameExtension = true,
                    Layout = layout
                };
                var cAppender = new ConsoleAppender
                {
                    Target = ConsoleAppender.ConsoleOut,
                    Layout = layout
                };
                BasicConfigurator.Configure(repo, rfAppender, cAppender);
                rfAppender.ActivateOptions();
                cAppender.ActivateOptions();
            }

            Main = LogManager.GetLogger("ICareAutoUpdateClient_Main", "Main");
           
        }

        public static ILog Main { get; }
    
    }
}