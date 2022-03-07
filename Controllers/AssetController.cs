using Microsoft.AspNetCore.Mvc;
using Portfolio.Domain.Models;
using Portfolio.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portfolio.Controllers
{
    public class AssetController : Controller
    {
        private readonly IRepository<Asset> _assetRepository;
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly IRepository<Coin> _coinRepository;
        private readonly IRepository<Portfolio.Domain.Models.Portfolio> _portfolioRepository;

        public AssetController(IRepository<Asset> assetRepository, IRepository<Coin> coinRepository, IRepository<Transaction> transactionRepository, IRepository<Portfolio.Domain.Models.Portfolio> portfolioRepository)
        {
            this._portfolioRepository = portfolioRepository;
            this._assetRepository = assetRepository;
            this._transactionRepository = transactionRepository;
            this._coinRepository = coinRepository;
        }
        public IActionResult Create(Guid portfolioId)
        {
            Asset asset = new Asset();

            return View(asset);
        }
        public IActionResult Delete(Guid id)
        {
            var portfolio = _assetRepository.Get(id);
            _assetRepository.Delete(portfolio);
            _assetRepository.SaveChanges();
            return RedirectToAction("Index", "Portfolio");
        }
        
    }
}
