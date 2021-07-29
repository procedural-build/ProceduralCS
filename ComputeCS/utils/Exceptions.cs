using System;

namespace ComputeCS.Exceptions
{
    [Serializable]
    public class NoObjectFoundException : Exception
    {
        public NoObjectFoundException() {  }

        public NoObjectFoundException(string message) : base(message)
        {

        }
    }
}