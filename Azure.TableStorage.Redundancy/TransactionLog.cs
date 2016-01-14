using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.TableStorage.Redundancy
{
    [Serializable]
    public class TransactionLog
    {
        public TransactionLog()
        {
            TransactionId = Guid.NewGuid().ToString();
        }

        public string Action { get; set; }
        public string TransactionId { get; set; }
        public string Object { get; set; }
        public string Type { get; set; }
    }
}
