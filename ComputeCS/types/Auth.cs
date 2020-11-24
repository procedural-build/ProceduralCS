using System;
using System.Collections.Generic;
using System.Globalization;

namespace ComputeCS.types
{
    public class AuthTokens : SerializeBase<AuthTokens>
    {
        public string Access { get; set; }

        public string Refresh { get; set; }

        public List<string> ErrorMessages = null;
    }
}
