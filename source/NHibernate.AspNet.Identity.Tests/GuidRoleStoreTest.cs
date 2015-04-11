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
    public class GuidRoleStoreTest
    {
        ISession _session;

        [TestInitialize]
        public void Initialize()
        {
            var factory = GuidSessionFactoryProvider.Instance.SessionFactory;
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
            var roleManager = new RoleManager<CustomRole, Guid>(new RoleStore<CustomRole,Guid,CustomUserRole>(this._session));
            var roleName = "Admin";

            roleManager.Create(new CustomRole(roleName));

            this._session.Flush();
            this._session.Clear();

            var actual = _session.Query<CustomRole>().FirstOrDefault(x => x.Name == roleName);

            Assert.IsNotNull(actual);
            Assert.AreEqual(roleName, actual.Name);
        }
    }

    public class CustomUserRole : IdentityUserRole<Guid> { }
    public class CustomUserClaim : IdentityUserClaim<Guid> { }
    public class CustomUserLogin : IdentityUserLogin { }
    public class CustomRole : IdentityRole<Guid, CustomUserRole>
    {
        public CustomRole() { }
        public CustomRole(string name) { Name = name; }
    }
    public class CustomUser : IdentityUser<Guid, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
    }

    //public class CustomUserStore : UserStore<ApplicationUser, CustomRole, int,
    //    CustomUserLogin, CustomUserRole, CustomUserClaim>
    //{
    //    public CustomUserStore(ApplicationDbContext context)
    //        : base(context)
    //    {
    //    }
    //}

    //public class CustomRoleStore : RoleStore<CustomRole, int, CustomUserRole>
    //{
    //    public CustomRoleStore(ApplicationDbContext context)
    //        : base(context)
    //    {
    //    }
    //} 
}
