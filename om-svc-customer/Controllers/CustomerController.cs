using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using om_svc_customer.Data;
using om_svc_customer.DTO;
using om_svc_customer.Models;
using om_svc_customer.Services;
using System.Net;

namespace om_svc_customer.Controllers
{
    [ApiController]
    [Route("api/customers/")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerDbContext _context;
        private readonly ICacheService _cacheService;

        public CustomerController(CustomerDbContext context, ICacheService cache)
        {
            this._context = context;
            this._cacheService = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers(int pageSize = 50, int currentNumber = 0)
        {
            IActionResult retval;

            var cacheList = _cacheService.GetData<IEnumerable<Customer>>(key: $"all/{pageSize}/{currentNumber}");
            if (cacheList != null)
            {
                retval = this.Ok(cacheList);
            }
            else
            {
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

                _cacheService.SetData($"all/{pageSize}/{currentNumber}", responseData, 10);
                retval = this.Ok(responseData);
            }
            

            return retval;
        }

        [HttpGet]
        [Route("{customerId}")]
        public async Task<IActionResult> GetCustomerById([FromRoute] Guid customerId)
        {
            IActionResult retval;

            var cacheItem = _cacheService.GetData<Customer>(key: $"{customerId}");

            if (cacheItem != null)
            {
                retval = this.Ok(cacheItem);
            }
            else
            {
                var customer = await this._context.Customers.Where(c => c.Id == customerId).FirstOrDefaultAsync();

                if (customer == null)
                {
                    retval = BadRequest("No user exists with the provided Id");
                }
                else
                {
                    _cacheService.SetData($"{customerId}", customer, 10);
                    retval = Ok(customer);
                }
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
                _cacheService.InvalidateKeys(new List<string>() { "all" });
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
                _cacheService.InvalidateKeys(new List<string>() { "all", $"{customerId}" });
            }

            return retval;
        }

        [HttpGet]
        [Route("{customerId}/address")]
        public async Task<IActionResult> GetCustomerShippingAddresses([FromRoute] Guid customerId)
        {
            IActionResult retval;

            var cacheList = _cacheService.GetData<IEnumerable<Address>>(key: $"{customerId}/address");
            if (cacheList != null)
            {
                retval = this.Ok(cacheList);
            }
            else
            {
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
                        var retObj = new RetrieveShippingAddressResponse
                        {
                            customerId = customerId,
                            shippingAddresses = addressList
                        };
                        this._cacheService.SetData($"{customerId}/address", retObj, 10);
                        retval = Ok(retObj);
                    }
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
                this._cacheService.InvalidateKeys(new List<string>{ $"{customerId}/address" });
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
                if(await this._context.SaveChangesAsync() > 0)
                {
                    this._cacheService.InvalidateKeys(new List<string> { $"{customerId}/address" });
                    retval = Ok(customerId);
                }
                else
                {
                    retval = this.StatusCode((int)HttpStatusCode.InternalServerError, "Unable to delete address");
                }
            }

            return retval;
        }

        [HttpGet]
        [Route("address/countries/all")]
        public async Task<IActionResult> GetAllCountries()
        {
            IActionResult retval;

            var cacheList = _cacheService.GetData<IEnumerable<Country>>(key: $"address/countries");

            if (cacheList != null)
            {
                retval = this.Ok(cacheList);
            }
            else
            {
                var countryList = await this._context.Countries.OrderBy(x => x.Name).ToListAsync();
                if(countryList == null || countryList.Count == 0)
                {
                    retval = this.NotFound("No countries available");
                }
                else
                {
                    this._cacheService.SetData($"address/countries", countryList, 10);
                    retval = Ok(countryList);
                }

            }

            return retval;
        }

    }
}
