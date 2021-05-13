using Microsoft.EntityFrameworkCore.Migrations;

namespace JobService.Service.Migrations
{
    public partial class v720update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Instances",
                table: "JobTypeSaga",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Instances",
                table: "JobTypeSaga");
        }
    }
}
