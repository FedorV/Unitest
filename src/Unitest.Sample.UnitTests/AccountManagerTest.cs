using System;
using FluentAssertions;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace HockyTest.Sample.UnitTests
{
    public class AccountManagerTest : TestWith<AccountManagerFixture, AccountManager>
    {
        private readonly string AccountFrom = "12345";
        private readonly string AccountTo = "54321";

        [Fact]
        public async Task FirstTest()
        {
            Given().ValidAccountExistsInDatabase(AccountFrom, 1000);
            And().ValidAccountExistsInDatabase(AccountTo, 0);

            var action = new Action(async () => 
            {
                await SUT.TransferMoney(10, "12345", "12345", new User());
            });

            action.Should().Throw<InvalidOperationException>("because cannot transfer money to same account");
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
