using Jenkins2SkypeMsg.utils.configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Jenkins2SkypeMsg.core
{
    class TaskLauncher
    {
        static List<Thread> threads;
        static bool threadsEnabled = false;
        
        public static void run()
        {
            Trace.WriteLine("Preparing new thread and launch jobs monitoring.");
            prepareThreads();
            runThreads();
        }

        private static void prepareThreads()
        {
            threads = new List<Thread>();
            foreach(JobConfiguration config in Config.Instance)
            {
                TaskExecutors task = new TaskExecutors(config);
                Thread thread = new Thread(task.planTask);
                thread.Name = config.name;
                threads.Add(thread);
            }
            threadsEnabled = true;
        }

        private static void runThreads()
        {
            foreach(Thread thread in threads)
            {
                thread.Start();
                Trace.WriteLine("++ thread " + thread.Name + " was started");
                Thread.Sleep(1000);
            }
        }

        public static void closeThreads()
        {
            if (threadsEnabled)
            {
                threadsEnabled = false;
                string currentThread = Thread.CurrentThread.Name;
                Trace.WriteLine("Trying kill all threads from '" + currentThread + "' thread...");

                foreach (Thread thread in threads)
                {
                    if (thread.Name != currentThread)
                    {
                        thread.Abort();
                        Trace.WriteLine("Thread " + thread.Name + " was aborted.");
                    }
                }
                Trace.WriteLine("Thread " + currentThread + " kill himself...");
                Config.Instance.Find(x => x.name == currentThread).enabled = false;
            }
        }

        public static bool isEnabled()
        {
            if (threads == null)
                return false;
            return threads.Count > 0 && threadsEnabled;
        }
    }
}
