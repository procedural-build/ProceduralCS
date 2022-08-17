using Grasshopper.Kernel;
using System.Threading.Tasks;

namespace ComputeGH.Grasshopper.Utils
{
    public abstract class PB_TaskCapableComponent<T> : GH_TaskCapableComponent<T>
    {
        protected PB_TaskCapableComponent(
            string name,
            string nickname,
            string description,
            string category,
            string subCategory) : base(name, nickname, description, category, subCategory)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (InPreSolve)
            {
                // First pass; collect input data and  Queue up the task
                TaskList.Add(CreateTask(DA));
                return;
            }

            if (!GetSolveResults(DA, out T result))
            {
                // Compute right here; collect input data and run task
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Parallel processing must be turned on!");
            }

            // Set output data
            if (result != null)
                SetOutputData(DA, result);
            else
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No result was solved");
        }

        public abstract Task<T> CreateTask(IGH_DataAccess DA);
        public abstract void SetOutputData(IGH_DataAccess DA, T result);

        protected Task<T> DefaultTask() => Task.FromResult<T>(default);
    }
}
