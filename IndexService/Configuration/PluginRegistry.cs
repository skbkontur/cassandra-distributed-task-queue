using System;

using SKBKontur.Catalogue.RemoteTaskQueue.Common.Xml;

namespace SKBKontur.Catalogue.RemoteTaskQueue.IndexService.Configuration
{
    public static class PluginRegistry
    {
        public static Type xmlRegistry = typeof(XmlPlugin);
    }
}