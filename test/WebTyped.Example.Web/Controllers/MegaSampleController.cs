using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebTyped_Example_Web.Controllers {
	[Route("api/[controller]")]
	public class MegaSampleController : Controller {
		public string GetThisStringFromQuery(string str) {
			return str;
		}

		[Route("explicit")]
		public string GetThisStringFromQueryExplicit([FromQuery]string str) {
			return str;
		}

		[Route("{str}")]
		public string GetThisStringFromRoute([FromRoute]string str) {
			return str;
		}

		[HttpPost]
		public string PostAndReturnThisStringFromQuery(string str) {
			return str;
		}
	}
}
