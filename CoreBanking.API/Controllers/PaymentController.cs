using CoreBanking.BLL.DTOs;
using CoreBanking.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public PaymentController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("transfer-secure")]
        public async Task<IActionResult> TransferSecure(TransferRequestDto request)
        {
            try
            {
                await _transactionService.TransferMoneyWithConcurrencyCheckAsync(request);
                return Ok("Transfer successful with Concurrency Protection.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("Data has been modified by another user. Please try again.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
