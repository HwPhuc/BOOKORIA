# BOOKORIA - Project structure (.NET 8)

## Đề xuất tổ chức thư mục trong project hiện tại

- `Domain/`
  - `Entities/`: thực thể nghiệp vụ (`Book`, `Order`, `Payment`, ...)
  - `Enums/`: enum nghiệp vụ (`OrderType`, `PaymentStatus`, ...)
- `Application/`
  - `Abstractions/`: interface service (`IStripeWebhookService`, `IEmailService`, ...)
- `Infrastructure/`
  - `Data/`: `BookoriaDbContext`
  - `Services/`: xử lý Stripe webhook, gửi ebook email
  - `Options/`: map cấu hình `Stripe`, `Email`
- `Controllers/`
  - API endpoint (ví dụ: `StripeWebhookController`)
- `Web/Contracts/`
  - request/response model cho API
- `Database/`
  - script SQL migration khởi tạo

## Giai đoạn tiếp theo

1. Tích hợp xác thực Stripe webhook signature bằng Stripe SDK.
2. Tích hợp SMTP/SendGrid thật cho `IEmailService`.
3. Thêm Identity + phân quyền `Admin/Customer`.
4. Thêm trang Bootstrap cho Catalog, Checkout, Admin Dashboard.
5. Thêm xuất Excel bằng `ClosedXML`.

## Cloudinary (quản lý ảnh bìa và file PDF)

- Cấu hình tại `appsettings.json` section `Cloudinary`:
  - `CloudName`, `ApiKey`, `ApiSecret`
  - `BookCoverFolder`, `BookPdfFolder`, `BookSamplePdfFolder`
- Service upload đã có:
  - Interface: `Application/Abstractions/ICloudinaryStorageService.cs`
  - Implementation: `Infrastructure/Services/CloudinaryStorageService.cs`
- Đăng ký DI trong `Program.cs`:
  - `builder.Services.AddScoped<ICloudinaryStorageService, CloudinaryStorageService>();`
