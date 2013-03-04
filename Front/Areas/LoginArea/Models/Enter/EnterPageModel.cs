using SKBKontur.Catalogue.Core.Web.Blocks.ActionButton;
using SKBKontur.Catalogue.Core.Web.PageModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.LoginArea.Models.Enter
{
    public class EnterPageModel : PageModelBase<EnterModelData>
    {
        public EnterPageModel(PageModelBaseParameters parameters)
            : base(parameters)
        {
            EnterPageText = parameters.Request.LanguageProvider.GetPageText<IEnterPageText>();
            Data = new EnterModelData();
        }

        public IEnterPageText EnterPageText { get; private set; }
        public override sealed EnterModelData Data { get; protected set; }
        public string UserNotFoundClass { get; set; }
        public string InvalidPasswordClass { get; set; }
        public string BackUrl { get; set; }
        public string AuthenticatorType { get; set; }
        public ActionButtonHtmlModel LoginButton { get; set; }
    }
}