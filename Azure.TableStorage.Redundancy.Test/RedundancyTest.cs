using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Azure.TableStorage.Utilities.Mock;
using Azure.TableStorage.Utilities.Tables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Azure.TableStorage.Redundancy.Test
{
    [TestClass]
    public class RedundancyTest
    {
        readonly string serviceBus = "Endpoint=sb://random.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=awdawdAWDAWDAWdwadad==";

        [TestInitialize]
        public void Init()
        {
            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(AzureTableStorageMockAssembly).Assembly));
            MefBase.Container = new CompositionContainer(catalog, true);
        }

        [TestCleanup]
        public void Clean()
        {
            IAzureTableUtility customerArchiver = MefBase.Container.GetExportedValue<IAzureTableUtility>();
            customerArchiver.ConnectionString = "UseDevelopmentStorage=True";
            customerArchiver.TableName = "TransactionLog";
            customerArchiver.DeleteTable();

            IAzureTableUtility customer = MefBase.Container.GetExportedValue<IAzureTableUtility>();
            customer.ConnectionString = "UseDevelopmentStorage=True";
            customer.TableName = "Customer";
            customer.DeleteTable();
        }

        [TestMethod]
        public void test_delete()
        {
            var redundantTable = new RedundantTableStorage<Customer>("UseDevelopmentStorage=true", "Customer", true, serviceBus, "RedundancyQueue", "UseDevelopmentStorage=true");
            var customer = new Customer();
            redundantTable.Insert(customer);
            redundantTable.Delete(customer.PartitionKey, customer.RowKey);
            IAzureTableUtility customerArchiver = MefBase.Container.GetExportedValue<IAzureTableUtility>();
            customerArchiver.ConnectionString = "UseDevelopmentStorage=True";
            customerArchiver.TableName = "TransactionLog";
            Assert.IsTrue(customerArchiver.FindByPartitionKey<TableEntity>("Customer").Count() == 2);

        }

        [TestMethod]
        public void test_save()
        {
            var redundantTable = new RedundantTableStorage<Customer>("UseDevelopmentStorage=true", "Customer", true, serviceBus, "RedundancyQueue", "UseDevelopmentStorage=true");

            for (int i = 0; i < 10; i++)
            {
                redundantTable.Insert(new Customer
                {
                    Email = i.ToString(),
                    FirstName = i.ToString(),
                    LastName = i.ToString()
                });
            }

            Assert.IsTrue(redundantTable.FindByPartitionKey($"{DateTime.Now:yyyy-MM-DD}").Count() == 10);
            IAzureTableUtility customerArchiver = MefBase.Container.GetExportedValue<IAzureTableUtility>();
            customerArchiver.ConnectionString = "UseDevelopmentStorage=True";
            customerArchiver.TableName = "TransactionLog";
            Assert.IsTrue(customerArchiver.FindByPartitionKey<TableEntity>("Customer").Count() == 10);
        }

        [TestMethod]
        public void test_insert()
        {
            var redundantTable = new RedundantTableStorage<Customer>("UseDevelopmentStorage=true", "Customer", true, serviceBus, "RedundancyQueue", "UseDevelopmentStorage=true");
            for (int i = 0; i < 10; i++)
            {
                redundantTable.Upsert(new Customer
                {
                    Email = i.ToString(),
                    FirstName = i.ToString(),
                    LastName = i.ToString()
                });
            }

            Assert.IsTrue(redundantTable.FindByPartitionKey($"{DateTime.Now:yyyy-MM-DD}").Count() == 10);
            IAzureTableUtility customerArchiver = MefBase.Container.GetExportedValue<IAzureTableUtility>();
            customerArchiver.ConnectionString = "UseDevelopmentStorage=True";
            customerArchiver.TableName = "TransactionLog";
            Assert.IsTrue(customerArchiver.FindByPartitionKey<TableEntity>("Customer").Count() == 10);
        }

        [TestMethod]
        public void test_delete_no_redundancy()
        {
            var redundantTable = new RedundantTableStorage<Customer>("UseDevelopmentStorage=true", "Customer", false, serviceBus, "RedundancyQueue", "UseDevelopmentStorage=true");
            var customer = new Customer();
            redundantTable.Insert(customer);
            redundantTable.Delete(customer.PartitionKey, customer.RowKey);
            IAzureTableUtility customerArchiver = MefBase.Container.GetExportedValue<IAzureTableUtility>();
            customerArchiver.ConnectionString = "UseDevelopmentStorage=True";
            customerArchiver.TableName = "TransactionLog";
            Assert.IsTrue(!customerArchiver.FindByPartitionKey<TableEntity>("Customer").Any());
        }

        [TestMethod]
        public void test_save_no_redundancy()
        {
            var redundantTable = new RedundantTableStorage<Customer>("UseDevelopmentStorage=true", "Customer", false, serviceBus, "RedundancyQueue", "UseDevelopmentStorage=true");
            for (int i = 0; i < 10; i++)
            {
                redundantTable.Insert(new Customer
                {
                    Email = i.ToString(),
                    FirstName = i.ToString(),
                    LastName = i.ToString()
                });
            }

            Assert.IsTrue(redundantTable.FindByPartitionKey($"{DateTime.Now:yyyy-MM-DD}").Count() == 10);
            IAzureTableUtility customerArchiver = MefBase.Container.GetExportedValue<IAzureTableUtility>();
            customerArchiver.ConnectionString = "UseDevelopmentStorage=True";
            customerArchiver.TableName = "TransactionLog";
            Assert.IsTrue(!customerArchiver.FindByPartitionKey<TableEntity>("Customer").Any());
        }

        [TestMethod]
        public void test_insert_no_redundancy()
        {
            var redundantTable = new RedundantTableStorage<Customer>("UseDevelopmentStorage=true", "Customer", false, serviceBus, "RedundancyQueue", "UseDevelopmentStorage=true");
            for (int i = 0; i < 10; i++)
            {
                redundantTable.Upsert(new Customer
                {
                    Email = i.ToString(),
                    FirstName = i.ToString(),
                    LastName = i.ToString()
                });
            }

            Assert.IsTrue(redundantTable.FindByPartitionKey($"{DateTime.Now:yyyy-MM-DD}").Count() == 10);
            IAzureTableUtility customerArchiver = MefBase.Container.GetExportedValue<IAzureTableUtility>();
            customerArchiver.ConnectionString = "UseDevelopmentStorage=True";
            customerArchiver.TableName = "TransactionLog";
            Assert.IsTrue(!customerArchiver.FindByPartitionKey<TableEntity>("Customer").Any());
        }

        [TestMethod]
        public void test_plan()
        {
            var customer = new Customer();
            TransactionLog log = new TransactionLog()
            {
                Action = "UPSERT",
                Object = JsonConvert.SerializeObject(customer),
                ObjectId = $"{customer.PartitionKey}|{customer.RowKey}",
                TableName = "Customer",
                Type = "Customer"
            };

            var action = log.Create("UseDevelopmentStorage=True");
            action?.Invoke();
        }
    }

    public class Customer : TableEntity
    {
        public Customer()
        {
            PartitionKey = $"{DateTime.Now:yyyy-MM-DD}";
            RowKey = Guid.NewGuid().ToString();
        }

        public string FirstName { get; set; }
        public string Email { get; set; }
        public string LastName { get; set; }
    }
}
