using om_svc_customer.Models;

namespace om_svc_customer.DTO
{
    public class RetrieveCustomerResponse
    {
        public int pageSize { get; set; }

        public int totalCount { get; set; }

        public required List<Customer> customers { get; set; }
    }
}
