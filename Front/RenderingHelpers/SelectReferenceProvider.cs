using System.Linq;

using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.Core.Web.ReferencesHelpers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.RenderingHelpers
{
    public class SelectReferenceProvider : ISelectReferenceProvider
    {
        public SelectReferenceProvider(IReferencesProvider referencesProvider)
        {
            this.referencesProvider = referencesProvider;
        }

        public ISelectItemModel[] GetReference(SelectReferenceConfig referenceConfig)
        {
            return referencesProvider.GetReference(referenceConfig.ReferenceType, referenceConfig.ReferenceFormat)
                .Select(item => new SelectItemModel
                {
                    Text = item.Description,
                    Value = item.Code
                })
                .ToArray();
        }

        private readonly IReferencesProvider referencesProvider;
    }
}