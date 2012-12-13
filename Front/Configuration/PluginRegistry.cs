using System;

using SKBKontur.Catalogue.RemoteTaskQueue.Common.Xml;
using SKBKontur.Catalogue.RemoteTaskQueue.Storage.ColumnFamilyRegistration;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Configuration
{
    public static class PluginRegistry
    {
        public static Type storageRegistry = typeof(StoragePlugin);
        public static Type xmlRegistry = typeof(XmlPlugin);
    }
}