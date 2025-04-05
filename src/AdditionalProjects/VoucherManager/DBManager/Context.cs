using VoucherManager.ConfigurationManager;
using VoucherManager.Model;
using Microsoft.EntityFrameworkCore;
using VoucherManager;

namespace VoucherCodesGenerator
{
    internal class Context: DbContext
    {
        public Context()
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        public DbSet<VoucherDbModel> Vouchers { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseMySQL(AppConfigManager.GetInstance().GetSection("ConnectionString"));
            optionsBuilder.UseMySQL(VMSettings.ConnectionString);
            //.LogTo(Console.WriteLine);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Write Fluent API configurations here

            //Property Configurations
            modelBuilder.Entity<VoucherDbModel>().Property(s => s.OriginId).IsRequired();
            modelBuilder.Entity<VoucherDbModel>().HasIndex(i => i.CardNumber);
        }
    }
}
