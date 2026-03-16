using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demosuelos.Api.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Proyectos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cliente = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proyectos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposEnsayo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposEnsayo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PuntosMuestreo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProyectoId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CoordenadaX = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CoordenadaY = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuntosMuestreo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PuntosMuestreo_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "Proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParametrosEnsayo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoEnsayoId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Requerido = table.Column<bool>(type: "bit", nullable: false),
                    MinReferencial = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxReferencial = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParametrosEnsayo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParametrosEnsayo_TiposEnsayo_TipoEnsayoId",
                        column: x => x.TipoEnsayoId,
                        principalTable: "TiposEnsayo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Muestras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PuntoMuestreoId = table.Column<int>(type: "int", nullable: false),
                    CodigoMuestra = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaRecepcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoMuestra = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EstadoMuestra = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Muestras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Muestras_PuntosMuestreo_PuntoMuestreoId",
                        column: x => x.PuntoMuestreoId,
                        principalTable: "PuntosMuestreo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnsayosRealizados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MuestraId = table.Column<int>(type: "int", nullable: false),
                    TipoEnsayoId = table.Column<int>(type: "int", nullable: false),
                    FechaEnsayo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Responsable = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnsayosRealizados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnsayosRealizados_Muestras_MuestraId",
                        column: x => x.MuestraId,
                        principalTable: "Muestras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnsayosRealizados_TiposEnsayo_TipoEnsayoId",
                        column: x => x.TipoEnsayoId,
                        principalTable: "TiposEnsayo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResultadosParametro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnsayoRealizadoId = table.Column<int>(type: "int", nullable: false),
                    ParametroEnsayoId = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CumpleRango = table.Column<bool>(type: "bit", nullable: true),
                    ObservacionTecnica = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadosParametro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultadosParametro_EnsayosRealizados_EnsayoRealizadoId",
                        column: x => x.EnsayoRealizadoId,
                        principalTable: "EnsayosRealizados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResultadosParametro_ParametrosEnsayo_ParametroEnsayoId",
                        column: x => x.ParametroEnsayoId,
                        principalTable: "ParametrosEnsayo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnsayosRealizados_MuestraId_TipoEnsayoId",
                table: "EnsayosRealizados",
                columns: new[] { "MuestraId", "TipoEnsayoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnsayosRealizados_TipoEnsayoId",
                table: "EnsayosRealizados",
                column: "TipoEnsayoId");

            migrationBuilder.CreateIndex(
                name: "IX_Muestras_CodigoMuestra",
                table: "Muestras",
                column: "CodigoMuestra",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Muestras_PuntoMuestreoId",
                table: "Muestras",
                column: "PuntoMuestreoId");

            migrationBuilder.CreateIndex(
                name: "IX_ParametrosEnsayo_TipoEnsayoId_Nombre",
                table: "ParametrosEnsayo",
                columns: new[] { "TipoEnsayoId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PuntosMuestreo_ProyectoId_Codigo",
                table: "PuntosMuestreo",
                columns: new[] { "ProyectoId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosParametro_EnsayoRealizadoId_ParametroEnsayoId",
                table: "ResultadosParametro",
                columns: new[] { "EnsayoRealizadoId", "ParametroEnsayoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosParametro_ParametroEnsayoId",
                table: "ResultadosParametro",
                column: "ParametroEnsayoId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposEnsayo_Codigo",
                table: "TiposEnsayo",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResultadosParametro");

            migrationBuilder.DropTable(
                name: "EnsayosRealizados");

            migrationBuilder.DropTable(
                name: "ParametrosEnsayo");

            migrationBuilder.DropTable(
                name: "Muestras");

            migrationBuilder.DropTable(
                name: "TiposEnsayo");

            migrationBuilder.DropTable(
                name: "PuntosMuestreo");

            migrationBuilder.DropTable(
                name: "Proyectos");
        }
    }
}
