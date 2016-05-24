using System;
using System.Collections.Generic;
using System.Reflection;
using Azure.TableStorage.Utilities.Tables;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Azure.TableStorage.Redundancy
{
    public static class ReplicationStrategy
    {
        public static Action Create(this TransactionLog transactionLogMessage, string connectionString)
        {
            IAzureTableUtility azureTableUtility = ContextFactory.Create(connectionString, transactionLogMessage.TableName);

            JObject jsonMessage = JsonConvert.DeserializeObject<JObject>(transactionLogMessage.Object);

            DynamicTableEntity temp = new DynamicTableEntity();
            foreach (KeyValuePair<string, JToken> keyValuePair in jsonMessage)
            {
                if (keyValuePair.Key.Equals("Timestamp"))
                    continue;
                
                if (keyValuePair.Key.Equals("PartitionKey"))
                    temp.PartitionKey = keyValuePair.Value.ToString();
                else if (keyValuePair.Key.Equals("RowKey"))
                    temp.RowKey = keyValuePair.Value.ToString();
                else
                    temp.Properties.Add(keyValuePair.Key, EntityProperty.CreateEntityPropertyFromObject(keyValuePair.Value));

            }
            temp.ETag = null;
            temp.Timestamp = DateTimeOffset.UtcNow;

            string actionType = transactionLogMessage.Action;
            if (actionType.Equals("UPSERT", StringComparison.OrdinalIgnoreCase) || actionType.Equals("INSERT", StringComparison.OrdinalIgnoreCase))
                return () => azureTableUtility.Upset<DynamicTableEntity>(temp);
            if (actionType.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                return () => azureTableUtility.DeleteEntity(temp.PartitionKey, temp.RowKey);

            return default(Action);
        }
    }
}