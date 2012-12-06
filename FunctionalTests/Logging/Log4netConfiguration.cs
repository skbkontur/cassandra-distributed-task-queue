using System;

using log4net.Config;

namespace FunctionalTests.Logging
{
    public static class Log4NetConfiguration
    {
        public static void InitializeOnce()
        {
            if(!initialized)
            {
                Type type = typeof(Log4NetConfiguration);
                XmlConfigurator.Configure(type.Assembly.GetManifestResourceStream(type, "log4net.config"));
                initialized = true;
            }
        }

        private static bool initialized;
    }
}