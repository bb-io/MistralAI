using Apps.MistralAI.DataSourceHandlers.Extensions;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.MistralAI.DataSourceHandlers.Static
{
    public class TopPDataSourceHandler : IDataSourceItemHandler
    {
        public IEnumerable<DataSourceItem> GetData(DataSourceContext context)
        {
            return DataSourceHandlersExtensions.GenerateFormattedFloatArray(0.0f, 1.0f, 0.1f)
                .Select(t => new DataSourceItem(t, t));
        }
    }
}
