using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using System.Threading;

/*  The QueueManager is a high-level utility class that puts Tasks onto
named queues.  This allows you to maintain several named queues 
(TaskQueue's - see below) that each manages a queue of async tasks.

Note that the "addToQueue" methods simply get the relevant named TaskQueue from
the Dictionary<string, TaskQueue> and then forward the task you want to add to 
that queue like:
`queues[queueName].addTask(a)`

A common use-type for this utility is like this:
```
// Instantiate a new class that has a "run" function which you want to run async
// on multiple threads
MyTaskClass task = new MyTaskClass(arg1, arg1)

// (optional) Set the number of concurrent tasks that you want to execute on the queue
QueueManager.setQueue("task_queue", 4);

// Queue the task to run
QueueManager.addToQueue("task_queue", () => { calc.run() });
```

A detailed example of calling QueueManager.addToQueue is also in the Swift WindThresholds
component which also does some getting/setting of Grasshopper parametrs and datatrees, to
a DataTreeCache class which stores the results cached results so that the latest results
are output from the component even if it is called in a deactivated way.  

Note for Grasshopper it is quite common to use a cache-class or cache-table like this so 
that cached resutls will be output from components that expire without having to run the 
whole calculation again, like this:
```
QueueManager.addToQueue(queueName, () => {
    try {
        // Run the calculation
        List<double[]> results = calc.run();

        // Put the results into the DataTree
        for (int i = 0; i < results.Count; i++) {
            GH_Path fullPath = new GH_Path(new int[] { calc.pathRef[0], i, calc.pathRef[1] });
            DataTreeCache.setCache(cacheKey, fullPath, results[i].ToList());
        }

        // Print the output message that this is done
        StringCache.AppendCache(this.InstanceGuid.ToString(), $"DONE patch {patch} for times {dateTimeRange}\n");
    } catch (Exception e) {
        StringCache.AppendCache(this.InstanceGuid.ToString(), e.ToString() + "\n");
    }
});
```

A DataTreeCache is has Grasshopper specific data types - so it does not belong in this
library, but for reference it is a simple class that indexes and caches some results in 
DataTree, like this:
```
public static class DataTreeCache
{
    private static Dictionary<string, DataTree<double>> cache = new Dictionary<string, DataTree<double>>();
    private static Object setCacheLock = new Object();

    public static void setCache(string key, GH_Path path, List<double> values)
    {
        lock (setCacheLock)
        {
            // Create the DataTree if it doesn't already exist
            if (!cache.ContainsKey(key)) {
                cache[key] = new DataTree<double>();
            }

            // Clear the cache first before setting a new set of values
            if (cache[key].PathExists(path)) {
                cache[key].RemovePath(path);
            }
            // Add in the new values
            cache[key].AddRange(values, path);
        }
    }

    public static DataTree<double> getCache(string key)
    {
        if (cache.ContainsKey(key)) { return cache[key]; }
        return new DataTree<double>();
    }

    public static bool ContainsKey(string key)
    {
        return cache.ContainsKey(key);
    }
}
```
*/

namespace ComputeCS.utils.Queue
{
    public static class QueueManager {
        // Initialise the app with some default queues called cfdRun and cfdPostProc
        public static Dictionary<string, TaskQueue> queues = new Dictionary<string, TaskQueue>() {
            { "cfdRun", new TaskQueue(1) },
            { "cfdPostProc", new TaskQueue(1) }
        };

        // Change the maximum concurrent threads/tasks that can be executed on a TaskQueue with
        // name "name"
        public static void setQueue(string name, int maxThreads)
        {
            if (!queues.ContainsKey(name))
            {
                queues.Add(name, new TaskQueue(maxThreads));
            }
            else
            {
                queues[name].setMaxThreads(maxThreads);
            }
        }


        // This is the most common use-type, add a callable function (a) onto a queue with
        // name queueName
        public static void addToQueue(string queueName, Action action)
        {
            if (!queues.ContainsKey(queueName))
            {
                setQueue(queueName, 1);
            }
            queues[queueName].addTask(action);
        }
    }

    public class TaskQueue
    {
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private List<Task> tasks;
        private int maxAvailableThreads;

        public TaskQueue(int maxThreads)
        {
            tasks = new List<Task>();
            setMaxThreads(maxThreads);
        }

        public void setMaxThreads(int maxThreads)
        {
            WaitUntilEmpty();
            semaphore = new SemaphoreSlim(maxThreads);
            maxAvailableThreads = maxThreads;
        }

        public int getMaxThreads()
        {
            return maxAvailableThreads;
        }

        public void WaitUntilAvailable()
        {
            semaphore.Wait();
            Thread.Sleep(100);
            semaphore.Release();
        }

        public void WaitUntilEmpty()
        {
            foreach (Task t in tasks)
            {
                if (t.IsCompleted)
                {
                    continue;
                }
                else
                {
                    t.Wait();
                }
            }
        }

        public void addTask(Action action)
        {
            // Wrap the action (task you want to run) in another task that will be executed async and
            // added to the queue (list) for this TaskQueue
            Thread.Sleep(100);
            System.Globalization.CultureInfo mainCulture = System.Globalization.CultureInfo.CurrentCulture;
            // Wrap our action (task) in another task that does the waiting/getting of locks and so on
            Task t = new Task(() =>
            {
                Thread.CurrentThread.CurrentCulture = mainCulture;
                semaphore.Wait();
                action();               // Actually execute the action (function) here 
                semaphore.Release();
            });
            // Add the task to the queue (list) and start executing it
            tasks.Add(t);
            t.Start();
            // Wait half a second to ensure thread is started before starting another (to ensure threads are added in order)
            Thread.Sleep(100);
        }
    }
}