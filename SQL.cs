using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using NLog;

namespace SQLConnection {
    public class SQL : IQueryDatabase, IAlterDatabase {
        #region Global Variables

        private Logger _log;
        SqlConnection _sqlConn;
        SqlCommand _sqlCmd;

        #endregion

        #region Constructor

        public SQL() {
            LogManager.LoadConfiguration("NLog.config");
            _log = LogManager.GetCurrentClassLogger();

            // Get ConnectionString from app.config
            string connectionString = ConfigurationManager
                                      .ConnectionStrings["ConnectionString"].ConnectionString;
            // Create a new SqlConnection
            _sqlConn = new SqlConnection(connectionString);
        }

        #endregion

        #region Database

        /// <summary>
        /// Creates a new database if one doesn't exist
        /// </summary>
        public void CreateDatabase() {
            // Create server connection string
            string connectionString = $"Data Source = {_sqlConn.DataSource};" +
                                      "Integrated Security = True;";
            // Create another SqlConnection
            SqlConnection sqlConn = new SqlConnection(connectionString);

            // String that holds the query
            string sqlQuery =
                $"IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name='{_sqlConn.Database}') " +
                $"CREATE DATABASE  {_sqlConn.Database}";
            // Create a SqlCommand
            using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConn)) {
                // Check if the connection is closed
                if (sqlConn.State == ConnectionState.Closed) {
                    sqlConn.Open();
                }
                // Execute the query
                sqlCommand.ExecuteNonQuery();
                // Close the sql connection
                sqlConn.Close();
            }
        }

        #endregion

        #region Database Table

        /// <summary>
        /// Creates a database table if one doesn't already exist
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <param name="tableStructure">The table structure or schema</param>
        public void CreateDatabaseTable(string tableName, string tableStructure) {
            // Query to create table and the table structure
            string sqlQuery =
                $"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{tableName}' AND xtype='U')" +
                $"CREATE TABLE {tableName} ({tableStructure}) ";

            try {
                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }
                    _sqlCmd.ExecuteNonQuery();
                    _sqlConn.Close();
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            }
        }

        /// <summary>
        /// Alters a database table
        /// </summary>
        /// <param name="tableName">The tables name</param>
        /// <param name="tableStructure">The tables structure</param>
        public void AlterDatabaseTable(string tableName, string tableStructure) {
            try {
                string sqlQuery = $"ALTER TABLE {tableName} ({tableStructure})";

                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }
                    _sqlCmd.ExecuteNonQuery();
                    _sqlConn.Close();
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            }
        }

        /// <summary>
        /// Saves a database table
        /// </summary>
        /// <param name="dtable">The datatable</param>
        public void SaveDatabaseTable(DataTable dtable) {
            string sqlQuery = $"SELECT * FROM {dtable.TableName}";
            try {
                using (SqlDataAdapter adapt = new SqlDataAdapter(sqlQuery, _sqlConn)) {
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }

                    // SqlCommandBuilder to create Insert, Update and Delete command
                    SqlCommandBuilder cmdBuild = new SqlCommandBuilder(adapt);
                    adapt.InsertCommand = cmdBuild.GetInsertCommand();
                    adapt.UpdateCommand = cmdBuild.GetUpdateCommand();
                    adapt.DeleteCommand = cmdBuild.GetDeleteCommand();

                    adapt.Update(dtable);
                    _sqlConn.Close();
                    dtable.AcceptChanges();
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            }
        }

        #endregion

        #region Table Record

        /// <summary>
        /// Inserts a parent record into a table
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="columnNames">The tables columns</param>
        /// <param name="columnValues">The columns values</param>
        /// <returns></returns>
        public int InsertParentRecord(string tableName, string columnNames, string columnValues) {
            int id = 0;

            try {
                string sqlQuery = 
                    $"INSERT INTO {tableName} ({columnNames}) " +
                    $"VALUES ({columnValues}) " +
                    $"SELECT SCOPE_IDENTITY()";

                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }
                    var output = _sqlCmd.ExecuteScalar();
                    if (!(output is DBNull)) {
                        id = (int)(decimal)output;
                    }
                    _sqlConn.Close();
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            }
            return id;
        }

        /// <summary>
        /// Inserts a record into a table
        /// </summary>
        /// <param name="tableName">Table name to insert record into</param>
        /// <param name="columnNames">Column name of the table</param>
        /// <param name="columnValues">Column value of the column</param>
        /// <returns>ID of the newly inserted record. Returns 0 if null</returns>
        public int InsertRecord(string tableName, string columnNames, string columnValues) {
            // ID (PK)
            int id = 0;
            // Query to insert into table
            string idName = columnNames.Split(new string[] { ", " }, StringSplitOptions.None)[0];
            string idValue = columnValues.Split(new string[] { ", " }, StringSplitOptions.None)[0];
            string sqlQuery =
                $"SET IDENTITY_INSERT {tableName} ON; " +
                $"IF NOT EXISTS (SELECT {idName} FROM {tableName} WHERE {idName}='{idValue}') " +
                $"INSERT INTO {tableName} ({columnNames}) VALUES ({columnValues});" +
                $"SET IDENTITY_INSERT {tableName} OFF;" +
                $"SELECT SCOPE_IDENTITY();";

            try {
                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }
                    id = _sqlCmd.ExecuteScalar() is DBNull ? 0 : (int)(decimal)_sqlCmd.ExecuteScalar();
                    _sqlConn.Close();
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            }
            return id;
        }

        /// <summary>
        /// Update a record into a table
        /// </summary>
        /// <param name="tableName">Table name to insert into</param>
        /// <param name="columnNamesAndValues">Column Names and Column Values</param>
        /// <param name="criteria">The criteria of updating</param>
        /// <returns>Boolean signifying if the operation was successful</returns>
        public bool UpdateRecord(string tableName, string columnNamesAndValues, string criteria) {
            bool isOK = false;
            string sqlQuery = $"UPDATE {tableName} SET {columnNamesAndValues} WHERE {criteria}";

            try {
                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }
                    _sqlCmd.ExecuteNonQuery();
                    isOK = true;
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
                isOK = false;
            }
            return isOK;
        }

        /// <summary>
        /// Delete a record from a table
        /// </summary>
        /// <param name="tableName">Table name to delete from</param>
        /// <param name="pkName">Column name of the primary key</param>
        /// <param name="pkID">Column value of the primary key</param>
        public void DeleteRecord(string tableName, string pkName, string pkID) {
            string sqlQuery = 
                $"DELETE FROM {tableName} WHERE {pkName}={pkID}" +
                "SELECT SCOPE_IDENTITY()";

            try {
                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }
                    _sqlCmd.ExecuteScalar();
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            }
        }

        #endregion

        #region Get Table

        /// <summary>
        /// Gets an updateable table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Updateable Table</returns>
        public DataTable GetDataTable(string tableName) {
            DataTable dtable = new DataTable(tableName);
            string sqlQuery = $"SELECT * FROM {tableName}";

            try {
                // DataAdapter to allow the creation of an updateable DataTable
                using (SqlDataAdapter adapt = new SqlDataAdapter(sqlQuery, _sqlConn)) {
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }
                    // Execute the query and populate the DataTable
                    adapt.Fill(dtable);
                    _sqlConn.Close();

                    // Specify the DataTable's Primary Key
                    dtable.PrimaryKey = new DataColumn[] { dtable.Columns[0] };

                    // Auto-increment the Primary Keys
                    dtable.Columns[0].AutoIncrement = true;

                    // Seed the Primary Keys
                    if (dtable.Rows.Count > 0) {
                        dtable.Columns[0].AutoIncrementSeed = long.Parse(dtable.Rows[dtable.Rows.Count - 1][0].ToString());
                    }

                    // Set the Auto-increment step to 1
                    dtable.Columns[0].AutoIncrementStep = 1;

                    // Setting the columns to not read only
                    foreach (DataColumn col in dtable.Columns) {
                        col.ReadOnly = false;
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            }
            return dtable;
        }
        /// <summary>
        /// Get a read-only Table
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="isReadOnly">Specify if read-only</param>
        /// <returns>An non-updateable table</returns>
        public DataTable GetDataTable(string tableName, bool isReadOnly) {
            if (!isReadOnly) {
                return GetDataTable(tableName);
            }

            DataTable dtable = new DataTable(tableName);
            string sqlQuery = $"SELECT * FROM {tableName}";

            try {
                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }

                    using (SqlDataReader read = _sqlCmd.ExecuteReader()) {
                        dtable.Load(read);
                        _sqlConn.Close();
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            }
            return dtable;
        }
        /// <summary>
        /// Get an updateable table
        /// </summary>
        /// <param name="sqlQuery">The query</param>
        /// <param name="tableName">The table name</param>
        /// <returns>Updateable Table</returns>
        public DataTable GetDataTable(string sqlQuery, string tableName) {
            DataTable dtable = new DataTable(tableName);

            try {
                // DataAdapter to allow the creation of an updateable DataTable
                using (SqlDataAdapter adapt = new SqlDataAdapter(sqlQuery, _sqlConn)) {
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }
                    // Execute the query and populate the DataTable
                    adapt.Fill(dtable);
                    _sqlConn.Close();

                    // Specify the DataTable's Primary Key
                    dtable.PrimaryKey = new DataColumn[] { dtable.Columns[0] };

                    // Auto-increment the Primary Keys
                    dtable.Columns[0].AutoIncrement = true;

                    // Seed the Primary Keys
                    if (dtable.Rows.Count > 0) {
                        dtable.Columns[0].AutoIncrementSeed = long.Parse(dtable.Rows[dtable.Rows.Count - 1][0].ToString());
                    }

                    // Set the Auto-increment step to 1
                    dtable.Columns[0].AutoIncrementStep = 1;

                    // Setting the columns to not read only
                    foreach (DataColumn col in dtable.Columns) {
                        col.ReadOnly = false;
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            }
            return dtable;
        }
        /// <summary>
        /// Get a read-only table
        /// </summary>
        /// <param name="sqlQuery">The query</param>
        /// <param name="tableName">The table name</param>
        /// <param name="isReadOnly">Specify if read-only</param>
        /// <returns>A non-updateable table</returns>
        public DataTable GetDataTable(string sqlQuery, string tableName, bool isReadOnly) {
            if (!isReadOnly) {
                return GetDataTable(tableName);
            }

            DataTable dtable = new DataTable(tableName);

            try {
                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }

                    using (SqlDataReader read = _sqlCmd.ExecuteReader()) {
                        dtable.Load(read);
                        _sqlConn.Close();
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            }
            return dtable;
        }

        #endregion
    }
}