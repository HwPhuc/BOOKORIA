using System.Text.Json;
using BOOKORIA.Domain.Enums;
using BOOKORIA.Infrastructure.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BOOKORIA.Pages.Admin;

public class IndexModel(BookoriaDbContext dbContext) : PageModel
{
    public int TotalBooks { get; private set; }
    public int TotalUsers { get; private set; }
    public int TotalOrders { get; private set; }
    public decimal TotalRevenue { get; private set; }

    public string MonthlyLabelsJson { get; private set; } = "[]";
    public string MonthlyRevenueJson { get; private set; } = "[]";
    public string OrderTypeLabelsJson { get; private set; } = "[]";
    public string OrderTypeDataJson { get; private set; } = "[]";
    public string OrderStatusLabelsJson { get; private set; } = "[]";
    public string OrderStatusDataJson { get; private set; } = "[]";

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadDashboardDataAsync(cancellationToken);
    }

    public async Task<IActionResult> OnGetExportExcelAsync(CancellationToken cancellationToken)
    {
        var data = await BuildExtendedReportDataAsync(cancellationToken);

        using var workbook = new XLWorkbook();

        var summarySheet = workbook.Worksheets.Add("TongQuan");
        summarySheet.Cell(1, 1).Value = "Chi so";
        summarySheet.Cell(1, 2).Value = "Gia tri";
        summarySheet.Cell(2, 1).Value = "Tong so sach";
        summarySheet.Cell(2, 2).Value = data.TotalBooks;
        summarySheet.Cell(3, 1).Value = "Tong so nguoi dung";
        summarySheet.Cell(3, 2).Value = data.TotalUsers;
        summarySheet.Cell(4, 1).Value = "Tong so don hang";
        summarySheet.Cell(4, 2).Value = data.TotalOrders;
        summarySheet.Cell(5, 1).Value = "Tong doanh thu da thanh toan";
        summarySheet.Cell(5, 2).Value = data.TotalRevenue;
        summarySheet.Cell(5, 2).Style.NumberFormat.Format = "#,##0";
        summarySheet.Columns().AdjustToContents();

        var monthlySheet = workbook.Worksheets.Add("DoanhThuTheoThang");
        monthlySheet.Cell(1, 1).Value = "Thang";
        monthlySheet.Cell(1, 2).Value = "Doanh thu";
        for (var i = 0; i < data.MonthlyRevenue.Count; i++)
        {
            monthlySheet.Cell(i + 2, 1).Value = data.MonthlyRevenue[i].Label;
            monthlySheet.Cell(i + 2, 2).Value = data.MonthlyRevenue[i].Revenue;
            monthlySheet.Cell(i + 2, 2).Style.NumberFormat.Format = "#,##0";
        }
        monthlySheet.Columns().AdjustToContents();

        var orderTypeSheet = workbook.Worksheets.Add("DonHangTheoLoai");
        orderTypeSheet.Cell(1, 1).Value = "Loai don";
        orderTypeSheet.Cell(1, 2).Value = "So luong";
        var orderTypeRows = 2;
        foreach (var item in data.OrderTypeCounts)
        {
            orderTypeSheet.Cell(orderTypeRows, 1).Value = item.Label;
            orderTypeSheet.Cell(orderTypeRows, 2).Value = item.Count;
            orderTypeRows++;
        }
        orderTypeSheet.Columns().AdjustToContents();

        var orderStatusSheet = workbook.Worksheets.Add("TrangThaiDonHang");
        orderStatusSheet.Cell(1, 1).Value = "Trang thai";
        orderStatusSheet.Cell(1, 2).Value = "So luong";
        var orderStatusRows = 2;
        foreach (var item in data.OrderStatusCounts)
        {
            orderStatusSheet.Cell(orderStatusRows, 1).Value = item.Label;
            orderStatusSheet.Cell(orderStatusRows, 2).Value = item.Count;
            orderStatusRows++;
        }
        orderStatusSheet.Columns().AdjustToContents();

        var paymentStatusSheet = workbook.Worksheets.Add("TrangThaiThanhToan");
        paymentStatusSheet.Cell(1, 1).Value = "Trang thai thanh toan";
        paymentStatusSheet.Cell(1, 2).Value = "So luong";
        var paymentRows = 2;
        foreach (var item in data.PaymentStatusCounts)
        {
            paymentStatusSheet.Cell(paymentRows, 1).Value = item.Label;
            paymentStatusSheet.Cell(paymentRows, 2).Value = item.Count;
            paymentRows++;
        }
        paymentStatusSheet.Columns().AdjustToContents();

        var shippingStatusSheet = workbook.Worksheets.Add("TrangThaiVanChuyen");
        shippingStatusSheet.Cell(1, 1).Value = "Trang thai van chuyen";
        shippingStatusSheet.Cell(1, 2).Value = "So luong";
        var shippingRows = 2;
        foreach (var item in data.ShippingStatusCounts)
        {
            shippingStatusSheet.Cell(shippingRows, 1).Value = item.Label;
            shippingStatusSheet.Cell(shippingRows, 2).Value = item.Count;
            shippingRows++;
        }
        shippingStatusSheet.Columns().AdjustToContents();

        var topBooksSheet = workbook.Worksheets.Add("TopSachBanChay");
        topBooksSheet.Cell(1, 1).Value = "Ten sach";
        topBooksSheet.Cell(1, 2).Value = "So luong";
        topBooksSheet.Cell(1, 3).Value = "Doanh thu";
        for (var i = 0; i < data.TopBooks.Count; i++)
        {
            topBooksSheet.Cell(i + 2, 1).Value = data.TopBooks[i].Title;
            topBooksSheet.Cell(i + 2, 2).Value = data.TopBooks[i].Quantity;
            topBooksSheet.Cell(i + 2, 3).Value = data.TopBooks[i].Revenue;
            topBooksSheet.Cell(i + 2, 3).Style.NumberFormat.Format = "#,##0";
        }
        topBooksSheet.Columns().AdjustToContents();

        var topCustomersSheet = workbook.Worksheets.Add("TopKhachHang");
        topCustomersSheet.Cell(1, 1).Value = "Email";
        topCustomersSheet.Cell(1, 2).Value = "So don thanh cong";
        topCustomersSheet.Cell(1, 3).Value = "Tong chi tieu";
        topCustomersSheet.Cell(1, 4).Value = "Lan mua cuoi";
        for (var i = 0; i < data.TopCustomers.Count; i++)
        {
            topCustomersSheet.Cell(i + 2, 1).Value = data.TopCustomers[i].Email;
            topCustomersSheet.Cell(i + 2, 2).Value = data.TopCustomers[i].OrderCount;
            topCustomersSheet.Cell(i + 2, 3).Value = data.TopCustomers[i].Revenue;
            topCustomersSheet.Cell(i + 2, 3).Style.NumberFormat.Format = "#,##0";
            topCustomersSheet.Cell(i + 2, 4).Value = data.TopCustomers[i].LastOrderAtUtc;
            topCustomersSheet.Cell(i + 2, 4).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
        }
        topCustomersSheet.Columns().AdjustToContents();

        var ordersSheet = workbook.Worksheets.Add("ChiTietDonHang");
        ordersSheet.Cell(1, 1).Value = "Ma don";
        ordersSheet.Cell(1, 2).Value = "Ngay tao";
        ordersSheet.Cell(1, 3).Value = "Khach hang";
        ordersSheet.Cell(1, 4).Value = "Loai don";
        ordersSheet.Cell(1, 5).Value = "Trang thai don";
        ordersSheet.Cell(1, 6).Value = "Thanh toan";
        ordersSheet.Cell(1, 7).Value = "Van chuyen";
        ordersSheet.Cell(1, 8).Value = "Ma van don";
        ordersSheet.Cell(1, 9).Value = "Tong tien";
        for (var i = 0; i < data.RecentOrders.Count; i++)
        {
            ordersSheet.Cell(i + 2, 1).Value = data.RecentOrders[i].OrderId.ToString();
            ordersSheet.Cell(i + 2, 2).Value = data.RecentOrders[i].CreatedAtUtc;
            ordersSheet.Cell(i + 2, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            ordersSheet.Cell(i + 2, 3).Value = data.RecentOrders[i].Customer;
            ordersSheet.Cell(i + 2, 4).Value = data.RecentOrders[i].OrderType;
            ordersSheet.Cell(i + 2, 5).Value = data.RecentOrders[i].OrderStatus;
            ordersSheet.Cell(i + 2, 6).Value = data.RecentOrders[i].PaymentStatus;
            ordersSheet.Cell(i + 2, 7).Value = data.RecentOrders[i].ShippingStatus;
            ordersSheet.Cell(i + 2, 8).Value = data.RecentOrders[i].TrackingCode;
            ordersSheet.Cell(i + 2, 9).Value = data.RecentOrders[i].TotalAmount;
            ordersSheet.Cell(i + 2, 9).Style.NumberFormat.Format = "#,##0";
        }
        ordersSheet.Columns().AdjustToContents();

        var inventorySheet = workbook.Worksheets.Add("TonKhoSach");
        inventorySheet.Cell(1, 1).Value = "Ten sach";
        inventorySheet.Cell(1, 2).Value = "Tac gia";
        inventorySheet.Cell(1, 3).Value = "Ebook";
        inventorySheet.Cell(1, 4).Value = "Sach giay";
        inventorySheet.Cell(1, 5).Value = "Ton kho";
        inventorySheet.Cell(1, 6).Value = "Trang thai";
        for (var i = 0; i < data.BookStocks.Count; i++)
        {
            inventorySheet.Cell(i + 2, 1).Value = data.BookStocks[i].Title;
            inventorySheet.Cell(i + 2, 2).Value = data.BookStocks[i].Author;
            inventorySheet.Cell(i + 2, 3).Value = data.BookStocks[i].PriceEbook;
            inventorySheet.Cell(i + 2, 3).Style.NumberFormat.Format = "#,##0";
            inventorySheet.Cell(i + 2, 4).Value = data.BookStocks[i].PricePrint;
            inventorySheet.Cell(i + 2, 4).Style.NumberFormat.Format = "#,##0";
            inventorySheet.Cell(i + 2, 5).Value = data.BookStocks[i].Stock;
            inventorySheet.Cell(i + 2, 6).Value = data.BookStocks[i].IsActive ? "Active" : "Inactive";
        }
        inventorySheet.Columns().AdjustToContents();

        await using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var fileName = $"bookoria-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.xlsx";
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    private async Task LoadDashboardDataAsync(CancellationToken cancellationToken)
    {
        var data = await BuildReportDataAsync(cancellationToken);

        TotalBooks = data.TotalBooks;
        TotalUsers = data.TotalUsers;
        TotalOrders = data.TotalOrders;
        TotalRevenue = data.TotalRevenue;

        MonthlyLabelsJson = JsonSerializer.Serialize(data.MonthlyRevenue.Select(x => x.Label).ToList());
        MonthlyRevenueJson = JsonSerializer.Serialize(data.MonthlyRevenue.Select(x => x.Revenue).ToList());
        OrderTypeLabelsJson = JsonSerializer.Serialize(data.OrderTypeCounts.Select(x => x.Label).ToList());
        OrderTypeDataJson = JsonSerializer.Serialize(data.OrderTypeCounts.Select(x => x.Count).ToList());
        OrderStatusLabelsJson = JsonSerializer.Serialize(data.OrderStatusCounts.Select(x => x.Label).ToList());
        OrderStatusDataJson = JsonSerializer.Serialize(data.OrderStatusCounts.Select(x => x.Count).ToList());
    }

    private async Task<DashboardReportData> BuildReportDataAsync(CancellationToken cancellationToken)
    {
        var totalBooks = await dbContext.Books.AsNoTracking().CountAsync(cancellationToken);
        var totalUsers = await dbContext.Users.AsNoTracking().CountAsync(cancellationToken);
        var totalOrders = await dbContext.Orders.AsNoTracking().CountAsync(cancellationToken);
        var totalRevenue = await dbContext.Orders
            .AsNoTracking()
            .Where(x => x.PaymentStatus == PaymentStatus.Succeeded)
            .SumAsync(x => (decimal?)x.TotalAmount, cancellationToken) ?? 0m;

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-5);
        var paidOrdersByMonth = await dbContext.Orders
            .AsNoTracking()
            .Where(x => x.PaymentStatus == PaymentStatus.Succeeded && x.CreatedAtUtc >= monthStart)
            .Select(x => new { x.CreatedAtUtc, x.TotalAmount })
            .ToListAsync(cancellationToken);

        var monthRevenueMap = paidOrdersByMonth
            .GroupBy(x => new { x.CreatedAtUtc.Year, x.CreatedAtUtc.Month })
            .ToDictionary(
                x => (x.Key.Year, x.Key.Month),
                x => x.Sum(v => v.TotalAmount));

        var monthlyRevenue = Enumerable.Range(0, 6)
            .Select(offset => monthStart.AddMonths(offset))
            .Select(month => new MonthlyRevenueRow(
                month.ToString("MM/yyyy"),
                monthRevenueMap.TryGetValue((month.Year, month.Month), out var revenue) ? revenue : 0m))
            .ToList();

        var orderTypeCountsRaw = await dbContext.Orders
            .AsNoTracking()
            .GroupBy(x => x.OrderType)
            .Select(x => new { x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var orderTypeCounts = Enum.GetValues<OrderType>()
            .Select(type => new ChartCountRow(
                type == OrderType.PhysicalBook ? "Sach giay" : "Ebook",
                orderTypeCountsRaw.FirstOrDefault(x => x.Key == type)?.Count ?? 0))
            .ToList();

        var orderStatusCountsRaw = await dbContext.Orders
            .AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(x => new { x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var orderStatusCounts = Enum.GetValues<OrderStatus>()
            .Select(status => new ChartCountRow(
                status.ToString(),
                orderStatusCountsRaw.FirstOrDefault(x => x.Key == status)?.Count ?? 0))
            .ToList();

        var topBooksRaw = await dbContext.OrderItems
            .AsNoTracking()
            .Where(x => x.Order.PaymentStatus == PaymentStatus.Succeeded)
            .GroupBy(x => new { x.BookId, x.Book.Title })
            .Select(x => new
            {
                x.Key.Title,
                Quantity = x.Sum(v => v.Quantity),
                Revenue = x.Sum(v => v.UnitPrice * v.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(10)
            .ToListAsync(cancellationToken);

        var topBooks = topBooksRaw
            .Select(x => new TopBookRow(x.Title, x.Quantity, x.Revenue))
            .ToList();

        return new DashboardReportData(
            totalBooks,
            totalUsers,
            totalOrders,
            totalRevenue,
            monthlyRevenue,
            orderTypeCounts,
            orderStatusCounts,
            topBooks);
    }

    private async Task<ExtendedReportData> BuildExtendedReportDataAsync(CancellationToken cancellationToken)
    {
        var dashboardData = await BuildReportDataAsync(cancellationToken);

        var paymentStatusRaw = await dbContext.Orders
            .AsNoTracking()
            .GroupBy(x => x.PaymentStatus)
            .Select(x => new { x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var paymentStatusCounts = Enum.GetValues<PaymentStatus>()
            .Select(status => new ChartCountRow(
                status.ToString(),
                paymentStatusRaw.FirstOrDefault(x => x.Key == status)?.Count ?? 0))
            .ToList();

        var shippingStatusRaw = await dbContext.Shipments
            .AsNoTracking()
            .GroupBy(x => x.ShippingStatus)
            .Select(x => new { x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var shippingStatusCounts = Enum.GetValues<ShippingStatus>()
            .Select(status => new ChartCountRow(
                status.ToString(),
                shippingStatusRaw.FirstOrDefault(x => x.Key == status)?.Count ?? 0))
            .ToList();

        var topCustomersRaw = await dbContext.Orders
            .AsNoTracking()
            .Where(x => x.PaymentStatus == PaymentStatus.Succeeded)
            .GroupBy(x => x.UserId)
            .Select(x => new
            {
                UserId = x.Key,
                OrderCount = x.Count(),
                Revenue = x.Sum(v => v.TotalAmount),
                LastOrderAtUtc = x.Max(v => v.CreatedAtUtc)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToListAsync(cancellationToken);

        var topCustomerUserIds = topCustomersRaw.Select(x => x.UserId).ToList();
        var customerEmails = await dbContext.Users
            .AsNoTracking()
            .Where(x => topCustomerUserIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Email })
            .ToDictionaryAsync(x => x.Id, x => x.Email, cancellationToken);

        var topCustomers = topCustomersRaw
            .Select(x => new TopCustomerRow(
                customerEmails.TryGetValue(x.UserId, out var email) ? email ?? x.UserId : x.UserId,
                x.OrderCount,
                x.Revenue,
                x.LastOrderAtUtc))
            .ToList();

        var recentOrders = await dbContext.Orders
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(300)
            .Select(x => new RecentOrderRow(
                x.Id,
                x.CreatedAtUtc,
                dbContext.Users
                    .Where(u => u.Id == x.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefault() ?? x.UserId,
                x.OrderType.ToString(),
                x.Status.ToString(),
                x.PaymentStatus.ToString(),
                x.Shipment != null ? x.Shipment.ShippingStatus.ToString() : "-",
                x.Shipment != null ? x.Shipment.TrackingCode : null,
                x.TotalAmount))
            .ToListAsync(cancellationToken);

        var bookStocks = await dbContext.Books
            .AsNoTracking()
            .OrderBy(x => x.Title)
            .Select(x => new BookStockRow(
                x.Title,
                x.Author,
                x.PriceEbook,
                x.PricePrint,
                x.Stock,
                x.IsActive))
            .ToListAsync(cancellationToken);

        return new ExtendedReportData(
            dashboardData,
            paymentStatusCounts,
            shippingStatusCounts,
            topCustomers,
            recentOrders,
            bookStocks);
    }

    private sealed record DashboardReportData(
        int TotalBooks,
        int TotalUsers,
        int TotalOrders,
        decimal TotalRevenue,
        IReadOnlyList<MonthlyRevenueRow> MonthlyRevenue,
        IReadOnlyList<ChartCountRow> OrderTypeCounts,
        IReadOnlyList<ChartCountRow> OrderStatusCounts,
        IReadOnlyList<TopBookRow> TopBooks);

    private sealed record MonthlyRevenueRow(string Label, decimal Revenue);

    private sealed record ChartCountRow(string Label, int Count);

    private sealed record TopBookRow(string Title, int Quantity, decimal Revenue);

    private sealed record ExtendedReportData(
        DashboardReportData DashboardData,
        IReadOnlyList<ChartCountRow> PaymentStatusCounts,
        IReadOnlyList<ChartCountRow> ShippingStatusCounts,
        IReadOnlyList<TopCustomerRow> TopCustomers,
        IReadOnlyList<RecentOrderRow> RecentOrders,
        IReadOnlyList<BookStockRow> BookStocks)
    {
        public int TotalBooks => DashboardData.TotalBooks;
        public int TotalUsers => DashboardData.TotalUsers;
        public int TotalOrders => DashboardData.TotalOrders;
        public decimal TotalRevenue => DashboardData.TotalRevenue;
        public IReadOnlyList<MonthlyRevenueRow> MonthlyRevenue => DashboardData.MonthlyRevenue;
        public IReadOnlyList<ChartCountRow> OrderTypeCounts => DashboardData.OrderTypeCounts;
        public IReadOnlyList<ChartCountRow> OrderStatusCounts => DashboardData.OrderStatusCounts;
        public IReadOnlyList<TopBookRow> TopBooks => DashboardData.TopBooks;
    }

    private sealed record TopCustomerRow(string Email, int OrderCount, decimal Revenue, DateTime LastOrderAtUtc);

    private sealed record RecentOrderRow(
        Guid OrderId,
        DateTime CreatedAtUtc,
        string Customer,
        string OrderType,
        string OrderStatus,
        string PaymentStatus,
        string ShippingStatus,
        string? TrackingCode,
        decimal TotalAmount);

    private sealed record BookStockRow(
        string Title,
        string Author,
        decimal PriceEbook,
        decimal PricePrint,
        int Stock,
        bool IsActive);
}
