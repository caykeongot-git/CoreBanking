namespace CoreBanking.BLL.DTOs
{
    public class TransferRequestDto
    {
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }

    public class DepositWithdrawRequestDto
    {
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}