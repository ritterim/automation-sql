using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace RimDev.Automation.Sql
{
    public class LocalDb : IDisposable
    {
        public static class Versions
        {
            public const string V11 = "v11.0";
            public const string V12 = "v12.0";
            public const string V13 = "v13.0";

            private static readonly Lazy<IReadOnlyList<string>> LazyInstalledVersions
                = new Lazy<IReadOnlyList<string>>(() =>
                {
                    return new[] {
                            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions\11.0", "ParentInstance", null)  == null ? null : V11,
                            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions\12.0", "ParentInstance", null)  == null ? null : V12,
                            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions\13.0", "ParentInstance", null)  == null ? null : V13,
                        }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList()
                    .AsReadOnly();
                });

            public static readonly IReadOnlyList<string> All
                = new List<string> { V11, V12, V13 }.AsReadOnly();

            public static IReadOnlyList<string> InstalledVersions
            {
                get { return LazyInstalledVersions.Value; }
            }
        }

        public string ConnectionString { get; private set; }

        public string DatabaseName { get; private set; }

        public string OutputFolder { get; private set; }

        public string DatabaseMdfPath { get; private set; }

        public string DatabaseLogPath { get; private set; }

        public string Version { get; protected set; }

        public string Location { get; protected set; }

        public Func<string> DatabaseSuffixGenerator { get; protected set; } 

        public int? ConnectionTimeout { get; protected set; }

        public bool MultipleActiveResultsSets { get; protected set; }

        public LocalDb(
            string databaseName = null,
            string version = Versions.V11,
            string location = null,
            string databasePrefix = "localdb",
            Func<string> databaseSuffixGenerator = null,
            int? connectionTimeout = null,
            bool multipleActiveResultSets = false)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("LocalDb only works on Windows platform");

            Location = location;
            Version = version;
            DatabaseSuffixGenerator = databaseSuffixGenerator ?? DateTime.Now.Ticks.ToString;
            ConnectionTimeout = connectionTimeout;
            MultipleActiveResultsSets = multipleActiveResultSets;
            DatabaseName = string.IsNullOrWhiteSpace(databaseName)
                ? string.Format("{0}_{1}", databasePrefix, DatabaseSuffixGenerator())
                : databaseName;

            CreateDatabase();
        }

        public IDbConnection OpenConnection()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        private void CreateDatabase()
        {
            OutputFolder = string.IsNullOrWhiteSpace(Location)
                ? (Location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                : Location;

            var mdfFilename = string.Format("{0}.mdf", DatabaseName);
            DatabaseMdfPath = Path.Combine(OutputFolder, mdfFilename);
            DatabaseLogPath = Path.Combine(OutputFolder, String.Format("{0}_log.ldf", DatabaseName));

            // Create Data Directory If It Doesn't Already Exist.
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            // If the database does not already exist, create it.
            if (!IsAttached())
            {
                var connectionString = String.Format(@"Data Source=(LocalDB)\{0};Initial Catalog=master;Integrated Security=True", Version);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = String.Format("CREATE DATABASE {0} ON (NAME = N'{0}', FILENAME = '{1}')", DatabaseName, DatabaseMdfPath);
                    cmd.ExecuteNonQuery();
                }
            }

            // Open newly created, or old database.
            ConnectionString = String.Format(
                @"Data Source=(LocalDB)\{0};Initial Catalog={1};Integrated Security=True;{2}{3}",
                Version,
                DatabaseName,
                ConnectionTimeout == null ? null : string.Format("Connection Timeout={0};", ConnectionTimeout),
                MultipleActiveResultsSets == true ? "MultipleActiveResultSets=true;" : null);
        }

        private void DetachDatabase()
        {
            try
            {
                var connectionString = String.Format(@"Data Source=(LocalDB)\{0};Initial Catalog=master;Integrated Security=True", Version);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = string.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; exec sp_detach_db '{0}'", DatabaseName);
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
            finally
            {
                if (File.Exists(DatabaseMdfPath)) File.Delete(DatabaseMdfPath);
                if (File.Exists(DatabaseLogPath)) File.Delete(DatabaseLogPath);
            }
        }

        public bool IsAttached()
        {
            return IsAttached(DatabaseName, Version);
        }

        public static bool IsAttached(string databaseName, string version = Versions.V11)
        {
            using (var connection = new SqlConnection(string.Format(@"Data Source=(LocalDB)\{0};Initial Catalog=master;Integrated Security=True", version)))
            {
                using (var command = new SqlCommand(string.Format("SELECT db_id('{0}')", databaseName), connection))
                {
                    connection.Open();
                    return command.ExecuteScalar() != DBNull.Value;
                }
            }
        }

        public void Dispose()
        {
            if (IsAttached())
            {
                DetachDatabase();
            }
        }
    }
}
