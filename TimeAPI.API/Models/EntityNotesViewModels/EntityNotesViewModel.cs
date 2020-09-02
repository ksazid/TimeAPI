using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Models.EntityNotesViewModels
{
    public class EntityNotesViewModel
    {
        public string id { get; set; }
        public string org_id { get; set; }
        public string entity_id { get; set; }
        public string title { get; set; }
        public string notes { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}
