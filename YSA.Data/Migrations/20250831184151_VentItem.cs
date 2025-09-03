using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YSA.Data.Migrations
{
    /// <inheritdoc />
    public partial class VentItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidoItems_Productos_ProductoId",
                table: "PedidoItems");

            migrationBuilder.RenameColumn(
                name: "ProductoId",
                table: "PedidoItems",
                newName: "VentaItemId");

            migrationBuilder.RenameIndex(
                name: "IX_PedidoItems_ProductoId",
                table: "PedidoItems",
                newName: "IX_PedidoItems_VentaItemId");

            migrationBuilder.CreateTable(
                name: "VentaItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: true),
                    ProductoId = table.Column<int>(type: "int", nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaItems_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VentaItems_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_VentaItems_CursoId",
                table: "VentaItems",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaItems_ProductoId",
                table: "VentaItems",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoItems_VentaItems_VentaItemId",
                table: "PedidoItems",
                column: "VentaItemId",
                principalTable: "VentaItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidoItems_VentaItems_VentaItemId",
                table: "PedidoItems");

            migrationBuilder.DropTable(
                name: "VentaItems");

            migrationBuilder.RenameColumn(
                name: "VentaItemId",
                table: "PedidoItems",
                newName: "ProductoId");

            migrationBuilder.RenameIndex(
                name: "IX_PedidoItems_VentaItemId",
                table: "PedidoItems",
                newName: "IX_PedidoItems_ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoItems_Productos_ProductoId",
                table: "PedidoItems",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id");
        }
    }
}
