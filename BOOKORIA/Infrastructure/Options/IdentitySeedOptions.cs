namespace BOOKORIA.Infrastructure.Options;

public class IdentitySeedOptions
{
    public const string SectionName = "IdentitySeed";

    public SeedUser Admin { get; set; } = new();
    public SeedUser Customer { get; set; } = new();
}

public class SeedUser
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
