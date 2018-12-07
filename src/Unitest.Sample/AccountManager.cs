using System;
using System.Threading.Tasks;

namespace Unitest.Sample
{
    public class AccountManager
    {
        private IAuthorizer Authorizer { get; }
        private IAccountRepository AccountRepository { get; }

        public AccountManager(IAuthorizer authorizer, IAccountRepository accountRepository)
        {
            Authorizer = authorizer;
            AccountRepository = accountRepository;
        }

        public async Task<bool> TransferMoney(decimal amount, string from, string to, User user)
        {
            if (amount <= 1)
                throw new ArgumentException($"Minimum amount for transfer is 1$");

            if (from == to)
                throw new ArgumentException($"Cannot transfer to same account number '{from}'");

            var fromAccount = await AccountRepository.GetAccount(from);
            if (fromAccount == null)
                throw new ArgumentException($"Account '{from}' not found");

            var toAccount = await AccountRepository.GetAccount(to);
            if (toAccount == null)
                throw new ArgumentException($"Account '{to}' not found");

            if (!await Authorizer.IsAuthorizedToPerformTransfer(fromAccount, user))
                throw new NotAuthorizedException(from, user.Name);

            var balance = await AccountRepository.GetBalance(fromAccount);
            if (balance < amount)
                throw new InvalidOperationException($"Not enough money on account '{from}'");

            var transaction = await AccountRepository.SubtractFromAccount(amount, fromAccount, -1);
            try
            {
                await AccountRepository.AddToAccount(amount, toAccount, transaction);
                await AccountRepository.CommitTransaction(transaction);
                return true;
            }
            catch (Exception e)
            {
                await AccountRepository.RollbackTransaction(transaction);
                return false;
            }
        }

        public async Task<bool> Withdraw(decimal amount, string from, User user)
        {
            if (amount <= 1)
                throw new ArgumentException($"Minimum amount for withdrawal is 1$");

            var fromAccount = await AccountRepository.GetAccount(from);
            if (fromAccount == null)
                throw new ArgumentException($"Account '{from}' not found");

            if (!await Authorizer.IsAuthorizedToPerformTransfer(fromAccount, user))
                throw new NotAuthorizedException(from, user.Name);

            var balance = await AccountRepository.GetBalance(fromAccount);
            if (balance < amount)
                throw new InvalidOperationException($"Not enough money on account '{from}'");

            var transaction = await AccountRepository.SubtractFromAccount(amount, fromAccount, -1);
            try
            {
                await AccountRepository.CommitTransaction(transaction);
                return true;
            }
            catch (Exception e)
            {
                await AccountRepository.RollbackTransaction(transaction);
                return false;
            }
        }

        public async Task<bool> Deposit(decimal amount, string to)
        {
            var toAccount = await AccountRepository.GetAccount(to);
            if (toAccount == null)
                throw new ArgumentException($"Account '{to}' not found");

            var transaction = await AccountRepository.AddToAccount(amount, toAccount, -1);
            try
            {
                await AccountRepository.CommitTransaction(transaction);
                return true;
            }
            catch (Exception e)
            {
                await AccountRepository.RollbackTransaction(transaction);
                return false;
            }
        }
    }
}
