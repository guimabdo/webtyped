using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Growinco.ProjectX.Models
{
    public class ODataModel : ODataModel<int?> { }
    public class ODataModel<TKey>
    {
        public IEnumerable<TKey> Ids { get; set; }

        public string Search { get; set; }

        [FromQuery(Name = "$skip")]
        public int? Skip { get; set; }

        [FromQuery(Name = "$top")]
        public int? Take { get; set; }

        [FromQuery(Name = "$orderby")]
        public string OrderBy { get; set; }

        [FromQuery(Name = "$select")]
        public string Select { get; set; }

        [FromQuery(Name = "$expand")]
        public string Expand { get; set; }

        [FromQuery(Name = "$count")]
        public bool? Count { get; set; }

        [FromQuery(Name = "$filter")]
        public string Filter { get; set; }
    }
}
