using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Function.ContainerRunner
{
    internal class ContainerRunRequest
    {
        public string ContainerGroupName { get; set; }
        public string Region { get; set; }
        public string ContainerImage { get; set; }
        public string ResourceGroupName { get; set; }

    }
}
