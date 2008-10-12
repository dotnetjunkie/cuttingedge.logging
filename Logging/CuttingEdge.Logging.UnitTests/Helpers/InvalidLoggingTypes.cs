using System;
using System.Collections.Generic;

namespace CuttingEdge.Logging.UnitTests.Helpers
{
    internal sealed class EventTypeEnumerator
    {
        public static IEnumerable<EventType> GetValidValues()
        {
            foreach (EventType type in Enum.GetValues(typeof(EventType)))
            {
                yield return type;
            }
        }

        public static IEnumerable<EventType> GetInvalidValues()
        {
            for (int i = -30; i < 30; i++)
            {
                EventType type = (EventType)i;

                if (EventType.Error != type && EventType.Information != type &&
                    EventType.Warning != type)
                {
                    yield return type;
                }
            }
        }
    }
}
