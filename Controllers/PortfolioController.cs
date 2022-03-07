using Microsoft.AspNetCore.Mvc;
using Portfolio.Domain.Models;
using Portfolio.Infrastructure.Repositories;
using Portfolio.Models;
using System;
using System.Linq;

namespace Portfolio.Controllers
{
    public class PortfolioController : Controller
    {
        private readonly IRepository<Asset> _assetRepository;
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly IRepository<Coin> _coinRepository;
        private readonly IRepository<Portfolio.Domain.Models.Portfolio> _portfolioRepository;

        public PortfolioController(IRepository<Asset> assetRepository, IRepository<Coin> coinRepository, IRepository<Transaction> transactionRepository, IRepository<Portfolio.Domain.Models.Portfolio> portfolioRepository)
        {
            this._portfolioRepository = portfolioRepository;
            this._assetRepository = assetRepository;
            this._transactionRepository = transactionRepository;
            this._coinRepository = coinRepository;
        }
        public IActionResult Index(Guid? accountId, Guid? portfolioId)
        {
            if (accountId == null)
            {
                accountId = Guid.Parse("3b7d2f1a-af13-4ffa-b5e9-733c23759101");
            }
            var portfolios = _portfolioRepository.AllById(accountId.Value).ToList();
            portfolioId = portfolioId ?? portfolios.FirstOrDefault().Id;
            ListPortfolioModel model = new ListPortfolioModel() { Portfolios = portfolios, SelectedPortfolioId = portfolioId };
            return View(model);
        }

        

        [HttpGet]
        public IActionResult Delete(Guid? id)
        {
            var portfolio = _portfolioRepository.Get(id.Value);
            return View(portfolio);
        }
        [HttpPost]
        public IActionResult DeleteConfirm(Guid? id)
        {
            var portfolio = _portfolioRepository.Get(id.Value);
            _portfolioRepository.Delete(portfolio);
            _portfolioRepository.SaveChanges();
            // return RedirectToAction("Index", "Portfolio");
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Create(Guid? accountId)
        {
            Portfolio.Domain.Models.Portfolio portfolio = new Portfolio.Domain.Models.Portfolio() { AccountId = accountId.Value };
            return View(portfolio);
        }
        [HttpPost]
        public ActionResult Create(Portfolio.Domain.Models.Portfolio model)
        {
            model.Id = Guid.NewGuid();
            _portfolioRepository.Add(model);
            _portfolioRepository.SaveChanges();
            return RedirectToAction("Index", "Portfolio", new { portfolioId = model.Id });
        }

    }
}
