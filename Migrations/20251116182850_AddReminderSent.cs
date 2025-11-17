using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang_CKC.Migrations
{
    public partial class AddReminderSent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReminderSent",
                table: "BAITAP",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderSent",
                table: "BAITAP");
        }
    }
}
