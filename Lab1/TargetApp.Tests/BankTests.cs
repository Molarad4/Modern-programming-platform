using TestFramework;

namespace TargetApp.Tests
{
    public class BankTests
    {
        private BankService _service;

        [BeforeEach]
        public void SetUp()
        {
            _service = new BankService();
           
            if (BankDbStorage.Accounts.Count == 0) BankDbStorage.InitializeSeedData();
            
            var acc = BankDbStorage.Accounts[0];
            acc.Balance = 1000m;
            acc.IsBlocked = false;
        }
        
        [MyTest]
        public void Test_OwnerName_Validation()
        {
            var account = BankDbStorage.Accounts[0];
            Assert.StringContains("Ivan", account.Owner); // Успех
        }

        [MyTest]
        public void Test_Account_Type_And_Nullability()
        {
            var account = BankDbStorage.Accounts[0];
            Assert.IsNotNull(account); // Успех
            Assert.IsInstanceOf<Account>(account); // Успех
        }
        
        [MyTest]
        public void Test_FAILED_WrongBalance()
        {
            // Этот тест ДОЛЖЕН упасть, чтобы показать работу ошибок
            Assert.AreEqual(999999m, BankDbStorage.Accounts[0].Balance); 
        }

        [MyTest]
        public void Test_FAILED_WrongOwner()
        {
            // Тоже упадет
            Assert.StringContains("Elon Musk", BankDbStorage.Accounts[0].Owner);
        }
        
        [MyTest]
        public void Test_Exception_On_NegativeTransfer()
        {
            // Успех, если упадет с InvalidOperationException
            Assert.Throws<InvalidOperationException>(() => 
                _service.Transfer(1, 2, 50000m));
        }

        [MyTest]
        public void Test_FAILED_WrongExceptionType()
        {
            // Упадет, так как мы ждем NullRef, а будет InvalidOperation
            Assert.Throws<NullReferenceException>(() => 
                _service.Transfer(1, 2, 50000m));
        }
        
        [MyTest]
        public async Task Test_Async_Payment_Processing()
        {
            bool result = await _service.ProcessExternalPaymentAsync(1, 200m);
            Assert.IsTrue(result);
            Assert.AreEqual(1200m, BankDbStorage.Accounts[0].Balance);
        }

        [MyTest]
        public async Task Test_Async_BlockedAccount_ShouldFail()
        {
            _service.BlockAccount(1);
            bool result = await _service.ProcessExternalPaymentAsync(1, 100m);
            Assert.IsFalse(result); // Успех, т.к. результат должен быть false
        }
        
        [MyTestCase(100, 900)]
        [MyTestCase(500, 500)]
        [MyTestCase(1000, 0)]
        public void Test_Withdrawal_Params(int amount, int expected)
        {
            _service.Transfer(1, 2, (decimal)amount);
            Assert.AreEqual((decimal)expected, BankDbStorage.Accounts[0].Balance);
        }

        [MyTestCase(1, true)]
        [MyTestCase(999, false)] // Несуществующий ID
        public void Test_Account_Exists_Params(int id, bool shouldExist)
        {
            var acc = BankDbStorage.Accounts.Find(a => a.Id == id);
            if (shouldExist) Assert.IsNotNull(acc);
            else Assert.IsNull(acc);
        }

      
        [MyTest]
        public void Test_ActiveAccounts_Count()
        {
            var active = _service.GetActiveAccounts();
            Assert.AreNotEqual(0, active.Count); // Успех
        }

        [MyTest]
        public void Test_EmptyCollection_Check()
        {
            BankDbStorage.ClearAll();
            Assert.IsEmpty(BankDbStorage.Accounts); // Успех
        }
        
    }
}