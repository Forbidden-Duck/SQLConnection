using System.Data;

namespace SQLConnection {
    interface IAlterDatabase {
        void CreateDatabase();

        void CreateDatabaseTable(string tableName, string tableStructure);
        void AlterDatabaseTable(string tableName, string tableStructure);
        void SaveDatabaseTable(DataTable table);

        int InsertRecord(string tableName, string columnNames, string columnValues);
        int InsertParentRecord(string tableName, string columnNames, string columnValues);
        bool UpdateRecord(string tableName, string columnNames, string criteria);
        void DeleteRecord(string tableName, string pkName, string pkID);
    }
}
