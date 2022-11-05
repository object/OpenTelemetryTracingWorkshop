using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApp
{
    public enum Backend
    {
        None,
        SqlServer,
        MySql
    };

    public class Words
    {
        [Key]
        public int Id { get; set; }
        public string Word { get; set; }
    }

    public class WordDbContext : DbContext
    {
        public static Backend Backend = Backend.SqlServer;

        public WordDbContext(DbContextOptions<WordDbContext> options) : base(options)
        {
        }

        public DbSet<Words> Words { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            optionsBuilder.UseSqlServer(configuration.GetConnectionString("SqlServer"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var words = new[] { "quick", "brown", "fox", "jumped", "over", "the", "lazy", "dog" };

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Words>()
                .ToTable("Words");
            modelBuilder.Entity<Words>()
                .Property(s => s.Word);

            for (var i = 0; i < words.Length; i++)
            {
                modelBuilder.Entity<Words>()
                    .HasData(new Words { Id = i+1, Word = words[i] });
            }
        }
    }

    public partial class SeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "words",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Word = table.Column<string>(maxLength: 20, nullable: false)
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("words");
        }
    }
}
