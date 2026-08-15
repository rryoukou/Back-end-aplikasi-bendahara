using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be.Migrations
{
    /// <inheritdoc />
    public partial class RefactorV2_AllChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PembayaranIuranKhususs_IuranKhususId",
                table: "PembayaranIuranKhususs");

            migrationBuilder.DropColumn(
                name: "NIS",
                table: "Siswas");

            migrationBuilder.RenameColumn(
                name: "BuktiNotaUrl",
                table: "TransaksiLains",
                newName: "BuktiFoto");

            migrationBuilder.RenameColumn(
                name: "NominalBayar",
                table: "PembayaranIuranKhususs",
                newName: "TotalTerbayar");

            migrationBuilder.RenameColumn(
                name: "TargetNominal",
                table: "IuranKhususs",
                newName: "TargetNominalPerSiswa");

            migrationBuilder.RenameColumn(
                name: "NamaEvent",
                table: "IuranKhususs",
                newName: "JudulIuran");

            migrationBuilder.AddColumn<string>(
                name: "BulanPeriode",
                table: "TransaksiKas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MingguKe",
                table: "TransaksiKas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TanggalBayarSpec",
                table: "TransaksiKas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoAbsen",
                table: "Siswas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Keterangan",
                table: "IuranKhususs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TanggalMulai",
                table: "IuranKhususs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsCustomOverride",
                table: "HariLiburs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnboarded",
                table: "Bendaharas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ExclusionIuranSiswas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IuranKhususId = table.Column<Guid>(type: "uuid", nullable: false),
                    SiswaId = table.Column<Guid>(type: "uuid", nullable: false),
                    TanggalDikecualikan = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Alasan = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExclusionIuranSiswas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExclusionIuranSiswas_IuranKhususs_IuranKhususId",
                        column: x => x.IuranKhususId,
                        principalTable: "IuranKhususs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExclusionIuranSiswas_Siswas_SiswaId",
                        column: x => x.SiswaId,
                        principalTable: "Siswas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PembayaranIuranKhususs_IuranKhususId_SiswaId",
                table: "PembayaranIuranKhususs",
                columns: new[] { "IuranKhususId", "SiswaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExclusionIuranSiswas_IuranKhususId_SiswaId",
                table: "ExclusionIuranSiswas",
                columns: new[] { "IuranKhususId", "SiswaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExclusionIuranSiswas_SiswaId",
                table: "ExclusionIuranSiswas",
                column: "SiswaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExclusionIuranSiswas");

            migrationBuilder.DropIndex(
                name: "IX_PembayaranIuranKhususs_IuranKhususId_SiswaId",
                table: "PembayaranIuranKhususs");

            migrationBuilder.DropColumn(
                name: "BulanPeriode",
                table: "TransaksiKas");

            migrationBuilder.DropColumn(
                name: "MingguKe",
                table: "TransaksiKas");

            migrationBuilder.DropColumn(
                name: "TanggalBayarSpec",
                table: "TransaksiKas");

            migrationBuilder.DropColumn(
                name: "NoAbsen",
                table: "Siswas");

            migrationBuilder.DropColumn(
                name: "Keterangan",
                table: "IuranKhususs");

            migrationBuilder.DropColumn(
                name: "TanggalMulai",
                table: "IuranKhususs");

            migrationBuilder.DropColumn(
                name: "IsCustomOverride",
                table: "HariLiburs");

            migrationBuilder.DropColumn(
                name: "IsOnboarded",
                table: "Bendaharas");

            migrationBuilder.RenameColumn(
                name: "BuktiFoto",
                table: "TransaksiLains",
                newName: "BuktiNotaUrl");

            migrationBuilder.RenameColumn(
                name: "TotalTerbayar",
                table: "PembayaranIuranKhususs",
                newName: "NominalBayar");

            migrationBuilder.RenameColumn(
                name: "TargetNominalPerSiswa",
                table: "IuranKhususs",
                newName: "TargetNominal");

            migrationBuilder.RenameColumn(
                name: "JudulIuran",
                table: "IuranKhususs",
                newName: "NamaEvent");

            migrationBuilder.AddColumn<string>(
                name: "NIS",
                table: "Siswas",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PembayaranIuranKhususs_IuranKhususId",
                table: "PembayaranIuranKhususs",
                column: "IuranKhususId");
        }
    }
}
