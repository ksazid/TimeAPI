using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models
{
    public class StorageSettings
    {
        public string StorageDefaultConnection { get; set; }
        public string Container { get; set; }
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string DefaultEndpointsProtocol { get; set; }
        public string EndpointSuffix { get; set; }

        public StorageSettings() { }
    }
}
