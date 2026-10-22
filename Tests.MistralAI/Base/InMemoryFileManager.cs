using System.Text;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Filters.Transformations;

namespace Tests.MistralAI.Base;

public class InMemoryFileManager : IFileManagementClient
{
    private readonly Dictionary<string, byte[]> _files = new();

    public FileReference Add(string name, string content, string? contentType = null)
    {
        _files[name] = Encoding.UTF8.GetBytes(content);
        return new FileReference { Name = name, ContentType = contentType };
    }

    public string Read(FileReference file) => Encoding.UTF8.GetString(_files[file.Name]);

    public List<Segment> Segments(FileReference file)
    {
        var loaded = Transformation.Load(new MemoryStream(_files[file.Name]), file.Name);
        Assert.IsTrue(loaded.Success, loaded.Error);
        return loaded.Value.GetUnits().SelectMany(unit => unit.Segments).ToList();
    }

    public Task<Stream> DownloadAsync(FileReference reference) =>
        Task.FromResult<Stream>(new MemoryStream(_files[reference.Name], writable: false));

    public async Task<FileReference> UploadAsync(Stream stream, string contentType, string fileName)
    {
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        _files[fileName] = buffer.ToArray();
        return new FileReference { Name = fileName, ContentType = contentType };
    }

    public static string Xliff(string sourceLanguage, string targetLanguage, params (string Source, string? Target)[] segments)
    {
        var units = segments.Select((segment, index) =>
            $"""<unit id="u{index + 1}"><segment id="s{index + 1}"{(segment.Target is null ? "" : " state=\"translated\"")}><source>{segment.Source}</source>{(segment.Target is null ? "" : $"<target>{segment.Target}</target>")}</segment></unit>""");

        return $"""<?xml version="1.0" encoding="utf-8"?><xliff xmlns="urn:oasis:names:tc:xliff:document:2.0" version="2.0" srcLang="{sourceLanguage}" trgLang="{targetLanguage}"><file id="f1" original="test.xlf">{string.Concat(units)}</file></xliff>""";
    }
}
