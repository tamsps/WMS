using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WMS.Domain.Migrations
{
    /// <inheritdoc />
    public partial class InitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("465f0e16-502e-428e-9f89-9a23716b4992"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("637abc17-1563-4361-b322-186753a939dc"));

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("6916eedf-6354-4b90-a767-a4e730fa9234"), new Guid("da9692f3-0ed8-488b-8360-5706e4beb7d7") });

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6916eedf-6354-4b90-a767-a4e730fa9234"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("da9692f3-0ed8-488b-8360-5706e4beb7d7"));

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
                name: "IX_Payments_OutboundId",
                table: "Payments",
                column: "OutboundId",
                unique: true,
                filter: "[OutboundId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Outbounds_OutboundId",
                table: "Payments",
                column: "OutboundId",
                principalTable: "Outbounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Outbounds_OutboundId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_OutboundId",
                table: "Payments");

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

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("465f0e16-502e-428e-9f89-9a23716b4992"), new DateTime(2026, 1, 17, 6, 35, 10, 801, DateTimeKind.Utc).AddTicks(2609), "System", "Warehouse Manager", "Manager", null, null },
                    { new Guid("637abc17-1563-4361-b322-186753a939dc"), new DateTime(2026, 1, 17, 6, 35, 10, 801, DateTimeKind.Utc).AddTicks(2612), "System", "Warehouse Staff", "WarehouseStaff", null, null },
                    { new Guid("6916eedf-6354-4b90-a767-a4e730fa9234"), new DateTime(2026, 1, 17, 6, 35, 10, 801, DateTimeKind.Utc).AddTicks(2023), "System", "System Administrator", "Admin", null, null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Email", "FirstName", "IsActive", "LastLoginDate", "LastName", "PasswordHash", "RefreshToken", "RefreshTokenExpiry", "UpdatedAt", "UpdatedBy", "Username" },
                values: new object[] { new Guid("da9692f3-0ed8-488b-8360-5706e4beb7d7"), new DateTime(2026, 1, 17, 6, 35, 10, 802, DateTimeKind.Utc).AddTicks(5172), "System", "admin@wms.com", "System", true, null, "Administrator", "$2a$11$D7Z5z8YqJ5qH5F0ZK5Z5Z.Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z", null, null, null, null, "admin" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId", "AssignedAt", "AssignedBy" },
                values: new object[] { new Guid("6916eedf-6354-4b90-a767-a4e730fa9234"), new Guid("da9692f3-0ed8-488b-8360-5706e4beb7d7"), new DateTime(2026, 1, 17, 6, 35, 10, 802, DateTimeKind.Utc).AddTicks(6915), "System" });
        }
    }
}
