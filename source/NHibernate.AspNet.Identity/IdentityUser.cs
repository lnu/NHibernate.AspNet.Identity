using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using NHibernate.AspNet.Identity.DomainModel;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace NHibernate.AspNet.Identity
{
    public class IdentityUser : IdentityUser<string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>, IUser
    {
        /// <summary>
        ///     Constructor which creates a new Guid for the Id
        /// </summary>
        public IdentityUser()
        {
            //Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     Constructor that takes a userName
        /// </summary>
        /// <param name="userName"></param>
        public IdentityUser(string userName)
            : this()
        {
            UserName = userName;
        }
    }

    public class IdentityUser<TKey, TLogin, TRole, TClaim> : EntityWithTypedId<TKey>, IUser<TKey>
        where TLogin : IdentityUserLogin
        where TRole : IdentityUserRole<TKey>
        where TClaim : IdentityUserClaim<TKey>
    {
        public virtual int AccessFailedCount { get; set; }

        public virtual string Email { get; set; }

        public virtual bool EmailConfirmed { get; set; }

        public virtual bool LockoutEnabled { get; set; }

        public virtual DateTime? LockoutEndDateUtc { get; set; }

        public virtual string PasswordHash { get; set; }

        public virtual string PhoneNumber { get; set; }

        public virtual bool PhoneNumberConfirmed { get; set; }

        public virtual bool TwoFactorEnabled { get; set; }

        public virtual string UserName { get; set; }

        public virtual string SecurityStamp { get; set; }

        public virtual ICollection<TRole> Roles { get; protected set; }

        public virtual ICollection<TClaim> Claims { get; protected set; }

        public virtual ICollection<IdentityUserLogin> Logins { get; protected set; }

        public IdentityUser()
        {
            this.Roles = new List<TRole>();
            this.Claims = new List<TClaim>();
            this.Logins = new List<IdentityUserLogin>();
        }

        public IdentityUser(string userName)
            : this()
        {
            this.UserName = userName;
        }
    }


    public class IdentityUserMap<TKey, TLogin, TRole, TClaim> : ClassMapping<IdentityUser<TKey, TLogin, TRole, TClaim>>
        where TLogin : IdentityUserLogin
        where TClaim : IdentityUserClaim<TKey>
        where TRole : IdentityUserRole<TKey>
    {
        public IdentityUserMap()
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
            this.Table("AspNetUsers");

            this.Property(x => x.AccessFailedCount);

            this.Property(x => x.Email);

            this.Property(x => x.EmailConfirmed);

            this.Property(x => x.LockoutEnabled);

            this.Property(x => x.LockoutEndDateUtc);

            this.Property(x => x.PasswordHash);

            this.Property(x => x.PhoneNumber);

            this.Property(x => x.PhoneNumberConfirmed);

            this.Property(x => x.TwoFactorEnabled);

            this.Property(x => x.UserName, map =>
            {
                map.Length(255);
                map.NotNullable(true);
                map.Unique(true);
            });

            this.Property(x => x.SecurityStamp);

            this.Bag(x => x.Claims, map =>
            {
                map.Key(k =>
                {
                    k.Column("UserId");
                    k.Update(false); // to prevent extra update afer insert
                });
                map.Cascade(Cascade.All | Cascade.DeleteOrphans);
            }, rel =>
            {
                rel.OneToMany();
            });

            this.Set(x => x.Logins, cam =>
            {
                cam.Table("AspNetUserLogins");
                cam.Key(km => km.Column("UserId"));
                cam.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
                     map =>
                     {
                         map.Component(comp =>
                         {
                             comp.Property(p => p.LoginProvider);
                             comp.Property(p => p.ProviderKey);
                         });
                     });

            this.Bag(x => x.Roles, map =>
            {
                map.Table("AspNetUserRoles");
                map.Key(k => k.Column("UserId"));
            }, rel => rel.ManyToMany(p => p.Column("RoleId")));
        }
    }

    public class IdentityUserMap : ClassMapping<IdentityUser>
    {
        public IdentityUserMap()
        {
            this.Table("AspNetUsers");
            this.Id(x => x.Id, m => m.Generator(new UUIDHexCombGeneratorDef("D")));

            this.Property(x => x.AccessFailedCount);

            this.Property(x => x.Email);

            this.Property(x => x.EmailConfirmed);

            this.Property(x => x.LockoutEnabled);

            this.Property(x => x.LockoutEndDateUtc);

            this.Property(x => x.PasswordHash);

            this.Property(x => x.PhoneNumber);

            this.Property(x => x.PhoneNumberConfirmed);

            this.Property(x => x.TwoFactorEnabled);

            this.Property(x => x.UserName, map =>
            {
                map.Length(255);
                map.NotNullable(true);
                map.Unique(true);
            });

            this.Property(x => x.SecurityStamp);

            this.Bag(x => x.Claims, map =>
            {
                map.Key(k =>
                {
                    k.Column("UserId");
                    k.Update(false); // to prevent extra update afer insert
                });
                map.Cascade(Cascade.All | Cascade.DeleteOrphans);
            }, rel =>
            {
                rel.OneToMany();
            });

            this.Set(x => x.Logins, cam =>
            {
                cam.Table("AspNetUserLogins");
                cam.Key(km => km.Column("UserId"));
                cam.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
                     map =>
                     {
                         map.Component(comp =>
                         {
                             comp.Property(p => p.LoginProvider);
                             comp.Property(p => p.ProviderKey);
                         });
                     });

            this.Bag(x => x.Roles, map =>
            {
                //map.Table("AspNetUserRoles");
                map.Key(k => k.Column("UserId"));
                map.Cascade(Cascade.DeleteOrphans);
                map.Inverse(true);
            }, rel => rel.OneToMany());
        }
    }
}