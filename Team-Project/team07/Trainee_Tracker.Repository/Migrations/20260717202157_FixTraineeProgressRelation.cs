using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trainee_Tracker.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FixTraineeProgressRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TraineeProgress_AspNetUsers_TraineeId1",
                table: "TraineeProgress");

            migrationBuilder.DropIndex(
                name: "IX_TraineeProgress_TraineeId1",
                table: "TraineeProgress");

            migrationBuilder.DropColumn(
                name: "TraineeId1",
                table: "TraineeProgress");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProgress_TraineeId",
                table: "TraineeProgress",
                column: "TraineeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeProgress_AspNetUsers_TraineeId",
                table: "TraineeProgress",
                column: "TraineeId",
                principalTable: "AspNetUsers",
                principalColumn: "guid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TraineeProgress_AspNetUsers_TraineeId",
                table: "TraineeProgress");

            migrationBuilder.DropIndex(
                name: "IX_TraineeProgress_TraineeId",
                table: "TraineeProgress");

            migrationBuilder.AddColumn<string>(
                name: "TraineeId1",
                table: "TraineeProgress",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProgress_TraineeId1",
                table: "TraineeProgress",
                column: "TraineeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeProgress_AspNetUsers_TraineeId1",
                table: "TraineeProgress",
                column: "TraineeId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
