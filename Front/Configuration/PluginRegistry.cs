using System;

using SKBKontur.Catalogue.RemoteTaskQueue.Common.Xml;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Configuration
{
    public static class PluginRegistry
    {
        public static Type xmlRegistry = typeof(XmlPlugin);
    }
}