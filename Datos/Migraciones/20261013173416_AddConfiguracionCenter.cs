using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVentasMotorrepuestos.API.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AddConfiguracionCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionesEmpresa",
                schema: "dbo",
                columns: table => new
                {
                    IdEmpresa = table.Column<int>(type: "int", nullable: false),
                    Moneda = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Idioma = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    ZonaHoraria = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    FormatoFecha = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    FormatoHora = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    ControlStockNegativo = table.Column<bool>(type: "bit", nullable: false),
                    AlertasStock = table.Column<bool>(type: "bit", nullable: false),
                    RequiereCajaAbierta = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesEmpresa", x => x.IdEmpresa);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesEmpresa_Empresas_IdEmpresa",
                        column: x => x.IdEmpresa,
                        principalSchema: "dbo",
                        principalTable: "Empresas",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreferenciasUsuario",
                schema: "dbo",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    Autoguardado = table.Column<bool>(type: "bit", nullable: false),
                    Notificaciones = table.Column<bool>(type: "bit", nullable: false),
                    Tema = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferenciasUsuario", x => x.IdUsuario);
                    table.ForeignKey(
                        name: "FK_PreferenciasUsuario_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalSchema: "dbo",
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosSucursales",
                schema: "dbo",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdSucursal = table.Column<int>(type: "int", nullable: false),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosSucursales", x => new { x.IdUsuario, x.IdSucursal });
                    table.ForeignKey(
                        name: "FK_UsuariosSucursales_Sucursales_IdSucursal",
                        column: x => x.IdSucursal,
                        principalSchema: "dbo",
                        principalTable: "Sucursales",
                        principalColumn: "IdSucursal",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosSucursales_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalSchema: "dbo",
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSucursales_IdSucursal",
                schema: "dbo",
                table: "UsuariosSucursales",
                column: "IdSucursal");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSucursales_Principal",
                schema: "dbo",
                table: "UsuariosSucursales",
                columns: new[] { "IdUsuario", "EsPrincipal" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesEmpresa",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PreferenciasUsuario",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UsuariosSucursales",
                schema: "dbo");
        }
    }
}
