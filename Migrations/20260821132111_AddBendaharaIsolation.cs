using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be.Migrations
{
    /// <inheritdoc />
    public partial class AddBendaharaIsolation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Langkah 1: Tambah kolom BendaharaId tanpa FK dulu ──────────
            // Default Guid.Empty agar tidak error saat insert kolom ke row existing.
            // Akan di-populate ke bendahara pertama di langkah 2, lalu FK ditambah.

            migrationBuilder.AddColumn<Guid>(
                name: "BendaharaId",
                table: "TransaksiLains",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BendaharaId",
                table: "TransaksiKas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BendaharaId",
                table: "Siswas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BendaharaId",
                table: "PengaturanKas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BendaharaId",
                table: "IuranKhususs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BendaharaId",
                table: "HariLiburs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // ── Langkah 2: Assign semua data existing ke bendahara pertama ──
            // Jika database kosong, SQL ini tidak melakukan apa-apa (safe).
            migrationBuilder.Sql(@"
                DO $$
                DECLARE first_id uuid;
                BEGIN
                    SELECT ""Id"" INTO first_id FROM ""Bendaharas"" ORDER BY ""CreatedAt"" LIMIT 1;
                    IF first_id IS NOT NULL THEN
                        UPDATE ""Siswas""        SET ""BendaharaId"" = first_id WHERE ""BendaharaId"" = '00000000-0000-0000-0000-000000000000';
                        UPDATE ""PengaturanKas"" SET ""BendaharaId"" = first_id WHERE ""BendaharaId"" = '00000000-0000-0000-0000-000000000000';
                        UPDATE ""HariLiburs""    SET ""BendaharaId"" = first_id WHERE ""BendaharaId"" = '00000000-0000-0000-0000-000000000000';
                        UPDATE ""TransaksiKas""  SET ""BendaharaId"" = first_id WHERE ""BendaharaId"" = '00000000-0000-0000-0000-000000000000';
                        UPDATE ""TransaksiLains"" SET ""BendaharaId"" = first_id WHERE ""BendaharaId"" = '00000000-0000-0000-0000-000000000000';
                        UPDATE ""IuranKhususs""  SET ""BendaharaId"" = first_id WHERE ""BendaharaId"" = '00000000-0000-0000-0000-000000000000';
                    END IF;
                END $$;
            ");

            // ── Langkah 3: Tambah index ──────────────────────────────────────
            migrationBuilder.CreateIndex(
                name: "IX_TransaksiLains_BendaharaId",
                table: "TransaksiLains",
                column: "BendaharaId");

            migrationBuilder.CreateIndex(
                name: "IX_TransaksiKas_BendaharaId",
                table: "TransaksiKas",
                column: "BendaharaId");

            migrationBuilder.CreateIndex(
                name: "IX_Siswas_BendaharaId",
                table: "Siswas",
                column: "BendaharaId");

            migrationBuilder.CreateIndex(
                name: "IX_PengaturanKas_BendaharaId",
                table: "PengaturanKas",
                column: "BendaharaId");

            migrationBuilder.CreateIndex(
                name: "IX_IuranKhususs_BendaharaId",
                table: "IuranKhususs",
                column: "BendaharaId");

            migrationBuilder.CreateIndex(
                name: "IX_HariLiburs_BendaharaId",
                table: "HariLiburs",
                column: "BendaharaId");

            migrationBuilder.AddForeignKey(
                name: "FK_HariLiburs_Bendaharas_BendaharaId",
                table: "HariLiburs",
                column: "BendaharaId",
                principalTable: "Bendaharas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IuranKhususs_Bendaharas_BendaharaId",
                table: "IuranKhususs",
                column: "BendaharaId",
                principalTable: "Bendaharas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PengaturanKas_Bendaharas_BendaharaId",
                table: "PengaturanKas",
                column: "BendaharaId",
                principalTable: "Bendaharas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Siswas_Bendaharas_BendaharaId",
                table: "Siswas",
                column: "BendaharaId",
                principalTable: "Bendaharas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksiKas_Bendaharas_BendaharaId",
                table: "TransaksiKas",
                column: "BendaharaId",
                principalTable: "Bendaharas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksiLains_Bendaharas_BendaharaId",
                table: "TransaksiLains",
                column: "BendaharaId",
                principalTable: "Bendaharas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HariLiburs_Bendaharas_BendaharaId",
                table: "HariLiburs");

            migrationBuilder.DropForeignKey(
                name: "FK_IuranKhususs_Bendaharas_BendaharaId",
                table: "IuranKhususs");

            migrationBuilder.DropForeignKey(
                name: "FK_PengaturanKas_Bendaharas_BendaharaId",
                table: "PengaturanKas");

            migrationBuilder.DropForeignKey(
                name: "FK_Siswas_Bendaharas_BendaharaId",
                table: "Siswas");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksiKas_Bendaharas_BendaharaId",
                table: "TransaksiKas");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksiLains_Bendaharas_BendaharaId",
                table: "TransaksiLains");

            migrationBuilder.DropIndex(
                name: "IX_TransaksiLains_BendaharaId",
                table: "TransaksiLains");

            migrationBuilder.DropIndex(
                name: "IX_TransaksiKas_BendaharaId",
                table: "TransaksiKas");

            migrationBuilder.DropIndex(
                name: "IX_Siswas_BendaharaId",
                table: "Siswas");

            migrationBuilder.DropIndex(
                name: "IX_PengaturanKas_BendaharaId",
                table: "PengaturanKas");

            migrationBuilder.DropIndex(
                name: "IX_IuranKhususs_BendaharaId",
                table: "IuranKhususs");

            migrationBuilder.DropIndex(
                name: "IX_HariLiburs_BendaharaId",
                table: "HariLiburs");

            migrationBuilder.DropColumn(
                name: "BendaharaId",
                table: "TransaksiLains");

            migrationBuilder.DropColumn(
                name: "BendaharaId",
                table: "TransaksiKas");

            migrationBuilder.DropColumn(
                name: "BendaharaId",
                table: "Siswas");

            migrationBuilder.DropColumn(
                name: "BendaharaId",
                table: "PengaturanKas");

            migrationBuilder.DropColumn(
                name: "BendaharaId",
                table: "IuranKhususs");

            migrationBuilder.DropColumn(
                name: "BendaharaId",
                table: "HariLiburs");
        }
    }
}
