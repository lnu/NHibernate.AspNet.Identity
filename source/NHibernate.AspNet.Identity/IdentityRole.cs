using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using NHibernate.AspNet.Identity.DomainModel;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;

namespace NHibernate.AspNet.Identity
{
    /// <summary>
    ///     Represents a Role entity
    /// </summary>
    public class IdentityRole : IdentityRole<string, IdentityUserRole>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public IdentityRole()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="roleName"></param>
        public IdentityRole(string roleName)
            : this()
        {
            Name = roleName;
        }
    }

    public class IdentityRole<TKey, TUserRole> : EntityWithTypedId<TKey>, IRole<TKey>
        where TUserRole : IdentityUserRole<TKey>
    {
        public virtual string Name { get; set; }

        public virtual ICollection<TUserRole> Users { get; protected set; }

        public IdentityRole()
        {
            this.Users = new List<TUserRole>();
        }

        public IdentityRole(string roleName)
            : this()
        {
            this.Name = roleName;
        }
    }

    public class IdentityRoleMap<TKey, TUserRole> : ClassMapping<IdentityRole<TKey, TUserRole>>
        where TUserRole : IdentityUserRole<TKey>
    {
        public IdentityRoleMap()
        {
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
            this.Table("AspNetRoles");
            this.Property(x => x.Name, map =>
            {
                map.Length(256);
                map.NotNullable(true);
                map.Unique(true);
            });
            this.Bag(x => x.Users, map =>
            {
                //map.Table("AspNetUserRoles");
                map.Key(k => k.Column("RoleId"));
                map.Cascade(Cascade.DeleteOrphans);
                map.Inverse(true);
            }, rel => rel.OneToMany());
        }
    }

    public class IdentityRoleMap : ClassMapping<IdentityRole>
    {
        public IdentityRoleMap()
        {

            this.Table("AspNetRoles");
            this.Id(x => x.Id, m => m.Generator(new UUIDHexCombGeneratorDef("D")));
            this.Property(x => x.Name, map =>
            {
                map.Length(255);
                map.NotNullable(true);
                map.Unique(true);
            });
            this.Bag(x => x.Users, map =>
            {
                map.Table("AspNetUserRoles");
                map.Cascade(Cascade.None);
                map.Key(k => k.Column("RoleId"));
            }, rel => rel.ManyToMany(p => p.Column("UserId")));
        }
    }
}