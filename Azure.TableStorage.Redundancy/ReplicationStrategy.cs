using System;
using AzureUtilities.Tables;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Azure.TableStorage.Redundancy
{
    public static class ReplicationStrategy
    {
        public static Action Create<T>(this TransactionLog transactionLogMessage, string connectionString) where T: TableEntity, new()
        {
            IAzureTableUtility azureTableUtility = ContextFactory.Create(connectionString, transactionLogMessage.TableName);
            T message = JsonConvert.DeserializeObject<T>(transactionLogMessage.Object);
            string actionType = transactionLogMessage.Action;
            if (actionType.Equals("UPSERT", StringComparison.OrdinalIgnoreCase) || actionType.Equals("INSERT", StringComparison.OrdinalIgnoreCase))
                return () => azureTableUtility.Upset<T>(message);
            if (actionType.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                return () => azureTableUtility.DeleteEntity(message.PartitionKey, message.RowKey);

            return default(Action);
        }
    }
}