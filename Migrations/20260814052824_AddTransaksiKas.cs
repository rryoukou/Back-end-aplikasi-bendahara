using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be.Migrations
{
    /// <inheritdoc />
    public partial class AddTransaksiKas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransaksiKas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SiswaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nominal = table.Column<decimal>(type: "numeric", nullable: false),
                    TanggalBayar = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Keterangan = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransaksiKas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransaksiKas_Siswas_SiswaId",
                        column: x => x.SiswaId,
                        principalTable: "Siswas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransaksiKas_SiswaId",
                table: "TransaksiKas",
                column: "SiswaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransaksiKas");
        }
    }
}
