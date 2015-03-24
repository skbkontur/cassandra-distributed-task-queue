namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding
{
    public interface IMetaLoaderFactory
    {
        IMetasLoader CreateLoader(string name);
    }
}