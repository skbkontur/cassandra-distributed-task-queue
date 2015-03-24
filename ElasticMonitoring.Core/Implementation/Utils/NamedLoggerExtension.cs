using log4net;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.Utils
{
    internal static class NamedLoggerExtension
    {
        public static void LogInfoFormat(this ILog logger, string name, string format, params object[] args)
        {
            logger.InfoFormat(name + ": " + format, args);
        }

        public static void LogWarnFormat(this ILog logger, string name, string format, params object[] args)
        {
            logger.WarnFormat(name + ": " + format, args);
        }
    }
}