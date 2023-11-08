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

namespace MalisImpDispenser
{
    public static class EventHandler
    {
        internal static void Load()
        {
            Client.MessageReceived += N3MessageReceived;
        }

        private static void N3MessageReceived(object sender, Message msg)
        {
            if (msg.Header.PacketType != PacketType.N3Message)
                return;

            var msgBody = msg.Body;

            if (msgBody is TradeMessage tradeMsg)
            {
                TradeActionEvent(sender, new TradeActionArgs(tradeMsg.Identity, new Identity((IdentityType)tradeMsg.Param1, tradeMsg.Param2), tradeMsg.Action));
            }
            else if (msgBody is CharacterActionMessage actionMsg)
            {
                switch (actionMsg.Action)
                {
                    case CharacterActionType.DeleteItem:
                        DeleteItemActionEvent(sender, new CharacterActionArgs());
                        break;
                    case CharacterActionType.TradeskillSource:
                        Logger.Information($"[REC] Tradeskill Source");
                        break;
                    case CharacterActionType.TradeskillTarget:
                        Logger.Information($"[REC] Tradeskill Target");
                        break;
                    case CharacterActionType.TradeskillNotValid:
                        Logger.Information($"[REC] Tradeskill Not Valid");
                        break;
                    case CharacterActionType.TradeskillResult:
                        Logger.Information($"[REC] Tradeskill Result, result id: {actionMsg.Parameter2}");
                        break;
                    default:
                        break;
                }
            }
            else if (msgBody is ContainerAddItem contAddItem && contAddItem.Target.Type == IdentityType.Container)
            {
                ContainerAddItemActionEvent(sender,new ContainerAddItemActionArgs());
            }
        }

        private static void TradeActionEvent(object sender, TradeActionArgs tradeArgs)
        {
            if (tradeArgs.Identity != DynelManager.LocalPlayer.Identity)
                return;

            if (tradeArgs.Identity1.Type != IdentityType.VendingMachine &&
                tradeArgs.Identity1 != DynelManager.LocalPlayer.Identity &&
                tradeArgs.Identity1 != BotBehavior.Context.TradeOrderTarget && 
                tradeArgs.Identity1 != BotBehavior.Context.TradeDebtTarget)
                return;

            switch (tradeArgs.Action)
            {
                case TradeAction.Open:
                    DynamicEvent.Trigger("TradeOpened");
                    break;
                case TradeAction.Complete:
                    DynamicEvent.Trigger("TradeCompleted");
                    break;
                case TradeAction.AddItem:
                    DynamicEvent.Trigger("TradeAddItem");
                    break;
                case TradeAction.Confirm:
                    DynamicEvent.Trigger("TradeConfirm");
                    break;
                case TradeAction.Accept:
                    DynamicEvent.Trigger("TradeAccept");
                    break;
                case TradeAction.UpdateCredits:
                    DynamicEvent.Trigger("TradeUpdateCredits");
                    break;
                case TradeAction.OtherPlayerAddItem:
                    DynamicEvent.Trigger("TradeOtherPlayerAddItem");
                    break;
            }
        }

        private static void DeleteItemActionEvent(object sender, CharacterActionArgs contArgs)
        {
            DynamicEvent.Trigger("ItemDeleted");
        }

        private static void ContainerAddItemActionEvent(object sender, ContainerAddItemActionArgs e)
        {
            DynamicEvent.Trigger("ContainerAddItem");
        }
    }

    public class TradeActionArgs : EventArgs
    {
        public Identity Identity { get; }
        public Identity Identity1 { get; }
        public TradeAction Action { get; }

        public TradeActionArgs(Identity identity, Identity identity1, TradeAction action)
        {
            Identity = identity;
            Identity1 = identity1;
            Action = action;
        }
    }

    public class CharacterActionArgs : EventArgs
    {
        public CharacterActionArgs()
        {
        }
    }
    public class ContainerAddItemActionArgs : EventArgs
    {
        public ContainerAddItemActionArgs()
        {
        }
    }


    public class PrivateMessageActionArgs : EventArgs
    {
        public uint Sender { get; }
        public string Message{ get; }

        public PrivateMessageActionArgs(uint sender, string message)
        {
            Sender = sender;
            Message = message;
        }
    }
}