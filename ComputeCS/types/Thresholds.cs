using System.Collections.Generic;

namespace ComputeCS.types
{
    public class Thresholds
    {
        public class WindThreshold : SerializeBase<WindThreshold>
        {
            private string field;
            public string Field
            {
                get => field;
                set => field = CheckForInvalidCharacters(value);
            }
            public double Value;

            private static string CheckForInvalidCharacters(string value)
            {
                return value.Replace(" ", "_");
            }
        }
        
        public class ComfortThreshold : SerializeBase<ComfortThreshold>
        {
            private string field;
            public string Field
            {
                get => field;
                set => field = CheckForInvalidCharacters(value);
            }
            public List<double?> Value;

            private static string CheckForInvalidCharacters(string value)
            {
                return value.Replace(" ", "_");
            }
        }
    }
}