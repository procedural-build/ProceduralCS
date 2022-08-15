using ComputeCS.Exceptions;
using ComputeCS.utils.Cache;
using ComputeCS.utils.Queue;
using Grasshopper.Kernel;
using Rhino;
using System;
using System.Threading;

namespace ComputeGH.Grasshopper.Utils
{
    public abstract class PB_Component : GH_Component
    {
        protected PB_Component(string name, string nickname, string description, string category,
            string subCategory) : base(name, nickname, description, category, subCategory)
        {
        }

        protected void ExpireSolutionThreadSafe(bool recompute = false)
        {
            var delegated = new ExpireSolutionDelegate(ExpireSolution);
            RhinoApp.InvokeOnUiThread(delegated, recompute);
        }

        protected delegate void ExpireSolutionDelegate(bool recompute);

        //public override GH_Exposure Exposure => GH_Exposure.hidden;

        //public override bool Obsolete => true;

        protected void HandleErrors()
        {
            var errors = StringCache.getCache(InstanceGuid.ToString());
            if (!string.IsNullOrEmpty(errors))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, errors);
            }
        }

        protected string QueueName;
        protected string CacheKey;

        public delegate string FunctionToQueue();

        public void PutOnQueue(FunctionToQueue functionToQueue, string cachedValues, bool create)
        {
            if (cachedValues != null && !create) return;

            // Get queue lock
            var queueLock = StringCache.getCache(QueueName);
            if (queueLock == "true") return;

            StringCache.setCache(QueueName, "true");
            StringCache.setCache(CacheKey, null);
            QueueManager.addToQueue(QueueName, () =>
            {
                try
                {
                    cachedValues = functionToQueue();
                    StringCache.setCache(CacheKey, cachedValues);
                    if (create)
                    {
                        StringCache.setCache(CacheKey + "create", "true");
                    }
                }
                catch (NoObjectFoundException)
                {
                    StringCache.setCache(CacheKey + "create", "");
                }
                catch (Exception e)
                {
                    StringCache.setCache(InstanceGuid.ToString(), e.Message);
                    StringCache.setCache(CacheKey, "error");
                    StringCache.setCache(CacheKey + "create", "");
                }


                ExpireSolutionThreadSafe(true);
                Thread.Sleep(2000);
                StringCache.setCache(QueueName, "");
            });
        }
    }
}