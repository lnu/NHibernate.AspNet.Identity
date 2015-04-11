using System;
using System.IO;
using System.Linq;
using NHibernate.AspNet.Identity.DomainModel;
using NHibernate.AspNet.Identity.Tests.Models;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using NHibernate.AspNet.Identity.Helpers;

namespace NHibernate.AspNet.Identity.Tests
{
    public sealed class GuidSessionFactoryProvider
    {
        private static volatile GuidSessionFactoryProvider _instance;
        private static object _syncRoot = new Object();

        private Configuration _configuration;

        public ISessionFactory SessionFactory { get; private set; }
        public string Name { get; private set; }

        /// <summary>
        /// constructor configures a SessionFactory based on the configuration passed in
        /// </summary>
        private GuidSessionFactoryProvider()
        {
            Name = "NHibernate.AspNet.Identity";

            //var baseEntityToIgnore = new[] { 
            //    typeof(SharpArch.Domain.DomainModel.Entity), 
            //    typeof(EntityWithTypedId<int>), 
            //    typeof(EntityWithTypedId<string>), 
            //    typeof(EntityWithTypedId<Guid>), 
            //};

            //var allEntities = new[] { 
            //    //typeof(IdentityUser), 
            //    //typeof(ApplicationUser), 
            //    //typeof(IdentityRole), 
            //    typeof(IdentityRole<Guid>), 
            //    //typeof(IdentityUserLogin), 
            //    //typeof(IdentityUserClaim), 
            //    //typeof(Foo), 
            //};

            //var mapper = new ConventionModelMapper();
            //DefineBaseClass(mapper, baseEntityToIgnore);
            //mapper.IsComponent((type, declared) => typeof(ValueObject).IsAssignableFrom(type));

            ////mapper.AddMapping<IdentityUserMap>();
            //mapper.AddMapping<IdentityRoleMap>();
            //mapper.AddMapping<IdentityRoleMap<Guid>>();
            ////mapper.AddMapping<IdentityUserClaimMap>();

            //var mapping = mapper.CompileMappingForEach(allEntities);

            var internalTypes = new[] {
                typeof(CustomUser), 
                typeof(CustomRole), 
                typeof(CustomUserRole), 
                typeof(CustomUserLogin), 
                typeof(CustomUserClaim), 
            };

            var mapping = MappingHelper.GetIdentityMappings<Guid>(internalTypes);
            Console.WriteLine(mapping.AsString());
            _configuration = new Configuration();
            _configuration.Configure("sqlite-nhibernate-config.xml");
            //foreach (var map in mapping)
            //{
            //    Console.WriteLine(map.AsString());
            //    _configuration.AddDeserializedMapping(map, null);
            //}
            _configuration.AddDeserializedMapping(mapping, null);


            //log4net.Config.XmlConfigurator.Configure();
            SessionFactory = _configuration.BuildSessionFactory();
        }

        public static GuidSessionFactoryProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new GuidSessionFactoryProvider();
                    }
                }
                return _instance;
            }
        }

        public void BuildSchema()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"schema.sql");

            // this NHibernate tool takes a configuration (with mapping info in)
            // and exports a database schema from it
            new SchemaExport(_configuration)
                .SetOutputFile(path)
                .Create(true, true /* DROP AND CREATE SCHEMA */);
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
