using CoreBanking.BLL.DTOs;
using CoreBanking.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoreBanking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(CreateAccountDto request)
        {
            try
            {
                var result = await _accountService.CreateAccountAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBalance(int id)
        {
            var result = await _accountService.GetAccountByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit(DepositWithdrawRequestDto request)
        {
            try
            {
                await _accountService.DepositAsync(request);
                return Ok(new { Message = "Deposit successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw(DepositWithdrawRequestDto request)
        {
            try
            {
                await _accountService.WithdrawAsync(request);
                return Ok(new { Message = "Withdrawal successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer(TransferRequestDto request)
        {
            try
            {
                await _accountService.TransferAsync(request);
                return Ok(new { Message = "Transfer successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
