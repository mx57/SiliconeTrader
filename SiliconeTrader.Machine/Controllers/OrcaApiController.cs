using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SiliconeTrader.Core;

namespace SiliconeTrader.Machine.Controllers
{
    [Route("api/ORCA/v1")]
    [ApiController]
    public partial class OrcaApiController : ControllerBase
    {
        private static Dictionary<DateTimeOffset, List<TradeResult>> GetTrades(DateTimeOffset? date = null)
        {
            ICoreService coreService = Application.Resolve<ICoreService>();
            string logsPath = Path.Combine(Directory.GetCurrentDirectory(), "log");
            var tradeResultPattern = new Regex($"{nameof(TradeResult)} (?<data>\\{{.*\\}})", RegexOptions.Compiled);
            var trades = new Dictionary<DateTimeOffset, List<TradeResult>>();

            if (Directory.Exists(logsPath))
            {
                foreach (string tradesLogFilePath in Directory.EnumerateFiles(logsPath, "*-trades.txt", SearchOption.TopDirectoryOnly))
                {
                    IEnumerable<string> logLines = Misc.Utils.ReadAllLinesWriteSafe(tradesLogFilePath);
                    foreach (string logLine in logLines)
                    {
                        Match match = tradeResultPattern.Match(logLine);
                        if (match.Success)
                        {
                            string data = match.Groups["data"].ToString();
                            string json = Misc.Utils.FixInvalidJson(data.Replace(nameof(OrderMetadata), ""))
                                .Replace("AveragePricePaid", nameof(ITradeResult.AveragePrice)); // Old property migration

                            TradeResult tradeResult = JsonConvert.DeserializeObject<TradeResult>(json);
                            if (tradeResult.IsSuccessful && tradeResult.Metadata?.IsTransitional != true)
                            {
                                DateTimeOffset tradeDate = tradeResult.SellDate.ToOffset(TimeSpan.FromHours(coreService.Config.TimezoneOffset)).Date;
                                if (date == null || date == tradeDate)
                                {
                                    if (!trades.ContainsKey(tradeDate))
                                    {
                                        trades.Add(tradeDate, new List<TradeResult>());
                                    }
                                    trades[tradeDate].Add(tradeResult);
                                }
                            }
                        }
                    }
                }
            }
            return trades;
        }
    }
}