using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang_CKC.Migrations
{
    public partial class AddLateSubmissionAndRemindToGrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LateSubmission",
                table: "BAITAP",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemindToGrade",
                table: "BAITAP",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LateSubmission",
                table: "BAITAP");

            migrationBuilder.DropColumn(
                name: "RemindToGrade",
                table: "BAITAP");
        }
    }
}
