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

        [AfterEach]
        public void TearDown()
        {
            _service = null;
        }

        [MyTest]
        public void Test_OwnerName_Validation()
        {
            var account = BankDbStorage.Accounts[0];
            Assert.StringContains("Ivan", account.Owner);
        }

        [MyTest("Тест проверяющий сам себя")]
        public void Test_CheckDescription()
        {
            var method = this.GetType().GetMethod("Test_CheckDescription");
            var attr = (MyTestAttribute)method.GetCustomAttributes(typeof(MyTestAttribute), false)[0];
            Assert.AreEqual("Тест проверяющий сам себя", attr.Description);
        }

        [MyTest]
        public void Test_Account_Type_And_Nullability()
        {
            var account = BankDbStorage.Accounts[0];
            Assert.IsNotNull(account);
            Assert.IsInstanceOf<Account>(account);
        }

        [MyTest]
        public void Test_FAILED_WrongBalance()
        {
            Assert.AreEqual(999999m, BankDbStorage.Accounts[0].Balance);
        }

        [MyTest]
        public void Test_FAILED_WrongOwner()
        {
            Assert.StringContains("Elon Musk", BankDbStorage.Accounts[0].Owner);
        }

        [MyTest]
        public void Test_Exception_On_NegativeTransfer()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _service.Transfer(1, 2, 50000m));
        }

        [MyTest]
        public void Test_FAILED_WrongExceptionType()
        {
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
            Assert.IsFalse(result);
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
        [MyTestCase(999, false)]
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
            Assert.AreNotEqual(0, active.Count);
        }

        [MyTest]
        public void Test_EmptyCollection_Check()
        {
            BankDbStorage.ClearAll();
            Assert.IsEmpty(BankDbStorage.Accounts);
        }

        
        
        
        [MyTest("Критический тест, должен запускаться при фильтре Priority>=2")]
        [Priority(2)]
        [Category("Critical")]
        [Author("Ivan Petrov")]
        public void Test_CriticalSecurityCheck()
        {
            var accounts = BankDbStorage.Accounts;
            foreach (var acc in accounts)
            {
                Assert.IsTrue(acc.Balance >= 0);
            }
        }
        
        [MyTest("Низкоприоритетный тест, может быть пропущен фильтром")]
        [Priority(1)]
        [Category("Smoke")]
        public void Test_LowPriorityCheck()
        {
            Assert.IsNotNull(BankDbStorage.Accounts);
        }
        
        [YieldTestCase(typeof(MoneyTransferGenerator))]
        public void YieldMoneyTransferTest(int fromId, int toId, int amount, int expectedBalanceFrom)
        {
            var fromBalanceBefore = BankDbStorage.Accounts.First(a => a.Id == fromId).Balance;
            _service.Transfer(fromId, toId, (decimal)amount);
            var fromBalanceAfter = BankDbStorage.Accounts.First(a => a.Id == fromId).Balance;
            Assert.AreEqual((decimal)expectedBalanceFrom, fromBalanceAfter);
        }
        
        [MyTest("Демонстрация Explain - успешный тест")]
        public void Test_Explain_Success()
        {
            int a = 100, b = 200;
            // Нормальный тест - не должен выбрасывать ошибку
            Assert.Explain(() => a + b == 300);
        }
        
        [MyTest("Демонстрация Explain - провальный тест с детальным разбором")]
        public void Test_Explain_Failure()
        {
            int x = 50, y = 75, z = 130;
            // При ошибке выведет: "x + y == z | (125 == 130 = false)"
            Assert.Explain(() => x + y == z);
        }
        
        [MyTest("Демонстрация Explain - сложное выражение")]
        public void Test_Explain_Complex()
        {
            int a = 10, b = 5, c = 20;
            // При ошибке покажет: "a > b && b * c == 100 | (True && False = false)"
            Assert.Explain(() => a > b && b * c == 100);
        }
    }
    
    public class MoneyTransferGenerator : YieldTestGenerator<object[]>
    {
        public override IEnumerable<object[]> GenerateTestCases()
        {
            // fromId, toId, amount, expectedBalanceFrom
            yield return new object[] { 1, 2, 100, 900 };    // снимаем 100 с 1го счета -> ожидаем 900
            yield return new object[] { 1, 2, 500, 500 };    // снимаем 500 -> ожидаем 500
            yield return new object[] { 1, 2, 1000, 0 };     // снимаем 1000 -> ожидаем 0
            yield return new object[] { 2, 1, 50, 450 };     // со 2го счета снимаем 50 -> ожидаем 450
        }
    }
}