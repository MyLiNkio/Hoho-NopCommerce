using VoucherManager.ConfigurationManager;
using VoucherManager.Model;
using Microsoft.EntityFrameworkCore;
using VoucherManager.DbModels;

namespace VoucherManager
{
    internal class VoucherNumbersContext : DbContext
    {
        public VoucherNumbersContext()
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        public DbSet<VoucherNumber> Vouchers { get; set; }
        public DbSet<VoucherNumberTemp> VouchersTemp { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseMySQL(AppConfigManager.GetInstance().GetSection("VoucherNumbersConnectionString"));
            optionsBuilder.UseMySQL(VMSettings.VoucherNumbersConnectionString);
            //.LogTo(Console.WriteLine);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Write Fluent API configurations here

            //Property Configurations
            modelBuilder.Entity<VoucherNumber>().HasIndex(i => i.CardNumber);
        }
    }
}
