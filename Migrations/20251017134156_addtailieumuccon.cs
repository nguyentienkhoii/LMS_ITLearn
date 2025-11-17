using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang_CKC.Migrations
{
    public partial class addtailieumuccon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Muc_BAI_BaiId",
                table: "Muc");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Muc",
                table: "Muc");

            migrationBuilder.RenameTable(
                name: "Muc",
                newName: "MUCCON");

            migrationBuilder.RenameIndex(
                name: "IX_Muc_BaiId",
                table: "MUCCON",
                newName: "IX_MUCCON_BaiId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MUCCON",
                table: "MUCCON",
                column: "MucId");

            migrationBuilder.CreateTable(
                name: "TAILIEU",
                columns: table => new
                {
                    MaTaiLieu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileTaiLieu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MaMucCon = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAILIEU", x => x.MaTaiLieu);
                    table.ForeignKey(
                        name: "FK_TAILIEU_MUCCON_MaMucCon",
                        column: x => x.MaMucCon,
                        principalTable: "MUCCON",
                        principalColumn: "MucId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TAILIEU_MaMucCon",
                table: "TAILIEU",
                column: "MaMucCon");

            migrationBuilder.AddForeignKey(
                name: "FK_MUCCON_BAI_BaiId",
                table: "MUCCON",
                column: "BaiId",
                principalTable: "BAI",
                principalColumn: "BaiId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MUCCON_BAI_BaiId",
                table: "MUCCON");

            migrationBuilder.DropTable(
                name: "TAILIEU");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MUCCON",
                table: "MUCCON");

            migrationBuilder.RenameTable(
                name: "MUCCON",
                newName: "Muc");

            migrationBuilder.RenameIndex(
                name: "IX_MUCCON_BaiId",
                table: "Muc",
                newName: "IX_Muc_BaiId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Muc",
                table: "Muc",
                column: "MucId");

            migrationBuilder.AddForeignKey(
                name: "FK_Muc_BAI_BaiId",
                table: "Muc",
                column: "BaiId",
                principalTable: "BAI",
                principalColumn: "BaiId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
