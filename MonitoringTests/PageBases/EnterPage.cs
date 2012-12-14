using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.TestBases;
using SKBKontur.Catalogue.WebTestCore.SystemControls;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases
{
    public class EnterPage : CommonPageBase
    {
        //страница ввода логина-пароля
        public EnterPage()
        {
            LoginInput = new TextInput("login");
            PasswordInput = new TextInput("password");
            LoginButton = new ButtonInput("loginSubmit");
        }

        public override void BrowseWaitVisible()
        {
            LoginInput.WaitVisible();
            PasswordInput.WaitVisible();
            LoginButton.WaitVisible();
        }

        public TasksListPage Login(string login, string password)
        {
            LoginInput.SetValueAndWait(login);
            PasswordInput.SetValueAndWait(password);
            LoginButton.Click();
            return GoTo<TasksListPage>();
        }

        private TextInput LoginInput { get; set; }
        private ButtonInput LoginButton { get; set; }
        private TextInput PasswordInput { get; set; }
    }
}