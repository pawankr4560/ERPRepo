using ERPWebAppData.Entity;
using MongoDB.Driver;
using WebApp.Data.Entity;

namespace WebApp.Data;

public sealed class MongoDbContext
{
    public MongoDbContext(IMongoDatabase database) => Database = database;

    public IMongoDatabase Database { get; }
    public IMongoCollection<User> Users => Database.GetCollection<User>("Users");
    public IMongoCollection<Role> Roles => Database.GetCollection<Role>("Roles");
    public IMongoCollection<Loan> Loans => Database.GetCollection<Loan>("Loans");
    public IMongoCollection<LoanCustomerDetail> LoanCustomerDetails => Database.GetCollection<LoanCustomerDetail>("LoanCustomerDetails");
    public IMongoCollection<LoanEMISchedule> LoanEMISchedules => Database.GetCollection<LoanEMISchedule>("LoanEMISchedules");
    public IMongoCollection<LoanPayment> LoanPayments => Database.GetCollection<LoanPayment>("LoanPayments");
    public IMongoCollection<MenuItems> MenuItems => Database.GetCollection<MenuItems>("MenuItems");
    public IMongoCollection<Product> Products => Database.GetCollection<Product>("Products");
    public IMongoCollection<Location> Locations => Database.GetCollection<Location>("Locations");
    public IMongoCollection<UnitOfMeasure> UnitOfMeasures => Database.GetCollection<UnitOfMeasure>("UnitOfMeasure");
    public IMongoCollection<OrderHistory> OrderHistory => Database.GetCollection<OrderHistory>("OrderHistory");
    public IMongoCollection<StripeCustomer> StripeCustomers => Database.GetCollection<StripeCustomer>("StripeCustomer");
    public IMongoCollection<LoanSetting> LoanSettings => Database.GetCollection<LoanSetting>("LoanSettings");
    public IMongoCollection<Car> Cars => Database.GetCollection<Car>("Cars");
    public IMongoCollection<Category> Categories => Database.GetCollection<Category>("Categories");
    public IMongoCollection<Booking> Bookings => Database.GetCollection<Booking>("Bookings");
    public IMongoCollection<BookingPayment> BookingPayments => Database.GetCollection<BookingPayment>("BookingPayments");
    public IMongoCollection<SequenceCounter> Counters => Database.GetCollection<SequenceCounter>("SequenceCounters");
}
