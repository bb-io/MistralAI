using Apps.MistralAI.Actions;
using Apps.MistralAI.Models.Requests;
using Blackbird.Applications.Sdk.Common.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.MistralAI.Base;

namespace Tests.MistralAI
{
    [TestClass]
    public class XliffTests :TestBase
    {
        [TestMethod]
        public async Task PostEditXliff_IssSuccess()
        {
            var action = new XliffActions(InvocationContext, FileManager);


            var response = await action.PostEditXliffAsync(
                new ProcessXliffRequest {Model = "ministral-3b-latest",
                File = new FileReference { Name = "677627238_PostXliff.xliff" }});

            Assert.IsNotNull(response);
        }
    }
}
