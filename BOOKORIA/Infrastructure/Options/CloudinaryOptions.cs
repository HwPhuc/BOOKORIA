namespace BOOKORIA.Infrastructure.Options;

public class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";

    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;

    public string BookCoverFolder { get; set; } = "bookoria/books/covers";
    public string BookPdfFolder { get; set; } = "bookoria/books/pdfs";
    public string BookSamplePdfFolder { get; set; } = "bookoria/books/samples";
}
