using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WMS.Domain.Migrations
{
    /// <inheritdoc />
    public partial class updatedb261 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentEvents_PaymentId",
                table: "PaymentEvents");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryEvents_DeliveryId",
                table: "DeliveryEvents");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("1cf68399-e5f7-4513-aa10-a2a905c969d0"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c6951fa4-b850-440b-8fc1-f9cf2bbb1f2e"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("96457f14-3cf7-47c1-9321-9fe787dd5adb"), new Guid("5af8cb24-0d72-4048-86bb-a9ca7c9ec859") });

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("96457f14-3cf7-47c1-9321-9fe787dd5adb"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("5af8cb24-0d72-4048-86bb-a9ca7c9ec859"));

            migrationBuilder.DropColumn(
                name: "ReturnInboundId",
                table: "Deliveries");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Users",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Roles",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Products",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentGateway",
                table: "Payments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalPaymentId",
                table: "Payments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentUrl",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Payments",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "SessionExpiresAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "Payments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GatewayEventId",
                table: "PaymentEvents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                table: "PaymentEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PaymentEvents",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Outbounds",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "OutboundItems",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Locations",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "InventoryTransactions",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Inventories",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Inbounds",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "InboundItems",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "EventData",
                table: "DeliveryEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                table: "DeliveryEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PartnerEventId",
                table: "DeliveryEvents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "DeliveryEvents",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<string>(
                name: "TrackingNumber",
                table: "Deliveries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Carrier",
                table: "Deliveries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceivedBy",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Deliveries",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("acee3821-07d6-41c0-94a0-f7c511de59f3"), new DateTime(2026, 1, 26, 2, 16, 34, 590, DateTimeKind.Utc).AddTicks(5927), "System", "Warehouse Manager", "Manager", null, null },
                    { new Guid("b7ae6236-c874-4782-85fa-0ec5696aa8c0"), new DateTime(2026, 1, 26, 2, 16, 34, 590, DateTimeKind.Utc).AddTicks(5318), "System", "System Administrator", "Admin", null, null },
                    { new Guid("e8499283-7171-4271-8f9d-3e0c2f391fbb"), new DateTime(2026, 1, 26, 2, 16, 34, 590, DateTimeKind.Utc).AddTicks(5931), "System", "Warehouse Staff", "WarehouseStaff", null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Email", "FirstName", "IsActive", "LastLoginDate", "LastName", "PasswordHash", "RefreshToken", "RefreshTokenExpiry", "UpdatedAt", "UpdatedBy", "Username" },
                values: new object[] { new Guid("34abfda8-e6e0-468c-98f3-a27dd1eca567"), new DateTime(2026, 1, 26, 2, 16, 34, 591, DateTimeKind.Utc).AddTicks(5319), "System", "admin@wms.com", "System", true, null, "Administrator", "$2a$11$D7Z5z8YqJ5qH5F0ZK5Z5Z.Z5Z5Z5Z5Z5Z5Z5Z5Z", null, null, null, null, "admin" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId", "AssignedAt", "AssignedBy" },
                values: new object[] { new Guid("b7ae6236-c874-4782-85fa-0ec5696aa8c0"), new Guid("34abfda8-e6e0-468c-98f3-a27dd1eca567"), new DateTime(2026, 1, 26, 2, 16, 34, 591, DateTimeKind.Utc).AddTicks(6928), "System" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SessionId",
                table: "Payments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentEvents_PaymentId_GatewayEventId",
                table: "PaymentEvents",
                columns: new[] { "PaymentId", "GatewayEventId" },
                unique: true,
                filter: "[GatewayEventId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryEvents_DeliveryId_PartnerEventId",
                table: "DeliveryEvents",
                columns: new[] { "DeliveryId", "PartnerEventId" },
                unique: true,
                filter: "[PartnerEventId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_SessionId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentEvents_PaymentId_GatewayEventId",
                table: "PaymentEvents");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryEvents_DeliveryId_PartnerEventId",
                table: "DeliveryEvents");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("acee3821-07d6-41c0-94a0-f7c511de59f3"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("e8499283-7171-4271-8f9d-3e0c2f391fbb"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("b7ae6236-c874-4782-85fa-0ec5696aa8c0"), new Guid("34abfda8-e6e0-468c-98f3-a27dd1eca567") });

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b7ae6236-c874-4782-85fa-0ec5696aa8c0"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("34abfda8-e6e0-468c-98f3-a27dd1eca567"));

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PaymentUrl",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SessionExpiresAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "GatewayEventId",
                table: "PaymentEvents");

            migrationBuilder.DropColumn(
                name: "IsProcessed",
                table: "PaymentEvents");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PaymentEvents");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Outbounds");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "OutboundItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Inbounds");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "InboundItems");

            migrationBuilder.DropColumn(
                name: "EventData",
                table: "DeliveryEvents");

            migrationBuilder.DropColumn(
                name: "IsProcessed",
                table: "DeliveryEvents");

            migrationBuilder.DropColumn(
                name: "PartnerEventId",
                table: "DeliveryEvents");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "DeliveryEvents");

            migrationBuilder.DropColumn(
                name: "ReceivedBy",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Deliveries");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentGateway",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalPaymentId",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrackingNumber",
                table: "Deliveries",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Carrier",
                table: "Deliveries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReturnInboundId",
                table: "Deliveries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("1cf68399-e5f7-4513-aa10-a2a905c969d0"), new DateTime(2026, 1, 24, 15, 43, 55, 350, DateTimeKind.Utc).AddTicks(9910), "System", "Warehouse Manager", "Manager", null, null },
                    { new Guid("96457f14-3cf7-47c1-9321-9fe787dd5adb"), new DateTime(2026, 1, 24, 15, 43, 55, 350, DateTimeKind.Utc).AddTicks(8838), "System", "System Administrator", "Admin", null, null },
                    { new Guid("c6951fa4-b850-440b-8fc1-f9cf2bbb1f2e"), new DateTime(2026, 1, 24, 15, 43, 55, 350, DateTimeKind.Utc).AddTicks(9914), "System", "Warehouse Staff", "WarehouseStaff", null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Email", "FirstName", "IsActive", "LastLoginDate", "LastName", "PasswordHash", "RefreshToken", "RefreshTokenExpiry", "UpdatedAt", "UpdatedBy", "Username" },
                values: new object[] { new Guid("5af8cb24-0d72-4048-86bb-a9ca7c9ec859"), new DateTime(2026, 1, 24, 15, 43, 55, 352, DateTimeKind.Utc).AddTicks(2607), "System", "admin@wms.com", "System", true, null, "Administrator", "$2a$11$D7Z5z8YqJ5qH5F0ZK5Z5Z.Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z", null, null, null, null, "admin" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId", "AssignedAt", "AssignedBy" },
                values: new object[] { new Guid("96457f14-3cf7-47c1-9321-9fe787dd5adb"), new Guid("5af8cb24-0d72-4048-86bb-a9ca7c9ec859"), new DateTime(2026, 1, 24, 15, 43, 55, 352, DateTimeKind.Utc).AddTicks(4997), "System" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentEvents_PaymentId",
                table: "PaymentEvents",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryEvents_DeliveryId",
                table: "DeliveryEvents",
                column: "DeliveryId");
        }
    }
}
