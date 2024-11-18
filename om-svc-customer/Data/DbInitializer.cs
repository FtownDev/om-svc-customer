using Microsoft.EntityFrameworkCore;
using om_svc_customer.Models;

namespace om_svc_customer.Data
{
    public class DbInitializer
    {
        public static void InitDb(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            SeedData(scope.ServiceProvider.GetService<CustomerDbContext>());
        }

        private static void SeedData(CustomerDbContext context)
        {

            context.Database.Migrate();

            if (context.Customers.Any())
            {
                Console.WriteLine("Already have data- skipping seed data");
                return;
            }

            var countries = new List<Country>()
            {
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "United States"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Canada"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Mexico"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "France"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Australia"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Spain"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Russia"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Italy"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Brazil"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Belgium"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Germany"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Greece"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Argentina"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "China"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "Japan"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "South Korea"
                },
                new Country()
                {
                    Id = Guid.NewGuid(),
                    Name = "New Zeland"
                },
            };

            var addresses = new List<Address>()
            {
               new Address
               {
                   Id = Guid.NewGuid(),
                   CountryId = countries.FirstOrDefault(c => c.Name =="United States").Id,
                   Country = "United States",
                   DependentLocality = "Auburn",
                   Locale = "NY",
                   PostalCode = "13021",
                   Street1 = "555 Green Street",
                   Street2 = "333 Red Ave",
               }
            };

            var customers = new List<Customer>()
            {
                new Customer()
                {
                    Id= Guid.NewGuid(),
                    Created = DateTime.UtcNow,
                    FirstName = "Joel",
                    LastName = "Miller",
                    PrimaryPhone = "3152443902",
                    Email = "jmiller46@testmail.com",
                    BillingAddressId = addresses.FirstOrDefault().Id,
                    Ext = "123",
                    Fax = "1234567890",
                    SecondaryPhone = "1112223333"
                }
            };

            var shippingAddresses = new List<CustomerShippingAddress>()
            {
                new CustomerShippingAddress()
                {
                    Id = Guid.NewGuid(),
                    AddressId = countries.Where(c => c.Name =="United States").FirstOrDefault().Id,
                    CustomerId = customers.FirstOrDefault().Id,
                }
            };

            context.AddRange(countries);
            context.SaveChanges();

            context.AddRange(addresses);
            context.SaveChanges();

            context.AddRange(customers);
            context.SaveChanges();

            context.AddRange(shippingAddresses);
            context.SaveChanges();
        }

    }
}
