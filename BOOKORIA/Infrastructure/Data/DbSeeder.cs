using BOOKORIA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(BookoriaDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Books.AnyAsync(cancellationToken))
        {
            return;
        }

        var literatureCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Văn học"
        };

        var businessCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Kinh tế"
        };

        var book1 = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Nhà giả kim",
            Author = "Paulo Coelho",
            Isbn = "9780061122415",
            PriceEbook = 59000,
            PricePrint = 129000,
            Stock = 100,
            Description = "Một hành trình theo đuổi vận mệnh cá nhân.",
            CoverUrl = "https://example.com/covers/nha-gia-kim.jpg",
            FullPdfUrl = "https://example.com/ebooks/nha-gia-kim.pdf",
            SamplePdfUrl = "https://example.com/samples/nha-gia-kim-sample.pdf",
            IsActive = true
        };

        var book2 = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Dạy con làm giàu",
            Author = "Robert T. Kiyosaki",
            Isbn = "9781612680194",
            PriceEbook = 69000,
            PricePrint = 149000,
            Stock = 80,
            Description = "Kiến thức nền tảng về tài chính cá nhân.",
            CoverUrl = "https://example.com/covers/day-con-lam-giau.jpg",
            FullPdfUrl = "https://example.com/ebooks/day-con-lam-giau.pdf",
            SamplePdfUrl = "https://example.com/samples/day-con-lam-giau-sample.pdf",
            IsActive = true
        };

        var book3 = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Tuổi trẻ đáng giá bao nhiêu",
            Author = "Rosie Nguyễn",
            Isbn = "9786047729029",
            PriceEbook = 49000,
            PricePrint = 99000,
            Stock = 120,
            Description = "Góc nhìn thực tế về học tập, trải nghiệm và phát triển bản thân.",
            CoverUrl = "https://example.com/covers/tuoi-tre-dang-gia-bao-nhieu.jpg",
            FullPdfUrl = "https://example.com/ebooks/tuoi-tre-dang-gia-bao-nhieu.pdf",
            SamplePdfUrl = "https://example.com/samples/tuoi-tre-dang-gia-bao-nhieu-sample.pdf",
            IsActive = true
        };

        dbContext.Categories.AddRange(literatureCategory, businessCategory);
        dbContext.Books.AddRange(book1, book2, book3);

        dbContext.BookCategories.AddRange(
            new BookCategory
            {
                BookId = book1.Id,
                CategoryId = literatureCategory.Id
            },
            new BookCategory
            {
                BookId = book2.Id,
                CategoryId = businessCategory.Id
            },
            new BookCategory
            {
                BookId = book3.Id,
                CategoryId = literatureCategory.Id
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
