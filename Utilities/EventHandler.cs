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
                        TradeskillSourceEvent(sender, new CharacterActionArgs());
                        break;
                    case CharacterActionType.TradeskillTarget:
                        TradeskillTargetEvent(sender, new CharacterActionArgs());
                        break;
                    case CharacterActionType.TradeskillNotValid:
                        TradeskillNotValidEvent(sender, new CharacterActionArgs());
                        break;
                    case CharacterActionType.TradeskillResult:
                        TradeskillResultEvent(sender, new CharacterActionArgs());
                        break;
                    default:
                        break;
                }
            }
            else if (msgBody is ContainerAddItem contAddItem && contAddItem.Target.Type == IdentityType.Container)
            {
                ContainerAddItemActionEvent(sender,new ContainerAddItemActionArgs());
            }
            else if (msgBody is FeedbackMessage feedBackMsg)
            {
                switch (feedBackMsg.MessageId)
                {
                    case 173819685:
                        AlreadyInTradeEvent(sender, new FeedbackMessageArgs());
                        break;
                    case 220669556:
                        TradeskillFailedEvent(sender, new FeedbackMessageArgs());
                        break;
                    default:
                        break;
                }
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
                case TradeAction.Decline:
                    DynamicEvent.Trigger("TradeDecline");
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

        private static void TradeskillSourceEvent(object sender, CharacterActionArgs e)
        {
            DynamicEvent.Trigger("TradeskillSource");
        }

        private static void TradeskillTargetEvent(object sender, CharacterActionArgs e)
        {
            DynamicEvent.Trigger("TradeskillTarget");
        }

        private static void TradeskillResultEvent(object sender, CharacterActionArgs e)
        {
            DynamicEvent.Trigger("TradeskillResult");
        }

        private static void TradeskillNotValidEvent(object sender, CharacterActionArgs e)
        {
            DynamicEvent.Trigger("TradeskillNotValid");
        }

        private static void AlreadyInTradeEvent(object sender, FeedbackMessageArgs e)
        {
            DynamicEvent.Trigger("AlreadyInTrade");
        }

        private static void TradeskillFailedEvent(object sender, FeedbackMessageArgs e)
        {
            DynamicEvent.Trigger("TradeskillFailed");
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

    public class FeedbackMessageArgs : EventArgs
    {
        public FeedbackMessageArgs()
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