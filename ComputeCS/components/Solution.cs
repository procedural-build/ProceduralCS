using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;

namespace ComputeCS.Components
{
    public static class CFDSolution
    {
        public static Dictionary<string, object> Setup(
            string inputJson,
            List<int> cpus,
            string solver,
            List<Dictionary<string, object>> boundaryConditions,
            Dictionary<string, int> iterations,
            int numberOfAngles,
            Dictionary<string, object> overrides = null
        )
        {
            var inputData = SerializeIO.InputsFromJson(inputJson);
            var solution = new types.CFDSolution
            {
                CPUs = cpus,
                Solver = solver,
                BoundaryConditions = boundaryConditions,
                Iterations = iterations,
                Angles = GetAngleListFromNumber(numberOfAngles),
                Overrides = overrides
            };
            var output = SerializeIO.OutputToJson(
                inputData,
                null,
                null,
                null,
                null,
                null,
                solution
            );

            return new Dictionary<string, object>
            {
                {"out", output}
            };
        }

        static List<double> GetAngleListFromNumber(
            int numberOfAngles
        )
        {
            return Enumerable.Range(0, numberOfAngles).Select(index => (double)index * 360 / numberOfAngles).ToList();
        }
    }
}