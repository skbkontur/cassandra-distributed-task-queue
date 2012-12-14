using System;
using System.Collections.Generic;
using System.Reflection;

using GroboContainer.Core;
using GroboContainer.Impl;

using NUnit.Framework;

using SKBKontur.Catalogue.AccessControl;
using SKBKontur.Catalogue.AccessControl.AccessRules;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests.TestBases;
using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.Catalogue.ServiceLib.Settings;
using SKBKontur.Catalogue.TestCore;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringTests
{
    [Ignore]
    public class ZzzRestoreStorage : CoreTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            IEnumerable<Assembly> assemblies = AssembliesLoader.Load();
            container = new Container(new ContainerConfiguration(assemblies));
            container.Configurator.ForAbstraction<IApplicationSettings>().UseInstances(ApplicationSettings.LoadDefault("functionalTestsSettings"));
            container.ConfigureCassandra();
            container.ClearAllBeforeTest();
        }

        [Test, Ignore]
        public void JustCleanAll()
        {
        }

        [Test, Ignore]
        public void CreateUser()
        {
            var userRepository = container.Get<IUserRepository>();
            var passwordService = container.Get<IPasswordService>();
            var accessControlService = container.Get<IAccessControlService>();
            userRepository.ReleaseLogin("user");
            var userId = Guid.NewGuid().ToString();
            userRepository.SaveUser(new User
                {
                    Id = userId,
                    PasswordHash = passwordService.GetPasswordHash("psw"),
                    Login = "user",
                    UserName = "user"
                });
            accessControlService.AddAccess(userId, new ResourseGroupAccessRule
                {
                    ResourseGroupName = ResourseGroups.AdminResourse
                });
        }

        private Container container;
    }
}