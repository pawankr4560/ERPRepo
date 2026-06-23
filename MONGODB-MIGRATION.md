# SQL Server to MongoDB migration

The application no longer uses EF Core or SQL Server at runtime. Keep the old
database and the `ERPWebAppData/Migrations` folder read-only until the MongoDB
data has been validated and the cutover is complete.

## 1. Prepare MongoDB

Set these values through `appsettings.Production.json`, environment variables,
or your secret store:

```text
MongoDbSettings__ConnectionString=mongodb+srv://...
MongoDbSettings__DatabaseName=WebAppDB
```

Create a backup of SQL Server before exporting. Use a separate MongoDB database
for rehearsals, for example `WebAppDB_MigrationTest`.

## 2. Export SQL tables

Export each table to UTF-8 JSON or CSV with SQL Server Import/Export Wizard,
`bcp`, or a small one-time .NET migration utility. Include all columns and do
not regenerate the existing integer or Guid IDs.

Suggested collection mapping:

| SQL source | MongoDB collection |
| --- | --- |
| AspNetUsers | Users |
| AspNetRoles | Roles |
| AspNetUserRoles | embedded `Users.Roles` names |
| Loan | Loans |
| LoanCustomerDetail | LoanCustomerDetails |
| LoanEMISchedule | LoanEMISchedules |
| LoanPayment | LoanPayments |
| MenuItem | MenuItems |
| Products | Products |
| Location | Locations |
| UnitOfMeasure | UnitOfMeasure |
| OrderHistory | OrderHistory |
| StripeCustomer | StripeCustomer |
| LoanSetting | LoanSettings |
| Cars | Cars |
| Categories | Categories |
| Bookings | Bookings |
| BookingPayments | BookingPayments |

## 3. Transform users and roles

New users use MongoDB ObjectId strings. Existing ASP.NET Identity user IDs are
usually GUID strings, so create a durable mapping:

```text
OldSqlUserId -> NewMongoObjectId
```

Apply that mapping to:

- `Loans.UserId`
- `Loans.ApprovedByUserId`
- `Loans.RejectedByUserId`
- `StripeCustomer.UserId`
- `Bookings.UserId`

For each user document:

- `_id`: the new ObjectId
- `Email`: existing email
- `NormalizedEmail`: `Email.Trim().ToUpperInvariant()`
- `UserName`: existing user name, or email when empty
- `NormalizedUserName`: normalized user name
- `PasswordHash`: preserve the existing ASP.NET Identity password hash
- `Roles`: role names resolved through `AspNetUserRoles` and `AspNetRoles`
- `IsDeleted`, `IsActive`, profile fields, login counters, and timestamps:
  preserve existing values

The application uses ASP.NET Core `PasswordHasher<User>`, so compatible
ASP.NET Identity password hashes can be copied without forcing password resets.
Test representative accounts before cutover.

## 4. Preserve public IDs

Keep the existing integer IDs for loans, schedules, payments, menus, locations,
UOMs, cars, categories, bookings, booking payments, customer details, and loan
settings. Keep existing Guid IDs for products, orders, and Stripe customer
records.

After import, set the `SequenceCounters` collection to at least the maximum
imported integer value for every sequence:

```javascript
db.SequenceCounters.updateOne(
  { _id: "Loans" },
  { $set: { Value: db.Loans.find().sort({ Id: -1 }).limit(1).next().Id } },
  { upsert: true }
)
```

Repeat for:

```text
LoanCustomerDetails
LoanEMISchedules
LoanPayments
MenuItems
LoanSettings
Cars
Categories
Bookings
BookingPayments
```

For an empty collection, set `Value` to `0`. This prevents newly inserted
documents from reusing an imported numeric ID.

## 5. Import order

Use this order so references can be checked as data is loaded:

1. Roles and Users
2. Categories, Locations, UnitOfMeasure, Products, MenuItems
3. Loans and LoanCustomerDetails
4. LoanEMISchedules and LoanPayments
5. Cars, Bookings, and BookingPayments
6. OrderHistory, StripeCustomer, and LoanSettings
7. SequenceCounters

`mongoimport` can load transformed JSON arrays, for example:

```powershell
mongoimport --uri "$env:MONGODB_URI" --db WebAppDB_MigrationTest `
  --collection Loans --file .\export\Loans.json --jsonArray
```

## 6. Validate before cutover

Compare SQL and MongoDB counts, excluding soft-deleted rows only when the API
also excludes them. Validate:

- every loan, booking, and Stripe customer references an existing user;
- every schedule and payment references an existing loan;
- every booking references an existing car;
- no duplicate normalized emails or user names exist;
- no duplicate loan numbers exist;
- decimal amounts and UTC timestamps retained their precision;
- admin and normal-user logins produce JWTs with the expected role claims;
- Angular screens receive the same numeric/Guid IDs and DTO property names.

Run the API once against the rehearsal database. Startup creates the required
unique and lookup indexes. Index creation intentionally fails when duplicate
email, user name, or loan-number data is present.

## 7. Cutover

1. Put the SQL-backed application into maintenance/read-only mode.
2. Export and transform a final delta.
3. Import the delta and update all sequence counters.
4. Run validation and login/API smoke tests.
5. Deploy the MongoDB-backed API configuration.
6. Keep the SQL backup unchanged until the rollback window has passed.

Do not run SQL EF migrations after cutover. Historical migration source files
are excluded from compilation and exist only as schema reference.
