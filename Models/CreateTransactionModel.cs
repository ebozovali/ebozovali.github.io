using Microsoft.AspNetCore.Mvc.Rendering;
using Portfolio.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Portfolio.Models
{
    public class CreateTransactionModel
    {
        public TransactionTypes Type { get; set; }

        public DateTime ExecutionDateTime { get; set; }

        public double Price { get; set; }

        public double Amount { get; set; }

        public double Fee { get; set; }
        public Guid? AssetId { get; set; }
        public Guid? PortfolioId { get; set; }
        public string Note { get; set; }
        public string CoinName { get; set; }

        [Required]
        [Display(Name = "Coin")]
        public Guid SelectedCoin { get; set; }
        public IEnumerable<SelectListItem> Coins { get; set; }

    }
}
