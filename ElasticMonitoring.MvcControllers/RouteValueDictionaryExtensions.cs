using System.Web.Routing;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers
{
    internal static class RouteValueDictionaryExtensions
    {
        public static RouteValueDictionary AppendArray(this RouteValueDictionary routeValueDictionary, string[] array, string name)
        {
            if(array != null)
            {
                for(var i = 0; i < array.Length; i++)
                    routeValueDictionary.Add(string.Format("{0}[{1}]", name, i), array[i]);
            }
            return routeValueDictionary;
        }
    }
}