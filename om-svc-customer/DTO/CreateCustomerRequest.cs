using om_svc_customer.Models;
using System.ComponentModel.DataAnnotations;

namespace om_svc_customer.DTO
{
    public class CreateCustomerRequest
    {
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
        public Address billingAddress { get; set; }
    }


}
