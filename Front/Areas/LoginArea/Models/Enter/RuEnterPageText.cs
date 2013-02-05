using SKBKontur.Catalogue.Core.Web.Languages;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.LoginArea.Models.Enter
{
    [Language("RU")]
    public class RuEnterPageText : IEnterPageText
    {
        public string Title { get { return "Вход в систему"; } }
        public string Email { get { return "Электронная почта"; } }
        public string Password { get { return "Пароль"; } }
        public string DoLogin { get { return "Войти"; } }
        public string Error { get { return "Ошибка!"; } }
        public string UserNotFound { get { return "Неверное имя пользователя"; } }
        public string InvalidPassword { get { return "Неверный пароль"; } }

        public string CertificateEnter { get { return "Войти по сертификату"; } }
        public string IsFirst { get { return "Впервые здесь?"; } }
        public string StartUse { get { return "Начните пользоваться бесплатно"; } }
        public string RememberPassword { get { return "Забыли пароль?"; } }
    }
}