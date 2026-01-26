using CoreBanking.BLL.DTOs;
using CoreBanking.BLL.Interfaces;
using CoreBanking.DAL.Entities;
using CoreBanking.DAL.Repositories;

namespace CoreBanking.BLL.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto request)
        {
            // 1. Map DTO -> Entity
            var customer = new Customer
            {
                FullName = request.FullName,
                IdentityNumber = request.IdentityNumber,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                IsKycVerified = true // Auto verify for demo
            };

            // 2. Save to DB
            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.CompleteAsync();

            // 3. Map Entity -> DTO return
            return new CustomerDto
            {
                Id = customer.Id,
                FullName = customer.FullName,
                IdentityNumber = customer.IdentityNumber,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                IsKycVerified = customer.IsKycVerified
            };
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            return customers.Select(c => new CustomerDto
            {
                Id = c.Id,
                FullName = c.FullName,
                IdentityNumber = c.IdentityNumber,
                Email = c.Email,
                IsKycVerified = c.IsKycVerified
            });
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null) return null;

            return new CustomerDto
            {
                Id = customer.Id,
                FullName = customer.FullName,
                IdentityNumber = customer.IdentityNumber,
                Email = customer.Email,
                IsKycVerified = customer.IsKycVerified
            };
        }
    }
}
