using AzureUtilities.Tables;

namespace Azure.TableStorage.Redundancy
{
    internal static class ContextFactory
    {
        internal static IAzureTableUtility Create(string connectionString, string tableName)
        {
            IAzureTableUtility azureTable = MefBase.Container.GetExportedValue<IAzureTableUtility>();
            azureTable.ConnectionString = connectionString;
            azureTable.TableName = tableName;
            azureTable.CreateTable();
            return azureTable;
        }
    }
}