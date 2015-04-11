using NHibernate.AspNet.Identity.DomainModel;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace NHibernate.AspNet.Identity
{
    /// <summary>
    ///     EntityType that represents one specific user claim
    /// </summary>
    public class IdentityUserRole : IdentityUserRole<string>
    {
    }

    /// <summary>
    ///     EntityType that represents one specific user claim
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class IdentityUserRole<TKey> : EntityWithTypedId<TKey>
    {
        public virtual TKey UserId { get; set; }

        public virtual TKey RoleId { get; set; }

    }

    public class IdentityUserRoleMap : ClassMapping<IdentityUserRole>
    {
        public IdentityUserRoleMap()
        {
            Table("AspNetUserRoles");
            Id(x => x.Id, m => m.Generator(new UUIDHexCombGeneratorDef("D")));
        }
    }


    public class IdentityUserRoleMap<TKey> : ClassMapping<IdentityUserRole<TKey>>
    {
        public IdentityUserRoleMap()
        {
            Table("AspNetUserRoles");
            IGeneratorDef generator;
            var genericType = this.GetType().GenericTypeArguments[0].Name;
            switch (genericType)
            {
                case "Guid":
                    this.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
                    break;
                case "String":
                    this.Id(x => x.Id, m => m.Generator(new UUIDHexCombGeneratorDef("D")));
                    break;
            }
        }
    }

}
