using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobService.Service.Migrations
{
    /// <inheritdoc />
    public partial class RecurringJobConsumerUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GlobalConcurrentJobLimit",
                table: "JobTypeSaga",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Properties",
                table: "JobTypeSaga",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CronExpression",
                table: "JobSaga",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EndDate",
                table: "JobSaga",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncompleteAttempts",
                table: "JobSaga",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobProperties",
                table: "JobSaga",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobState",
                table: "JobSaga",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastProgressLimit",
                table: "JobSaga",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastProgressSequenceNumber",
                table: "JobSaga",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastProgressValue",
                table: "JobSaga",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextStartDate",
                table: "JobSaga",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartDate",
                table: "JobSaga",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "JobSaga",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JobAttemptSaga_JobSaga_JobId",
                table: "JobAttemptSaga",
                column: "JobId",
                principalTable: "JobSaga",
                principalColumn: "CorrelationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobAttemptSaga_JobSaga_JobId",
                table: "JobAttemptSaga");

            migrationBuilder.DropColumn(
                name: "GlobalConcurrentJobLimit",
                table: "JobTypeSaga");

            migrationBuilder.DropColumn(
                name: "Properties",
                table: "JobTypeSaga");

            migrationBuilder.DropColumn(
                name: "CronExpression",
                table: "JobSaga");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "JobSaga");

            migrationBuilder.DropColumn(
                name: "IncompleteAttempts",
                table: "JobSaga");

            migrationBuilder.DropColumn(
                name: "JobProperties",
                table: "JobSaga");

            migrationBuilder.DropColumn(
                name: "JobState",
                table: "JobSaga");

            migrationBuilder.DropColumn(
                name: "LastProgressLimit",
                table: "JobSaga");

            migrationBuilder.DropColumn(
                name: "LastProgressSequenceNumber",
                table: "JobSaga");

            migrationBuilder.DropColumn(
                name: "LastProgressValue",
                table: "JobSaga");

            migrationBuilder.DropColumn(
                name: "NextStartDate",
                table: "JobSaga");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "JobSaga");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "JobSaga");
        }
    }
}
