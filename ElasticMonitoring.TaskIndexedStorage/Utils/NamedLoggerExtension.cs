using System;
using System.Linq;

using log4net;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils
{
    public static class NamedLoggerExtension
    {
        public static void LogInfoFormat(this ILog logger, string format, params object[] args)
        {
            logger.InfoFormat(FormatLoggerName(logger) + ": " + format, args);
        }

        public static void LogWarnFormat(this ILog logger, string format, params object[] args)
        {
            logger.WarnFormat(FormatLoggerName(logger) + ": " + format, args);
        }

        public static void LogErrorFormat(this ILog logger, Exception e, string format, params object[] args)
        {
            logger.Error(string.Format(FormatLoggerName(logger) + ": " + format, args), e);
        }

        private static string FormatLoggerName(ILog logger)
        {
            return logger.Logger.Name.Split('.').Last();
        }
    }
}