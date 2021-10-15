namespace ComputeCS.types
{
    public class EnergyPlusMaterial : SerializeBase<EnergyPlusMaterial>
    {
        public string Name;
        public string Preset;
        public EnergyPlusMaterialOverrides Overrides;
    }

    public class EnergyPlusMaterialOverrides : SerializeBase<EnergyPlusMaterialOverrides>
    {
        public string Roughness;
        public double Thickness;
        public double Conductivity;
        public double Density;
        public double SpecificHeat;
        public double? ThermalAbsroptance;
        public double? SolarAbsorptance;
        public double? VisibleAbsorptance;
    }
    
    public class EnergyPlusConstruction : SerializeBase<EnergyPlusConstruction>
    {
        public string Name
        {
            get => name;
            set => name = UpdateName(value);
        }
        public string Preset;
        public EnergyPlusConstructionOverrides Overrides;

        private string name;

        private static string UpdateName(string _name)
        {
            if (_name.EndsWith("*"))
            {
                return _name;
            }

            return _name + "*";
        }
    }

    public class EnergyPlusConstructionOverrides : SerializeBase<EnergyPlusConstructionOverrides>
    {
        public string Roughness;
    }
    
    public class EnergyPlusBuilding : SerializeBase<EnergyPlusBuilding>
    {
        public string Name;
        public string Preset;
        public EnergyPlusMaterialOverrides Overrides;
    }
    
    public class EnergyPlusZone : SerializeBase<EnergyPlusZone>
    {
        public string Name;
        public string Preset;
        public EnergyPlusMaterialOverrides Overrides;
    }
    
    public class EnergyPlusLoad : SerializeBase<EnergyPlusLoad>
    {
        public string Name;
        public string Preset;
        public EnergyPlusMaterialOverrides Overrides;
    }
}