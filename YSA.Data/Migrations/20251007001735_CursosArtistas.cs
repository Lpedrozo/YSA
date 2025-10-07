using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YSA.Data.Migrations
{
    /// <inheritdoc />
    public partial class CursosArtistas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Artistas_InstructorId",
                table: "Cursos");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_InstructorId",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "InstructorId",
                table: "Cursos");

            migrationBuilder.CreateTable(
                name: "CursoInstructores",
                columns: table => new
                {
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    ArtistaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CursoInstructores", x => new { x.CursoId, x.ArtistaId });
                    table.ForeignKey(
                        name: "FK_CursoInstructores_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CursoInstructores_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CursoInstructores_ArtistaId",
                table: "CursoInstructores",
                column: "ArtistaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CursoInstructores");

            migrationBuilder.AddColumn<int>(
                name: "InstructorId",
                table: "Cursos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_InstructorId",
                table: "Cursos",
                column: "InstructorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_Artistas_InstructorId",
                table: "Cursos",
                column: "InstructorId",
                principalTable: "Artistas",
                principalColumn: "Id");
        }
    }
}
