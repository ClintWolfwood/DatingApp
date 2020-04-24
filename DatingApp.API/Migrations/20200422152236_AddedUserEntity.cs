using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DatingApp.API.Migrations
{
  public partial class AddedUserEntity : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {

      migrationBuilder.CreateTable(
          name: "Users",
          columns: table => new
          {
            Id = table.Column<int>(nullable: false)
                  .Annotation("Sqlite:Autoincrement", true),
            Username = table.Column<string>(nullable: true),
            PasswordHash = table.Column<byte[]>(nullable: true),
            PasswordSalt = table.Column<byte[]>(nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Users", x => x.Id);
          });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "Users");

      migrationBuilder.AlterColumn<string>(
          name: "Name",
          table: "Values",
          type: "nvarchar(max)",
          nullable: true,
          oldClrType: typeof(string),
          oldNullable: true);

      migrationBuilder.AlterColumn<int>(
          name: "Id",
          table: "Values",
          type: "int",
          nullable: false,
          oldClrType: typeof(int))
          .Annotation("Sqlite:Autoincrement", true)
          .OldAnnotation("Sqlite:Autoincrement", true);
    }
  }
}
