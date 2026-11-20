using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qms.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedDCRworkflowtables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DCRRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Standard = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: true),
                    DocumentTitle = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DocumentNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CurrentRevisionNumber = table.Column<int>(type: "int", nullable: true),
                    NewRevisionNumber = table.Column<int>(type: "int", nullable: true),
                    EffectiveDateofImplementation = table.Column<DateOnly>(type: "date", nullable: true),
                    ProcessOwnerId = table.Column<int>(type: "int", nullable: true),
                    ApprovalDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DescriptionBeforechange = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DescriptionAfterchange = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Reasonforchange = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AttachmentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdatedIntoSystem = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DCRRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DCRRequests_AppUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DCRRequests_AppUsers_ProcessOwnerId",
                        column: x => x.ProcessOwnerId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DCRRequests_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "DocumentTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DCRRequests_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DCRRequests_QMSDepartments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "QMSDepartments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkFlowInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DCRRequestId = table.Column<int>(type: "int", nullable: false),
                    DepartmentApprovalMappingId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFlowInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkFlowInstances_DCRRequests_DCRRequestId",
                        column: x => x.DCRRequestId,
                        principalTable: "DCRRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkFlowInstances_DepartmentApprovalMappings_DepartmentApprovalMappingId",
                        column: x => x.DepartmentApprovalMappingId,
                        principalTable: "DepartmentApprovalMappings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false),
                    StageOrder = table.Column<int>(type: "int", nullable: false),
                    DepartmentApprovalLevelId = table.Column<int>(type: "int", nullable: false),
                    ApproverId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionTakenAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowStages_AppUsers_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowStages_DepartmentApprovalLevels_DepartmentApprovalLevelId",
                        column: x => x.DepartmentApprovalLevelId,
                        principalTable: "DepartmentApprovalLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowStages_WorkFlowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkFlowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowStageId = table.Column<int>(type: "int", nullable: false),
                    ActionByUserId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ActionAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowActions_AppUsers_ActionByUserId",
                        column: x => x.ActionByUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowActions_WorkflowStages_WorkflowStageId",
                        column: x => x.WorkflowStageId,
                        principalTable: "WorkflowStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DCRRequests_CreatedByUserId",
                table: "DCRRequests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DCRRequests_DepartmentId",
                table: "DCRRequests",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DCRRequests_DocumentId",
                table: "DCRRequests",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DCRRequests_DocumentTypeId",
                table: "DCRRequests",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DCRRequests_ProcessOwnerId",
                table: "DCRRequests",
                column: "ProcessOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DCRRequests_ReferenceNumber",
                table: "DCRRequests",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowActions_ActionByUserId",
                table: "WorkflowActions",
                column: "ActionByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowActions_WorkflowStageId",
                table: "WorkflowActions",
                column: "WorkflowStageId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlowInstances_DCRRequestId",
                table: "WorkFlowInstances",
                column: "DCRRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlowInstances_DepartmentApprovalMappingId",
                table: "WorkFlowInstances",
                column: "DepartmentApprovalMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStages_ApproverId",
                table: "WorkflowStages",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStages_DepartmentApprovalLevelId",
                table: "WorkflowStages",
                column: "DepartmentApprovalLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStages_WorkflowInstanceId_StageOrder",
                table: "WorkflowStages",
                columns: new[] { "WorkflowInstanceId", "StageOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowActions");

            migrationBuilder.DropTable(
                name: "WorkflowStages");

            migrationBuilder.DropTable(
                name: "WorkFlowInstances");

            migrationBuilder.DropTable(
                name: "DCRRequests");
        }
    }
}
