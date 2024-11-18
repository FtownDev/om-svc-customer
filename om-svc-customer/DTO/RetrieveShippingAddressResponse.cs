using om_svc_customer.Models;

namespace om_svc_customer.DTO
{
    public class RetrieveShippingAddressResponse
    {
        public Guid customerId { get; set; }

        public required List<Address> shippingAddresses { get; set; }
    }
}
