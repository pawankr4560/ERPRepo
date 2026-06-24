
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

            modelBuilder.Entity<Loan>()
                .HasQueryFilter(x => !x.IsDeleted);

            modelBuilder.Entity<Loan>()
                .HasIndex(x => new { x.IsDeleted, x.Status, x.Active });

            modelBuilder.Entity<Loan>()
                .Property(x => x.LoanAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Loan>()
                .Property(x => x.Rate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Loan>()
                .Property(x => x.EMI)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LoanEMISchedule>()
                .HasQueryFilter(x => !x.IsDeleted);

            modelBuilder.Entity<LoanEMISchedule>()
                .HasIndex(x => new { x.IsDeleted, x.IsPaid, x.DueDate });

            modelBuilder.Entity<LoanEMISchedule>()
                .Property(x => x.EMIAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LoanEMISchedule>()
                .Property(x => x.PrincipalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LoanEMISchedule>()
                .Property(x => x.InterestAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LoanEMISchedule>()
                .Property(x => x.OutstandingBalance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LoanPayment>()
                .HasQueryFilter(x => !x.IsDeleted);

            modelBuilder.Entity<LoanPayment>()
                .HasIndex(x => new { x.IsDeleted, x.PaymentStatus, x.PaymentDate });

            modelBuilder.Entity<Product>()
                .HasQueryFilter(x => !x.IsDeleted);

            modelBuilder.Entity<OrderHistory>()
                .HasQueryFilter(x => !x.IsDeleted);
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
        public DbSet<BookingPayment> BookingPayments { get; set; }
    }
}
