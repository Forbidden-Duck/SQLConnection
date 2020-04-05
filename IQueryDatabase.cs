using System.Data;

/// <summary>
/// Interface methods for returning data
/// </summary>
namespace SQLConnection {
    interface IQueryDatabase {
        /// <summary>
        /// Get a data table
        /// </summary>
        /// <param name="tableName">The name of the table to retreive</param>
        /// <returns>A DataTable</returns>
        DataTable GetDataTable(string tableName);
        /// <summary>
        /// Get a data table
        /// </summary>
        /// <param name="tableName">The name of the table to retreive</param>
        /// <param name="isReadOnly">If it will be read only</param>
        /// <returns>A DataTable</returns>
        DataTable GetDataTable(string tableName, bool isReadOnly);
        /// <summary>
        /// Get a data table
        /// </summary>
        /// <param name="sqlQuery">The query to execute</param>
        /// <param name="tableName">The name of the table to retreive</param>
        /// <returns>A DataTable</returns>
        DataTable GetDataTable(string sqlQuery, string tableName);
        /// <summary>
        /// Get a data table
        /// </summary>
        /// <param name="sqlQuery">The query to execute</param>
        /// <param name="tableName">The name of the table to retreive</param>
        /// <param name="isReadOnly">If it will be read only</param>
        /// <returns>A DataTable</returns>
        DataTable GetDataTable(string sqlQuery, string tableName, bool isReadOnly);
    }
}
