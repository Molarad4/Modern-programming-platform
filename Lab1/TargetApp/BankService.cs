using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TargetApp
{
    public class BankService
    {
        public void Transfer(int fromId, int toId, decimal amount)
        {
            var fromAcc = BankDbStorage.Accounts.FirstOrDefault(a => a.Id == fromId);
            var toAcc = BankDbStorage.Accounts.FirstOrDefault(a => a.Id == toId);

            if (fromAcc == null || toAcc == null)
                throw new Exception("Account not found");

            if (fromAcc.Balance < amount)
                throw new InvalidOperationException("Insufficient funds");

            fromAcc.Balance -= amount;
            toAcc.Balance += amount;
        }

        public async Task<bool> ProcessExternalPaymentAsync(int accountId, decimal amount)
        {
            await Task.Delay(100);

            var account = BankDbStorage.Accounts.FirstOrDefault(a => a.Id == accountId);
            if (account == null || account.IsBlocked) return false;

            account.Balance += amount;
            return true;
        }

        public List<Account> GetActiveAccounts()
        {
            return BankDbStorage.Accounts.Where(a => !a.IsBlocked).ToList();
        }

        public void BlockAccount(int id)
        {
            var account = BankDbStorage.Accounts.FirstOrDefault(a => a.Id == id);
            if (account != null) account.IsBlocked = true;
        }
    }
}