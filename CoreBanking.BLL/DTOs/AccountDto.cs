namespace CoreBanking.BLL.DTOs
{
    public class CreateAccountDto
    {
        public int CustomerId { get; set; }
        public decimal InitialDeposit { get; set; }
    }

    public class AccountDto
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }
}