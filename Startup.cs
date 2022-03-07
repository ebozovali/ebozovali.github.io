using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NoobsMuc.Coinmarketcap.Client;
using Portfolio.Context;
using Portfolio.Domain.Models;
using Portfolio.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Portfolio
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<PortfolioContext>();
            services.AddTransient<IRepository<Asset>, AssetRepository>();
            services.AddTransient<IRepository<Coin>, CoinRepository>();
            services.AddTransient<IRepository<Transaction>, TransactionRepository>();
            services.AddTransient<IRepository<Portfolio.Domain.Models.Portfolio>, PortfolioRepository>();
            services.AddControllersWithViews();
            CreateInitialDatabase();

        }

        public void CreateInitialDatabase()
        {
            using (var context = new PortfolioContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                Guid bnbId=new Guid();
                var coinRepository = new CoinRepository(context);
                ICoinmarketcapClient m_Sut = new CoinmarketcapClient("00dc1ee6-871a-4bc2-8d9c-0bd028c84975");
                var retValue = m_Sut.GetCurrencies(50);
                foreach (var item in retValue)
                {
                    var coin = new Coin { Id = Guid.NewGuid(), Name = item.Name, DayChange = item.PercentChange24h, ShortName = item.Symbol, Price = item.Price };
                    if (item.Name == "BNB")
                        bnbId = coin.Id;
                    coinRepository.Add(coin);
                }

                var accountofElber = new Account { Id = Guid.Parse("3b7d2f1a-af13-4ffa-b5e9-733c23759101"), Username = "Elber" };
                var accountRepository = new AccountRepository(context);
                accountRepository.Add(accountofElber);

                var portfolioofElber = new Portfolio.Domain.Models.Portfolio { Id = Guid.Parse("3b7d2f1a-af13-4ffa-b5e9-733c23759193"), Name = "Elber's longterm portfolio",AccountId = accountofElber.Id, };
                var portfolioRepository = new PortfolioRepository(context);
                portfolioRepository.Add(portfolioofElber);

                var asset = new Asset { Id = Guid.Parse("3b7d2f1a-af13-4ffa-b5e9-733c23759120"),AverageBuyPrice = 500,Holdings=100,PortfolioId= portfolioofElber.Id, ProfitLoss=0,CoinId = bnbId };
                var assetRepository = new AssetRepository(context);
                assetRepository.Add(asset);


                var transaction = new Portfolio.Domain.Models.Transaction {Id= Guid.Parse("3b7d2f1a-af13-4ffa-b5e9-733c23759131"),Amount=100, AssetId = asset.Id, ExecutionDateTime = new DateTime(2022, 01, 01, 16, 32, 0),Fee=0,Price=500,Type = Domain.Models.Enums.TransactionTypes.Buy };
                var transactionRepository = new TransactionRepository(context);
                transactionRepository.Add(transaction);

                assetRepository.Add(asset);

                portfolioRepository.SaveChanges();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
