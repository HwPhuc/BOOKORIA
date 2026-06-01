using BOOKORIA.Domain.Entities;
using BOOKORIA.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Infrastructure.Data;

public class BookoriaDbContext(DbContextOptions<BookoriaDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    private static readonly Guid LiteratureCategoryId = Guid.Parse("3f8cda9f-f848-4f1d-95ec-c1de54f8f1d9");
    private static readonly Guid BusinessCategoryId = Guid.Parse("f8e06f4a-b00a-451b-a1ee-c4e69bc32ba2");

    private static readonly Guid NhaGiaKimBookId = Guid.Parse("a4bf3f79-e652-4ea0-9097-f39f6b6445d6");
    private static readonly Guid DayConLamGiauBookId = Guid.Parse("e95fec82-f792-4b79-b74d-bfc95f556774");
    private static readonly Guid TuoiTreDangGiaBaoNhieuBookId = Guid.Parse("b2f4c8f8-dab6-47ee-a4d5-c286f8ad95f6");

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<BookCategory> BookCategories => Set<BookCategory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<EbookDelivery> EbookDeliveries => Set<EbookDelivery>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentTracking> ShipmentTrackings => Set<ShipmentTracking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(x => x.PriceEbook).HasPrecision(18, 2);
            entity.Property(x => x.PricePrint).HasPrecision(18, 2);
            entity.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<BookCategory>(entity =>
        {
            entity.HasKey(x => new { x.BookId, x.CategoryId });
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Payment>()
            .HasIndex(x => x.StripeSessionId)
            .IsUnique()
            .HasFilter("[StripeSessionId] IS NOT NULL");

        modelBuilder.Entity<EbookDelivery>()
            .HasIndex(x => x.DownloadToken)
            .IsUnique();

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = LiteratureCategoryId,
                Name = "Văn học"
            },
            new Category
            {
                Id = BusinessCategoryId,
                Name = "Kinh tế"
            });

        modelBuilder.Entity<Book>().HasData(
            new Book
            {
                Id = NhaGiaKimBookId,
                Title = "Nhà giả kim",
                Author = "Paulo Coelho",
                Isbn = "9780061122415",
                PriceEbook = 59000m,
                PricePrint = 129000m,
                Stock = 100,
                Description = "Một hành trình theo đuổi vận mệnh cá nhân.",
                CoverUrl = "https://example.com/covers/nha-gia-kim.jpg",
                FullPdfUrl = "https://example.com/ebooks/nha-gia-kim.pdf",
                SamplePdfUrl = "https://example.com/samples/nha-gia-kim-sample.pdf",
                IsActive = true
            },
            new Book
            {
                Id = DayConLamGiauBookId,
                Title = "Dạy con làm giàu",
                Author = "Robert T. Kiyosaki",
                Isbn = "9781612680194",
                PriceEbook = 69000m,
                PricePrint = 149000m,
                Stock = 80,
                Description = "Kiến thức nền tảng về tài chính cá nhân.",
                CoverUrl = "https://example.com/covers/day-con-lam-giau.jpg",
                FullPdfUrl = "https://example.com/ebooks/day-con-lam-giau.pdf",
                SamplePdfUrl = "https://example.com/samples/day-con-lam-giau-sample.pdf",
                IsActive = true
            },
            new Book
            {
                Id = TuoiTreDangGiaBaoNhieuBookId,
                Title = "Tuổi trẻ đáng giá bao nhiêu",
                Author = "Rosie Nguyễn",
                Isbn = "9786047729029",
                PriceEbook = 49000m,
                PricePrint = 99000m,
                Stock = 120,
                Description = "Góc nhìn thực tế về học tập, trải nghiệm và phát triển bản thân.",
                CoverUrl = "https://example.com/covers/tuoi-tre-dang-gia-bao-nhieu.jpg",
                FullPdfUrl = "https://example.com/ebooks/tuoi-tre-dang-gia-bao-nhieu.pdf",
                SamplePdfUrl = "https://example.com/samples/tuoi-tre-dang-gia-bao-nhieu-sample.pdf",
                IsActive = true
            });

        modelBuilder.Entity<BookCategory>().HasData(
            new BookCategory
            {
                BookId = NhaGiaKimBookId,
                CategoryId = LiteratureCategoryId
            },
            new BookCategory
            {
                BookId = DayConLamGiauBookId,
                CategoryId = BusinessCategoryId
            },
            new BookCategory
            {
                BookId = TuoiTreDangGiaBaoNhieuBookId,
                CategoryId = LiteratureCategoryId
            });
    }
}
