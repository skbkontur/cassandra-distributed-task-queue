using SKBKontur.Catalogue.WebTestCore.SystemControls;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.PageBases
{
    public class DefaultPage : CommonPageBase
    {
        //дефолтная страница - выбор способа входа, только для тестов
        public DefaultPage()
        {
            loginLink = Link.ByLinkText("Login");
        }

        public override void BrowseWaitVisible()
        {
            loginLink.WaitVisible();
        }

        public EnterPage Enter()
        {
            loginLink.Click();
            return GoTo<EnterPage>();
        }

        private readonly Link loginLink;
    }
}