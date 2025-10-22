using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YSA.Data.Migrations
{
    /// <inheritdoc />
    public partial class Articulos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articulos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Resumen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContenidoTexto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NombrePersonaDestacada = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BiografiaCortaDestacado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrlFotoDestacado = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UrlImagenPrincipal = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articulos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticuloFotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticuloId = table.Column<int>(type: "int", nullable: false),
                    UrlFoto = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticuloFotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticuloFotos_Articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "Articulos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticuloFotos_ArticuloId",
                table: "ArticuloFotos",
                column: "ArticuloId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticuloFotos");

            migrationBuilder.DropTable(
                name: "Articulos");
        }
    }
}
