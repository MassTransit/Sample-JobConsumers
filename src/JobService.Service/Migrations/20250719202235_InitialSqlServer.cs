using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobService.Service.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobSaga",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<int>(type: "int", nullable: false),
                    Submitted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ServiceAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobTimeout = table.Column<TimeSpan>(type: "time", nullable: true),
                    Job = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RetryAttempt = table.Column<int>(type: "int", nullable: false),
                    Started = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Completed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: true),
                    Faulted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobSlotWaitToken = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobRetryDelayToken = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IncompleteAttempts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastProgressValue = table.Column<long>(type: "bigint", nullable: true),
                    LastProgressLimit = table.Column<long>(type: "bigint", nullable: true),
                    LastProgressSequenceNumber = table.Column<long>(type: "bigint", nullable: true),
                    JobState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobProperties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CronExpression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeZoneId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NextStartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSaga", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "JobTypeSaga",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<int>(type: "int", nullable: false),
                    ActiveJobCount = table.Column<int>(type: "int", nullable: false),
                    ConcurrentJobLimit = table.Column<int>(type: "int", nullable: false),
                    OverrideJobLimit = table.Column<int>(type: "int", nullable: true),
                    OverrideLimitExpiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActiveJobs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instances = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GlobalConcurrentJobLimit = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTypeSaga", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "JobAttemptSaga",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<int>(type: "int", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RetryAttempt = table.Column<int>(type: "int", nullable: false),
                    ServiceAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstanceAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Started = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Faulted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusCheckTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobAttemptSaga", x => x.CorrelationId);
                    table.ForeignKey(
                        name: "FK_JobAttemptSaga_JobSaga_JobId",
                        column: x => x.JobId,
                        principalTable: "JobSaga",
                        principalColumn: "CorrelationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobAttemptSaga_JobId_RetryAttempt",
                table: "JobAttemptSaga",
                columns: new[] { "JobId", "RetryAttempt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobAttemptSaga");

            migrationBuilder.DropTable(
                name: "JobTypeSaga");

            migrationBuilder.DropTable(
                name: "JobSaga");
        }
    }
}
