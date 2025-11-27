using Microsoft.Xrm.Sdk;
using System;
using System.Runtime.Serialization;

namespace FacilityDataProvider
{
    [DataContract]
    public class Facility
    {
        [DataMember]
        public Guid facility_id { get; set; }
        [DataMember]
        public Guid client_id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string facility_type { get; set; }
        [DataMember]
        public string address { get; set; }
        [DataMember]
        public DateTime created_on { get; set; }
        [DataMember]
        public DateTime modified_on { get; set; }

        public Entity ToEntity(ITracingService tracingService)
        {
            Entity entity = new Entity("ltornyi_facility");
            
            // Map data to entity
            entity["ltornyi_facilityid"] = facility_id;
            entity["ltornyi_clientid"] = new EntityReference("account", client_id);
            entity["ltornyi_name"] = name;
            entity["ltornyi_facilitytype"] = facility_type;
            entity["ltornyi_address"] = address;
            entity["ltornyi_created_on"] = created_on;
            entity["ltornyi_modified_on"] = modified_on;

            tracingService.Trace("Added id:{0}, name:{1}, type:{2}, created:{3} to results", facility_id, name, facility_type, created_on);

            return entity;
        }
    }


}
