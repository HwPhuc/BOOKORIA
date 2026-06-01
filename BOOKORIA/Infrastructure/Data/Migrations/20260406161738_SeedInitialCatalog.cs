using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BOOKORIA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "CoverUrl", "Description", "FullPdfUrl", "IsActive", "Isbn", "PriceEbook", "PricePrint", "SamplePdfUrl", "Stock", "Title" },
                values: new object[,]
                {
                    { new Guid("a4bf3f79-e652-4ea0-9097-f39f6b6445d6"), "Paulo Coelho", "https://example.com/covers/nha-gia-kim.jpg", "Một hành trình theo đuổi vận mệnh cá nhân.", "https://example.com/ebooks/nha-gia-kim.pdf", true, "9780061122415", 59000m, 129000m, "https://example.com/samples/nha-gia-kim-sample.pdf", 100, "Nhà giả kim" },
                    { new Guid("b2f4c8f8-dab6-47ee-a4d5-c286f8ad95f6"), "Rosie Nguyễn", "https://example.com/covers/tuoi-tre-dang-gia-bao-nhieu.jpg", "Góc nhìn thực tế về học tập, trải nghiệm và phát triển bản thân.", "https://example.com/ebooks/tuoi-tre-dang-gia-bao-nhieu.pdf", true, "9786047729029", 49000m, 99000m, "https://example.com/samples/tuoi-tre-dang-gia-bao-nhieu-sample.pdf", 120, "Tuổi trẻ đáng giá bao nhiêu" },
                    { new Guid("e95fec82-f792-4b79-b74d-bfc95f556774"), "Robert T. Kiyosaki", "https://example.com/covers/day-con-lam-giau.jpg", "Kiến thức nền tảng về tài chính cá nhân.", "https://example.com/ebooks/day-con-lam-giau.pdf", true, "9781612680194", 69000m, 149000m, "https://example.com/samples/day-con-lam-giau-sample.pdf", 80, "Dạy con làm giàu" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("3f8cda9f-f848-4f1d-95ec-c1de54f8f1d9"), "Văn học" },
                    { new Guid("f8e06f4a-b00a-451b-a1ee-c4e69bc32ba2"), "Kinh tế" }
                });

            migrationBuilder.InsertData(
                table: "BookCategories",
                columns: new[] { "BookId", "CategoryId" },
                values: new object[,]
                {
                    { new Guid("a4bf3f79-e652-4ea0-9097-f39f6b6445d6"), new Guid("3f8cda9f-f848-4f1d-95ec-c1de54f8f1d9") },
                    { new Guid("b2f4c8f8-dab6-47ee-a4d5-c286f8ad95f6"), new Guid("3f8cda9f-f848-4f1d-95ec-c1de54f8f1d9") },
                    { new Guid("e95fec82-f792-4b79-b74d-bfc95f556774"), new Guid("f8e06f4a-b00a-451b-a1ee-c4e69bc32ba2") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BookCategories",
                keyColumns: new[] { "BookId", "CategoryId" },
                keyValues: new object[] { new Guid("a4bf3f79-e652-4ea0-9097-f39f6b6445d6"), new Guid("3f8cda9f-f848-4f1d-95ec-c1de54f8f1d9") });

            migrationBuilder.DeleteData(
                table: "BookCategories",
                keyColumns: new[] { "BookId", "CategoryId" },
                keyValues: new object[] { new Guid("b2f4c8f8-dab6-47ee-a4d5-c286f8ad95f6"), new Guid("3f8cda9f-f848-4f1d-95ec-c1de54f8f1d9") });

            migrationBuilder.DeleteData(
                table: "BookCategories",
                keyColumns: new[] { "BookId", "CategoryId" },
                keyValues: new object[] { new Guid("e95fec82-f792-4b79-b74d-bfc95f556774"), new Guid("f8e06f4a-b00a-451b-a1ee-c4e69bc32ba2") });

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: new Guid("a4bf3f79-e652-4ea0-9097-f39f6b6445d6"));

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: new Guid("b2f4c8f8-dab6-47ee-a4d5-c286f8ad95f6"));

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "Id",
                keyValue: new Guid("e95fec82-f792-4b79-b74d-bfc95f556774"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("3f8cda9f-f848-4f1d-95ec-c1de54f8f1d9"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f8e06f4a-b00a-451b-a1ee-c4e69bc32ba2"));
        }
    }
}
