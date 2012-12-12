using SKBKontur.Catalogue.Core.Web.PageTexts;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.LoginArea.Models.Enter
{
    public interface IEnterPageText : IPageText
    {
        string Title { get; }
        string Email { get; }
        string Password { get; }
        string DoLogin { get; }
        string Error { get; }
        string UserNotFound { get; }
        string InvalidPassword { get; }

        string CertificateEnter { get; }
        string IsFirst { get; }
        string StartUse { get; }
        string RememberPassword { get; }
    }
}