using log4net;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation.Utils
{
    internal static class NamedLoggerExtension
    {
        public static void LogInfoFormat(this ILog logger, string format, params object[] args)
        {
            logger.InfoFormat(logger.Logger.Name + ": " + format, args);
        }

        public static void LogWarnFormat(this ILog logger, string format, params object[] args)
        {
            logger.WarnFormat(logger.Logger.Name + ": " + format, args);
        }
    }
}