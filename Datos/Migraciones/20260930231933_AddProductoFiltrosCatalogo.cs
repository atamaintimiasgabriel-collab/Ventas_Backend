using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVentasMotorrepuestos.API.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AddProductoFiltrosCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsNuevoIngreso",
                schema: "dbo",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsOferta",
                schema: "dbo",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsRecomendado",
                schema: "dbo",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioOferta",
                schema: "dbo",
                table: "Productos",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsNuevoIngreso",
                schema: "dbo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "EsOferta",
                schema: "dbo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "EsRecomendado",
                schema: "dbo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "PrecioOferta",
                schema: "dbo",
                table: "Productos");
        }
    }
}
