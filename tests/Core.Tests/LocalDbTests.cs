using System;
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
        public void LocalDB_Throws_Valid_Exception_When_Not_Supported_Version()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using (var db = new LocalDb(version: "vBad")) {}
            });
        }

    }
}
