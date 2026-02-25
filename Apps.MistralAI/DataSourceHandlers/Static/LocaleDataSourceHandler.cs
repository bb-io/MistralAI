using Blackbird.Applications.Sdk.Common.Dynamic;
using System.Globalization;

namespace Apps.MistralAI.DataSourceHandlers.Static;

public class LocaleDataSourceHandler : IDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData(DataSourceContext context)
    {
        return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Where(x => string.IsNullOrEmpty(context.SearchString) || x.DisplayName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(c => new DataSourceItem(c.Name, c.DisplayName));
    }
}