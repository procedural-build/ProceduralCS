namespace ComputeCS.types
{
    public class WindThresholds
    {
        public class Threshold : SerializeBase<Threshold>
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
    }
}