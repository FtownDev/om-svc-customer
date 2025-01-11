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

            var cacheList = _cacheService.GetData<RetrieveCustomerResponse>(key: $"all/{pageSize}/{currentNumber}");
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
                await _cacheService.InvalidateKeys(new List<string>() { "all" });
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
                var shippingAddressesToDelete = this._context.CustomerShippingAddresses.Where(a => a.CustomerId == customerId);

                var addressIds = shippingAddressesToDelete.Select(a => a.AddressId);

                if (shippingAddressesToDelete.Any()) 
                {
                    _context.CustomerShippingAddresses.RemoveRange(shippingAddressesToDelete);
                }

                var addressesToDelete = this._context.Addresses.Where(a => addressIds.Contains(a.Id));

                this._context.Addresses.RemoveRange(addressesToDelete);

                this._context.Customers.Remove(customer);

                retval = await this._context.SaveChangesAsync() > 0 ? this.Ok() : this.StatusCode((int)HttpStatusCode.InternalServerError, "Unable to delete customer");
                await _cacheService.InvalidateKeys(new List<string>() { "all", $"{customerId}" });
            }

            return retval;
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchCustomers([FromBody] CustomerSearchRequest request, [FromQuery] int pageSize = 50,[FromQuery] int currentPage = 1)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                query = query.Where(c => c.FirstName.ToLower().Contains(request.FirstName));
            }

            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                query = query.Where(c => c.LastName.ToLower().Contains(request.LastName));
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                query = query.Where(c => c.Email.ToLower().Contains(request.Email));
            }

            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                query = query.Where(c => c.PrimaryPhone.ToLower().Contains(request.Phone)
                    || (c.SecondaryPhone != null && c.SecondaryPhone.ToLower().Contains(request.Phone)));
            }

            //  pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var customers = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(customers);
        }

        [HttpGet]
        [Route("{customerId}/address")]
        public async Task<IActionResult> GetCustomerShippingAddresses([FromRoute] Guid customerId)
        {
            IActionResult retval;

            var cacheList = _cacheService.GetData<RetrieveShippingAddressResponse>(key: $"{customerId}/address");
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
                await this._cacheService.InvalidateKeys(new List<string> { $"{customerId}/address" });
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
                    await this._cacheService.InvalidateKeys(new List<string> { $"{customerId}/address" });
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
