using CoreBanking.BLL.DTOs;
using CoreBanking.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreBanking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly ICreditScoreService _creditScoreService;

        public LoanController(ILoanService loanService, ICreditScoreService creditScoreService)
        {
            _loanService = loanService;
            _creditScoreService = creditScoreService;
        }

        [HttpGet("check-limit/{customerId}")]
        public async Task<IActionResult> CheckLimit(int customerId)
        {
            var result = await _creditScoreService.CalculateScoreAsync(customerId);
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterLoan(LoanApplicationDto request)
        {
            // Check điểm tín dụng trước
            var score = await _creditScoreService.CalculateScoreAsync(request.CustomerId);
            if (score.Rating == "Poor")
                return BadRequest("Credit score too low for loan.");

            var loan = await _loanService.RegisterLoanAsync(request);
            return Ok(loan);
        }

        [HttpPost("disburse/{loanId}")]
        public async Task<IActionResult> Disburse(int loanId)
        {
            try
            {
                var result = await _loanService.DisburseLoanAsync(loanId);
                if (result) return Ok("Disbursement successful. Money added to account.");
                return BadRequest("Cannot disburse (Loan not pending or not found).");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
