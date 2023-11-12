using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using AOSharp.Clientless;
using SmokeLounge.AOtomation.Messaging.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using AOSharp.Common.GameData;
using System.Threading.Tasks;
using AOSharp.Clientless.Logging;

namespace MalisImpDispenser
{
    internal static class Action
    {
        internal static void UseItemOnItem(Identity source, Identity target)
        {
            Client.Send(new CharacterActionMessage
            {
                Action = CharacterActionType.UseItemOnItem,
                Target = target,
                Parameter1 = (int)source.Type,
                Parameter2 = source.Instance,
            });
        }

        internal static void Tradeskill(int finalQl)
        {
            Client.Send(new CharacterActionMessage
            {
                Action = CharacterActionType.TradeskillBuildPressed,
                Target = new Identity(IdentityType.None, finalQl),
            });
        }

        internal static void TradeSkillAdd(CharacterActionType type, Identity identity)
        {
            Client.Send(new CharacterActionMessage
            {
                Action = type,
                Target = Identity.None,
                Parameter1 = (int)identity.Type,
                Parameter2 = identity.Instance
            });
        }

        internal static void OpenShop(Identity identity)
        {
            Client.Send(new GenericCmdMessage
            {
                Count = 0xFF,
                Action = GenericCmdAction.Use,
                Temp4 = 1,
                User = DynelManager.LocalPlayer.Identity,
                Target = identity,
            });
        }
    }
}