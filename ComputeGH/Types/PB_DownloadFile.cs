using ComputeCS.types;
using Grasshopper.Kernel.Types;

namespace ComputeGH.Types
{
    public class PB_DownloadFile : GH_Goo<DownloadFile>
    {
        public PB_DownloadFile() : base() { }

        public PB_DownloadFile(DownloadFile def) : base(def) { }

        public PB_DownloadFile(GH_Goo<DownloadFile> other) : base(other) { }

        public override bool IsValid => m_value != null && m_value.Content != null;

        public override string TypeName => typeof(DownloadFile).Name;

        public override string TypeDescription => $"An instance of {TypeName}";

        public override IGH_Goo Duplicate() => new PB_DownloadFile(this);

        public override string ToString() =>
            IsValid ? $"{m_value.FilePathUnix}[{m_value.Size}b]" : "InvalidFile";

        public override string IsValidWhyNot
        {
            get
            {
                if (Value == null)
                    return "No data";
                if (Value.Content == null)
                    return "File is empty";
                return string.Empty;
            }
        }

        public override bool CastTo<Q>(ref Q target)
        {
            if (typeof(Q).IsAssignableFrom(typeof(DownloadFile)))
            {
                if (Value == null)
                    target = default;
                else
                    target = (Q)(object)Value;
                return true;
            }

            target = default;
            return false;
        }

        public override bool CastFrom(object source)
        {
            if (source == null)
            {
                return false;
            }

            if (typeof(DownloadFile).IsAssignableFrom(source.GetType()))
            {
                Value = (DownloadFile)source;
                return true;
            }

            return false;
        }
    }
}
