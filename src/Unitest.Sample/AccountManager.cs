using System;
using System.Threading.Tasks;

namespace HockyTest.Sample
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
            if (from == to)
                throw new ArgumentException("Cannot transfer to same account");

            var fromAccount = await AccountRepository.GetAccount(from);
            var toAccount = await AccountRepository.GetAccount(to);

            if (fromAccount == null || toAccount == null)
                throw new ArgumentException("Account not found");

            if (!await Authorizer.IsAuthorizedToPerformTransfer(fromAccount, user))
                throw new NotAuthorizedException();

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
    }
}
