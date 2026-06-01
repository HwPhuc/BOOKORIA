CREATE TABLE [Books]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Title] NVARCHAR(300) NOT NULL,
    [Author] NVARCHAR(200) NOT NULL,
    [Isbn] NVARCHAR(30) NULL,
    [PriceEbook] DECIMAL(18,2) NOT NULL,
    [PricePrint] DECIMAL(18,2) NOT NULL,
    [Stock] INT NOT NULL,
    [Description] NVARCHAR(3000) NOT NULL,
    [CoverUrl] NVARCHAR(1000) NULL,
    [FullPdfUrl] NVARCHAR(1000) NULL,
    [SamplePdfUrl] NVARCHAR(1000) NULL,
    [IsActive] BIT NOT NULL,
    [RowVersion] ROWVERSION NOT NULL
);

CREATE TABLE [Categories]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(120) NOT NULL
);

CREATE TABLE [BookCategories]
(
    [BookId] UNIQUEIDENTIFIER NOT NULL,
    [CategoryId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_BookCategories] PRIMARY KEY ([BookId], [CategoryId]),
    CONSTRAINT [FK_BookCategories_Books] FOREIGN KEY ([BookId]) REFERENCES [Books]([Id]),
    CONSTRAINT [FK_BookCategories_Categories] FOREIGN KEY ([CategoryId]) REFERENCES [Categories]([Id])
);

CREATE TABLE [Orders]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [UserId] NVARCHAR(450) NOT NULL,
    [OrderType] INT NOT NULL,
    [TotalAmount] DECIMAL(18,2) NOT NULL,
    [Status] INT NOT NULL,
    [PaymentStatus] INT NOT NULL,
    [CreatedAtUtc] DATETIME2 NOT NULL,
    [RowVersion] ROWVERSION NOT NULL
);

CREATE TABLE [OrderItems]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [OrderId] UNIQUEIDENTIFIER NOT NULL,
    [BookId] UNIQUEIDENTIFIER NOT NULL,
    [Quantity] INT NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    [ItemType] NVARCHAR(50) NOT NULL,
    CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id]),
    CONSTRAINT [FK_OrderItems_Books] FOREIGN KEY ([BookId]) REFERENCES [Books]([Id])
);

CREATE TABLE [Payments]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [OrderId] UNIQUEIDENTIFIER NOT NULL,
    [StripeSessionId] NVARCHAR(200) NULL,
    [StripePaymentIntentId] NVARCHAR(200) NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [Status] INT NOT NULL,
    [CreatedAtUtc] DATETIME2 NOT NULL,
    CONSTRAINT [FK_Payments_Orders] FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id])
);

CREATE UNIQUE INDEX [IX_Payments_StripeSessionId]
ON [Payments]([StripeSessionId])
WHERE [StripeSessionId] IS NOT NULL;

CREATE TABLE [EbookDeliveries]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [OrderId] UNIQUEIDENTIFIER NOT NULL,
    [EmailTo] NVARCHAR(320) NOT NULL,
    [SentAtUtc] DATETIME2 NULL,
    [DownloadToken] NVARCHAR(120) NOT NULL,
    [ExpiredAtUtc] DATETIME2 NOT NULL,
    CONSTRAINT [FK_EbookDeliveries_Orders] FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id])
);

CREATE UNIQUE INDEX [IX_EbookDeliveries_DownloadToken]
ON [EbookDeliveries]([DownloadToken]);

CREATE TABLE [Shipments]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [OrderId] UNIQUEIDENTIFIER NOT NULL,
    [Carrier] NVARCHAR(120) NULL,
    [TrackingCode] NVARCHAR(120) NULL,
    [ShippingStatus] INT NOT NULL,
    [LastUpdatedAtUtc] DATETIME2 NOT NULL,
    [RowVersion] ROWVERSION NOT NULL,
    CONSTRAINT [FK_Shipments_Orders] FOREIGN KEY ([OrderId]) REFERENCES [Orders]([Id])
);

CREATE TABLE [ShipmentTrackings]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ShipmentId] UNIQUEIDENTIFIER NOT NULL,
    [Status] INT NOT NULL,
    [Note] NVARCHAR(500) NULL,
    [TimestampUtc] DATETIME2 NOT NULL,
    CONSTRAINT [FK_ShipmentTrackings_Shipments] FOREIGN KEY ([ShipmentId]) REFERENCES [Shipments]([Id])
);
