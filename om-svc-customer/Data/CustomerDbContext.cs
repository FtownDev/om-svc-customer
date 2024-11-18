
using Microsoft.EntityFrameworkCore;
using om_svc_customer.Models;
using System.Diagnostics.Metrics;
using System.Net;

namespace om_svc_customer.Data
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Country> Countries { get; set; }
        public DbSet<Address> Addresses { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<CustomerShippingAddress> CustomerShippingAddresses { get; set; }

    }
}
