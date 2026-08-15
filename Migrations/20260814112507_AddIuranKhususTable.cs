using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be.Migrations
{
    /// <inheritdoc />
    public partial class AddIuranKhususTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IuranKhususs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NamaEvent = table.Column<string>(type: "text", nullable: false),
                    TargetNominal = table.Column<decimal>(type: "numeric", nullable: false),
                    TanggalDibuat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IuranKhususs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PembayaranIuranKhususs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IuranKhususId = table.Column<Guid>(type: "uuid", nullable: false),
                    SiswaId = table.Column<Guid>(type: "uuid", nullable: false),
                    NominalBayar = table.Column<decimal>(type: "numeric", nullable: false),
                    TanggalBayar = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsLunas = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PembayaranIuranKhususs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PembayaranIuranKhususs_IuranKhususs_IuranKhususId",
                        column: x => x.IuranKhususId,
                        principalTable: "IuranKhususs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PembayaranIuranKhususs_Siswas_SiswaId",
                        column: x => x.SiswaId,
                        principalTable: "Siswas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PembayaranIuranKhususs_IuranKhususId",
                table: "PembayaranIuranKhususs",
                column: "IuranKhususId");

            migrationBuilder.CreateIndex(
                name: "IX_PembayaranIuranKhususs_SiswaId",
                table: "PembayaranIuranKhususs",
                column: "SiswaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PembayaranIuranKhususs");

            migrationBuilder.DropTable(
                name: "IuranKhususs");
        }
    }
}
