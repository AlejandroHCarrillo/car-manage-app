using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCarGarage.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleProcedures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastCost = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleProcedures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleProcedures_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleProcedures_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcedureCompletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcedureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDateCovered = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcedureCompletions_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcedureCompletions_VehicleProcedures_ProcedureId",
                        column: x => x.ProcedureId,
                        principalTable: "VehicleProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureCompletions_CreatedByUserId",
                table: "ProcedureCompletions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureCompletions_ProcedureId",
                table: "ProcedureCompletions",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleProcedures_CreatedByUserId",
                table: "VehicleProcedures",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleProcedures_VehicleId",
                table: "VehicleProcedures",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcedureCompletions");

            migrationBuilder.DropTable(
                name: "VehicleProcedures");
        }
    }
}
