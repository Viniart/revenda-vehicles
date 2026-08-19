using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Revenda.Vehicles.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vehicles");

            migrationBuilder.CreateTable(
                name: "vehicles",
                schema: "vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    brand = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    model = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    color = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    license_plate = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sales",
                schema: "vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    payment_code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales", x => x.id);
                    table.ForeignKey(
                        name: "FK_sales_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalSchema: "vehicles",
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sales_vehicle_id",
                schema: "vehicles",
                table: "sales",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_buyer",
                schema: "vehicles",
                table: "sales",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_payment_code",
                schema: "vehicles",
                table: "sales",
                column: "payment_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_license_plate",
                schema: "vehicles",
                table: "vehicles",
                column: "license_plate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_status_price",
                schema: "vehicles",
                table: "vehicles",
                columns: new[] { "status", "price" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sales",
                schema: "vehicles");

            migrationBuilder.DropTable(
                name: "vehicles",
                schema: "vehicles");
        }
    }
}
