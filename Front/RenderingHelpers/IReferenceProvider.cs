namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.RenderingHelpers
{
    public interface IReferencesProvider
    {
        ReferenceItem[] GetReference(string referenceType, string format);
        string GetDescription(string referenceType, string format, string code);
        string GetShortDescription(string referenceType, string format, string code);
        string GetCode(string referenceType, string referenceFormat, string value);

    }
}