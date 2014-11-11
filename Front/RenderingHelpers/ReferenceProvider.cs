using System;
using System.Linq;
using System.Reflection;

using SKBKontur.Catalogue.ReferencesCore;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.RenderingHelpers
{
    public class ReferencesProvider : IReferencesProvider
    {
        public ReferencesProvider(Func<Assembly, IReferenceCore> createReferenceCore)
        {
            referenceCore = createReferenceCore(GetType().Assembly);
        }

        public ReferenceItem[] GetReference(string referenceType, string format)
        {
            return referenceCore.GetReference(referenceType, format, null)
                .Select(item => new ReferenceItem
                {
                    Code = item.Code,
                    Description = item.Description,
                    ShortDescription = item.ShortDescription,
                    IsMajor = item.IsMajor
                })
                .ToArray();
        }

        public string GetDescription(string referenceType, string format, string code)
        {
            return referenceCore.GetDescription(referenceType, format, code, null);
        }

        public string GetShortDescription(string referenceType, string format, string code)
        {
            return referenceCore.GetShortDescription(referenceType, format, code, null);
        }

        public string GetCode(string referenceType, string referenceFormat, string value)
        {
            var codes = FindCodes(referenceType, referenceFormat, value);
            return codes.Length == 1 ? codes[0] : null;
        }

        private string[] FindCodes(string referenceType, string format, string description)
        {
            return referenceCore.FindCodes(referenceType, format, description).Select(info => info.Code).ToArray();
        }

        private readonly IReferenceCore referenceCore;
    }
}