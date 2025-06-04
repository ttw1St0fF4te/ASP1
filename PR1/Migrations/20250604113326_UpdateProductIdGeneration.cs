using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PR1.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductIdGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Сброс последовательности ID для таблицы Products
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    max_id integer;
                BEGIN
                    -- Получаем максимальный ID из таблицы
                    SELECT COALESCE(MAX(""Id""), 0) INTO max_id FROM ""Products"";
                    
                    -- Сбрасываем последовательность
                    EXECUTE 'ALTER SEQUENCE ""Products_Id_seq"" RESTART WITH ' || (max_id + 1)::text;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // В Down ничего не делаем, так как это просто сброс последовательности
        }
    }
}
