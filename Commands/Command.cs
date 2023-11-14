using AOSharp.Clientless;
using AOSharp.Clientless.Chat;
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
    public static class Command
    {
        private static readonly Dictionary<CommandType, Action<CommandBase>> _commands = new Dictionary<CommandType, Action<CommandBase>>()
        {
            { CommandType.Create,  cmd => CreateCommand.Process(cmd)},
            { CommandType.Aosetups,  cmd => AOSetupsCommand.Process((AoSetupsCmd)cmd)},
            { CommandType.Status,  cmd => StatusCommand.Process((StatusCmd)cmd)},
            { CommandType.Ladder,  cmd => LadderCommand.Process((LadderCmd)cmd)},
            { CommandType.Designer,  cmd => DesignerCommand.Process((DesignerCmd)cmd)},
            { CommandType.Help,  cmd => HelpCommand.Process(cmd)},
            { CommandType.Alias, cmd => AliasCommand.Process(cmd)   },
        };

        public static void Invoke(CommandType type, CommandBase cmdBase) => _commands[type].Invoke(cmdBase);
    }

    public class CommandBase
    {
        public int RequesterId;

        public CommandBase(int requesterId)
        {
            RequesterId = requesterId;
        }
    }

    public class LadderCmd : CommandBase
    {
        public string Skill1;
        public int Amount1;
        public string Skill2;
        public int Amount2;
        public string Skill3;
        public int Amount3;

        public LadderCmd(int requesterId) : base(requesterId) { }
    }

    public class TradeSkillCmd : CommandBase
    {
        public int Ql;
        public string Implant;
        public List<string> Clusters;

        public TradeSkillCmd(int requesterId) : base(requesterId) { }
    }

    public class AoSetupsCmd : CommandBase
    {
        public string Url;

        public AoSetupsCmd(int requesterId) : base(requesterId) { }
    }

    public class StatusCmd : CommandBase
    {
        public StatusCmd(int requesterId) : base(requesterId) { }
    }

    public class DesignerCreateCmd : DesignerCmd
    {
        public string Slot;

        public DesignerCreateCmd(string slot, int requesterId, DesignAct action) : base(0, action, requesterId)
        {
            Slot = slot;
        }
    }

    public class DesignerAOSetupsCmd : DesignerCmd
    {
        public string Url;

        public DesignerAOSetupsCmd(int requesterId, DesignAct action, string url) : base(0, action, requesterId)
        {
            Url = Utils.ExtractAosetupsUrlId(url);
        }
    }

    public class DesignerOrderCmd : DesignerCmd
    {
        public DesignerOrderCmd(int requesterId, DesignAct action) : base(0, action, requesterId) { }
    }

    public class DesignerShowCmd : DesignerCmd
    {
        public DesignerShowCmd(int requesterId, DesignAct action) : base(0, action, requesterId) { }
    }

    public class DesignerRemoveCmd : DesignerCmd
    {
        public DesignerRemoveCmd(int index, int requesterId, DesignAct action) : base(index, action, requesterId) { }
    }

    public class DesignerModifyClusterCmd : DesignerCmd
    {
        public string ClusterName;

        public DesignerModifyClusterCmd(int index, string clusterName, int requesterId, DesignAct action) : base(index, action, requesterId)
        {
            ClusterName = clusterName;
            Index = index;
        }
    }

    public class DesignerModifyQlCmd : DesignerCmd
    {
        public int Ql;

        public DesignerModifyQlCmd(int index, int ql, int requesterId, DesignAct action) : base(index, action, requesterId)
        {
            Ql = ql;
            Index = index;
        }
    }

    public class DesignerCmd : CommandBase
    {
        public DesignAct Action;
        public int Index;

        public DesignerCmd(int index, DesignAct action, int requesterId) : base(requesterId)
        {
            Action = action;
            Index = index;
        }
    }

    public class TradeskillBestCmd : CommandBase
    {
        public int Ability;
        public int Treatment;
        public string Implant;
        public List<string> Clusters;
        public int? StarIndex;

        public TradeskillBestCmd(int requesterId) : base(requesterId) { }
    }

    public enum CommandType
    {
        Create,
        Ladder,
        Aosetups,
        Status,
        Designer,
        Help,
        Test,
        Alias
    }

    public enum DesignAct
    {
        None,
        Create,
        Remove,
        Modify,
        Show,
        Order,
        Aosetups
    }
}