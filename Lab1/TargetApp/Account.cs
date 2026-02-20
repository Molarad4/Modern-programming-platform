namespace TargetApp
{
    public class Account
    {
        public int Id { get; set; }
        public string Owner { get; set; }
        public decimal Balance { get; set; }
        public bool IsBlocked { get; set; }

        public Account(int id, string owner, decimal initialBalance)
        {
            Id = id;
            Owner = owner;
            Balance = initialBalance;
            IsBlocked = false;
        }
    }
}