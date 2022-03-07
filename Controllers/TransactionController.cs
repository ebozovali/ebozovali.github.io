using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Portfolio.Domain.Models;
using Portfolio.Infrastructure.Repositories;
using Portfolio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portfolio.Controllers
{
    public class TransactionController : Controller
    {
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly IRepository<Asset> _assetRepository;
        private readonly IRepository<Coin> _coinRepository;

        public TransactionController(IRepository<Transaction> transactionRepository, IRepository<Asset> assetRepository, IRepository<Coin> coinRepository)
        {
            this._transactionRepository = transactionRepository;
            this._assetRepository = assetRepository;
            this._coinRepository = coinRepository;
        }
        public IActionResult Index(Guid id)
        {
            var transactions = _transactionRepository.AllById(id).ToList();
            return View(transactions);
        }
        public IActionResult Create(Guid? assetId, Guid portfolioId, Guid? coinId)
        {
            CreateTransactionModel transaction = new CreateTransactionModel();
            if (coinId != null)
            {
                Asset asset = GetAsset(assetId, portfolioId, coinId.Value);
                transaction = new CreateTransactionModel()
                {
                    AssetId = asset.Id,
                    CoinName = _coinRepository.Get(coinId.Value).Name,
                    PortfolioId = asset.PortfolioId,
                    SelectedCoin = coinId.Value,
                    Price = GetCoinPrice(coinId)
                };
            }

            transaction.Coins = new SelectList(_coinRepository.All().OrderBy(n => n.Name), "Id", "Name", coinId);
            transaction.ExecutionDateTime = DateTime.Now;
            return View(transaction);
        }

        private double GetCoinPrice(Guid? coinId)
        {
            return _coinRepository.Get(coinId.Value).Price;
        }

        private Asset GetAsset(Guid? assetId, Guid portfolioId, Guid coinId)
        {
            Asset asset = new Asset();
            if (assetId == null)
            {
                //Check coins assets
                bool isThereAssetInPortfolio = _assetRepository.AllById(portfolioId).Any(n => n.CoinId == coinId);
                if (isThereAssetInPortfolio)
                {
                    asset = _assetRepository.AllById(portfolioId).FirstOrDefault(n => n.CoinId == coinId);
                }
                else
                {
                    asset.CoinId = coinId;
                    asset.PortfolioId = portfolioId;
                    _assetRepository.Add(asset);
                    _assetRepository.SaveChanges();
                }

            }
            else
            {
                asset = _assetRepository.Get(assetId.Value);
            }
            return asset;
        }

        [HttpPost]
        public ActionResult Create(CreateTransactionModel transactionModel)
        {
            if (ModelState.IsValid)
            {
                bool isAssetBeenCreated = _assetRepository.Any(n => n.CoinId == transactionModel.SelectedCoin && n.PortfolioId == transactionModel.PortfolioId);
                var asset = isAssetBeenCreated ? _assetRepository.Get(transactionModel.AssetId ?? _assetRepository.Find(n => n.CoinId == transactionModel.SelectedCoin).FirstOrDefault().Id) : GetAsset(null, transactionModel.PortfolioId.Value, transactionModel.SelectedCoin);
                Transaction transaction = new Transaction()
                {
                    Type = transactionModel.Type,
                    Amount = transactionModel.Amount,
                    AssetId = asset.Id,
                    ExecutionDateTime = transactionModel.ExecutionDateTime,
                    Fee = transactionModel.Fee,
                    Note = transactionModel.Note,
                    Price = transactionModel.Price
                };
                _transactionRepository.Add(transaction);
                _transactionRepository.SaveChanges();

                //Update asset info
                switch (transactionModel.Type)
                {
                    case Domain.Models.Enums.TransactionTypes.Buy:
                        asset.Holdings += transactionModel.Amount;
                        break;
                    case Domain.Models.Enums.TransactionTypes.Sell:
                        asset.Holdings -= transactionModel.Amount;
                        break;
                    case Domain.Models.Enums.TransactionTypes.TransferIn:
                        asset.Holdings += transactionModel.Amount;
                        break;
                    case Domain.Models.Enums.TransactionTypes.TransferOut:
                        asset.Holdings -= transactionModel.Amount;
                        break;
                    default:
                        break;
                }
                asset.AverageBuyPrice = GetAverageBuyPriceofAsset(asset.Id);
                _assetRepository.Update(asset);
                _assetRepository.SaveChanges();

                return RedirectToAction("Index", "Portfolio", new { portfolioId = asset.PortfolioId });
            }
            return View(transactionModel);
        }

        private double GetAverageBuyPriceofAsset(Guid id)
        {
            var totalBuy = _transactionRepository.AllById(id)
                    .Where(n => n.Type == Domain.Models.Enums.TransactionTypes.Buy || n.Type == Domain.Models.Enums.TransactionTypes.TransferIn).Sum(n => n.Price * n.Amount);
            var totalAmount = _transactionRepository.AllById(id).Where(n => n.Type == Domain.Models.Enums.TransactionTypes.Buy || n.Type == Domain.Models.Enums.TransactionTypes.TransferIn).Sum(n => n.Amount);
            return totalAmount != 0 ? totalBuy / totalAmount : 0;

        }

        public IActionResult Delete(Guid id)
        {
            var trans = _transactionRepository.Get(id);

            //Update asset info
            //ToDo: Add as assetrepo service to make single responsibility
            var asset = _assetRepository.Get(trans.AssetId);
            switch (trans.Type)
            {
                case Domain.Models.Enums.TransactionTypes.Buy:
                    asset.Holdings -= trans.Amount;
                    break;
                case Domain.Models.Enums.TransactionTypes.Sell:
                    asset.Holdings += trans.Amount;
                    break;
                case Domain.Models.Enums.TransactionTypes.TransferIn:
                    asset.Holdings -= trans.Amount;
                    break;
                case Domain.Models.Enums.TransactionTypes.TransferOut:
                    asset.Holdings += trans.Amount;
                    break;
                default:
                    break;
            }
            _transactionRepository.Delete(trans);
            _transactionRepository.SaveChanges();
            asset.AverageBuyPrice = GetAverageBuyPriceofAsset(asset.Id);
            _assetRepository.Update(asset);
            _assetRepository.SaveChanges();

            return RedirectToAction("Index", "Portfolio", new { portfolioId = asset.PortfolioId });
        }
    }

}
