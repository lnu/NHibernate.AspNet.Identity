using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHibernate.AspNet.Identity.Helpers
{
    public static class MappingHelper
    {
        public static HbmMapping GetIdentityMappings(System.Type[] additionalTypes)
        {
            var baseEntityToIgnore = new[] { 
                typeof(IdentityRole<string,IdentityUserRole>),
                typeof(IdentityUser<string, IdentityUserLogin,IdentityUserRole, IdentityUserClaim>),
                typeof(IdentityUserClaim<string>),
                typeof(IdentityUserRole<string>),
            };

			var allEntities = new List<System.Type> { 
				typeof(IdentityUser), 
				typeof(IdentityRole), 
				typeof(IdentityUserRole), 
				typeof(IdentityUserLogin), 
				typeof(IdentityUserClaim),
			};
            allEntities.AddRange(additionalTypes);

            var mapper = new ConventionModelMapper();
            DefineBaseClass(mapper, baseEntityToIgnore.ToArray());
            mapper.IsComponent((type, declared) => typeof(NHibernate.AspNet.Identity.DomainModel.ValueObject).IsAssignableFrom(type));

            mapper.AddMapping<IdentityUserMap>();
            mapper.AddMapping<IdentityRoleMap>();
            mapper.AddMapping<IdentityUserRoleMap>();
            mapper.AddMapping<IdentityUserClaimMap>();

            return mapper.CompileMappingFor(allEntities);
        }

        /// <summary>
        /// Gets a mapping that can be used with NHibernate.
        /// </summary>
        /// <param name="additionalTypes">Additional Types that are to be added to the mapping, this is useful for adding your ApplicationUser class</param>
        /// <returns></returns>
        public static HbmMapping GetIdentityMappings<TKey>(System.Type[] additionalTypes)
        // where TRole : IdentityUserRole<TKey>
        {
            var baseEntityToIgnore = new[] { 
                typeof(NHibernate.AspNet.Identity.DomainModel.EntityWithTypedId<TKey>)
            };

            var allEntities = new List<System.Type> { 
                typeof(IdentityUser<TKey,IdentityUserLogin,IdentityUserRole<TKey>,IdentityUserClaim<TKey>>), 
                typeof(IdentityRole<TKey,IdentityUserRole<TKey>>), 
                typeof(IdentityUserLogin), 
                typeof(IdentityUserClaim<TKey>),
            };
            allEntities.AddRange(additionalTypes);

            var mapper = new ConventionModelMapper();
            DefineBaseClass(mapper, baseEntityToIgnore.ToArray());
            mapper.IsComponent((type, declared) => typeof(NHibernate.AspNet.Identity.DomainModel.ValueObject).IsAssignableFrom(type));

            mapper.AddMapping<IdentityUserMap<TKey, IdentityUserLogin, IdentityUserRole<TKey>, IdentityUserClaim<TKey>>>();
            mapper.AddMapping<IdentityRoleMap<TKey, IdentityUserRole<TKey>>>();
            mapper.AddMapping<IdentityUserRoleMap<TKey>>();
            mapper.AddMapping<IdentityUserClaimMap<TKey>>();

            return mapper.CompileMappingFor(allEntities);
        }

        private static void DefineBaseClass(ConventionModelMapper mapper, System.Type[] baseEntityToIgnore)
        {
            if (baseEntityToIgnore == null) return;
            mapper.IsEntity((type, declared) =>
                baseEntityToIgnore.Any(x => x.IsAssignableFrom(type)) &&
                !baseEntityToIgnore.Any(x => x == type) &&
                !type.IsInterface);
            mapper.IsRootEntity((type, declared) => baseEntityToIgnore.Any(x => x == type.BaseType));
        }
    }
}
