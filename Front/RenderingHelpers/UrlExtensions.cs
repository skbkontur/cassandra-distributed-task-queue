namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.RenderingHelpers
{
    public static class UrlExtensions
    {
        public static string UnescapeBraces(this string url)
        {
            return url.Replace("%7B", "{").Replace("%7D", "}");
        }
    }
}