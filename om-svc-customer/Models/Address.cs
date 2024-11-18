using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace om_svc_customer.Models
{
    [Table("Addresses")]
    public class Address
    {
        public Guid Id { get; set; }

        [Required]
        public string Street1 { get; set; }

        public string Street2 { get; set; }

        [Required]
        public string DependentLocality { get; set; } // State

        [Required]
        public string Locale { get; set; } // City

        [Required]
        public string PostalCode { get; set; }

        [Required]
        public Guid CountryId { get; set; }

        [Required]
        public string Country { get; set; }
    }
}
