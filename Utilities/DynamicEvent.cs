using AOSharp.Clientless;
using AOSharp.Clientless.Logging;
using AOSharp.Common.GameData;
using Newtonsoft.Json;
using SmokeLounge.AOtomation.Messaging.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MalisImpDispenser
{
    internal static class DynamicEvent
    {
        public static Dictionary<string, EventInfo> _dynamicEvents = new Dictionary<string, EventInfo>();

        internal static void Trigger(string eventName, int tick = 1, int delayTriggerMs = 0)
        {
            Task.Delay(delayTriggerMs);
            AddOrGetEvent(eventName, tick)?.Handler?.Invoke(null, EventArgs.Empty);
        }

        internal static void Reset()
        {
            _dynamicEvents = new Dictionary<string, EventInfo>();
        }

        internal static EventInfo AddOrGetEvent(string eventName, int tick)
        {
            if (_dynamicEvents.TryGetValue(eventName, out EventInfo eventInfo))
                return eventInfo;

            eventInfo = new EventInfo
            {
                IsTriggered = false,
                Handler = (sender, e) =>
                {
                    _dynamicEvents[eventName].IsTriggered = true;
                    Logger.Information($"Dynamic event {eventName} was triggered!");
                }
            };

            _dynamicEvents.Add(eventName, eventInfo);

            return eventInfo;
        }

        internal static bool IsTriggered(string eventName)
        {
            if (_dynamicEvents.TryGetValue(eventName, out var eventInfo))
            {
                bool triggered = eventInfo.IsTriggered;
                eventInfo.IsTriggered = false;
                return triggered;
            }

            return false;
        }

        internal class EventInfo
        {
            public EventHandler<EventArgs> Handler { get; set; }
            public bool IsTriggered { get; set; }
        }
    }
}