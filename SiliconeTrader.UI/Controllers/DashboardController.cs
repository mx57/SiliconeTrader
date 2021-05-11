using Microsoft.AspNetCore.Mvc;
using SiliconeTrader.UI.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SiliconeTrader.UI.Controllers
{
    public class DashboardController : Controller
    {
        [HttpPost]
        public IActionResult Buy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BuyDefault()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Help()
        {
            return View();
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        public IActionResult Log()
        {
            return View();
        }

        public IActionResult Market()
        {
            return View();
        }

        [HttpPost]
        public IActionResult MarketPairs(List<string> signalsFilter)
        {
            return View();
        }

        public IActionResult RefreshAccount()
        {
            return View();
        }

        public IActionResult RestartServices()
        {
            return View();
        }

        public IActionResult Rules()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SaveConfig()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Sell()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Settings(SettingsViewModel model)
        {
            return View();
        }

        public IActionResult SignalNames()
        {
            return View();
        }

        public IActionResult Stats()
        {
            return View();
        }

        public async Task<IActionResult> Status()
        {
            try
            {
                var orcaBase = Environment.GetEnvironmentVariable("ORCA_API_URL");
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(orcaBase)
                };

                var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "api/ORCA/v1/status"));

                return Json(new
                {
                    InstanceName = "DEFAULT",
                    ReadOnlyMode = true,
                    Data = await response.Content.ReadAsStringAsync()
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    InstanceName = "DEFAULT",
                    ReadOnlyMode = true,
                    Data = ex.Message
                });
            }
        }

        public IActionResult Trades(DateTimeOffset id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult TradingPairs()
        {
            return View();
        }
    }
}