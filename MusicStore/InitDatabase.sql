/*
    MusicStore: completely clears application tables and inserts test data.
    Intended for Microsoft SQL Server / SQL Server Management Studio 2022.

    IMPORTANT: this script permanently deletes current data from application tables.
*/

USE [MusicStore];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;

BEGIN TRY
    BEGIN TRANSACTION;

    /* Delete data from child tables before parent tables. */
    DELETE FROM [dbo].[PromotionPlates];
    DELETE FROM [dbo].[SaleItems];
    DELETE FROM [dbo].[Reservations];
    DELETE FROM [dbo].[StockMovements];
    DELETE FROM [dbo].[Sales];
    DELETE FROM [dbo].[Promotions];
    DELETE FROM [dbo].[Plates];
    DELETE FROM [dbo].[Customers];
    DELETE FROM [dbo].[Artists];
    DELETE FROM [dbo].[Publishers];
    DELETE FROM [dbo].[Genres];
    DELETE FROM [dbo].[Users];

    /* Reset IDENTITY so test identifiers remain consistent. */
    /* A new table handles the first value after RESEED differently,
       so account for whether an IDENTITY value has previously been issued. */
    DECLARE @IdentityTable sysname;
    DECLARE @ReseedValue int;
    DECLARE @ReseedSql nvarchar(500);
    DECLARE IdentityTables CURSOR LOCAL FAST_FORWARD FOR
        SELECT [Name] FROM (VALUES
            (N'dbo.SaleItems'), (N'dbo.Reservations'), (N'dbo.StockMovements'),
            (N'dbo.Sales'), (N'dbo.Promotions'), (N'dbo.Plates'),
            (N'dbo.Customers'), (N'dbo.Artists'), (N'dbo.Publishers'),
            (N'dbo.Genres'), (N'dbo.Users')) AS T([Name]);

    OPEN IdentityTables;
    FETCH NEXT FROM IdentityTables INTO @IdentityTable;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @ReseedValue = CASE WHEN EXISTS (
            SELECT 1 FROM sys.identity_columns
            WHERE object_id = OBJECT_ID(@IdentityTable) AND last_value IS NULL
        ) THEN 1 ELSE 0 END;
        SET @ReseedSql = N'DBCC CHECKIDENT (''' + @IdentityTable +
            N''', RESEED, ' + CONVERT(nvarchar(10), @ReseedValue) +
            N') WITH NO_INFOMSGS;';
        EXEC sys.sp_executesql @ReseedSql;
        FETCH NEXT FROM IdentityTables INTO @IdentityTable;
    END;
    CLOSE IdentityTables;
    DEALLOCATE IdentityTables;

    DECLARE @Today date = CONVERT(date, GETDATE());

    INSERT INTO [dbo].[Genres] ([Name])
    VALUES
        (N'Rock'),
        (N'Jazz'),
        (N'Pop'),
        (N'Classical'),
        (N'Electronic'),
        (N'Blues');

    INSERT INTO [dbo].[Artists] ([Name])
    VALUES
        (N'Northern Lights'),
        (N'Blue Avenue Quartet'),
        (N'Anna Mironova'),
        (N'Harmony Symphony Orchestra'),
        (N'Neon Pulse'),
        (N'River Stone'),
        (N'The Paper Planes'),
        (N'Ilya Vetrov');

    INSERT INTO [dbo].[Publishers] ([Name])
    VALUES
        (N'Aurora Records'),
        (N'Blue Note House'),
        (N'Melody Plus'),
        (N'Pulse Music'),
        (N'Independent Sound');

    INSERT INTO [dbo].[Customers] ([Name], [TotalSpent], [RegisteredAt])
    VALUES
        (N'Alexey Sokolov',  161.96, DATEADD(day, -420, @Today)),
        (N'Maria Koval',      94.02, DATEADD(day, -260, @Today)),
        (N'Dmitry Orlov',    229.94, DATEADD(day, -190, @Today)),
        (N'Olga Levchenko',  127.46, DATEADD(day, -120, @Today)),
        (N'Ivan Bondar',      85.98, DATEADD(day,  -45, @Today)),
        (N'Elena Tkachenko', 109.97, DATEADD(day,  -12, @Today));

    /* Study-project demo credentials: admin/admin123, manager/manager123, seller/seller123. */
    INSERT INTO [dbo].[Users] ([Login], [PasswordHash], [Role])
    VALUES
        (N'admin',   N'PBKDF2-SHA256$210000$AsGwAxQnNT2ux0RXatCu9A==$N5eyKQHqjroi5lVnwJ7gQTVFIKPyjhSPBuNYBTNsyu8=', N'Administrator'),
        (N'manager', N'PBKDF2-SHA256$210000$10CvPBhIgxK0cI10D1DZAA==$pU3tX8zBaHe534NHhOqdOutZafdhB0wSfSyJCjMxRZ8=', N'Manager'),
        (N'seller',  N'PBKDF2-SHA256$210000$7JcaljzIKZy+57gDc82n+Q==$+Qinhag6hHQvI7Ys44IuYt+LOct5PKS0dnFSSB46uKY=', N'Seller');

    INSERT INTO [dbo].[Plates]
        ([Title], [ArtistId], [PublisherId], [GenreId], [TrackCount],
         [ReleaseYear], [CostPrise], [SalePrise], [Quantity])
    VALUES
        (N'Midnight Roads',       1, 1, 1, 10, 2024, 24.50, 39.99, 18),
        (N'Frozen Skyline',       1, 1, 1,  9, 2022, 21.00, 36.99, 12),
        (N'Coffee at Five',       2, 2, 2,  8, 2021, 18.25, 32.99, 20),
        (N'Blue Avenue Live',     2, 2, 2, 11, 2025, 25.00, 45.99,  7),
        (N'Warm Wind',             3, 3, 3, 12, 2026, 27.80, 44.99, 25),
        (N'City Lights',           3, 3, 3, 10, 2023, 23.40, 42.99, 14),
        (N'The Four Seasons',      4, 3, 4, 16, 2020, 31.00, 49.99,  9),
        (N'Rachmaninoff Concertos',4, 3, 4,  7, 2019, 19.60, 31.99, 11),
        (N'Digital Rain',         5, 4, 5, 13, 2026, 26.00, 46.99, 30),
        (N'After Midnight',       5, 4, 5, 10, 2024, 20.00, 34.99, 16),
        (N'Dust and Water',       6, 5, 6,  9, 2022, 22.70, 38.99, 13),
        (N'Open Windows',         7, 5, 1, 11, 2025, 17.30, 29.99, 22);

    /* Two promotions are active relative to the run date; two provide history. */
    INSERT INTO [dbo].[Promotions]
        ([Name], [GenreId], [DiscountPercent], [StartDate], [EndDate])
    VALUES
        (N'Rock Week',            1, 10.00, DATEADD(day,  -7, @Today), DATEADD(day, 14, @Today)),
        (N'Electronic September', 5, 15.00, DATEADD(day,  -3, @Today), DATEADD(day, 27, @Today)),
        (N'Spring Jazz',          2, 12.50, DATEADD(day, -90, @Today), DATEADD(day, -60, @Today)),
        (N'Classical Evening',    4,  8.00, DATEADD(day,  30, @Today), DATEADD(day, 45, @Today));

    INSERT INTO [dbo].[PromotionPlates] ([PromotionId], [PlateId])
    VALUES
        (1, 1), (1, 2), (1, 12),
        (2, 9), (2, 10),
        (3, 3), (3, 4),
        (4, 7), (4, 8);

    INSERT INTO [dbo].[Sales] ([CustomerId], [SaleDate], [TotalAmount])
    VALUES
        (1, DATEADD(day, -180, @Today),  79.98),
        (1, DATEADD(day,  -35, @Today),  81.98),
        (2, DATEADD(day,   -6, @Today),  94.02),
        (3, DATEADD(day,   -2, @Today),  89.98),
        (3, @Today,                         139.96),
        (4, DATEADD(day,   -1, @Today), 127.46),
        (5, @Today,                          85.98),
        (6, @Today,                         109.97);

    INSERT INTO [dbo].[SaleItems]
        ([SaleId], [PlateId], [Quantity], [UnitPrise], [DiscountPercent])
    VALUES
        (1,  1, 2, 39.99,  0.00),
        (2,  5, 1, 44.99,  0.00),
        (2,  2, 1, 36.99,  0.00),
        (3,  3, 3, 32.99,  5.00),
        (4,  7, 2, 49.99, 10.00),
        (5, 10, 4, 34.99,  0.00),
        (6, 12, 5, 29.99, 15.00),
        (7,  6, 2, 42.99,  0.00),
        (8,  8, 1, 31.99,  0.00),
        (8, 11, 2, 38.99,  0.00);

    /* Status: 1 — active, 2 — completed, 3 — cancelled. */
    INSERT INTO [dbo].[Reservations]
        ([CustomerId], [PlatesId], [Quantity], [FulfilledQuantity], [ReservedAt], [ExpiresAt], [Status])
    VALUES
        (1,  9, 2, 0, DATEADD(day, -1,  @Today), DATEADD(day,  5, @Today), 1),
        (2,  4, 1, 0, DATEADD(day, -2,  @Today), DATEADD(day,  3, @Today), 1),
        (3,  7, 1, 0, DATEADD(day, -12, @Today), DATEADD(day, -5, @Today), 4),
        (4,  3, 2, 2, DATEADD(day, -20, @Today), DATEADD(day,-14, @Today), 2),
        (5, 12, 1, 0, DATEADD(day, -8,  @Today), DATEADD(day, -1, @Today), 3);

    /* MovementType: 1 — receipt, 2 — sale, 3 — write-off. */
    INSERT INTO [dbo].[StockMovements]
        ([PlateId], [QuantityChange], [MovementType], [CreatedAt], [Reason])
    VALUES
        (1,   30, 1, DATEADD(day, -210, @Today), N'Initial receipt'),
        (3,   35, 1, DATEADD(day, -120, @Today), N'Publisher delivery'),
        (5,   40, 1, DATEADD(day,  -60, @Today), N'New release delivery'),
        (7,   20, 1, DATEADD(day,  -45, @Today), N'Stock replenishment'),
        (9,   45, 1, DATEADD(day,  -20, @Today), N'New release delivery'),
        (12,  35, 1, DATEADD(day,  -15, @Today), N'Stock replenishment'),
        (1,   -2, 2, DATEADD(day, -180, @Today), N'Sale'),
        (5,   -1, 2, DATEADD(day,  -35, @Today), N'Sale'),
        (2,   -1, 2, DATEADD(day,  -35, @Today), N'Sale'),
        (3,   -3, 2, DATEADD(day,   -6, @Today), N'Sale'),
        (7,   -2, 2, DATEADD(day,   -2, @Today), N'Sale'),
        (10,  -4, 2, @Today,                     N'Sale'),
        (12,  -5, 2, DATEADD(day,   -1, @Today), N'Sale'),
        (6,   -2, 2, @Today,                     N'Sale'),
        (11,  -1, 3, DATEADD(day,   -4, @Today), N'Damaged packaging');

    COMMIT TRANSACTION;

    PRINT N'MusicStore was successfully cleared and populated with test data.';

    /* Final check: row counts for all application tables. */
    SELECT N'Artists'         AS [TableName], COUNT(*) AS [RowCount] FROM [dbo].[Artists]
    UNION ALL SELECT N'Customers',       COUNT(*) FROM [dbo].[Customers]
    UNION ALL SELECT N'Genres',          COUNT(*) FROM [dbo].[Genres]
    UNION ALL SELECT N'Plates',          COUNT(*) FROM [dbo].[Plates]
    UNION ALL SELECT N'PromotionPlates', COUNT(*) FROM [dbo].[PromotionPlates]
    UNION ALL SELECT N'Promotions',      COUNT(*) FROM [dbo].[Promotions]
    UNION ALL SELECT N'Publishers',      COUNT(*) FROM [dbo].[Publishers]
    UNION ALL SELECT N'Reservations',    COUNT(*) FROM [dbo].[Reservations]
    UNION ALL SELECT N'SaleItems',       COUNT(*) FROM [dbo].[SaleItems]
    UNION ALL SELECT N'Sales',           COUNT(*) FROM [dbo].[Sales]
    UNION ALL SELECT N'StockMovements',  COUNT(*) FROM [dbo].[StockMovements]
    UNION ALL SELECT N'Users',           COUNT(*) FROM [dbo].[Users]
    ORDER BY [TableName];
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
