
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ERPWebAppData.Entity;
using WebApp.Data.Entity;

namespace WebApp.Data
{
    public class WebAppDbContext : IdentityDbContext<User>
    {
        public WebAppDbContext(DbContextOptions<WebAppDbContext> options)
           : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<MenuItems> MenuItem { get; set; }
        public DbSet<LoanPayment> LoanPayment { get; set; }
        public DbSet<LoanEMISchedule> LoanEMISchedule { get; set; }
        public DbSet<Loan> Loan { get; set; }
        public DbSet<LoanCustomerDetail> LoanCustomerDetail { get; set; }
        public DbSet<LoanSetting> LoanSetting { get; set; }
        public DbSet<UnitOfMeasure> UnitOfMeasure { get; set; }
        public DbSet<OrderHistory> OrderHistory { get; set; }
        public DbSet<StripeCustomer> StripeCustomer { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Booking> Bookings { get; set; }
    }
}
