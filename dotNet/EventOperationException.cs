using System;
using System.Runtime.Serialization;

namespace IntervalTreeClocks
{
    [Serializable]
    public class EventOperationException : Exception
    {
        public EventOperationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        public EventOperationException(string message, Event e1, Event e2)
            : base(string.Format("{0} - Event1: {1} - Event2: {2}", message, e1, e2))
        {
            Event1 = e1;
            Event2 = e2;
        }

        public Event Event1 { get; private set; }
        public Event Event2 { get; private set; }
    }
}