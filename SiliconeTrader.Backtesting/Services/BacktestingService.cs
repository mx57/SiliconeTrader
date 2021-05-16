using SiliconeTrader.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SiliconeTrader.Signals.Base;
using SiliconeTrader.Trading;

namespace SiliconeTrader.Backtesting
{
    internal class BacktestingService : ConfigrableServiceBase<BacktestingConfig>, IBacktestingService
    {
        public const string SNAPSHOT_FILE_EXTENSION = "bin";

        public override string ServiceName => Constants.ServiceNames.BacktestingService;

        IBacktestingConfig IBacktestingService.Config => this.Config;

        public object SyncRoot { get; private set; } = new object();

        private readonly ILoggingService loggingService;
        private readonly IHealthCheckService healthCheckService;
        private readonly ITasksService tasksService;
        private ISignalsService signalsService;
        private ITradingService tradingService;
        private BacktestingLoadSnapshotsTimedTask backtestingLoadSnapshotsTimedTask;
        private BacktestingSaveSnapshotsTimedTask backtestingSaveSnapshotsTimedTask;

        public BacktestingService(ILoggingService loggingService, IHealthCheckService healthCheckService, ITasksService tasksService)
        {
            this.loggingService = loggingService;
            this.healthCheckService = healthCheckService;
            this.tasksService = tasksService;
        }

        public void Start()
        {
            loggingService.Info($"Start Backtesting service... (Replay: {this.Config.Replay})");

            signalsService = Application.Resolve<ISignalsService>();
            tradingService = Application.Resolve<ITradingService>();

            if (this.Config.Replay)
            {
                backtestingLoadSnapshotsTimedTask = tasksService.AddTask(
                    name: nameof(BacktestingLoadSnapshotsTimedTask),
                    task: new BacktestingLoadSnapshotsTimedTask(loggingService, healthCheckService, tradingService, this),
                    interval: this.Config.SnapshotsInterval / this.Config.ReplaySpeed * 1000,
                    startDelay: Constants.TaskDelays.HighDelay,
                    startTask: false,
                    runNow: false,
                    skipIteration: 0);
            }

            backtestingSaveSnapshotsTimedTask = tasksService.AddTask(
                name: nameof(BacktestingSaveSnapshotsTimedTask),
                task: new BacktestingSaveSnapshotsTimedTask(loggingService, healthCheckService, tradingService, signalsService, this),
                interval: this.Config.SnapshotsInterval * 1000,
                startDelay: Constants.TaskDelays.HighDelay,
                startTask: false,
                runNow: false,
                skipIteration: 0);

            if (this.Config.DeleteLogs)
            {
                loggingService.DeleteAllLogs();
            }

            string virtualAccountPath = Path.Combine(Directory.GetCurrentDirectory(), tradingService.Config.VirtualAccountFilePath);
            if (File.Exists(virtualAccountPath) && (this.Config.DeleteAccountData || !String.IsNullOrWhiteSpace(this.Config.CopyAccountDataPath)))
            {
                File.Delete(virtualAccountPath);
            }

            if (!String.IsNullOrWhiteSpace(this.Config.CopyAccountDataPath))
            {
                File.Copy(Path.Combine(Directory.GetCurrentDirectory(), this.Config.CopyAccountDataPath), virtualAccountPath, true);
            }

            if (this.Config.Replay)
            {
                Application.Speed = this.Config.ReplaySpeed;
            }

            Application.Resolve<ICoreService>().Started += this.OnCoreServiceStarted;

            loggingService.Info("Backtesting service started");
        }

        public void Stop()
        {
            loggingService.Info("Stop Backtesting service...");

            if (this.Config.Replay)
            {
                tasksService.RemoveTask(nameof(BacktestingLoadSnapshotsTimedTask), stopTask: true);
            }
            tasksService.RemoveTask(nameof(BacktestingSaveSnapshotsTimedTask), stopTask: true);

            healthCheckService.RemoveHealthCheck(Constants.HealthChecks.BacktestingSignalsSnapshotTaken);
            healthCheckService.RemoveHealthCheck(Constants.HealthChecks.BacktestingTickersSnapshotTaken);
            healthCheckService.RemoveHealthCheck(Constants.HealthChecks.BacktestingSignalsSnapshotLoaded);
            healthCheckService.RemoveHealthCheck(Constants.HealthChecks.BacktestingTickersSnapshotLoaded);

            Application.Resolve<ICoreService>().Started -= this.OnCoreServiceStarted;

            loggingService.Info("Backtesting service stopped");
        }

        public void Complete(int skippedSignalSnapshots, int skippedTickerSnapshots)
        {
            loggingService.Info("Backtesting results:");

            double lagAmount = 0;
            foreach (KeyValuePair<string, ITimedTask> kvp in tasksService.GetAllTasks().OrderBy(t => t.Key))
            {
                string taskName = kvp.Key;
                ITimedTask task = kvp.Value;

                double averageWaitTime = Math.Round(task.TotalLagTime / task.RunCount, 3);
                if (averageWaitTime > 0) lagAmount += averageWaitTime;
                loggingService.Info($" [+] {taskName} Run times: {task.RunCount}, average wait time: " + averageWaitTime);
            }

            loggingService.Info($"Lag value: {lagAmount}. Lower the ReplaySpeed if lag value is positive.");
            loggingService.Info($"Skipped signal snapshots: {skippedSignalSnapshots}");
            loggingService.Info($"Skipped ticker snapshots: {skippedTickerSnapshots}");

            tradingService.SuspendTrading(forced: true);
            signalsService.StopTrailing();
            signalsService.Stop();
        }

        public string GetSnapshotFilePath(string snapshotEntity)
        {
            DateTimeOffset date = DateTimeOffset.UtcNow;
            return Path.Combine(
                Directory.GetCurrentDirectory(),
                this.Config.SnapshotsPath,
                snapshotEntity,
                date.ToString("yyyy-MM-dd"),
                date.ToString("HH"),
                date.ToString("mm-ss-fff")
            ) + "." + SNAPSHOT_FILE_EXTENSION;
        }

        public Dictionary<string, IEnumerable<ISignal>> GetCurrentSignals()
        {
            return backtestingLoadSnapshotsTimedTask.GetCurrentSignals() ?? new Dictionary<string, IEnumerable<ISignal>>();
        }

        public Dictionary<string, ITicker> GetCurrentTickers()
        {
            return backtestingLoadSnapshotsTimedTask.GetCurrentTickers() ?? new Dictionary<string, ITicker>();
        }

        public int GetTotalSnapshots()
        {
            return backtestingLoadSnapshotsTimedTask.GetTotalSnapshots();
        }

        private void OnCoreServiceStarted()
        {
            tasksService.GetTask<TradingTimedTask>(nameof(TradingTimedTask)).SkipIteration = this.Config.TradingSpeedEasing;
            tasksService.GetTask<TradingTimedTask>(nameof(TradingTimedTask)).LoggingEnabled = false;
            tasksService.GetTask<TradingRulesTimedTask>(nameof(TradingRulesTimedTask)).SkipIteration = this.Config.TradingRulesSpeedEasing;
            tasksService.GetTask<SignalRulesTimedTask>(nameof(SignalRulesTimedTask)).SkipIteration = this.Config.SignalRulesSpeedEasing;
            tasksService.GetTask<SignalRulesTimedTask>(nameof(SignalRulesTimedTask)).LoggingEnabled = false;
        }
    }
}
