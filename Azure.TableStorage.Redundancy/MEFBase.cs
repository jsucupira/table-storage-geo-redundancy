using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.TableStorage.Redundancy
{
    public static class MefBase
    {
        public static CompositionContainer Container { get; set; }
    }
}
