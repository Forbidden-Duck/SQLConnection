using System;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using NLog;

/// <summary>
/// Assign code to the methods
/// Interface located in IQueryDatabase and IAlterDatabase
/// </summary>
namespace SQLConnection {
    public class SQL : IQueryDatabase, IAlterDatabase {
        #region Global Variables

        // Create a variable for logging errors
        private Logger _log;
        // Create 2 variables for our SQL Connection and Command
        SqlConnection _sqlConn;
        SqlCommand _sqlCmd;

        #endregion

        #region Constructor

        /// <summary>
        /// Create a new instance of SQL
        /// </summary>
        public SQL() {
            // Load the logger configuration
            // Assign the logger to _log
            LogManager.LoadConfiguration("NLog.config");
            _log = LogManager.GetCurrentClassLogger();

            // Get the connection string from our app.config
            string connectionString = ConfigurationManager
                                      .ConnectionStrings["ConnectionString"].ConnectionString;
            // Assign a new SQL Connection to _sqlConn
            _sqlConn = new SqlConnection(connectionString);
        }

        #endregion

        #region Database

        /// <summary>
        /// Creates a new database if one doesn't exist
        /// </summary>
        public void CreateDatabase() {
            // Create a new connection string
            // This is done so we can connect to the server
            // Without connecting to a specific database itself (or it causes errors)
            string connectionString = $"Data Source = {_sqlConn.DataSource};" +
                                      "Integrated Security = True;";
            // Create and assign a new SQL Connection with the new connection string
            SqlConnection sqlConn = new SqlConnection(connectionString);

            // Create and assign a new SQL Query
            string sqlQuery =
                $"IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name='{_sqlConn.Database}') " +
                $"CREATE DATABASE  {_sqlConn.Database}";
            // Create and assign a new SQL Command
            using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConn)) {
                // Check if the SQL Connection is closed
                // Open the connection if closed
                if (sqlConn.State == ConnectionState.Closed) {
                    sqlConn.Open();
                }
                // Execute the query
                sqlCommand.ExecuteNonQuery(); // Executes a query that does not return a value
                // Close the SQL Connection
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
            // Create and assign a new SQL Query
            string sqlQuery =
                $"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{tableName}' AND xtype='U')" +
                $"CREATE TABLE {tableName} ({tableStructure}) ";

            // Try to execute the query
            // Catch any errors
            // Finally close the SQL Connection
            try {
                // Assign a new SQL Command to _sqlCmd
                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    // Check if the SQL Connection is closed
                    // Open the SQL Connection
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }
                    // Execute the query
                    _sqlCmd.ExecuteNonQuery();
                }
            } catch (Exception e) {
                // Print the error into the console
                // Print the error into the logger
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            } finally {
                // Close the SQL Connection
                _sqlConn.Close();
            }
        }

        /// <summary>
        /// Alters a database table
        /// </summary>
        /// <param name="tableName">The tables name</param>
        /// <param name="tableStructure">The tables structure</param>
        public void AlterDatabaseTable(string tableName, string tableStructure) {
            // Create and assign a new SQL Query
            string sqlQuery = $"ALTER TABLE {tableName} ({tableStructure})";

            // Try to execute the query
            // Catch any errors
            // Finally close the SQL Connection
            try {
                // Assign a new SQL Command to _sqlCmd
                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    // Check if the SQL Connection is closed
                    // Open the SQL Connection
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }
                    // Execute the query
                    _sqlCmd.ExecuteNonQuery();
                }
            } catch (Exception e) {
                // Log the error in the console
                // Log the error in the logger
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            } finally {
                // Close the SQL Connection
                _sqlConn.Close();
            }
        }

        /// <summary>
        /// Saves a database table
        /// </summary>
        /// <param name="dtable">The datatable</param>
        public void SaveDatabaseTable(DataTable dtable) {
            // Create and assign a new SQL Query
            string sqlQuery = $"SELECT * FROM {dtable.TableName}";

            // Try to execute the query
            // Catch any errors
            // Finally close the connection
            try {
                // Create and assign a new SQL Data Adapter
                using (SqlDataAdapter adapt = new SqlDataAdapter(sqlQuery, _sqlConn)) {
                    // Check if the SQL Connection is closed
                    // Open the SQL Connection
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }

                    // Create and assign a new SQL Command Builder
                    SqlCommandBuilder cmdBuild = new SqlCommandBuilder(adapt);
                    // Assign a new Insert, Update and Delete command to cmdBuild
                    adapt.InsertCommand = cmdBuild.GetInsertCommand();
                    adapt.UpdateCommand = cmdBuild.GetUpdateCommand();
                    adapt.DeleteCommand = cmdBuild.GetDeleteCommand();

                    // Update the changes to the dtable
                    // Accept the changes in the dtable
                    adapt.Update(dtable);
                    dtable.AcceptChanges();
                }
            } catch (Exception e) {
                // Log the error to the console
                // Log the error to the logger
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            } finally {
                // Close the SQL Connection
                _sqlConn.Close();
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
            // Create a variable for the Primary Key
            // Create and assign a new SQL Query
            int id = 0;
            string sqlQuery =
                $"INSERT INTO {tableName} ({columnNames}) " +
                $"VALUES ({columnValues}) " +
                $"SELECT SCOPE_IDENTITY()";

            // Try to execute the query
            // Catch any errors
            // Finally close the SQL Connection
            try {
                // Assign a new SQL Command to _sqlCmd
                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    // Check if the SQL Connection is closed
                    // Open the SQL Connection
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }

                    // Create and assign the SQL Execute
                    var output = _sqlCmd.ExecuteScalar(); // Executes the query and returns the first value
                    // If the output is not DBNull
                    // Assign the Primary Key to id
                    if (!(output is DBNull)) {
                        id = (int)(decimal)output;
                    }
                }
            } catch (Exception e) {
                // Log the error to the console
                // Log the error to the logger
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            } finally {
                // Close the SQL Connection
                _sqlConn.Close();
            }

            // Return id
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
            // Create a variable for the Primary Key
            int id = 0;
            // Create variables for getting the Primary Key's Column Name and Value
            // Create and assign a new SQL Query
            string idName = columnNames.Split(new string[] { ", " }, StringSplitOptions.None)[0];
            string idValue = columnValues.Split(new string[] { ", " }, StringSplitOptions.None)[0];
            string sqlQuery =
                $"SET IDENTITY_INSERT {tableName} ON; " +
                $"IF NOT EXISTS (SELECT {idName} FROM {tableName} WHERE {idName}='{idValue}') " +
                $"INSERT INTO {tableName} ({columnNames}) VALUES ({columnValues});" +
                $"SET IDENTITY_INSERT {tableName} OFF;" +
                $"SELECT SCOPE_IDENTITY();";

            // Try to execute the query
            // Catch any errors
            // Finally close the SQL Connection
            try {
                // Assign a new SQL Command to _sqlCmd
                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    // Check if the SQL Connection is closed
                    // Open the SQL Connection
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }

                    // Create and assign the SQL Execute
                    var output = _sqlCmd.ExecuteScalar();
                    // If the output is not DBNull
                    // Assign the Primary Key to id
                    if (!(output is DBNull)) {
                        id = (int)(decimal)output;
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            } finally {
                _sqlConn.Close();
            }

            // Return id
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
            // Create a variable for checking if the query was executed successfully
            // Create and assign a new SQL Query
            bool isOK = false;
            string sqlQuery = $"UPDATE {tableName} SET {columnNamesAndValues} WHERE {criteria}";

            // Try to execute the query
            // Catch any errors
            // Finally close the SQL Connection
            try {
                // Assign a new SQL Command to _sqlCmd
                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    // Check if the SQL Connection is closed
                    // Open the SQL Connection
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }

                    // Execute the query
                    // Assign isOK with true
                    _sqlCmd.ExecuteNonQuery();
                    isOK = true;
                }
            } catch (Exception e) {
                // Log the error to the console
                // Log the error to the logger
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
                // Assign isOK with false
                isOK = false;
            } finally {
                // Close the SQL Connection
                _sqlConn.Close();
            }

            // Return isOK
            return isOK;
        }

        /// <summary>
        /// Delete a record from a table
        /// </summary>
        /// <param name="tableName">Table name to delete from</param>
        /// <param name="pkName">Column name of the primary key</param>
        /// <param name="pkID">Column value of the primary key</param>
        public void DeleteRecord(string tableName, string pkName, string pkID) {
            // Create and assign a new SQL Query
            string sqlQuery =
                $"DELETE FROM {tableName} WHERE {pkName}={pkID}" +
                "SELECT SCOPE_IDENTITY()";

            // Try to execute the query
            // Catch any errors
            // Finally close the SQL Connection
            try {
                // Assign a new SQL Command to _sqlCmd
                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    // Check if the SQL Connection is closed
                    // Open the SQL Connection
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }

                    // Execute the query
                    _sqlCmd.ExecuteScalar();
                }
            } catch (Exception e) {
                // Log the error to the console
                // log the error to the logger
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            } finally {
                // Close the SQL Connection
                _sqlConn.Close();
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
            // Create and assign a new Data Table
            // Create and assign a new SQL Query
            DataTable dtable = new DataTable(tableName);
            string sqlQuery = $"SELECT * FROM {tableName}";

            // Try to execute the query
            // Catch any errors
            // Finally close the SQL Connection
            try {
                // Create and assign a new SQL Data Adapter
                using (SqlDataAdapter adapt = new SqlDataAdapter(sqlQuery, _sqlConn)) {
                    // Check if the SQL Connection is closed
                    // Open the SQL Connection
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }

                    // Fill the dtable
                    adapt.Fill(dtable);

                    // Provide the column that the Primary Key is in
                    // Change the primay key type to int (sometimes it reads it as a String)
                    // Auto increment the Primary Key
                    dtable.PrimaryKey = new DataColumn[] { dtable.Columns[0] };
                    dtable.Columns[0].DataType = typeof(int);
                    dtable.Columns[0].AutoIncrement = true;

                    // If the Data Table row count is greater than 0
                    // Seed the Primary Keys
                    if (dtable.Rows.Count > 0) {
                        dtable.Columns[0].AutoIncrementSeed = long.Parse(dtable.Rows[dtable.Rows.Count - 1][0].ToString());
                    }

                    // Set the Auto-Increment to 1
                    dtable.Columns[0].AutoIncrementStep = 1;

                    // For each column in the Data Table
                    // Set the column readonly to false
                    foreach (DataColumn col in dtable.Columns) {
                        col.ReadOnly = false;
                    }
                }
            } catch (Exception e) {
                // Log the error in the console
                // Log the error in the logger
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            } finally {
                // Close the SQL Connection
                _sqlConn.Close();
            }

            // Return dtable
            return dtable;
        }
        /// <summary>
        /// Get a read-only Table
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="isReadOnly">Specify if read-only</param>
        /// <returns>An non-updateable table</returns>
        public DataTable GetDataTable(string tableName, bool isReadOnly) {
            // If not read only
            // Return and execute GetDataTable(string tableName)
            if (!isReadOnly) {
                return GetDataTable(tableName);
            }

            // Create and assign a new Data Table
            // Create and assign a new SQL Query
            DataTable dtable = new DataTable(tableName);
            string sqlQuery = $"SELECT * FROM {tableName}";

            // Try to execute the query
            // Catch any errors
            // Finally close the SQL Connection
            try {
                // Assign a new SQL Command to _sqlCmd
                using (_sqlCmd = new SqlCommand(sqlQuery, _sqlConn)) {
                    // Check if the SQL Connection is closed
                    // Open the SQL Connection
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }

                    // Create and assign a new SQL Data Reader
                    using (SqlDataReader read = _sqlCmd.ExecuteReader()) {
                        // Load the Data Table
                        dtable.Load(read);
                    }
                }
            } catch (Exception e) {
                // Log the error in the console
                // Log the error in the logger
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            } finally {
                // Close the SQL Connection
                _sqlConn.Close();
            }

            // Return dtable
            return dtable;
        }

        /// <summary>
        /// Get an updateable table
        /// </summary>
        /// <param name="sqlQuery">The query</param>
        /// <param name="tableName">The table name</param>
        /// <returns>Updateable Table</returns>
        public DataTable GetDataTable(string sqlQuery, string tableName) {
            /*
             * Refer to "public DataTable GetDataTable(string tableName)"
             * for the code comments.
             * Note: This method does not create a SQL Query as it is already provided
            */
            DataTable dtable = new DataTable(tableName);

            try {
                using (SqlDataAdapter adapt = new SqlDataAdapter(sqlQuery, _sqlConn)) {
                    if (_sqlConn.State == ConnectionState.Closed) {
                        _sqlConn.Open();
                    }

                    adapt.Fill(dtable);

                    dtable.PrimaryKey = new DataColumn[] { dtable.Columns[0] };
                    dtable.Columns[0].AutoIncrement = true;

                    if (dtable.Rows.Count > 0) {
                        dtable.Columns[0].AutoIncrementSeed = long.Parse(dtable.Rows[dtable.Rows.Count - 1][0].ToString());
                    }

                    dtable.Columns[0].AutoIncrementStep = 1;

                    foreach (DataColumn col in dtable.Columns) {
                        col.ReadOnly = false;
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            } finally {
                _sqlConn.Close();
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
            /*
             * Refer to "public DataTable GetDataTable(string tableName, bool isReadOnly)"
             * for the code comments.
             * Note: This method does not create a SQL Query as it is already provided
            */
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
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
                _log.Error(e.ToString());
            } finally {
                _sqlConn.Close();
            }

            return dtable;
        }

        #endregion
    }
}