using System.Web.Mvc;

using SKBKontur.Catalogue.AccessControl;
using SKBKontur.Catalogue.Core.Web.Blocks.Button;
using SKBKontur.Catalogue.Core.Web.Controllers;
using SKBKontur.Catalogue.Core.Web.CookiesManagement;
using SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.LoginArea.Models.Enter;
using SKBKontur.Catalogue.RemoteTaskQueue.Front.RenderingHelpers;
using SKBKontur.Catalogue.ServiceLib.Settings;

using ControllerBase = SKBKontur.Catalogue.Core.Web.Controllers.ControllerBase;

namespace SKBKontur.Catalogue.RemoteTaskQueue.Front.Areas.LoginArea.Controllers
{
    public class EnterController : ControllerBase
    {
        public EnterController(ControllerBaseParameters parameters, IAuthenticatorFactory authenticatorFactory, IEmptyHtmlModelsCreator<EnterModelData> htmlModelsCreator)
            : base(parameters)
        {
            this.authenticatorFactory = authenticatorFactory;
            this.htmlModelsCreator = htmlModelsCreator;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Run(string backUrl, string message, string authenticatorType)
        {
            if(!string.IsNullOrEmpty(Cookies.KonturPortalTokenString.Value))
            {
                var userSession = authenticatorFactory.GetAuthenticator(authenticatorType)
                    .AuthenticateByPortalToken(Cookies.KonturPortalTokenString.Value, ApplicationSettings.GetKonturPortalCrypt());
                if (userSession != null)
                {
                    Cookies.SessionString = new Cookie<string>(UserSession.ToSessionString(userSession), null);
                    return Redirect(backUrl ?? successUrl);
                }
            }
            if(!string.IsNullOrEmpty(Cookies.SessionString.Value) && !string.IsNullOrEmpty(Cookies.KonturPortalTokenString.Value))
                return Redirect(backUrl ?? successUrl);

            var enterPageModel = new EnterPageModel(PageModelBaseParameters);
            enterPageModel.Data.Login = Cookies.Login.Value;
            if(message == userNotFound)
            {
                enterPageModel.UserNotFoundClass = "login_field__error login_field__userNotFound";
                enterPageModel.Data.ErrorMessage = enterPageModel.EnterPageText.UserNotFound;
            }
            if(message == invalidPassword)
            {
                enterPageModel.InvalidPasswordClass = "login_field__error login_field__invalidPassword";
                enterPageModel.Data.ErrorMessage = enterPageModel.EnterPageText.InvalidPassword;
            }
            enterPageModel.BackUrl = backUrl;
            enterPageModel.AuthenticatorType = authenticatorType;
            enterPageModel.LoginButton = htmlModelsCreator.ButtonFor(new ButtonOptions
                {
                    Id = "loginSubmit",
                    Style = ButtonStyle.Enter,
                    Title = enterPageModel.EnterPageText.DoLogin
                });
            return View("EnterView", enterPageModel);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Login(string backUrl, string authenticatorType, string login, string password)
        {
            Cookies.Login = new Cookie<string>(login, null);
            try
            {
                var userSession = authenticatorFactory.GetAuthenticator(authenticatorType).Authenticate(login, password);
                Cookies.SetSession(userSession);
            }
            catch(UserNotFoundException)
            {
                return RedirectToAction(string.Empty, new {backUrl, message = userNotFound, authenticatorType});
            }
            catch(InvalidPasswordException)
            {
                return RedirectToAction(string.Empty, new {backUrl, message = invalidPassword, authenticatorType});
            }
            return Redirect(string.IsNullOrEmpty(backUrl) ? successUrl : backUrl);
        }

        private readonly IAuthenticatorFactory authenticatorFactory;
        private readonly IEmptyHtmlModelsCreator<EnterModelData> htmlModelsCreator;
        private const string userNotFound = "UserNotFound";
        private const string invalidPassword = "InvalidPassword";
        private const string successUrl = "/AdminTools/RemoteTaskQueue";
    }
}