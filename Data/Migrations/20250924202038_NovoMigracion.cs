using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace proyectoIngSoft.Data.Migrations
{
    /// <inheritdoc />
    public partial class NovoMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "T_Usuarios",
                newName: "IdUser");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "t_Paternidad",
                newName: "IdPater");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "t_Maternidad",
                newName: "IdMater");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "t_Fallecimiento",
                newName: "IdFallec");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "t_EnfermedadFamiliar",
                newName: "IdEnfermedadFam");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "t_Enfermedad",
                newName: "IdEnfermedad");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "t_Accidente",
                newName: "IdAccidente");

            migrationBuilder.CreateTable(
                name: "t_TiposDescanso",
                columns: table => new
                {
                    IdTDescanso = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_TiposDescanso", x => x.IdTDescanso);
                });

            migrationBuilder.CreateTable(
                name: "ValidarDatos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DNI = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Ubigeo = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Captcha = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidarDatos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "t_Descanso",
                columns: table => new
                {
                    IdDescanso = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TipoDescansoId = table.Column<int>(type: "integer", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccidenteId = table.Column<int>(type: "integer", nullable: true),
                    MaternidadId = table.Column<int>(type: "integer", nullable: true),
                    PaternidadId = table.Column<int>(type: "integer", nullable: true),
                    EnfermedadId = table.Column<int>(type: "integer", nullable: true),
                    FallecimientoId = table.Column<int>(type: "integer", nullable: true),
                    EnfermedadFamId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_Descanso", x => x.IdDescanso);
                    table.ForeignKey(
                        name: "FK_t_Descanso_T_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "T_Usuarios",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_Descanso_t_Accidente_AccidenteId",
                        column: x => x.AccidenteId,
                        principalTable: "t_Accidente",
                        principalColumn: "IdAccidente");
                    table.ForeignKey(
                        name: "FK_t_Descanso_t_EnfermedadFamiliar_EnfermedadFamId",
                        column: x => x.EnfermedadFamId,
                        principalTable: "t_EnfermedadFamiliar",
                        principalColumn: "IdEnfermedadFam");
                    table.ForeignKey(
                        name: "FK_t_Descanso_t_Enfermedad_EnfermedadId",
                        column: x => x.EnfermedadId,
                        principalTable: "t_Enfermedad",
                        principalColumn: "IdEnfermedad");
                    table.ForeignKey(
                        name: "FK_t_Descanso_t_Fallecimiento_FallecimientoId",
                        column: x => x.FallecimientoId,
                        principalTable: "t_Fallecimiento",
                        principalColumn: "IdFallec");
                    table.ForeignKey(
                        name: "FK_t_Descanso_t_Maternidad_MaternidadId",
                        column: x => x.MaternidadId,
                        principalTable: "t_Maternidad",
                        principalColumn: "IdMater");
                    table.ForeignKey(
                        name: "FK_t_Descanso_t_Paternidad_PaternidadId",
                        column: x => x.PaternidadId,
                        principalTable: "t_Paternidad",
                        principalColumn: "IdPater");
                    table.ForeignKey(
                        name: "FK_t_Descanso_t_TiposDescanso_TipoDescansoId",
                        column: x => x.TipoDescansoId,
                        principalTable: "t_TiposDescanso",
                        principalColumn: "IdTDescanso",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosMedicos",
                columns: table => new
                {
                    IdDocumento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DescansoId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Tamaño = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Archivo = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosMedicos", x => x.IdDocumento);
                    table.ForeignKey(
                        name: "FK_DocumentosMedicos_t_Descanso_DescansoId",
                        column: x => x.DescansoId,
                        principalTable: "t_Descanso",
                        principalColumn: "IdDescanso",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "t_TiposDescanso",
                columns: new[] { "IdTDescanso", "Nombre" },
                values: new object[,]
                {
                    { 1, "Enfermedad" },
                    { 2, "Maternidad" },
                    { 3, "Paternidad" },
                    { 4, "Fallecimiento Familiar" },
                    { 5, "Enfermedad Familiar" },
                    { 6, "Accidente" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosMedicos_DescansoId",
                table: "DocumentosMedicos",
                column: "DescansoId");

            migrationBuilder.CreateIndex(
                name: "IX_t_Descanso_AccidenteId",
                table: "t_Descanso",
                column: "AccidenteId");

            migrationBuilder.CreateIndex(
                name: "IX_t_Descanso_EnfermedadFamId",
                table: "t_Descanso",
                column: "EnfermedadFamId");

            migrationBuilder.CreateIndex(
                name: "IX_t_Descanso_EnfermedadId",
                table: "t_Descanso",
                column: "EnfermedadId");

            migrationBuilder.CreateIndex(
                name: "IX_t_Descanso_FallecimientoId",
                table: "t_Descanso",
                column: "FallecimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_t_Descanso_MaternidadId",
                table: "t_Descanso",
                column: "MaternidadId");

            migrationBuilder.CreateIndex(
                name: "IX_t_Descanso_PaternidadId",
                table: "t_Descanso",
                column: "PaternidadId");

            migrationBuilder.CreateIndex(
                name: "IX_t_Descanso_TipoDescansoId",
                table: "t_Descanso",
                column: "TipoDescansoId");

            migrationBuilder.CreateIndex(
                name: "IX_t_Descanso_UserId",
                table: "t_Descanso",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentosMedicos");

            migrationBuilder.DropTable(
                name: "ValidarDatos");

            migrationBuilder.DropTable(
                name: "t_Descanso");

            migrationBuilder.DropTable(
                name: "t_TiposDescanso");

            migrationBuilder.RenameColumn(
                name: "IdUser",
                table: "T_Usuarios",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdPater",
                table: "t_Paternidad",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdMater",
                table: "t_Maternidad",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdFallec",
                table: "t_Fallecimiento",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdEnfermedadFam",
                table: "t_EnfermedadFamiliar",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdEnfermedad",
                table: "t_Enfermedad",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IdAccidente",
                table: "t_Accidente",
                newName: "Id");
        }
    }
}
