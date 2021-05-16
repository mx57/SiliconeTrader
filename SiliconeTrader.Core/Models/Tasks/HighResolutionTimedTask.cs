using System;
using System.Diagnostics;
using System.Threading;

namespace SiliconeTrader.Core
{
    public abstract class HighResolutionTimedTask : ITimedTask
    {
        /// <summary>
        /// Raised on unhandled exception
        /// </summary>
        public event UnhandledExceptionEventHandler UnhandledException;

        /// <summary>
        /// Delay before starting the task in milliseconds
        /// </summary>
        public double StartDelay { get; set; } = 0;

        /// <summary>
        /// Periodic execution interval in milliseconds
        /// </summary>
        public double Interval { get; set; } = 1000;

        /// <summary>
        /// How often to skip task execution (in RunCount)
        /// </summary>
        public int SkipIteration { get; set; } = 0;

        /// <summary>
        /// The priority of the timer thread
        /// </summary>
        public ThreadPriority Priorty { get; set; } = ThreadPriority.Normal;

        /// <summary>
        /// Stopwatch to use for timing the intervals
        /// </summary>
        public Stopwatch Stopwatch { get; set; }

        /// <summary>
        /// Indicates whether the task is currently running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Number of times the task has been run
        /// </summary>
        public long RunCount { get; private set; }

        /// <summary>
        /// Total time it took to run the task in milliseconds
        /// </summary>
        public double TotalRunTime { get; private set; }

        /// <summary>
        /// Total task run delay in milliseconds
        /// </summary>
        public double TotalLagTime { get; private set; }

        private Thread timerThread;
        private Stopwatch runWatch;
        private ManualResetEvent timingEvent;
        private ManualResetEvent blockingEvent;

        /// <summary>
        /// Start the task
        /// </summary>
        public void Start()
        {
            if (!this.IsRunning)
            {
                this.IsRunning = true;
                runWatch = new Stopwatch();
                timingEvent = new ManualResetEvent(false);
                blockingEvent = new ManualResetEvent(true);

                timerThread = new Thread(() =>
                {
                    if (this.Stopwatch == null)
                    {
                        this.Stopwatch = Stopwatch.StartNew();
                    }
                    else if (!this.Stopwatch.IsRunning)
                    {
                        this.Stopwatch.Restart();
                    }

                    long startTime = this.Stopwatch.ElapsedMilliseconds;
                    double nextRunTime = this.StartDelay + this.Interval;

                    while (this.IsRunning)
                    {
                        blockingEvent.WaitOne();
                        long elapsedTime = this.Stopwatch.ElapsedMilliseconds;
                        long runningTime = elapsedTime - startTime;
                        if (nextRunTime < runningTime) nextRunTime = runningTime;
                        double waitTime = nextRunTime - runningTime;
                        if (waitTime > 0)
                        {
                            if (timingEvent.WaitOne((int)(waitTime)))
                            {
                                break;
                            }
                        }

                        if (this.SkipIteration == 0 || this.RunCount % this.SkipIteration != 0)
                        {
                            runWatch.Restart();
                            this.SafeRun();
                            long runTime = runWatch.ElapsedMilliseconds;
                            this.TotalLagTime += runTime - this.Interval;
                            this.TotalRunTime += runTime;
                        }
                        this.RunCount++;
                        nextRunTime += this.Interval;
                    }
                });

                timerThread.Priority = this.Priorty;
                timerThread.Start();
            }
        }

        /// <summary>
        /// Stop the task
        /// </summary>
        public void Stop()
        {
            this.Stop(true);
        }

        /// <summary>
        /// Stop the task
        /// </summary>
        /// <remarks>
        /// This function is waiting an executing thread (unless join is set to false).
        /// </remarks>
        public void Stop(bool terminateThread)
        {
            if (this.IsRunning)
            {
                this.IsRunning = false;
                timingEvent.Set();
                blockingEvent.Set();
                runWatch.Stop();

                if (!terminateThread)
                {
                    timerThread?.Join();
                    timerThread = null;
                }

                timingEvent.Dispose();
            }
        }

        /// <summary>
        /// Temporarily pause the task
        /// </summary>
        public void Pause()
        {
            blockingEvent.Reset();
        }

        /// <summary>
        ///  Continue running the task
        /// </summary>
        public void Continue()
        {
            blockingEvent.Set();
        }

        /// <summary>
        /// Manually run the task
        /// </summary>
        public void RunNow()
        {
            this.SafeRun();
        }

        /// <summary>
        /// This method must be implemented by the child class and must contain the code
        /// to be executed periodically.
        /// </summary>
        protected abstract void Run();

        /// <summary>
        /// Wrap Run method in Try/Catch
        /// </summary>
        private void SafeRun()
        {
            try
            {
                this.Run();
            }
            catch (Exception ex)
            {
                UnhandledException?.Invoke(this, new UnhandledExceptionEventArgs(ex, false));
            }
        }
    }
}
