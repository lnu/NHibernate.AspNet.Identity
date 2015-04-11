﻿using NHibernate.AspNet.Identity.DomainModel;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace NHibernate.AspNet.Identity
{
    /// <summary>
    ///     EntityType that represents one specific user claim
    /// </summary>
    public class IdentityUserClaim : IdentityUserClaim<string>
    {
    }

    /// <summary>
    ///     EntityType that represents one specific user claim
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class IdentityUserClaim<TKey>:EntityWithTypedId<TKey>
    {
        public virtual string ClaimType { get; set; }

        public virtual string ClaimValue { get; set; }

        public virtual TKey UserId { get; set; }
    }

    public class IdentityUserClaimMap : ClassMapping<IdentityUserClaim>
    {
        public IdentityUserClaimMap()
        {
            Table("AspNetUserClaims");
            Id(x => x.Id, m => m.Generator(new UUIDHexCombGeneratorDef("D")));
            Property(x => x.ClaimType);
            Property(x => x.ClaimValue);
            //ManyToOne(x => x.User, m => m.Column("UserId"));
        }

    }
          public class IdentityUserClaimMap<TKey> : ClassMapping<IdentityUserClaim<TKey>>
    {
        public IdentityUserClaimMap()
        {
            Table("AspNetUserClaims");
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
            Property(x => x.ClaimType);
            Property(x => x.ClaimValue);
            //ManyToOne(x => x.User, m => m.Column("UserId"));
        }
    }

}
