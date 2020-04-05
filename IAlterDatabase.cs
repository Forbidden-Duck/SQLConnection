using System.Data;

/// <summary>
/// Interface methods for altering a database
/// </summary>
namespace SQLConnection {
    interface IAlterDatabase {
        /// <summary>
        /// Create the Database
        /// </summary>
        void CreateDatabase();

        /// <summary>
        /// Create a table
        /// </summary>
        /// <param name="tableName">The tables name</param>
        /// <param name="tableStructure">The tables structure</param>
        void CreateDatabaseTable(string tableName, string tableStructure);
        /// <summary>
        /// Alters a table
        /// </summary>
        /// <param name="tableName">The tables name</param>
        /// <param name="tableStructure">The tables structure</param>
        void AlterDatabaseTable(string tableName, string tableStructure);
        /// <summary>
        /// Saves a table
        /// </summary>
        /// <param name="table">A DataTable that represents the table</param>
        void SaveDatabaseTable(DataTable table);

        /// <summary>
        /// Insert a record into a table
        /// </summary>
        /// <param name="tableName">The tables name</param>
        /// <param name="columnNames">Column names to insert</param>
        /// <param name="columnValues">Column values that represent the column names</param>
        /// <returns></returns>
        int InsertRecord(string tableName, string columnNames, string columnValues);
        /// <summary>
        /// Insert a parent record into a table
        /// </summary>
        /// <param name="tableName">The tables name</param>
        /// <param name="columnNames">Column names to insert</param>
        /// <param name="columnValues">Column values that represent the column names</param>
        /// <returns></returns>
        int InsertParentRecord(string tableName, string columnNames, string columnValues);
        /// <summary>
        /// Update a record in a table
        /// </summary>
        /// <param name="tableName">The tables name</param>
        /// <param name="columnNames">Column names to insert</param>
        /// <param name="criteria">Record criteria</param>
        /// <returns></returns>
        bool UpdateRecord(string tableName, string columnNames, string criteria);
        /// <summary>
        /// Delete a record from a table
        /// </summary>
        /// <param name="tableName">The tables name</param>
        /// <param name="pkName">The name of the primary key</param>
        /// <param name="pkID">The primary key value</param>
        void DeleteRecord(string tableName, string pkName, string pkID);
    }
}
