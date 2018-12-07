using System;

namespace Unitest.Sample
{
    public class NotAuthorizedException: Exception
    {
        public string AccountNumber { get; }
        public string UserName { get; }

        public NotAuthorizedException(string accountNumber, string userName)
            :base($"Cannot withdraw from account '{accountNumber}' because user '{userName}' is not authorized to perform withdrawal")
        {
            AccountNumber = accountNumber;
            UserName = userName;
        }
    }
}