using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobService.Service.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobAttemptSaga",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<int>(type: "integer", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    RetryAttempt = table.Column<int>(type: "integer", nullable: false),
                    ServiceAddress = table.Column<string>(type: "text", nullable: true),
                    InstanceAddress = table.Column<string>(type: "text", nullable: true),
                    Started = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Faulted = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    StatusCheckTokenId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobAttemptSaga", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "JobSaga",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<int>(type: "integer", nullable: false),
                    Submitted = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ServiceAddress = table.Column<string>(type: "text", nullable: true),
                    JobTimeout = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Job = table.Column<string>(type: "text", nullable: true),
                    JobTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    RetryAttempt = table.Column<int>(type: "integer", nullable: false),
                    Started = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Completed = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Faulted = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    JobSlotWaitToken = table.Column<Guid>(type: "uuid", nullable: true),
                    JobRetryDelayToken = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSaga", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "JobTypeSaga",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<int>(type: "integer", nullable: false),
                    ActiveJobCount = table.Column<int>(type: "integer", nullable: false),
                    ConcurrentJobLimit = table.Column<int>(type: "integer", nullable: false),
                    OverrideJobLimit = table.Column<int>(type: "integer", nullable: true),
                    OverrideLimitExpiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ActiveJobs = table.Column<string>(type: "text", nullable: true),
                    Instances = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTypeSaga", x => x.CorrelationId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobAttemptSaga_JobId_RetryAttempt",
                table: "JobAttemptSaga",
                columns: new[] { "JobId", "RetryAttempt" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobAttemptSaga");

            migrationBuilder.DropTable(
                name: "JobSaga");

            migrationBuilder.DropTable(
                name: "JobTypeSaga");
        }
    }
}
