using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVentasMotorrepuestos.API.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AddUserInterfacePreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(name: "ReproducirSonidos", schema: "dbo", table: "PreferenciasUsuario", type: "bit", nullable: false, defaultValue: true);
            migrationBuilder.AddColumn<bool>(name: "ModoOscuro", schema: "dbo", table: "PreferenciasUsuario", type: "bit", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "Animaciones", schema: "dbo", table: "PreferenciasUsuario", type: "bit", nullable: false, defaultValue: true);
            migrationBuilder.AddColumn<bool>(name: "MostrarImagenesCatalogo", schema: "dbo", table: "PreferenciasUsuario", type: "bit", nullable: false, defaultValue: true);
            migrationBuilder.AddColumn<string>(name: "TamanoFuente", schema: "dbo", table: "PreferenciasUsuario", type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "normal");
            migrationBuilder.AddColumn<string>(name: "DensidadInformacion", schema: "dbo", table: "PreferenciasUsuario", type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "comoda");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ReproducirSonidos", schema: "dbo", table: "PreferenciasUsuario");
            migrationBuilder.DropColumn(name: "ModoOscuro", schema: "dbo", table: "PreferenciasUsuario");
            migrationBuilder.DropColumn(name: "Animaciones", schema: "dbo", table: "PreferenciasUsuario");
            migrationBuilder.DropColumn(name: "MostrarImagenesCatalogo", schema: "dbo", table: "PreferenciasUsuario");
            migrationBuilder.DropColumn(name: "TamanoFuente", schema: "dbo", table: "PreferenciasUsuario");
            migrationBuilder.DropColumn(name: "DensidadInformacion", schema: "dbo", table: "PreferenciasUsuario");
        }
    }
}
