using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.MistralAI.DataSourceHandlers.Static;

public class TimestampGranularitiesSourceHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData()
    {
        return new List<DataSourceItem>
        {
            new( "word", "Word" ),
            new( "segment", "Segment"),
        };
    }
}