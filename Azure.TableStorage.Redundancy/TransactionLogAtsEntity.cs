using Microsoft.WindowsAzure.Storage.Table;

namespace Azure.TableStorage.Redundancy
{
    internal class TransactionLogAtsEntity : TableEntity
    {
        public string Action { get; set; }
        public string Object { get; set; }
        public string TableName { get; set; }
        public string ObjectId { get; set; }
    }
}