using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demosuelos.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposTrazabilidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaEnsayo",
                table: "EnsayosRealizados");

            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "PuntosMuestreo",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsCalculado",
                table: "ParametrosEnsayo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaRecepcion",
                table: "Muestras",
                type: "date",
                nullable: false,
                defaultValueSql: "CAST(GETDATE() AS date)",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaMuestreo",
                table: "Muestras",
                type: "date",
                nullable: false,
                defaultValueSql: "CAST(GETDATE() AS date)");

            migrationBuilder.AddColumn<decimal>(
                name: "ProfundidadFinal",
                table: "Muestras",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProfundidadInicial",
                table: "Muestras",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAsignacion",
                table: "EnsayosRealizados",
                type: "date",
                nullable: false,
                defaultValueSql: "CAST(GETDATE() AS date)");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEjecucion",
                table: "EnsayosRealizados",
                type: "date",
                nullable: false,
                defaultValueSql: "CAST(GETDATE() AS date)");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaValidacion",
                table: "EnsayosRealizados",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sector",
                table: "PuntosMuestreo");

            migrationBuilder.DropColumn(
                name: "EsCalculado",
                table: "ParametrosEnsayo");

            migrationBuilder.DropColumn(
                name: "FechaMuestreo",
                table: "Muestras");

            migrationBuilder.DropColumn(
                name: "ProfundidadFinal",
                table: "Muestras");

            migrationBuilder.DropColumn(
                name: "ProfundidadInicial",
                table: "Muestras");

            migrationBuilder.DropColumn(
                name: "FechaAsignacion",
                table: "EnsayosRealizados");

            migrationBuilder.DropColumn(
                name: "FechaEjecucion",
                table: "EnsayosRealizados");

            migrationBuilder.DropColumn(
                name: "FechaValidacion",
                table: "EnsayosRealizados");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaRecepcion",
                table: "Muestras",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "CAST(GETDATE() AS date)");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEnsayo",
                table: "EnsayosRealizados",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
