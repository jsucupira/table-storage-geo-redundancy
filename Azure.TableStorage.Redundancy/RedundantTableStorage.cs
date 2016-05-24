using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using Azure.TableStorage.Utilities.Service_Bus;
using Azure.TableStorage.Utilities.Tables;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Azure.TableStorage.Redundancy
{
    public class RedundantTableStorage<T> where T : TableEntity, new()
    {
        private readonly bool _enableRedundancy;
        private readonly string _serviceBusQueue;
        private readonly string _objectName;

        [Import]
        private IAzureTableUtility _azureTableUtility;

        [Import]
        private IServiceBusContext _serviceBusContext;

        [Import]
        private IAzureTableUtility _transactionTableUtility;

        public RedundantTableStorage(string connectionString, string tableName, bool enableRedundancy = false, string serviceBusConnectionString = null, string serviceBusQueue = null, string transactionLogConnectionString = null)
        {
            MefBase.Container.SatisfyImportsOnce(this);

            _serviceBusQueue = serviceBusQueue;
            _enableRedundancy = enableRedundancy;
            _azureTableUtility.ConnectionString = connectionString;
            _azureTableUtility.TableName = tableName;
            _azureTableUtility.CreateTable();
            _objectName = typeof(T).Name;

            if (_enableRedundancy)
            {
                if (string.IsNullOrEmpty(transactionLogConnectionString))
                    throw new ApplicationException($"The {transactionLogConnectionString} cannot be null when transaction log is enabled.");

                _transactionTableUtility.ConnectionString = transactionLogConnectionString;
                _transactionTableUtility.TableName = "TransactionLog";
                _transactionTableUtility.CreateTable();
                _serviceBusContext.ConnectionString = serviceBusConnectionString;
            }
        }

        public void Delete(string partitionKey, string rowKey)
        {
            T entity = FindBy(partitionKey, rowKey);
            if (entity != null)
            {
                _azureTableUtility.DeleteEntity(partitionKey, rowKey);
                Redundant(entity, "DELETE");
            }
        }

        public List<T> ExecuteQuery(TableQuery<T> exQuery)
        {
            return _azureTableUtility.ExecuteQuery(exQuery);
        }

        public T FindBy(string partitionKey, string rowKey)
        {
            return _azureTableUtility.FindBy<T>(partitionKey, rowKey);
        }

        public IEnumerable<T> FindByPartitionKey(string partitionKey)
        {
            return _azureTableUtility.FindByPartitionKey<T>(partitionKey);
        }

        public TableResult Insert(TableEntity entity)
        {
            TableResult result = _azureTableUtility.Insert(entity);
            if (result.HttpStatusCode == (int)HttpStatusCode.NoContent)
                Redundant(entity, "INSERT");

            return result;
        }

        private void Redundant(TableEntity entity, string action)
        {
            if (_enableRedundancy)
            {
                TransactionLog log = new TransactionLog
                {
                    Action = action,
                    Object = JsonConvert.SerializeObject(entity),
                    Type = _objectName,
                    ObjectId = $"{entity.PartitionKey}|{entity.RowKey}",
                    TableName = _azureTableUtility.TableName
                };
                _serviceBusContext.AddToQueue(_serviceBusQueue, log);
                _transactionTableUtility.Upset<TransactionLogAtsEntity>(log.Map());
            }
        }

        public void Upsert(TableEntity entity)
        {
            _azureTableUtility.Upset<T>(entity);
            Redundant(entity, "UPSERT");
        }
    }
}