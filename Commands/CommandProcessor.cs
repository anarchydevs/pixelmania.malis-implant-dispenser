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
    public class CommandProcessor
    {
        private static readonly Dictionary<CommandType, Func<string[], int, CommandBase>> _commands = new Dictionary<CommandType, Func<string[], int, CommandBase>>()
        {
            { CommandType.Create, (msg,id) => TradeskillActionBase(msg,id) },
            { CommandType.Aosetups, (msg,id) => AOsetupsPresetAction(msg,id) },
            { CommandType.Status, (msg,id) => StatusAction(msg,id) },
            { CommandType.Designer, (msg,id) => DesignAction(msg,id) },
            { CommandType.Ladder, (msg,id) => LadderAction(msg,id) },
            { CommandType.Help, (msg,id) => CommandBaseAction(msg,id)   },
            { CommandType.Alias, (msg,id) => CommandBaseAction(msg,id)   },
            { CommandType.Debug, (msg,id) => CommandBaseAction(msg,id)   },
        };

        public static bool TryProcess(PrivateMessage privMsg, out CommandType command, out CommandBase commandBase)
        {
            commandBase = null;
            string[] commandParts = privMsg.Message.ToLower().Split(' ');

            if (!Enum.TryParse(Utils.ToTitleCase(commandParts[0]), out command))
            {
                //Client.SendPrivateMessage(privMsg.SenderId, ScriptTemplate.RespondMsg(Color.Red, $"Invalid command '{commandParts[0]}'"));
                return false;
            }

            commandParts = commandParts.Length > 1 ? commandParts.Skip(1).ToArray() : null;

            if (!_commands.TryGetValue(command, out var action))
            {
                Client.SendPrivateMessage(privMsg.SenderId, ScriptTemplate.RespondMsg(Color.Red, $"Command '{command}' has no actions"));
                return false;
            }

            return (commandBase = _commands[command].Invoke(commandParts, (int)privMsg.SenderId)) != null;
        }

        private static CommandBase CommandBaseAction(string[] msg, int requesterId)
        {
            CommandBase helpCmd = new CommandBase(requesterId);

            return helpCmd;
        }

        private static LadderCmd LadderAction(string[] cmdParts, int requesterId)
        {
            if (cmdParts.Length != 6 || !int.TryParse(cmdParts[1], out int skillAmount1) || !int.TryParse(cmdParts[3], out int skillAmount2) || !int.TryParse(cmdParts[5], out int skillAmount3))
            {
                Client.SendPrivateMessage(requesterId, ScriptTemplate.RespondMsg(Color.Red, $"Error processing. Command example: ladder treatment 500"));
                return null;
            }

            var cmd = new LadderCmd(requesterId);
           
            cmd.Skill1 = cmdParts[0];
            cmd.Amount1 = skillAmount1;
            cmd.Skill2 = cmdParts[2];
            cmd.Amount2 = skillAmount2;
            cmd.Skill3 = cmdParts[4];
            cmd.Amount3 = skillAmount3;

            return cmd;
        }

        private static AoSetupsCmd AOsetupsPresetAction(string[] cmdParts, int requesterId)
        {
            AoSetupsCmd aoSetupsCmd = new AoSetupsCmd(requesterId);
         
            if (cmdParts.Length != 1)
            {
                Client.SendPrivateMessage(requesterId, ScriptTemplate.RespondMsg(Color.Red, $"Error processing. Invalid aosetups link"));
                return null;
            }

            aoSetupsCmd.Url = Utils.ExtractAosetupsUrlId(cmdParts[0]);

            return aoSetupsCmd;
        }

        private static CommandBase TradeskillActionBase(string[] cmdParts, int requesterId)
        {
            if (cmdParts.Length > 1 && int.TryParse(cmdParts[1], out _))
                return TradeskillBestAction(cmdParts, requesterId);

            return TradeskillAction(cmdParts, requesterId);
        }

        private static TradeSkillCmd TradeskillAction(string[] cmdParts, int requesterId)
        {
            TradeSkillCmd tradeSkillCmd = new TradeSkillCmd(requesterId);

            if (cmdParts.Length == 1 || cmdParts.Length == 2 || cmdParts.Length > 5)
            {
                Client.SendPrivateMessage(requesterId, ScriptTemplate.RespondMsg(Color.Red, "Error processing. Too few / many arguments, Example: create 169 head complit sense nanopool"));
                return null;
            }

            if (!int.TryParse(cmdParts[0], out int ql))
            {
                Client.SendPrivateMessage(requesterId, ScriptTemplate.RespondMsg(Color.Red, $"Error processing. Invalid ql number, Command example: create 169 head complit sense nanopool"));
                return null;
            }

            tradeSkillCmd.Ql = ql;
            tradeSkillCmd.Implant = cmdParts[1];
            tradeSkillCmd.Clusters = cmdParts.Skip(2).ToList();

            return tradeSkillCmd;
        }


        private static TradeskillBestCmd TradeskillBestAction(string[] cmdParts, int requesterId)
        {
            TradeskillBestCmd tsBestCmd = new TradeskillBestCmd(requesterId);

            if (cmdParts.Length < 4 || cmdParts.Length > 6)
            {
                Client.SendPrivateMessage(requesterId, ScriptTemplate.RespondMsg(Color.Red, "Error processing. Too few / many arguments, Command example: create 100 400 leg stamina agility*"));
                return null;
            }

            if (!int.TryParse(cmdParts[0], out int ability))
            {
                Client.SendPrivateMessage(requesterId, ScriptTemplate.RespondMsg(Color.Red, $"Error processing. Invalid treatment number,  Command example: create 100 400 leg stamina agility*"));
                return null;
            }

            if (!int.TryParse(cmdParts[1], out int treatment))
            {
                Client.SendPrivateMessage(requesterId, ScriptTemplate.RespondMsg(Color.Red, $"Error processing. Invalid ability number, Command example: create 100 400 leg stamina agility*"));
                return null;
            }

            var clusters = cmdParts.Skip(3).ToList();
            var starredClusters = clusters.Where(word => word.EndsWith("*")).ToList();
            int? starIndex = null;

            if (starredClusters.Count > 1)
            {
                Client.SendPrivateMessage(requesterId, ScriptTemplate.RespondMsg(Color.Red, $"Error processing: More than one cluster ends with '*'"));
                return null;
            }

            if (starredClusters.Count == 1)
            {
                starIndex = clusters.IndexOf(starredClusters[0]);
                clusters[starIndex.Value] = clusters[starIndex.Value].TrimEnd('*');
            }

            tsBestCmd.Ability = ability;
            tsBestCmd.Treatment = treatment;
            tsBestCmd.Implant = cmdParts[2];
            tsBestCmd.StarIndex = starIndex;
            tsBestCmd.Clusters = clusters;

            return tsBestCmd;
        }

        private static CommandBase DesignAction(string[] cmdParts, int requesterId)
        {
            if (cmdParts == null || !Enum.TryParse(Utils.ToTitleCase(cmdParts[0]), out DesignAct action))
            {
                Client.SendPrivateMessage(requesterId, ScriptTemplate.RespondMsg(Color.Red, $"Invalid designer command: Try designer show"));
                return null;
            }

            switch (action)
            {
                case DesignAct.Order:
                    return new DesignerOrderCmd(requesterId, action);
                case DesignAct.Show:
                    return new DesignerShowCmd(requesterId, action);
                case DesignAct.Create:
                    return new DesignerCreateCmd(cmdParts.Length == 2? cmdParts[1] :null, requesterId, action);
                case DesignAct.Modify:
                    if (!int.TryParse(cmdParts[1], out int index))
                        return null;
                    if (cmdParts.Length == 2)
                        return new DesignerCmd(index, action, requesterId);
                    if (cmdParts.Length == 3)
                    {
                        if (int.TryParse(cmdParts[2], out int ql))
                            return new DesignerModifyQlCmd(index, ql, requesterId, action);
                        else
                            return new DesignerModifyClusterCmd(index, cmdParts[2], requesterId, action);
                    }
                    break;
                case DesignAct.Remove:
                    if (!int.TryParse(cmdParts[1], out index))
                        return null;
                    return new DesignerRemoveCmd(index, requesterId, action);
                case DesignAct.Aosetups:
                    return new DesignerAOSetupsCmd(requesterId, action, cmdParts[1]);
            }
            return null;
        }


        private static StatusCmd StatusAction(string[] cmdParts, int requesterId)
        {
            return new StatusCmd(requesterId);
        }
    }
}