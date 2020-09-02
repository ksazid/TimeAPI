using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace TimeAPI.API.Models.EntityHistoryLogViewModels
{
    public class EntityHistoryLogViewModel
    {
        public string id { get; set; }
        public string entity_id { get; set; }
        public string event_ondate { get; set; }
        public string event_time { get; set; }
        public string event_type { get; set; }
        public string event_desc { get; set; }
        public string event_before { get; set; }
        public string event_after { get; set; }
        public string event_user_id { get; set; }
        public string event_modified_by { get; set; }
        public string created_date { get; set; }
        public string createdby { get; set; }
        public string modified_date { get; set; }
        public string modifiedby { get; set; }
        public bool is_deleted { get; set; }
    }
}
