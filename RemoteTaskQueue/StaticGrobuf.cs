using GroBuf;
using GroBuf.DataMembersExtracters;

namespace RemoteQueue
{
    public static class StaticGrobuf
    {
        public static ISerializer GetSerializer()
        {
            return serializer;
        }

        private static readonly ISerializer serializer = new Serializer(new PropertiesExtractor(), null, GroBufOptions.MergeOnRead);
    }
}