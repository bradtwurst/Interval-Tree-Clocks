using System;
using System.Runtime.Serialization;

namespace IntervalTreeClocks
{
    [Serializable]
    public class IdOperationException : Exception
    {
        public IdOperationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        public IdOperationException(string message, Id i1, Id i2)
            : base(string.Format("{0} - Id1: {1} - Id2: {2}", message, i1, i2))
        {
            Id1 = i1;
            Id2 = i2;
        }

        public Id Id1 { get; private set; }
        public Id Id2 { get; private set; }
    }
}