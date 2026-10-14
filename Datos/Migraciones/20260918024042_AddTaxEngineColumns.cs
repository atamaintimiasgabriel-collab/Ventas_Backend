using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVentasMotorrepuestos.API.Datos.Migraciones
{
    public partial class AddTaxEngineColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfiguracionFiscalJson",
                schema: "dbo",
                table: "Empresas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClasificacionTributaria",
                schema: "dbo",
                table: "Productos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "GRA");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseImponible",
                schema: "dbo",
                table: "VentaDetalles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ClasificacionTributariaAplicada",
                schema: "dbo",
                table: "VentaDetalles",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConceptoTributarioAplicado",
                schema: "dbo",
                table: "VentaDetalles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseImponible",
                schema: "dbo",
                table: "PedidoDetalles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ClasificacionTributariaAplicada",
                schema: "dbo",
                table: "PedidoDetalles",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConceptoTributarioAplicado",
                schema: "dbo",
                table: "PedidoDetalles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalLinea",
                schema: "dbo",
                table: "PedidoDetalles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfiguracionFiscalJson",
                schema: "dbo",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ClasificacionTributaria",
                schema: "dbo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "BaseImponible",
                schema: "dbo",
                table: "VentaDetalles");

            migrationBuilder.DropColumn(
                name: "ClasificacionTributariaAplicada",
                schema: "dbo",
                table: "VentaDetalles");

            migrationBuilder.DropColumn(
                name: "ConceptoTributarioAplicado",
                schema: "dbo",
                table: "VentaDetalles");

            migrationBuilder.DropColumn(
                name: "BaseImponible",
                schema: "dbo",
                table: "PedidoDetalles");

            migrationBuilder.DropColumn(
                name: "ClasificacionTributariaAplicada",
                schema: "dbo",
                table: "PedidoDetalles");

            migrationBuilder.DropColumn(
                name: "ConceptoTributarioAplicado",
                schema: "dbo",
                table: "PedidoDetalles");

            migrationBuilder.DropColumn(
                name: "TotalLinea",
                schema: "dbo",
                table: "PedidoDetalles");
        }
    }
}
