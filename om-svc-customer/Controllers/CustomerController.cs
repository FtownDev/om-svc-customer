using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using om_svc_customer.Data;
using om_svc_customer.DTO;
using om_svc_customer.Models;
using System.Net;

namespace om_svc_customer.Controllers
{
    [ApiController]
    [Route("api/customers/")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerDbContext _context;

        public CustomerController(CustomerDbContext context)
        {
            this._context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers(int pageSize = 50, int currentNumber = 0)
        {
            IActionResult retval;

            var customerList = await this._context.Customers.OrderBy(x => x.LastName)
            .ThenBy(b => b.Id)
            .Skip(currentNumber)
            .Take(pageSize)
            .ToListAsync();

            var responseData = new RetrieveCustomerResponse
            {
                pageSize = pageSize,
                totalCount = currentNumber + pageSize,
                customers = customerList
            };

            return Ok(responseData);
        }

        [HttpGet]
        [Route("{customerId}")]
        public async Task<IActionResult> GetCustomerById([FromRoute] Guid customerId)
        {
            IActionResult retval;

            var customer = await this._context.Customers.Where(c => c.Id == customerId).FirstOrDefaultAsync();

            if (customer == null)
            {
                retval = BadRequest("No user exists with the provided Id");
            }
            else
            {
                retval = Ok(customer);
            }

            return retval;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
        {
            IActionResult retval;

            request.billingAddress.Id = Guid.NewGuid();

            this._context.Addresses.Add(request.billingAddress);

            var newCustomer = new Customer
            {
                Id = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PrimaryPhone = request.PrimaryPhone,
                SecondaryPhone = request.SecondaryPhone,
                Ext = request.Ext,
                Fax = request.Fax,
                BillingAddressId = request.billingAddress.Id
            };

            this._context.Customers.Add(newCustomer);

            var newShippingAddress = new CustomerShippingAddress
            {
                Id = Guid.NewGuid(),
                CustomerId = newCustomer.Id,
                AddressId = newCustomer.BillingAddressId
            };

            this._context.CustomerShippingAddresses.Add(newShippingAddress);

            var result = await this._context.SaveChangesAsync() > 0;

            if (!result)
            {
                retval = this.StatusCode((int)HttpStatusCode.InternalServerError, "Unable to create customer");
            }
            else
            {
                retval = Ok(newCustomer);
            }

            return retval;
        }

        [HttpDelete]
        [Route("{customerId}")]
        public async Task<IActionResult> DeleteCustomer([FromRoute] Guid customerId)
        {
            IActionResult retval;

            var customer = await this._context.Customers.FindAsync(customerId);

            if (customer == null)
            {
                retval = this.NotFound();
            }
            else
            {
                this._context.Customers.Remove(customer);

                retval = await this._context.SaveChangesAsync() > 0 ? this.Ok() : this.StatusCode((int)HttpStatusCode.InternalServerError, "Unable to delete customer");
            }

            return retval;
        }

        [HttpGet]
        [Route("{customerId}/address")]
        public async Task<IActionResult> GetCustomerShippingAddresses([FromRoute] Guid customerId)
        {
            IActionResult retval;

            var addressIdList = await this._context.CustomerShippingAddresses.Where(c => c.CustomerId == customerId)
                .Select(x => x.AddressId)
                .ToListAsync();

            if (addressIdList.Count == 0)
            {
                retval = BadRequest("No addresses exist for the given customer");
            }
            else
            {
                var addressList = await this._context.Addresses.Where(a => addressIdList.Contains(a.Id)).ToListAsync();

                if (!addressIdList.Any() || addressIdList.Count == 0)
                {
                    retval = BadRequest("No user exists with the provided Id");
                }
                else
                {

                    retval = Ok(new RetrieveShippingAddressResponse
                    {
                        customerId = customerId,
                        shippingAddresses = addressList
                    });
                }
            }

            return retval;
        }

        [HttpPost]
        [Route("{customerId}/address")]
        public async Task<IActionResult> CreateCustomerShippingAddress([FromRoute] Guid customerId, [FromBody] Address shippingAddress)
        {
            IActionResult retval;

            shippingAddress.Id = Guid.NewGuid();

            this._context.Addresses.Add(shippingAddress);

            this._context.CustomerShippingAddresses.Add(new CustomerShippingAddress { CustomerId = customerId, AddressId = shippingAddress.Id });

            var result = await this._context.SaveChangesAsync() > 0;

            if (!result)
            {
                retval = this.StatusCode((int)HttpStatusCode.InternalServerError, "Unable to create address");
            }
            else
            {
                retval = Ok(shippingAddress);
            }

            return retval;
        }

        [HttpDelete]
        [Route("{customerId}/address/{addressId}")]
        public async Task<IActionResult> DeleteCustomerShippingAddress([FromRoute] Guid customerId, [FromRoute] Guid addressId)
        {
            IActionResult retval;

            var customer = await this._context.Customers.FindAsync(customerId);

            var address = await this._context.Addresses.FindAsync(addressId);

            if (customer == null)
            {
                retval = this.NotFound("Customer not found");
            }
            else if (address == null)
            {
                retval = this.NotFound("Address not found");
            }
            else if (customer.BillingAddressId == addressId)
            {
                retval = this.BadRequest("Cannot delete current billing address while it is still active.");
            }
            else
            {
                this._context.Addresses.Remove(address);
                retval = await this._context.SaveChangesAsync() > 0 ? this.Ok() : this.StatusCode((int)HttpStatusCode.InternalServerError, "Unable to delete address");
            }

            return retval;
        }

        [HttpGet]
        [Route("address/countries/all")]
        public async Task<IActionResult> GetAllCountries()
        {
            IActionResult retval;

            var customerList = await this._context.Countries.OrderBy(x => x.Name)
            .ToListAsync();


            return Ok(customerList);
        }

    }
}
