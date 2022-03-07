using System.Collections.Generic;

namespace Portfolio.Models
{
    public class TransactionIndexModel
    {
        public double TotalProfitLoss { get; set; } 
        public IEnumerable<Portfolio.Domain.Models.Transaction> Transactions { get; set; }
    }
}