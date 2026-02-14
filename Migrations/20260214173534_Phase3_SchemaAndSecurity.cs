using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telemedicine.API.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_SchemaAndSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "tbl_Users");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "tbl_Users",
                newName: "IX_tbl_Users_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tbl_Users",
                table: "tbl_Users",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "tbl_Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Appointments_tbl_Users_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tbl_Appointments_tbl_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Audits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionBy = table.Column<int>(type: "int", nullable: true),
                    ActionStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Audits_tbl_Users_ActionBy",
                        column: x => x.ActionBy,
                        principalTable: "tbl_Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_ConsultationRooms",
                columns: table => new
                {
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_ConsultationRooms", x => x.RoomId);
                    table.ForeignKey(
                        name: "FK_tbl_ConsultationRooms_tbl_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "tbl_Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_DoctorNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_DoctorNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_DoctorNotes_tbl_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "tbl_Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Chats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Chats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Chats_tbl_ConsultationRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "tbl_ConsultationRooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_Chats_tbl_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "tbl_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Appointments_DoctorId",
                table: "tbl_Appointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Appointments_PatientId",
                table: "tbl_Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Audits_ActionBy",
                table: "tbl_Audits",
                column: "ActionBy");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Chats_RoomId",
                table: "tbl_Chats",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Chats_SenderId",
                table: "tbl_Chats",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_ConsultationRooms_AppointmentId",
                table: "tbl_ConsultationRooms",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_DoctorNotes_AppointmentId",
                table: "tbl_DoctorNotes",
                column: "AppointmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_Audits");

            migrationBuilder.DropTable(
                name: "tbl_Chats");

            migrationBuilder.DropTable(
                name: "tbl_DoctorNotes");

            migrationBuilder.DropTable(
                name: "tbl_ConsultationRooms");

            migrationBuilder.DropTable(
                name: "tbl_Appointments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tbl_Users",
                table: "tbl_Users");

            migrationBuilder.RenameTable(
                name: "tbl_Users",
                newName: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_tbl_Users_Email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");
        }
    }
}
