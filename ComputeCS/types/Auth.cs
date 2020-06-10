using System;
using System.Globalization;

namespace ComputeCS.types
{
    public class AuthTokens : SerializeBase<AuthTokens>
    {
        public string Access { get; set; }

        public string Refresh { get; set; }
    }
}
