using SiliconeTrader.Core;
using System.IO;
using System.Linq;
using ZeroFormatter;

namespace SiliconeTrader.Backtesting
{
    internal class BacktestingSaveSnapshotsTimedTask : HighResolutionTimedTask
    {
        private readonly ILoggingService loggingService;
        private readonly IHealthCheckService healthCheckService;
        private readonly ITradingService tradingService;
        private readonly ISignalsService signalsService;
        private readonly IBacktestingService backtestingService;

        public BacktestingSaveSnapshotsTimedTask(ILoggingService loggingService, IHealthCheckService healthCheckService, ITradingService tradingService, ISignalsService signalsService, IBacktestingService backtestingService)
        {
            this.loggingService = loggingService;
            this.healthCheckService = healthCheckService;
            this.tradingService = tradingService;
            this.signalsService = signalsService;
            this.backtestingService = backtestingService;
        }

        protected override void Run()
        {
            if (backtestingService.Config.Enabled && !backtestingService.Config.Replay)
            {
                this.TakeSignalsSnapshot();
                this.TakeTickersSnapshot();
            }
        }

        private void TakeSignalsSnapshot()
        {
            System.Collections.Generic.IEnumerable<SignalData> signals = signalsService.GetAllSignals().Select(s => SignalData.FromSignal(s));

            byte[] signalBytes = ZeroFormatterSerializer.Serialize(signals);
            string signalsSnapshotFilePath = backtestingService.GetSnapshotFilePath(Constants.SnapshotEntities.Signals);
            var signalsSnapshotFile = new FileInfo(signalsSnapshotFilePath);
            signalsSnapshotFile.Directory.Create();
            File.WriteAllBytes(signalsSnapshotFilePath, signalBytes);

            healthCheckService.UpdateHealthCheck(Constants.HealthChecks.BacktestingSignalsSnapshotTaken, $"Signals: {signals.Count()}");
        }

        private void TakeTickersSnapshot()
        {
            System.Collections.Generic.IEnumerable<TickerData> tickers = tradingService.Exchange.GetTickers().Select(t => TickerData.FromTicker(t));

            byte[] tickerBytes = ZeroFormatterSerializer.Serialize(tickers);
            string tickersSnapshotFilePath = backtestingService.GetSnapshotFilePath(Constants.SnapshotEntities.Tickers);
            var tickersSnapshotFile = new FileInfo(tickersSnapshotFilePath);
            tickersSnapshotFile.Directory.Create();
            File.WriteAllBytes(tickersSnapshotFilePath, tickerBytes);

            healthCheckService.UpdateHealthCheck(Constants.HealthChecks.BacktestingTickersSnapshotTaken, $"Tickers: {tickers.Count()}");
        }
    }
}
