using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Unitest.Sample.UnitTests
{
    public class AccountManagerTest: TestWith<AccountManagerFixture> //IClassFixture<AccountManagerFixture> 
    {
        private readonly string AccountFrom = "12345";
        private readonly string AccountTo = "54321";

        public AccountManagerTest(AccountManagerFixture fixture)
            : base(fixture)
        { }

        [Fact]
        public async Task FirstTest()
        {
            Given().ValidAccountExistsInDatabase(AccountFrom, 1000);
            And().ValidAccountExistsInDatabase(AccountTo, 0);

            var action = new Action(() =>
            {
                SUT<AccountManager>().TransferMoney(10, "12345", "12345", new User()).GetAwaiter().GetResult();
            });

            action.Should().Throw<ArgumentException>("because cannot transfer money to same account");
        }
    }

    public class AccountManagerFixture : Fixture
    {
        public AccountManagerFixture ValidAccountExistsInDatabase(string accountNumber, decimal amount)
        {
            SubstituteFor<IAccountRepository>()
                .GetAccount(Arg.Is(accountNumber))
                .Returns(new Account
                {
                    Number = accountNumber,
                    Debit = amount
                });
            return this;
        }
    }
}
