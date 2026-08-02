namespace CapyBooks.Infrastructure.ExternalServices.GoogleBooks;

internal sealed class GoogleBooksResponse
{
    public List<GoogleBooksItem>? Items { get; set; }
}

internal sealed class GoogleBooksItem
{
    public string? Id { get; set; }

    public GoogleBooksVolumeInfo? VolumeInfo { get; set; }
}

internal sealed class GoogleBooksVolumeInfo
{
    public string? Title { get; set; }

    public List<string>? Authors { get; set; }

    public string? Description { get; set; }

    public string? PublishedDate { get; set; }

    public List<GoogleBooksIdentifier>? IndustryIdentifiers { get; set; }

    public GoogleBooksImageLinks? ImageLinks { get; set; }

    public List<string>? Categories { get; set; }
}

internal sealed class GoogleBooksIdentifier
{
    public string? Type { get; set; }

    public string? Identifier { get; set; }
}

internal sealed class GoogleBooksImageLinks
{
    public string? Thumbnail { get; set; }
}
