using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ComplexTypeQuery {
    [Route("api/[controller]")]
    public class MyController
    {
        [HttpGet]
        public IQueryable<object> Query([FromQuery]ComplexType obj = null)
        {
            return null;
        }
    }

    public class ComplexType
    {
        [FromQuery(Name = "$skip")]
        public int? Skip { get; set; }

        [FromQuery(Name = "$take")]
        public int? Take { get; set; }

        [FromQuery(Name = "$orderby")]
        public string OrderBy { get; set; }
    }
}
