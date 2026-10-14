using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVentasMotorrepuestos.API.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AddDurableUserSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SesionesUsuarios",
                schema: "dbo",
                columns: table => new
                {
                    IdSesionUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaUltimaActividad = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRevocacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoRevocacion = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    RevocadaPor = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionesUsuarios", x => x.IdSesionUsuario);
                    table.CheckConstraint("CK_SesionesUsuarios_FechaExpiracion", "[FechaExpiracion] > [FechaCreacion]");
                    table.CheckConstraint("CK_SesionesUsuarios_FechaRevocacion", "[FechaRevocacion] IS NULL OR [FechaRevocacion] >= [FechaCreacion]");
                    table.CheckConstraint("CK_SesionesUsuarios_FechaUltimaActividad", "[FechaUltimaActividad] IS NULL OR [FechaUltimaActividad] >= [FechaCreacion]");
                    table.ForeignKey(
                        name: "FK_SesionesUsuarios_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalSchema: "dbo",
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SesionUsuarioRefreshTokens",
                schema: "dbo",
                columns: table => new
                {
                    IdRefreshToken = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSesionUsuario = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<byte[]>(type: "varbinary(32)", maxLength: 32, nullable: false),
                    TokenHashVersion = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaConsumido = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByTokenId = table.Column<long>(type: "bigint", nullable: true),
                    FechaRevocacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoRevocacion = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ReuseDetectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionUsuarioRefreshTokens", x => x.IdRefreshToken);
                    table.CheckConstraint("CK_SesionUsuarioRefreshTokens_FechaConsumido", "[FechaConsumido] IS NULL OR [FechaConsumido] >= [FechaCreacion]");
                    table.CheckConstraint("CK_SesionUsuarioRefreshTokens_FechaExpiracion", "[FechaExpiracion] > [FechaCreacion]");
                    table.CheckConstraint("CK_SesionUsuarioRefreshTokens_FechaRevocacion", "[FechaRevocacion] IS NULL OR [FechaRevocacion] >= [FechaCreacion]");
                    table.CheckConstraint("CK_SesionUsuarioRefreshTokens_ReuseDetectedAt", "[ReuseDetectedAt] IS NULL OR [ReuseDetectedAt] >= [FechaCreacion]");
                    table.CheckConstraint("CK_SesionUsuarioRefreshTokens_TokenHashLength", "DATALENGTH([TokenHash]) = 32");
                    table.CheckConstraint("CK_SesionUsuarioRefreshTokens_TokenHashVersion", "[TokenHashVersion] > 0");
                    table.ForeignKey(
                        name: "FK_SesionUsuarioRefreshTokens_SesionUsuarioRefreshTokens_ReplacedByTokenId",
                        column: x => x.ReplacedByTokenId,
                        principalSchema: "dbo",
                        principalTable: "SesionUsuarioRefreshTokens",
                        principalColumn: "IdRefreshToken",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SesionUsuarioRefreshTokens_SesionesUsuarios_IdSesionUsuario",
                        column: x => x.IdSesionUsuario,
                        principalSchema: "dbo",
                        principalTable: "SesionesUsuarios",
                        principalColumn: "IdSesionUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SesionesUsuarios_FechaExpiracion",
                schema: "dbo",
                table: "SesionesUsuarios",
                column: "FechaExpiracion");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesUsuarios_Usuario_Revocacion_Expiracion",
                schema: "dbo",
                table: "SesionesUsuarios",
                columns: new[] { "IdUsuario", "FechaRevocacion", "FechaExpiracion" });

            migrationBuilder.CreateIndex(
                name: "UQ_SesionesUsuarios_SessionId",
                schema: "dbo",
                table: "SesionesUsuarios",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SesionUsuarioRefreshTokens_ReplacedByTokenId",
                schema: "dbo",
                table: "SesionUsuarioRefreshTokens",
                column: "ReplacedByTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_SesionUsuarioRefreshTokens_Sesion_Consumido_Revocacion",
                schema: "dbo",
                table: "SesionUsuarioRefreshTokens",
                columns: new[] { "IdSesionUsuario", "FechaConsumido", "FechaRevocacion" });

            migrationBuilder.CreateIndex(
                name: "IX_SesionUsuarioRefreshTokens_Sesion_Expiracion",
                schema: "dbo",
                table: "SesionUsuarioRefreshTokens",
                columns: new[] { "IdSesionUsuario", "FechaExpiracion" });

            migrationBuilder.CreateIndex(
                name: "UQ_SesionUsuarioRefreshTokens_TokenHash",
                schema: "dbo",
                table: "SesionUsuarioRefreshTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SesionUsuarioRefreshTokens",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SesionesUsuarios",
                schema: "dbo");
        }
    }
}
