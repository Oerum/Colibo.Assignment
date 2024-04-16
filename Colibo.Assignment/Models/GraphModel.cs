using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colibo.Assignment.Models
{
    public class GraphModel
    {
        [JsonProperty("@odata.context")]
        public string? OdataContext { get; set; }

        [JsonProperty("value")]
        public List<Value>? Value { get; set; }
    }

    public class Value
    {
        [JsonProperty("businessPhones")]
        public List<string>? BusinessPhones { get; set; }

        [JsonProperty("displayName")]
        public string? DisplayName { get; set; }

        [JsonProperty("givenName")]
        public string? GivenName { get; set; }

        [JsonProperty("jobTitle")]
        public string? JobTitle { get; set; }

        [JsonProperty("mail")]
        public string? Mail { get; set; }

        [JsonProperty("mobilePhone")]
        public string? MobilePhone { get; set; }

        [JsonProperty("officeLocation")]
        public string? OfficeLocation { get; set; }

        [JsonProperty("preferredLanguage")]
        public string? PreferredLanguage { get; set; }

        [JsonProperty("surname")]
        public string? Surname { get; set; }

        [JsonProperty("userPrincipalName")]
        public string? UserPrincipalName { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }
        [JsonProperty("EmployeeId")]
        public string? EmployeeId { get; set; }
    }
}
