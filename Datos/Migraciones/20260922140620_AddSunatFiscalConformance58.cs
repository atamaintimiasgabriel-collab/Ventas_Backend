using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVentasMotorrepuestos.API.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AddSunatFiscalConformance58 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoMotivoNota",
                table: "Comprobantes",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CorrelativoDocumentoOrigen",
                table: "Comprobantes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerieDocumentoOrigen",
                table: "Comprobantes",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SustentoNota",
                table: "Comprobantes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoDocumentoOrigen",
                table: "Comprobantes",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoOperacionSunat",
                table: "Comprobantes",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VersionNormativa",
                table: "Comprobantes",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoMotivoNota",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "CorrelativoDocumentoOrigen",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "SerieDocumentoOrigen",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "SustentoNota",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "TipoDocumentoOrigen",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "TipoOperacionSunat",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "VersionNormativa",
                table: "Comprobantes");
        }
    }
}
