using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YSA.Data.Migrations
{
    /// <inheritdoc />
    public partial class PlanesSuscripciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanesSuscripcion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DuracionDias = table.Column<int>(type: "int", nullable: false),
                    LimitePublicaciones = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ComisionPorcentaje = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TieneVisibilidadPrioritaria = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPorId = table.Column<int>(type: "int", nullable: true),
                    PermitePromocionesExtras = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MaxPromocionesSimultaneas = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DescuentoComisionAdicional = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesSuscripcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanesSuscripcion_AspNetUsers_ModificadoPorId",
                        column: x => x.ModificadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SuscripcionesArtistas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArtistaId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    SnapshotNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SnapshotPrecio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SnapshotDuracionDias = table.Column<int>(type: "int", nullable: false),
                    SnapshotLimitePublicaciones = table.Column<int>(type: "int", nullable: false),
                    SnapshotComisionPorcentaje = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SnapshotTieneVisibilidadPrioritaria = table.Column<bool>(type: "bit", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ComprobanteUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidadoPorId = table.Column<int>(type: "int", nullable: true),
                    FechaValidacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotasAdmin = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    PublicacionesUsadas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuscripcionesArtistas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuscripcionesArtistas_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SuscripcionesArtistas_AspNetUsers_ValidadoPorId",
                        column: x => x.ValidadoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SuscripcionesArtistas_PlanesSuscripcion_PlanId",
                        column: x => x.PlanId,
                        principalTable: "PlanesSuscripcion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanesSuscripcion_ModificadoPorId",
                table: "PlanesSuscripcion",
                column: "ModificadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesArtistas_ArtistaId",
                table: "SuscripcionesArtistas",
                column: "ArtistaId");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesArtistas_PlanId",
                table: "SuscripcionesArtistas",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesArtistas_ValidadoPorId",
                table: "SuscripcionesArtistas",
                column: "ValidadoPorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuscripcionesArtistas");

            migrationBuilder.DropTable(
                name: "PlanesSuscripcion");
        }
    }
}
