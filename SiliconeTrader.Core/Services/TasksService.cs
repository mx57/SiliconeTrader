using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SiliconeTrader.Core
{
    internal class TasksService : ITasksService
    {
        private ConcurrentDictionary<string, ITimedTask> tasks = new ConcurrentDictionary<string, ITimedTask>();
        private Stopwatch syncStopSwatch = new Stopwatch();
        private UnhandledExceptionEventHandler exceptionHandler;

        public T AddTask<T>(string name, T task, double interval, double startDelay = 0, bool startTask = true, bool runNow = false, int skipIteration = 0)
            where T : ITimedTask
        {
            tasks[name] = task;
            task.Interval = interval;
            task.StartDelay = startDelay;
            task.Stopwatch = syncStopSwatch;
            task.SkipIteration = skipIteration;

            if (exceptionHandler != null)
            {
                task.UnhandledException += exceptionHandler;
            }
            if (startTask)
            {
                task.Start();
            }
            if (runNow)
            {
                task.RunNow();
            }
            return task;
        }

        public void RemoveTask(string name, bool stopTask = true)
        {
            if (tasks.TryRemove(name, out ITimedTask task))
            {
                if (stopTask)
                {
                    task.Stop();
                }
                if (exceptionHandler != null)
                {
                    task.UnhandledException -= exceptionHandler;
                }
                task.Stopwatch = null;
            }
        }

        public void StartAllTasks()
        {
            foreach (ITimedTask task in tasks.Values)
            {
                task.Start();
            }
        }

        public void StopAllTasks()
        {
            foreach (ITimedTask task in tasks.Values)
            {
                task.Stop();
            }
            syncStopSwatch.Reset();
        }

        public void RemoveAllTasks()
        {
            foreach (string taskName in tasks.Keys)
            {
                this.RemoveTask(taskName);
            }
            syncStopSwatch.Reset();
        }

        public ITimedTask GetTask(string name)
        {
            if (tasks.TryGetValue(name, out ITimedTask task))
            {
                return task;
            }
            else
            {
                return null;
            }
        }

        public T GetTask<T>(string name)
        {
            return (T)this.GetTask(name);
        }

        public IEnumerable<KeyValuePair<string, ITimedTask>> GetAllTasks()
        {
            return tasks.AsEnumerable();
        }

        public void SetUnhandledExceptionHandler(UnhandledExceptionEventHandler handler)
        {
            exceptionHandler = handler;
        }
    }
}
