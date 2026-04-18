using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YSA.Data.Migrations
{
    /// <inheritdoc />
    public partial class Paquetes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaqueteId",
                table: "VentaItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Paquetes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DescripcionCorta = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DescripcionLarga = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    UrlImagen = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EsDestacado = table.Column<bool>(type: "bit", nullable: false),
                    EsRecomendado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paquetes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaqueteCursos",
                columns: table => new
                {
                    PaqueteId = table.Column<int>(type: "int", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaqueteCursos", x => new { x.PaqueteId, x.CursoId });
                    table.ForeignKey(
                        name: "FK_PaqueteCursos_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaqueteCursos_Paquetes_PaqueteId",
                        column: x => x.PaqueteId,
                        principalTable: "Paquetes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaqueteProductos",
                columns: table => new
                {
                    PaqueteId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaqueteProductos", x => new { x.PaqueteId, x.ProductoId });
                    table.ForeignKey(
                        name: "FK_PaqueteProductos_Paquetes_PaqueteId",
                        column: x => x.PaqueteId,
                        principalTable: "Paquetes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaqueteProductos_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VentaItems_PaqueteId",
                table: "VentaItems",
                column: "PaqueteId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaItems_Tipo",
                table: "VentaItems",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_PaqueteCursos_CursoId",
                table: "PaqueteCursos",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_PaqueteCursos_PaqueteId",
                table: "PaqueteCursos",
                column: "PaqueteId");

            migrationBuilder.CreateIndex(
                name: "IX_PaqueteProductos_PaqueteId",
                table: "PaqueteProductos",
                column: "PaqueteId");

            migrationBuilder.CreateIndex(
                name: "IX_PaqueteProductos_ProductoId",
                table: "PaqueteProductos",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Paquetes_EsDestacado",
                table: "Paquetes",
                column: "EsDestacado");

            migrationBuilder.CreateIndex(
                name: "IX_Paquetes_FechaPublicacion",
                table: "Paquetes",
                column: "FechaPublicacion");

            migrationBuilder.AddForeignKey(
                name: "FK_VentaItems_Paquetes_PaqueteId",
                table: "VentaItems",
                column: "PaqueteId",
                principalTable: "Paquetes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VentaItems_Paquetes_PaqueteId",
                table: "VentaItems");

            migrationBuilder.DropTable(
                name: "PaqueteCursos");

            migrationBuilder.DropTable(
                name: "PaqueteProductos");

            migrationBuilder.DropTable(
                name: "Paquetes");

            migrationBuilder.DropIndex(
                name: "IX_VentaItems_PaqueteId",
                table: "VentaItems");

            migrationBuilder.DropIndex(
                name: "IX_VentaItems_Tipo",
                table: "VentaItems");

            migrationBuilder.DropColumn(
                name: "PaqueteId",
                table: "VentaItems");
        }
    }
}
