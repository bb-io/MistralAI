using Apps.MistralAI.DataSourceHandlers;
using Apps.MistralAI.DataSourceHandlers.Static;
using Tests.MistralAI.Base;

namespace Tests.MistralAI
{
    [TestClass]
    public class DataHandlerTests : TestBase
    {
        [TestMethod]
        public async Task DataHandler_UploadFile_Test()
        {
            var dataHandler = new ModelsDataHandler(InvocationContext);

            var response = await dataHandler.GetDataAsync(new Blackbird.Applications.Sdk.Common.Dynamic.DataSourceContext { }, CancellationToken.None);

            foreach (var item in response)
            {
                Console.WriteLine($"{item.Key} : {item.Value}");
            }

            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task DataHandler_UploadFile_WithSearchString_Test()
        {
            var dataHandler = new ModelsDataHandler(InvocationContext);

            var response = await dataHandler.GetDataAsync(new Blackbird.Applications.Sdk.Common.Dynamic.DataSourceContext { SearchString = "voxtral" }, CancellationToken.None);

            foreach (var item in response)
            {
                Console.WriteLine($"{item.Key} : {item.Value}");
            }

            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task LocaleDataSourceHandler_IsSuccess()
        {
            var dataHandler = new LocaleDataSourceHandler();

            var response = dataHandler.GetData(new Blackbird.Applications.Sdk.Common.Dynamic.DataSourceContext { });

            foreach (var item in response)
            {
                Console.WriteLine($"{item.DisplayName} : {item.Value}");
            }

            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task LocaleDataSourceHandler_WithSearchString_IsSuccess()
        {
            var dataHandler = new LocaleDataSourceHandler();

            var response = dataHandler.GetData(new Blackbird.Applications.Sdk.Common.Dynamic.DataSourceContext { SearchString = "voxtral" });

            foreach (var item in response)
            {
                Console.WriteLine($"{item.DisplayName} : {item.Value}");
            }

            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task AudioModelsDataHandler_IsSuccess()
        {
            var dataHandler = new AudioModelsDataHandler(InvocationContext);

            var response = await dataHandler.GetDataAsync(new Blackbird.Applications.Sdk.Common.Dynamic.DataSourceContext { }, CancellationToken.None);

            foreach (var item in response)
            {
                Console.WriteLine($"{item.DisplayName} : {item.Value}");
            }

            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task AudioModelsDataHandler_WithSearchString_IsSuccess()
        {
            var dataHandler = new AudioModelsDataHandler(InvocationContext);

            var response = await dataHandler.GetDataAsync(new Blackbird.Applications.Sdk.Common.Dynamic.DataSourceContext { SearchString = "mini" }, CancellationToken.None);

            foreach (var item in response)
            {
                Console.WriteLine($"{item.DisplayName} : {item.Value}");
            }

            Assert.IsNotNull(response);
        }
    }
}