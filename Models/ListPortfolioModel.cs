using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portfolio.Models
{
    public class ListPortfolioModel
    {
        public Guid? SelectedPortfolioId { get; set; }
        public Guid AccountId { get; set; } = Guid.Parse("3b7d2f1a-af13-4ffa-b5e9-733c23759101");
        public IEnumerable<Portfolio.Domain.Models.Portfolio> Portfolios { get; set; }
    }
}
