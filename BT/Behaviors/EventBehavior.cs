using BehaviourTree.FluentBuilder;
using BehaviourTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOSharp.Common.GameData;
using AOSharp.Common.Helpers;
using AOSharp.Clientless.Logging;

namespace MalisImpDispenser
{
    internal static class EventTrigger
    {
        internal static BehaviourStatus Status(string eventName) => DynamicEvent.IsTriggered(eventName) ? BehaviourStatus.Succeeded : BehaviourStatus.Running;
    }
}