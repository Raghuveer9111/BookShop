using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookShop.Migrations
{
    /// <inheritdoc />
    public partial class AddrelationBetweenUserAndCardTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "BookId",
                table: "CardItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "CardItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardItems_UserId",
                table: "CardItems",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CardItems_AspNetUsers_UserId",
                table: "CardItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardItems_AspNetUsers_UserId",
                table: "CardItems");

            migrationBuilder.DropIndex(
                name: "IX_CardItems_UserId",
                table: "CardItems");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CardItems");

            migrationBuilder.AlterColumn<int>(
                name: "BookId",
                table: "CardItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
