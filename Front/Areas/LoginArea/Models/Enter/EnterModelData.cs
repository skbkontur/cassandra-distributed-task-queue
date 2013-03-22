using SKBKontur.Catalogue.Core.Web.PageModels;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.LoginArea.Models.Enter
{
    public class EnterModelData : ModelData
    {
        public string ErrorMessage { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

    }
}