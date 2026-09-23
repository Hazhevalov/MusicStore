using Exam.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exam.Migrations;

[DbContext(typeof(ApplicationContext))]
[Migration("20260922120000_CriticalHardening")]
public partial class CriticalHardening : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        DropProtectedForeignKeys(migrationBuilder);

        migrationBuilder.RenameColumn("Password", "Users", "PasswordHash");
        migrationBuilder.AddColumn<bool>("IsArchived", "Plates", "bit", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<byte[]>("RowVersion", "Plates", "rowversion", rowVersion: true, nullable: false);
        migrationBuilder.AddColumn<int>("FulfilledQuantity", "Reservations", "int", nullable: false, defaultValue: 0);

        migrationBuilder.AlterColumn<string>("Login", "Users", "nvarchar(64)", maxLength: 64, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(max)");
        migrationBuilder.AlterColumn<string>("PasswordHash", "Users", "nvarchar(512)", maxLength: 512, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(max)");
        migrationBuilder.AlterColumn<string>("Role", "Users", "nvarchar(32)", maxLength: 32, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(max)");
        migrationBuilder.AlterColumn<string>("Title", "Plates", "nvarchar(200)", maxLength: 200, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(max)");
        migrationBuilder.AlterColumn<string>("Name", "Artists", "nvarchar(150)", maxLength: 150, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(max)");
        migrationBuilder.AlterColumn<string>("Name", "Genres", "nvarchar(100)", maxLength: 100, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(max)");
        migrationBuilder.AlterColumn<string>("Name", "Publishers", "nvarchar(150)", maxLength: 150, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(max)");
        migrationBuilder.AlterColumn<string>("Name", "Customers", "nvarchar(150)", maxLength: 150, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(max)");
        migrationBuilder.AlterColumn<string>("Name", "Promotions", "nvarchar(150)", maxLength: 150, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(max)");
        migrationBuilder.AlterColumn<string>("Reason", "StockMovements", "nvarchar(500)", maxLength: 500, nullable: false, oldClrType: typeof(string), oldType: "nvarchar(max)");

        migrationBuilder.CreateIndex("IX_Users_Login", "Users", "Login", unique: true);
        migrationBuilder.AddCheckConstraint("CK_Plates_Quantity", "Plates", "[Quantity] >= 0");
        migrationBuilder.AddCheckConstraint("CK_Plates_TrackCount", "Plates", "[TrackCount] > 0");
        migrationBuilder.AddCheckConstraint("CK_Plates_ReleaseYear", "Plates", "[ReleaseYear] BETWEEN 1877 AND 2100");
        migrationBuilder.AddCheckConstraint("CK_Plates_Prices", "Plates", "[CostPrise] >= 0 AND [SalePrise] >= 0");
        migrationBuilder.AddCheckConstraint("CK_Customers_TotalSpent", "Customers", "[TotalSpent] >= 0");
        migrationBuilder.AddCheckConstraint("CK_Promotions_Discount", "Promotions", "[DiscountPercent] > 0 AND [DiscountPercent] <= 100");
        migrationBuilder.AddCheckConstraint("CK_Promotions_Dates", "Promotions", "[EndDate] >= [StartDate]");
        migrationBuilder.AddCheckConstraint("CK_Reservations_Quantity", "Reservations", "[Quantity] > 0");
        migrationBuilder.AddCheckConstraint("CK_Reservations_Fulfilled", "Reservations", "[FulfilledQuantity] >= 0 AND [FulfilledQuantity] <= [Quantity]");
        migrationBuilder.AddCheckConstraint("CK_Reservations_Dates", "Reservations", "[ExpiresAt] >= [ReservedAt]");
        migrationBuilder.AddCheckConstraint("CK_Reservations_Status", "Reservations", "[Status] BETWEEN 1 AND 4");
        migrationBuilder.AddCheckConstraint("CK_SaleItems_Quantity", "SaleItems", "[Quantity] > 0");
        migrationBuilder.AddCheckConstraint("CK_SaleItems_UnitPrice", "SaleItems", "[UnitPrise] >= 0");
        migrationBuilder.AddCheckConstraint("CK_SaleItems_Discount", "SaleItems", "[DiscountPercent] >= 0 AND [DiscountPercent] <= 100");
        migrationBuilder.AddCheckConstraint("CK_Sales_TotalAmount", "Sales", "[TotalAmount] >= 0");
        migrationBuilder.AddCheckConstraint("CK_StockMovements_Quantity", "StockMovements", "[QuantityChange] <> 0");
        migrationBuilder.AddCheckConstraint("CK_StockMovements_Type", "StockMovements", "[MovementType] BETWEEN 1 AND 3");

        AddProtectedForeignKeys(migrationBuilder, ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        DropProtectedForeignKeys(migrationBuilder);
        migrationBuilder.DropIndex("IX_Users_Login", "Users");

        foreach (var (name, table) in new[]
        {
            ("CK_Plates_Quantity", "Plates"), ("CK_Plates_TrackCount", "Plates"),
            ("CK_Plates_ReleaseYear", "Plates"), ("CK_Plates_Prices", "Plates"),
            ("CK_Customers_TotalSpent", "Customers"), ("CK_Promotions_Discount", "Promotions"),
            ("CK_Promotions_Dates", "Promotions"), ("CK_Reservations_Quantity", "Reservations"),
            ("CK_Reservations_Fulfilled", "Reservations"), ("CK_Reservations_Dates", "Reservations"),
            ("CK_Reservations_Status", "Reservations"), ("CK_SaleItems_Quantity", "SaleItems"),
            ("CK_SaleItems_UnitPrice", "SaleItems"), ("CK_SaleItems_Discount", "SaleItems"),
            ("CK_Sales_TotalAmount", "Sales"), ("CK_StockMovements_Quantity", "StockMovements"),
            ("CK_StockMovements_Type", "StockMovements")
        }) migrationBuilder.DropCheckConstraint(name, table);

        migrationBuilder.DropColumn("IsArchived", "Plates");
        migrationBuilder.DropColumn("RowVersion", "Plates");
        migrationBuilder.DropColumn("FulfilledQuantity", "Reservations");

        migrationBuilder.AlterColumn<string>("Login", "Users", "nvarchar(max)", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(64)", oldMaxLength: 64);
        migrationBuilder.AlterColumn<string>("PasswordHash", "Users", "nvarchar(max)", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(512)", oldMaxLength: 512);
        migrationBuilder.AlterColumn<string>("Role", "Users", "nvarchar(max)", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(32)", oldMaxLength: 32);
        migrationBuilder.AlterColumn<string>("Title", "Plates", "nvarchar(max)", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(200)", oldMaxLength: 200);
        migrationBuilder.AlterColumn<string>("Name", "Artists", "nvarchar(max)", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(150)", oldMaxLength: 150);
        migrationBuilder.AlterColumn<string>("Name", "Genres", "nvarchar(max)", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(100)", oldMaxLength: 100);
        migrationBuilder.AlterColumn<string>("Name", "Publishers", "nvarchar(max)", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(150)", oldMaxLength: 150);
        migrationBuilder.AlterColumn<string>("Name", "Customers", "nvarchar(max)", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(150)", oldMaxLength: 150);
        migrationBuilder.AlterColumn<string>("Name", "Promotions", "nvarchar(max)", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(150)", oldMaxLength: 150);
        migrationBuilder.AlterColumn<string>("Reason", "StockMovements", "nvarchar(max)", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(500)", oldMaxLength: 500);
        migrationBuilder.RenameColumn("PasswordHash", "Users", "Password");
        AddProtectedForeignKeys(migrationBuilder, ReferentialAction.Cascade);
    }

    private static void DropProtectedForeignKeys(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_Plates_Artists_ArtistId", "Plates");
        migrationBuilder.DropForeignKey("FK_Plates_Genres_GenreId", "Plates");
        migrationBuilder.DropForeignKey("FK_Plates_Publishers_PublisherId", "Plates");
        migrationBuilder.DropForeignKey("FK_Reservations_Customers_CustomerId", "Reservations");
        migrationBuilder.DropForeignKey("FK_Reservations_Plates_PlatesId", "Reservations");
        migrationBuilder.DropForeignKey("FK_SaleItems_Plates_PlateId", "SaleItems");
        migrationBuilder.DropForeignKey("FK_Sales_Customers_CustomerId", "Sales");
        migrationBuilder.DropForeignKey("FK_StockMovements_Plates_PlateId", "StockMovements");
    }

    private static void AddProtectedForeignKeys(MigrationBuilder migrationBuilder, ReferentialAction action)
    {
        migrationBuilder.AddForeignKey("FK_Plates_Artists_ArtistId", "Plates", "ArtistId", "Artists", principalColumn: "Id", onDelete: action);
        migrationBuilder.AddForeignKey("FK_Plates_Genres_GenreId", "Plates", "GenreId", "Genres", principalColumn: "Id", onDelete: action);
        migrationBuilder.AddForeignKey("FK_Plates_Publishers_PublisherId", "Plates", "PublisherId", "Publishers", principalColumn: "Id", onDelete: action);
        migrationBuilder.AddForeignKey("FK_Reservations_Customers_CustomerId", "Reservations", "CustomerId", "Customers", principalColumn: "Id", onDelete: action);
        migrationBuilder.AddForeignKey("FK_Reservations_Plates_PlatesId", "Reservations", "PlatesId", "Plates", principalColumn: "Id", onDelete: action);
        migrationBuilder.AddForeignKey("FK_SaleItems_Plates_PlateId", "SaleItems", "PlateId", "Plates", principalColumn: "Id", onDelete: action);
        migrationBuilder.AddForeignKey("FK_Sales_Customers_CustomerId", "Sales", "CustomerId", "Customers", principalColumn: "Id", onDelete: action);
        migrationBuilder.AddForeignKey("FK_StockMovements_Plates_PlateId", "StockMovements", "PlateId", "Plates", principalColumn: "Id", onDelete: action);
    }
}
