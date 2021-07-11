using ComputeCS.Grasshopper;
using ComputeCS.utils.Cache;
using Grasshopper.Kernel;
using Rhino;

namespace ComputeGH.Grasshopper.Utils
{
    public abstract class PB_Component : GH_Component
    {
        protected PB_Component(string name, string nickname, string description, string category,
            string subCategory) : base(name, nickname, description, category, subCategory)
        {}
        
        public void ExpireSolutionThreadSafe(bool recompute = false)
        {
            var delegated = new ExpireSolutionDelegate(ExpireSolution);
            RhinoApp.InvokeOnUiThread(delegated, recompute);
        }

        public void HandleErrors()
        {
            var errors = StringCache.getCache(InstanceGuid.ToString());
            if (!string.IsNullOrEmpty(errors))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, errors);
            }
        }
    }
}