
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using MelonLoader;
using MelonLoader.Logging;
using SR2E;
using SR2E.Managers;
using SR2E.Menus;
using Color = UnityEngine.Color;

namespace NewSR2MP
{
    public class SRMP
    {
        private class Logger
        {
            internal MelonLogger.Instance melonLogger;
            internal StringBuilder fileLogger;
        }

        private static Logger logger;
        
        internal static string logPath;
        
        static SRMP()
        {
            logger = new Logger()
            {
                melonLogger = new MelonLogger.Instance("New SR2MP"),
                fileLogger = new StringBuilder()
            };
            
        }

        public static void Log(string message)
        {
            Log(message, 100);
        }
        public static void Log(string message, int sr2eSize)
        {
            SR2ELogManager.SendMessage($"<size={sr2eSize}%>{message}</size>");
            logger.melonLogger.Msg(message);
            File.AppendAllText(logPath, $"\n[INFO] {message}");
        }
        
        public static void Error(string message)
        {
            SR2ELogManager.SendError(message);
            logger.melonLogger.Error(message);
            File.AppendAllText(logPath, $"\n[ERROR] {message}");
        }
        
        public static void Warn(string message)
        {
            SR2ELogManager.SendWarning(message);
            logger.melonLogger.Warning(message);
            File.AppendAllText(logPath, $"\n[WARNING] {message}");
        }

        public static void Debug(string message)
        {
            File.AppendAllText(logPath, $"\n[DEBUG] {message}");
        }
        public static void DebugWarn(string message)
        {
            File.AppendAllText(logPath, $"\n[DEBUG-WARN] {message}");
        }
    }
}
