using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebTyped.Tests {
	[TestClass]
	public class ExtrernalAssemblyTest
    {
    
		[TestMethod]
		public async Task GenerateFromAssemblyTest() {
            var options = new Options(".\\")
            {
                GenericReturnType = new  ClientType {
                    Name = "Observable",
                    Module = "rxjs"
                },
                CustomMap = new Dictionary<string, ClientType> {
                    {
                        "ExternalLib.Models2.ExternalModel2",
                        new ClientType { Module ="out", Name = "MissingTypeFromSomewhere" } }
                }
            };
            
            //var options = new Options(".\\", 
            //    false, 
            //    //ServiceMode.Angular, 
            //    new ClientType
            //    {
            //        Name = "Observable",
            //        Module = "rxjs"
            //    },
            //    new string[0], "", 
            //    false, 
            //    null,
            //    null
            //);
            var generator = new Generator(
                new string[] {
@"
using ExternalLib.Models;
using ExternalLib.Models2;
public class Test : ExternalModel {
    public int Test2 {get;set;}
    public ExternalModel2 Missing {get;set;}
}
"
                }
                ,
                new string[] {
                    @"../../../../ExternalLib\bin\Debug\netstandard2.0\ExternalLib.dll"
                },
                new Package[] {
                    new Package
                    {
                        Name = "MSTest.TestFramework",
                        Csproj = @"../../../WebTyped.Tests.csproj"
                        // Version = "1.1.18"
                    },
                     new Package
                    {
                        Name = "AutoMapper",
                        Csproj = @"../../../WebTyped.Tests.csproj"
                    }
                },
                new string[] {
                    "AutoMapper.Mapper",
                    "ExternalLib.Models.*",
                    "Cblx.*Model"
                },
                options);
            var output = await generator.GenerateOutputsAsync();
            Assert.IsTrue(output.ContainsKey("./autoMapper/mapper.ts"));
		}

        [TestMethod]
        public async Task GenerateCustomFromServiceParameterTest()
        {
            var options = new Options(".\\")
            {
                CustomMap = new Dictionary<string, ClientType> {
                    {
                        "ODataParameters",
                        new ClientType { Module ="out", Name = "ODataParameters" }
                    }
                }
            };

            var generator = new Generator(
              new string[] {
@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cblx.Backend.Models;
using Cblx.Backend.OData;
using Itz.Teia.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Itz.Teia.Web.Controllers
{
    [ApiController]
    [Route(""[controller]"")]
    [Authorize]
    public class ThingController : ControllerBase
        {
            readonly ThingBusiness b;
            public ThingController(ThingBusiness b)
            {
                this.b = b;
            }

            [HttpGet]
            [AllowAnonymous]
            public ODataResult<ThingModel> Query([FromQuery]ODataParameters query)
            {
                return b.Query(query);
            }

            [HttpPost]
            public async Task Save([FromBody]ThingSaveModel model)
            {
                await b.Save(model);
            }
        }
    }

"
              }
              ,
              new string[0],
              new Package[0],
              new string[0],
              options);
            var output = await generator.GenerateOutputsAsync();
        }

        [TestMethod]
        public async Task GenerateCustomMapTest()
        {
            var options = new Options(".\\")
            {
                GenericReturnType = new ClientType
                {
                    Name = "Observable",
                    Module = "rxjs"
                },
                CustomMap = new Dictionary<string, ClientType> {
                    {
                        "ExternalModel",
                        new ClientType { Module ="out", Name = "MissingTypeFromSomewhere" }
                    },
                    {
                        "ExternalModel2",
                        new ClientType { Module ="out", Name = "MissingTypeFromSomewhere2" } 
                    }
                }
            };

            //var options = new Options(".\\", 
            //    false, 
            //    //ServiceMode.Angular, 
            //    new ClientType
            //    {
            //        Name = "Observable",
            //        Module = "rxjs"
            //    },
            //    new string[0], "", 
            //    false, 
            //    null,
            //    null
            //);
            var generator = new Generator(
                new string[] {
@"
using ExternalLib.Models;
using ExternalLib.Models2;
public class Test : ExternalModel {
    public int Test2 {get;set;}
    public ExternalModel2 Missing {get;set;}
}
"
                }
                ,
                new string[0],
                new Package[0],
                new string[0],
                options);
            var output = await generator.GenerateOutputsAsync();
        }

        //string Read(string file) {
        //	return File.ReadAllText($"../../../Inputs/{file}");
        //}

        //async Task AssertOutput(string file) {
        //	var cs = Read($"{file}.cs");
        //	var output = await TestHelpers.Generate(cs);
        //	Assert.AreEqual(Read($"{file}-output.ts"), output.First().Value);
        //}
    }
}
