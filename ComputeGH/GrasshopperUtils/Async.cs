//using System;
//using ComputeCS.utils.Cache;
//using ComputeCS.utils.Queue;

//namespace ComputeGH.Grasshopper.Utils
//{
//    public class Async
//    {
//        public static void ExecuteAsync(
//            string cacheKey,
//            string queueName,
//            string executeable
//        )
//        {
//            // Get Cache to see if we already did this
//            var cachedValues = StringCache.getCache(cacheKey);

//            if (cachedValues == null)
//            {
//                // Get queue lock
//                var queueLock = StringCache.getCache(queueName);
//                if (queueLock != "true")
//                {
//                    StringCache.setCache(queueName, "true");
//                    QueueManager.addToQueue(queueName, () =>
//                    {
//                        try
//                        {
//                            var results = executeable;
//                            StringCache.setCache(cacheKey, results);
//                        }
//                        catch (Exception e)
//                        {
//                            //StringCache.AppendCache(this.InstanceGuid.ToString(), e.ToString() + "\n");
//                        }

//                        StringCache.setCache(queueName, "");
//                    });
//                    //ExpireSolution(true);
//                }
//            }
//        }
//    }
//}