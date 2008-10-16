using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CuttingEdge.Logging.UnitTests.Helpers
{
    internal static class EventTypeEnumerator
    {
        public static readonly ReadOnlyCollection<LoggingEventType> ValidValues;

        static EventTypeEnumerator()
        {
            List<LoggingEventType> types = new List<LoggingEventType>();

            foreach (LoggingEventType eventType in Enum.GetValues(typeof(LoggingEventType)))
            {
                types.Add(eventType);
            }

            ValidValues = new ReadOnlyCollection<LoggingEventType>(types.ToArray());
        }

        public static IEnumerable<LoggingEventType> GetValidValues()
        {
            return ValidValues;
        }

        public static IEnumerable<LoggingEventType> GetInvalidValues()
        {
            for (int i = -30; i < 30; i++)
            {
                LoggingEventType type = (LoggingEventType)i;

                if (!ValidValues.Contains(type))
                {
                    yield return type;
                }
            }
        }
    }
}