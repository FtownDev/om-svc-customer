using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace om_svc_customer.Models
{
    [Table("CustomerShippingAddresses")]
    public class CustomerShippingAddress
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid AddressId { get; set; }
    }
}