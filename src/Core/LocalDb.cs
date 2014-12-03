using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace RimDev.Automation.Sql
{
    public class LocalDb : IDisposable
    {
        public static string DatabaseDirectory = "Data";

        public static class Versions
        {
            public const string V11 = "v11.0";
            public const string V12 = "v12.0";
        }

        public string ConnectionString { get; private set; }

        public string DatabaseName { get; private set; }

        public string OutputFolder { get; private set; }

        public string DatabaseMdfPath { get; private set; }

        public string DatabaseLogPath { get; private set; }

        public LocalDb(string databaseName = null, string version = Versions.V11, string location = null, string databasePrefix = "localdb")
        {
            Location = location;
            Version = version;
            DatabaseName = string.IsNullOrWhiteSpace(databaseName)
                ? string.Format("{0}_{1}", databasePrefix, DateTime.Now.Ticks)
                : databaseName;

            CreateDatabase();
        }

        public string Version { get; protected set; }

        public string Location { get; protected set; }

        public IDbConnection OpenConnection()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        private void CreateDatabase()
        {
            var basePath = string.IsNullOrWhiteSpace(Location)
                ? (Location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                : Location;

            OutputFolder = Path.Combine(basePath, DatabaseDirectory);
            var mdfFilename = string.Format("{0}.mdf", DatabaseName);
            DatabaseMdfPath = Path.Combine(OutputFolder, mdfFilename);
            DatabaseLogPath = Path.Combine(OutputFolder, String.Format("{0}_log.ldf", DatabaseName));

            // Create Data Directory If It Doesn't Already Exist.
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            // If the database does not already exist, create it.
            var connectionString = String.Format(@"Data Source=(LocalDB)\{0};Initial Catalog=master;Integrated Security=True", Version);
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                DetachDatabase();
                cmd.CommandText = String.Format("CREATE DATABASE {0} ON (NAME = N'{0}', FILENAME = '{1}')", DatabaseName, DatabaseMdfPath);
                cmd.ExecuteNonQuery();
            }

            // Open newly created, or old database.
            ConnectionString = String.Format(@"Data Source=(LocalDB)\{0};Initial Catalog={1};Integrated Security=True;", Version, DatabaseName);
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
            const string sql = "SELECT 1 FROM master.sys.databases WHERE name = @0";
            using (var connection = new SqlConnection(string.Format(@"Data Source=(LocalDB)\{0};Initial Catalog=master;Integrated Security=True", version)))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.Add("@0", SqlDbType.NVarChar);
                cmd.Parameters["@0"].Value = databaseName;
                var count = (int)cmd.ExecuteScalar();

                return count == 1;
            }
        }

        public void Dispose()
        {
            DetachDatabase();
        }
    }
}
