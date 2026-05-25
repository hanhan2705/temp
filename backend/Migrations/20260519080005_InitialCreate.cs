using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nhan_vien",
                columns: table => new
                {
                    id_nv = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ho_ten = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    vai_tro = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    trang_thai = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nhan_vien", x => x.id_nv);
                });

            migrationBuilder.CreateTable(
                name: "thiet_bi",
                columns: table => new
                {
                    id_tb = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ten_thiet_bi = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    loai = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ngay_mua = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    nguyen_gia = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    trang_thai = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_thiet_bi", x => x.id_tb);
                });

            migrationBuilder.CreateTable(
                name: "yeu_cau",
                columns: table => new
                {
                    id_yc = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_nv = table.Column<long>(type: "bigint", nullable: false),
                    loai_yeu_cau = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ngay_gui = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    trang_thai_duyet = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_yeu_cau", x => x.id_yc);
                    table.ForeignKey(
                        name: "FK_yeu_cau_nhan_vien_id_nv",
                        column: x => x.id_nv,
                        principalTable: "nhan_vien",
                        principalColumn: "id_nv",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "khau_hao",
                columns: table => new
                {
                    id_kh = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_tb = table.Column<long>(type: "bigint", nullable: false),
                    gia_tri_con_lai = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    phuong_phap_tinh = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khau_hao", x => x.id_kh);
                    table.ForeignKey(
                        name: "FK_khau_hao_thiet_bi_id_tb",
                        column: x => x.id_tb,
                        principalTable: "thiet_bi",
                        principalColumn: "id_tb",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lich_su_cap_phat",
                columns: table => new
                {
                    id_cp = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_nv = table.Column<long>(type: "bigint", nullable: false),
                    id_tb = table.Column<long>(type: "bigint", nullable: false),
                    ngay_cap = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    trang_thai = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lich_su_cap_phat", x => x.id_cp);
                    table.ForeignKey(
                        name: "FK_lich_su_cap_phat_nhan_vien_id_nv",
                        column: x => x.id_nv,
                        principalTable: "nhan_vien",
                        principalColumn: "id_nv",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lich_su_cap_phat_thiet_bi_id_tb",
                        column: x => x.id_tb,
                        principalTable: "thiet_bi",
                        principalColumn: "id_tb",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lich_su_thu_hoi",
                columns: table => new
                {
                    id_th = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_nv = table.Column<long>(type: "bigint", nullable: false),
                    id_tb = table.Column<long>(type: "bigint", nullable: false),
                    ngay_thu_hoi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tinh_trang = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lich_su_thu_hoi", x => x.id_th);
                    table.ForeignKey(
                        name: "FK_lich_su_thu_hoi_nhan_vien_id_nv",
                        column: x => x.id_nv,
                        principalTable: "nhan_vien",
                        principalColumn: "id_nv",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lich_su_thu_hoi_thiet_bi_id_tb",
                        column: x => x.id_tb,
                        principalTable: "thiet_bi",
                        principalColumn: "id_tb",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "yeu_cau_thiet_bi",
                columns: table => new
                {
                    id_yc = table.Column<long>(type: "bigint", nullable: false),
                    id_tb = table.Column<long>(type: "bigint", nullable: false),
                    so_luong = table.Column<int>(type: "integer", nullable: false),
                    ghi_chu = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_yeu_cau_thiet_bi", x => new { x.id_yc, x.id_tb });
                    table.ForeignKey(
                        name: "FK_yeu_cau_thiet_bi_thiet_bi_id_tb",
                        column: x => x.id_tb,
                        principalTable: "thiet_bi",
                        principalColumn: "id_tb",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_yeu_cau_thiet_bi_yeu_cau_id_yc",
                        column: x => x.id_yc,
                        principalTable: "yeu_cau",
                        principalColumn: "id_yc",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_khau_hao_id_tb",
                table: "khau_hao",
                column: "id_tb",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lich_su_cap_phat_id_nv",
                table: "lich_su_cap_phat",
                column: "id_nv");

            migrationBuilder.CreateIndex(
                name: "IX_lich_su_cap_phat_id_tb",
                table: "lich_su_cap_phat",
                column: "id_tb");

            migrationBuilder.CreateIndex(
                name: "IX_lich_su_thu_hoi_id_nv",
                table: "lich_su_thu_hoi",
                column: "id_nv");

            migrationBuilder.CreateIndex(
                name: "IX_lich_su_thu_hoi_id_tb",
                table: "lich_su_thu_hoi",
                column: "id_tb");

            migrationBuilder.CreateIndex(
                name: "IX_nhan_vien_email",
                table: "nhan_vien",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_yeu_cau_id_nv",
                table: "yeu_cau",
                column: "id_nv");

            migrationBuilder.CreateIndex(
                name: "IX_yeu_cau_thiet_bi_id_tb",
                table: "yeu_cau_thiet_bi",
                column: "id_tb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "khau_hao");

            migrationBuilder.DropTable(
                name: "lich_su_cap_phat");

            migrationBuilder.DropTable(
                name: "lich_su_thu_hoi");

            migrationBuilder.DropTable(
                name: "yeu_cau_thiet_bi");

            migrationBuilder.DropTable(
                name: "thiet_bi");

            migrationBuilder.DropTable(
                name: "yeu_cau");

            migrationBuilder.DropTable(
                name: "nhan_vien");
        }
    }
}
