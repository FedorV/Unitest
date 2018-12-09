using System;
using System.Threading.Tasks;
using NSubstitute;

namespace Unitest.Sample.UnitTests
{
    public class AccountManagerFixture : Fixture
    {
        public AccountManagerFixture ValidAccountExistsInDatabase(string accountNumber)
        {
            SubstituteFor<IAccountRepository>()
                .GetAccount(Arg.Is(accountNumber))
                .Returns(new Account
                {
                    Number = accountNumber
                });
            return this;
        }

        public AccountManagerFixture AccountDoesNotExist(string accountNumber)
        {
            SubstituteFor<IAccountRepository>()
                .GetAccount(Arg.Is(accountNumber))
                .Returns((Account)null);
            return this;
        }

        public AccountManagerFixture AccountDoesNotHaveEnoughFundsForWithdrawal(string accountNumber, decimal withdrawalAmount)
        {
            SubstituteFor<IAccountRepository>()
                .GetBalance(Arg.Is<Account>(x => x.Number == accountNumber))
                .Returns(Math.Max(0, withdrawalAmount - 10));
            return this;
        }

        public AccountManagerFixture AccountHasEnoughFundsForWithdrawal(string accountNumber, decimal withdrawalAmount)
        {
            SubstituteFor<IAccountRepository>()
                .GetBalance(Arg.Is<Account>(x => x.Number == accountNumber))
                .Returns(Math.Max(0, withdrawalAmount + 10));
            return this;
        }

        public async Task<AccountManagerFixture> TransactionWasCommited()
        {
            await SubstituteFor<IAccountRepository>()
                .Received()
                .CommitTransaction(Arg.Any<int>());
            return this;
        }

        public async Task<AccountManagerFixture> DidWithdrawalForAccount(string account, decimal amount)
        {
            await SubstituteFor<IAccountRepository>()
                .Received()
                .SubtractFromAccount(amount, Arg.Is<Account>(x => x.Number == account), Arg.Any<int>());
            return this;
        }

        public async Task<AccountManagerFixture> DidDepositToAccount(string account, decimal amount)
        {
            await SubstituteFor<IAccountRepository>()
                .Received()
                .AddToAccount(amount, Arg.Is<Account>(x => x.Number == account), Arg.Any<int>());
            return this;
        }

        public AccountManagerFixture UserIsNotAuthorisedToWithdraw(string accountNumber, User user)
        {
            SubstituteFor<IAuthorizer>()
                .IsAuthorizedToPerformTransfer(Arg.Is<Account>(x => x.Number == accountNumber), Arg.Is(user))
                .Returns(false);
            return this;
        }

        public AccountManagerFixture UserIsAuthorisedToWithdraw(string accountNumber, User user)
        {
            SubstituteFor<IAuthorizer>()
                .IsAuthorizedToPerformTransfer(Arg.Is<Account>(x => x.Number == accountNumber), Arg.Is(user))
                .Returns(true);
            return this;
        }
    }
}