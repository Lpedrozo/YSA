using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YSA.Data.Migrations
{
    /// <inheritdoc />
    public partial class CursosPresenciales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TipoEntidad",
                table: "RecursosActividades",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "TipoCurso",
                table: "Cursos",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "AtendidoPor",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cedula",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CedulaRepresentante",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsMenorEdad",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExperienciaTatuaje",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreRepresentante",
                table: "AspNetUsers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClasesPresenciales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Lugar = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, defaultValue: "Estudio de la Academia"),
                    CapacidadMaxima = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Programada"),
                    NotasInstructor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrlMeet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClasesPresenciales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClasesPresenciales_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InscripcionesClases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClasePresencialId = table.Column<int>(type: "int", nullable: false),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstadoAsistencia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pendiente"),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comentario = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InscripcionesClases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InscripcionesClases_AspNetUsers_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InscripcionesClases_ClasesPresenciales_ClasePresencialId",
                        column: x => x.ClasePresencialId,
                        principalTable: "ClasesPresenciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecursosActividades_TipoEntidad_EntidadId",
                table: "RecursosActividades",
                columns: new[] { "TipoEntidad", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Cedula",
                table: "AspNetUsers",
                column: "Cedula");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_WhatsApp",
                table: "AspNetUsers",
                column: "WhatsApp");

            migrationBuilder.CreateIndex(
                name: "IX_ClasesPresenciales_CursoId",
                table: "ClasesPresenciales",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_ClasesPresenciales_Estado",
                table: "ClasesPresenciales",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_ClasesPresenciales_FechaHoraInicio",
                table: "ClasesPresenciales",
                column: "FechaHoraInicio");

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionesClases_ClasePresencialId",
                table: "InscripcionesClases",
                column: "ClasePresencialId");

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionesClases_ClasePresencialId_EstudianteId",
                table: "InscripcionesClases",
                columns: new[] { "ClasePresencialId", "EstudianteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionesClases_EstadoAsistencia",
                table: "InscripcionesClases",
                column: "EstadoAsistencia");

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionesClases_EstudianteId",
                table: "InscripcionesClases",
                column: "EstudianteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InscripcionesClases");

            migrationBuilder.DropTable(
                name: "ClasesPresenciales");

            migrationBuilder.DropIndex(
                name: "IX_RecursosActividades_TipoEntidad_EntidadId",
                table: "RecursosActividades");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Cedula",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_WhatsApp",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TipoCurso",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "AtendidoPor",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Cedula",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CedulaRepresentante",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EsMenorEdad",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ExperienciaTatuaje",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NombreRepresentante",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "TipoEntidad",
                table: "RecursosActividades",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
