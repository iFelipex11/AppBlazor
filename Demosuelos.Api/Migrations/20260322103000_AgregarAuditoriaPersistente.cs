using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demosuelos.Api.Migrations
{
    public partial class AgregarAuditoriaPersistente : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddAuditColumns(migrationBuilder, "AspNetUsers");
            AddAuditColumns(migrationBuilder, "Proyectos");
            AddAuditColumns(migrationBuilder, "PuntosMuestreo");
            AddAuditColumns(migrationBuilder, "Muestras");
            AddAuditColumns(migrationBuilder, "TiposEnsayo");
            AddAuditColumns(migrationBuilder, "ParametrosEnsayo");
            AddAuditColumns(migrationBuilder, "EnsayosRealizados");
            AddAuditColumns(migrationBuilder, "ResultadosParametro");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropAuditColumns(migrationBuilder, "ResultadosParametro");
            DropAuditColumns(migrationBuilder, "EnsayosRealizados");
            DropAuditColumns(migrationBuilder, "ParametrosEnsayo");
            DropAuditColumns(migrationBuilder, "TiposEnsayo");
            DropAuditColumns(migrationBuilder, "Muestras");
            DropAuditColumns(migrationBuilder, "PuntosMuestreo");
            DropAuditColumns(migrationBuilder, "Proyectos");
            DropAuditColumns(migrationBuilder, "AspNetUsers");
        }

        private static void AddAuditColumns(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActualizadoPor",
                table: table,
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                table: table,
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: table,
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: table,
                type: "datetime2",
                nullable: true);
        }

        private static void DropAuditColumns(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.DropColumn(
                name: "ActualizadoPor",
                table: table);

            migrationBuilder.DropColumn(
                name: "CreadoPor",
                table: table);

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: table);

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: table);
        }
    }
}
