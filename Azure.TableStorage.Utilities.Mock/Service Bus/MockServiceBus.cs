using System;
using System.ComponentModel.Composition;
using Azure.TableStorage.Utilities.Service_Bus;

namespace Azure.TableStorage.Utilities.Mock.Service_Bus
{
    [Export(typeof (IServiceBusContext))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MockServiceBus : IServiceBusContext
    {
        private string _connectionString;

        public void AddToQueue(string queueName, object item)
        {
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (string.IsNullOrEmpty(_connectionString))
                    _connectionString = value;
                else
                    throw new InvalidOperationException("Connections string can only be set once.");
            }
        }
    }
}