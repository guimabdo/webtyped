using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WebTyped.Example.Web.Models;

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

		[Route("route/{str}")]
		public string GetThisStringFromRoute([FromRoute]string str) {
			return str;
		}

		[Route("these/{str}")]
		public IEnumerable<string> GetTheseStrings([FromRoute]string str, [FromQuery]string str2) {
			return new List<string> { str, str2 } ;
		}

		[HttpPost]
		public string PostAndReturnThisStringFromQuery(string str) {
			return str;
		}

		[HttpPost("explicit")]
		public string PostAndReturnThisStringFromQueryExplicit([FromQuery]string str) {
			return str;
		}

		[HttpPost("{str}")]
		public string PostAndReturnThisStringFromRoute([FromRoute]string str) {
			return str;
		}

		[HttpPost("these/{str}")]
		public IEnumerable<string> PostAndReturnTheseStrings([FromRoute]string str, [FromQuery]string str2, [FromBody]string str3) {
			return new List<string> { str, str2, str3 };
		}

		public class Model {
			public int Number { get; set; }
			public string Text { get; set; }
		}

		[HttpPost("model")]
		public Model PostAndReturnModel([FromBody]Model model) {
			return model;
		}

		[HttpPost("modelA1")]
		public ModelA PostAndReturnModelSameName([FromBody]ModelA model) {
			return model;
		}

		[HttpPost("modelA2")]
		public WebTyped.Example.Web.OtherModels.ModelA PostAndReturnModelSameName2([FromBody]WebTyped.Example.Web.OtherModels.ModelA model) {
			return model;
		}

		//Not working yet
		[HttpPost("tuple")]
		public (string str, int number) PostAndReturnTuple_NotWorkingYet([FromBody](string str, int number) tuple) {
			return tuple;
		}

		//[HttpPost("modelA")]
		//public ModelA PostAndReturnModelA([FromBody]ModelA modelA) {
		//	return modelA;
		//}
	}
}
