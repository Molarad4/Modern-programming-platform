namespace TargetApp
{
    public static class BankDbStorage
    {
        public static List<Account> Accounts { get; set; } = new List<Account>();

        public static void InitializeSeedData()
        {
            Accounts = new List<Account>
            {
                new Account(1, "Ivan", 1000m),
                new Account(2, "Oleg", 500m)
            };
        }

        public static void ClearAll() => Accounts.Clear();
    }
}