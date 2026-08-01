using System.Text.Json.Serialization;

namespace CapyBooks.Infrastructure.ExternalServices.OpenLibrary;

internal sealed class OpenLibrarySearchResponse
{
    public List<OpenLibraryDoc> Docs { get; set; } = [];
}

internal sealed class OpenLibraryDoc
{
    public string? Title { get; set; }

    [JsonPropertyName("author_name")]
    public List<string>? AuthorName { get; set; }

    public List<string>? Isbn { get; set; }

    [JsonPropertyName("cover_i")]
    public int? CoverId { get; set; }

    [JsonPropertyName("first_publish_year")]
    public int? FirstPublishYear { get; set; }

    public List<string>? Subject { get; set; }

    public string? Key { get; set; }
}

internal sealed class OpenLibraryBookData
{
    public string? Title { get; set; }

    public List<OpenLibraryAuthor>? Authors { get; set; }

    [JsonPropertyName("publish_date")]
    public string? PublishDate { get; set; }

    public List<OpenLibrarySubject>? Subjects { get; set; }

    public OpenLibraryCover? Cover { get; set; }
}

internal sealed class OpenLibraryAuthor
{
    public string? Name { get; set; }
}

internal sealed class OpenLibrarySubject
{
    public string? Name { get; set; }
}

internal sealed class OpenLibraryCover
{
    public string? Large { get; set; }

    public string? Medium { get; set; }
}
