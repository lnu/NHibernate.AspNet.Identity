using System;
using System.Linq;
using System.Security.Claims;
using System.Transactions;
using Microsoft.AspNet.Identity;
using NHibernate.AspNet.Identity.Tests.Models;
using NHibernate.Linq;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace NHibernate.AspNet.Identity.Tests
{
    [TestClass]
    public class RoleStoreTest
    {
        ISession _session;

        [TestInitialize]
        public void Initialize()
        {
            var factory = SessionFactoryProvider.Instance.SessionFactory;
            _session = factory.OpenSession();
            SessionFactoryProvider.Instance.BuildSchema();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _session.Close();
        }

        [TestMethod]
        public void WhenCeateRoleWithGuidIdAsync()
        {
            var roleManager = new RoleManager<IdentityRole<Guid>,Guid>(new RoleStore<IdentityRole<Guid>,Guid>(this._session));
            var roleName = "Admin";

            roleManager.Create(new IdentityRole<Guid>(roleName));

            this._session.Flush();
            this._session.Clear();

            var actual = _session.Query<IdentityRole<Guid>>().FirstOrDefault(x => x.Name == roleName);

            Assert.IsNotNull(actual);
            Assert.AreEqual(roleName, actual.Name);
        }

        [TestMethod]
        public void WhenCeateRoleWithDefaultIdAsync()
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(this._session));
            var roleName = "Admin";

            roleManager.Create(new IdentityRole(roleName));

            this._session.Flush();
            this._session.Clear();

            var actual = _session.Query<IdentityRole>().FirstOrDefault(x => x.Name == roleName);

            Assert.IsNotNull(actual);
            Assert.AreEqual(roleName, actual.Name);
        }
    }
}
