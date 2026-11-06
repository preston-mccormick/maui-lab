using System.Text.Json;

namespace MauiLab.Contacts;

public readonly record struct GenerateProgress(int Finished, int Total)
{
    public double Fraction => Total <= 0 ? 0 : (double)Finished / Total;
}

public sealed class CatalogFile
{
    public required string RelativePath { get; init; }

    public required byte[] Bytes { get; init; }
}

public sealed class CatalogDownload
{
    public required IReadOnlyList<Contact> Contacts { get; init; }

    public required IReadOnlyList<CatalogFile> Files { get; init; }
}

public interface IContactCatalog
{
    Task<CatalogDownload> DownloadAsync(IProgress<GenerateProgress>? progress, CancellationToken cancellationToken = default);
}

public sealed class RandomUserCatalog : IContactCatalog
{
    public const string PeopleUrl =
        "https://randomuser.me/api/1.4/?results=1000&seed=maui-lab&nat=us&inc=name,location,email,phone,picture";

    public const string PickerUrl =
        "https://randomuser.me/api/1.4/?results=12&seed=maui-lab-photos&inc=picture";

    readonly HttpClient _http;

    public RandomUserCatalog(HttpClient? http = null)
    {
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
    }

    public async Task<CatalogDownload> DownloadAsync(IProgress<GenerateProgress>? progress, CancellationToken cancellationToken = default)
    {
        progress?.Report(new GenerateProgress(0, 1));
        var peopleJson = await _http.GetStringAsync(PeopleUrl, cancellationToken);
        var contacts = ContactImporter.Import(peopleJson);
        var pickerJson = await _http.GetStringAsync(PickerUrl, cancellationToken);
        var portraits = MediumUrls(peopleJson).Distinct(StringComparer.Ordinal).ToList();
        var pickers = MediumUrls(pickerJson).ToList();
        var total = portraits.Count + pickers.Count;
        var files = new List<CatalogFile>(total);
        var finished = 0;
        foreach (var url in portraits)
            files.Add(await Download(url, "portraits/" + ContactImporter.PortraitName(url)));
        foreach (var url in pickers)
            files.Add(await Download(url, "picker/" + ContactImporter.PortraitName(url)));

        progress?.Report(new GenerateProgress(total, total));
        return new CatalogDownload { Contacts = contacts, Files = files };

        async Task<CatalogFile> Download(string url, string relative)
        {
            var bytes = await _http.GetByteArrayAsync(url, cancellationToken);
            finished++;
            progress?.Report(new GenerateProgress(finished, total));
            return new CatalogFile { RelativePath = relative, Bytes = bytes };
        }
    }

    static IEnumerable<string> MediumUrls(string json)
    {
        using var document = JsonDocument.Parse(json);
        foreach (var person in document.RootElement.GetProperty("results").EnumerateArray())
        {
            var url = person.GetProperty("picture").GetProperty("medium").GetString();
            if (!string.IsNullOrWhiteSpace(url))
                yield return url;
        }
    }
}
