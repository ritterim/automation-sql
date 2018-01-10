using System;
using System.Diagnostics;
using RimDev.Automation.Sql;
using Xunit;

namespace RimDev.Automation.Core
{
    public class LocalDbTests
    {
        [Fact]
        public void Can_CreateLocalDB_With_V11()
        {
            using (var db = new LocalDb(version: LocalDb.Versions.V11))
            {
                Assert.NotNull(db);
            }
        }

        [Fact]
        public void Can_CreateLocalDB_With_V12()
        {
            using (var db = new LocalDb(version: LocalDb.Versions.V12))
            {
                Assert.NotNull(db);
            }
        }

        [Fact]
        public void Can_CreateLocalDB_With_V13()
        {
            using (var db = new LocalDb(version: LocalDb.Versions.V13))
            {
                Assert.NotNull(db);
            }
        }

        [Fact]
        public void LocalDB_Versions_From_Registry()
        {
            var result = LocalDb.Versions.InstalledVersions;
            Assert.Equal(LocalDb.Versions.All, result, StringComparer.CurrentCultureIgnoreCase);
        }

        [Fact]
        public void LocalDB_has_localdb_in_name()
        {
            using (var db = new LocalDb())
            {
                Assert.Contains("localdb", db.DatabaseName);
            }
        }

        [Fact]
        public void LocalDb_defaults_to_V11()
        {
            using (var db = new LocalDb())
            {
                Assert.Equal(LocalDb.Versions.V11, db.Version);
            }
        }

        [Fact]
        public void LocalDb_set_databaseSuffixGenerator()
        {
            var guid = Guid.NewGuid().ToString("N");
            using (var db = new LocalDb(databaseSuffixGenerator: () => guid ))
            {
                Assert.Contains(guid, db.DatabaseName);
                Debug.WriteLine(db.DatabaseName);
            }
        }

        [Fact]
        public void LocalDb_allows_configuration_of_connection_timeout()
        {
            const int Timeout = 1000;

            using (var db = new LocalDb(connectionTimeout: Timeout))
            {
                Console.WriteLine(db.ConnectionString);
                Assert.Contains(string.Format("Connection Timeout={0};", Timeout), db.ConnectionString);
            }
        }

        [Fact]
        public void LocalDb_allows_configuration_of_MultipleActiveResultSets()
        {
            using (var db = new LocalDb(multipleActiveResultSets: true))
            {
                Console.WriteLine(db.ConnectionString);
                Assert.Contains("MultipleActiveResultSets=true;", db.ConnectionString);
            }
        }
    }
}
