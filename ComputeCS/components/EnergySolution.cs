using System.Collections.Generic;
using System.IO;
using System.Linq;
using ComputeCS.types;

namespace ComputeCS.Components
{
    public class EnergySolution
    {
        public static string Setup(
            string inputJson,
            List<string> buildings,
            List<string> zones,
            List<string> loads,
            List<string> constructions,
            List<string> materials,
            string epwFile,
            string overrides = null
        )
        {
            var inputData = new Inputs().FromJson(inputJson);

            if (!File.Exists(epwFile))
            {
                throw new FileNotFoundException($"EPW file: {epwFile} does not exist!");
            }
            
            var solution = new types.EnergySolution
            {
                Buildings = buildings.Select(building => new EnergyPlusBuilding().FromJson(building)).ToList(),
                Zones = zones.Select(zone => new EnergyPlusZone().FromJson(zone)).ToList(),
                Loads = loads.Select(load => new EnergyPlusLoad().FromJson(load)).ToList(),
                Constructions = constructions.Select(construction => new EnergyPlusConstruction().FromJson(construction)).ToList(),
                Materials = materials.Select(material => new EnergyPlusMaterial().FromJson(material)).ToList(),
                EPWFile = epwFile,
            };

            inputData.EnergySolution = solution;
            return inputData.ToJson();
        }
    }
}