using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public static class DbInitializer
    {
        public static void ApplyMigrationsAndPatch(AppDbContext context)
        {
            try
            {
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB] Migrate warning: {ex.Message}");
            }

            // Bổ sung cột nếu migration chưa chạy (PostgreSQL)
            var sql = @"
                ALTER TABLE thiet_bi ADD COLUMN IF NOT EXISTS id_nv_su_dung bigint NULL;
                ALTER TABLE yeu_cau ADD COLUMN IF NOT EXISTS id_nv_gui bigint NULL;
                ALTER TABLE yeu_cau ADD COLUMN IF NOT EXISTS ly_do character varying(500) NULL;
                ALTER TABLE khau_hao ADD COLUMN IF NOT EXISTS thoi_gian_su_dung integer NULL;
                ALTER TABLE khau_hao ADD COLUMN IF NOT EXISTS gia_tri_thu_hoi decimal(15,2) NULL;
                ALTER TABLE khau_hao ADD COLUMN IF NOT EXISTS ky_tinh character varying(50) NULL;
                CREATE INDEX IF NOT EXISTS IX_thiet_bi_id_nv_su_dung ON thiet_bi (id_nv_su_dung);
                CREATE INDEX IF NOT EXISTS IX_yeu_cau_id_nv_gui ON yeu_cau (id_nv_gui);
            ";
            try
            {
                context.Database.ExecuteSqlRaw(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB] Patch schema warning: {ex.Message}");
            }

            FixPostgresSequences(context);
        }

        /// <summary>
        /// Đồng bộ sequence sau khi seed/insert tay — tránh lỗi duplicate PK khi thêm bản ghi mới.
        /// </summary>
        private static void FixPostgresSequences(AppDbContext context)
        {
            var seqSql = @"
                SELECT setval(pg_get_serial_sequence('nhan_vien', 'id_nv'), COALESCE((SELECT MAX(id_nv) FROM nhan_vien), 1), true);
                SELECT setval(pg_get_serial_sequence('thiet_bi', 'id_tb'), COALESCE((SELECT MAX(id_tb) FROM thiet_bi), 1), true);
                SELECT setval(pg_get_serial_sequence('yeu_cau', 'id_yc'), COALESCE((SELECT MAX(id_yc) FROM yeu_cau), 1), true);
                SELECT setval(pg_get_serial_sequence('lich_su_cap_phat', 'id_cp'), COALESCE((SELECT MAX(id_cp) FROM lich_su_cap_phat), 1), true);
                SELECT setval(pg_get_serial_sequence('lich_su_thu_hoi', 'id_th'), COALESCE((SELECT MAX(id_th) FROM lich_su_thu_hoi), 1), true);
            ";
            try
            {
                context.Database.ExecuteSqlRaw(seqSql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB] Fix sequences warning: {ex.Message}");
            }
        }
    }
}
