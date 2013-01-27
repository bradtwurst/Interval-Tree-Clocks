using System;
using System.Runtime.Serialization;

namespace IntervalTreeClocks
{
    [Serializable]
    public class StampOperationException : Exception
    {
        public StampOperationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        public StampOperationException(string message)
            : base(message)
        {
        }

    }
}