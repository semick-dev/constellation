using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Constellation.Drone.Downloader.Record
{
    public class LiteClient
    {

        public SqliteConnection Connection { get; set; }

        public LiteClient(string fileUrl) {

            Connection = new SqliteConnection($"Data Source={fileUrl}");

            CreateDroneWorkTables();

        }

        /// <summary>
        /// Creates a table (if one doesn't exist) to contain the results of each completed download operation.
        /// 
        /// downloads
        ///     id INT PRIMARY KEY
        ///     payloadtype
        ///     url
        ///     quality
        ///     downloadpath
        ///     thumbnailpath
        /// 
        /// </summary>
        public void CreateDroneWorkTables()
        {
            Connection.Open();
            var cmd = Connection.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS downloads(id INTEGER PRIMARY KEY,
            url TEXT, quality TEXT, payloadtype TEXT, downloadPath TEXT, thumbnailPath TEXT)";
            cmd.ExecuteNonQuery();
            Connection.Close();
        }

    }
}
