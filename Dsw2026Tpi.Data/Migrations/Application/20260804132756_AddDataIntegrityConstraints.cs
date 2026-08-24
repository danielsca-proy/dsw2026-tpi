using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsw2026Tpi.Data.Migrations.Application
{
    /// <inheritdoc />
    public partial class AddDataIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AvailabilitySlots_DoctorId_Start",
                table: "AvailabilitySlots");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_AvailabilitySlotId",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "UX_Specialities_Name",
                table: "Specialities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_AvailabilitySlots_DoctorId_Start",
                table: "AvailabilitySlots",
                columns: new[] { "DoctorId", "Start" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Appointments_AvailabilitySlotId_Booked",
                table: "Appointments",
                column: "AvailabilitySlotId",
                unique: true,
                filter: "[Status] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Specialities_Name",
                table: "Specialities");

            migrationBuilder.DropIndex(
                name: "UX_AvailabilitySlots_DoctorId_Start",
                table: "AvailabilitySlots");

            migrationBuilder.DropIndex(
                name: "UX_Appointments_AvailabilitySlotId_Booked",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilitySlots_DoctorId_Start",
                table: "AvailabilitySlots",
                columns: new[] { "DoctorId", "Start" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AvailabilitySlotId",
                table: "Appointments",
                column: "AvailabilitySlotId");
        }
    }
}
