using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace om_svc_customer.Models
{
    [Table("Customers")]
    public class Customer
    {
        public Guid Id { get; set; }

        [Required]
        public DateTimeOffset Created { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PrimaryPhone { get; set; }

        public string Ext { get; set; }

        public string SecondaryPhone { get; set; }

        public string Fax { get; set; }

        [Required]
        public Guid BillingAddressId { get; set; }
    }
}
