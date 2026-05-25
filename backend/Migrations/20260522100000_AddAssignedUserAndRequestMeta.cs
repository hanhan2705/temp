using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    public partial class AddAssignedUserAndRequestMeta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "id_nv_su_dung",
                table: "thiet_bi",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "id_nv_gui",
                table: "yeu_cau",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ly_do",
                table: "yeu_cau",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_thiet_bi_id_nv_su_dung",
                table: "thiet_bi",
                column: "id_nv_su_dung");

            migrationBuilder.CreateIndex(
                name: "IX_yeu_cau_id_nv_gui",
                table: "yeu_cau",
                column: "id_nv_gui");

            migrationBuilder.AddForeignKey(
                name: "FK_thiet_bi_nhan_vien_id_nv_su_dung",
                table: "thiet_bi",
                column: "id_nv_su_dung",
                principalTable: "nhan_vien",
                principalColumn: "id_nv");

            migrationBuilder.AddForeignKey(
                name: "FK_yeu_cau_nhan_vien_id_nv_gui",
                table: "yeu_cau",
                column: "id_nv_gui",
                principalTable: "nhan_vien",
                principalColumn: "id_nv");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_thiet_bi_nhan_vien_id_nv_su_dung", table: "thiet_bi");
            migrationBuilder.DropForeignKey(name: "FK_yeu_cau_nhan_vien_id_nv_gui", table: "yeu_cau");
            migrationBuilder.DropIndex(name: "IX_thiet_bi_id_nv_su_dung", table: "thiet_bi");
            migrationBuilder.DropIndex(name: "IX_yeu_cau_id_nv_gui", table: "yeu_cau");
            migrationBuilder.DropColumn(name: "id_nv_su_dung", table: "thiet_bi");
            migrationBuilder.DropColumn(name: "id_nv_gui", table: "yeu_cau");
            migrationBuilder.DropColumn(name: "ly_do", table: "yeu_cau");
        }
    }
}
