using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace om_svc_customer.Models
{
    [Table("Countries")]
    public class Country
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
