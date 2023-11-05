using AOSharp.Clientless;
using AOSharp.Clientless.Chat;
using AOSharp.Clientless.Logging;
using AOSharp.Common.GameData;
using MalisItemFinder;
using SmokeLounge.AOtomation.Messaging.GameData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MalisImpDispenser
{
    public class Main : ClientlessPluginEntry
    {
        public static string PluginDir;
        internal static BotBehavior BotBehavior;
        internal static Settings Settings;

        private bool _loaded = false;
        AutoResetInterval _loadPeriod;

        public override void Init(string pluginDir)
        {
            Logger.Information("- Mali's Imp Dispenser -");

            PluginDir = pluginDir;
            ImplantDesigner.Load($"{pluginDir}\\JSON");

            Settings = new Settings($"{pluginDir}\\JSON");
            Settings.Load();
            BotBehavior = new BotBehavior(100);

            EventHandler.Load();

            Client.Chat.PrivateMessageReceived += (e, msg) => PrivateMessageAction(msg);
            Client.OnUpdate += BotInit;
        }

        private void BotInit(object sender, double delta)
        {
            if (!Client.InPlay || DynelManager.VendingMachines.Count() != 15)
                return;

            Client.OnUpdate -= BotInit;
            Logger.Information("Bot Init");
            Utils.OpenBags();

            var bagCheck = Inventory.Items.Where(x => x.UniqueIdentity.Type == IdentityType.Container && x.Slot.Type == IdentityType.Inventory).ToList();

            bool errorsFounds = false;

            if (!Inventory.Items.Any(x => x.Name == "Implant Disassembly Clinic" && x.Slot.Instance == 0x40))
            {
                Logger.Error("Disassembly clinic not found / or is in wrong slot. To fix the slot, empty your inventory then put the clinic in your inventory.");
                errorsFounds = true;
            }

            if (Inventory.NumFreeSlots < 12)
            {
                Logger.Error("Need at least 12 free inventory slots.");
                errorsFounds = true;
            }

            if (bagCheck.Count() == 0 || bagCheck.Any(x => Inventory.GetNextAvailableSlot() < x.Slot.Instance))
            {
                Logger.Error("Bags not found / are in wrong slots. Put all bags inside your bank, while leaving the clinic in your inventory, then withdraw them.");
                errorsFounds = true;
            }

            var shopToTeleport = DynelManager.VendingMachines.FirstOrDefault(x => x.Name == "Advanced ICC Shiny Clusters");

            if (Vector3.Distance(shopToTeleport.Transform.Position, DynelManager.LocalPlayer.Transform.Position) > 4f)
            {
                Logger.Error("Please move your bot closer to 'Advanced ICC Shiny Clusters' shop.");
                errorsFounds = true;
            }

            if (errorsFounds)
            {
                Logger.Error("Bot failed to load. Please resolve all problems above");
                return;
            }

            foreach (var Item in Inventory.Items.Where(x => x.Slot.Type == IdentityType.Inventory))
            {
                if (Item.Name == "Implant Disassembly Clinic")
                    continue;

                if (Item.UniqueIdentity.Type == IdentityType.Container)
                    continue;

                Item.Delete();
            }

            DynelManager.LocalPlayer.MoveTo(shopToTeleport.Transform.Heading * new Vector3(0, 0f, 2f) + shopToTeleport.Transform.Position, Quaternion.AngleAxis(90, new Vector3(0.15, 1, 0)));

            ShopCache.Load($"{PluginDir}\\JSON");
            ShopCache.SetStats(Settings.ComputerLiteracySkill);

            Tradeskill.SetStats(Settings.NanoProgrammingSkill, 
                Settings.QuantumFieldSkill, 
                Settings.PsychologySkill, 
                Settings.WeaponSmithingSkill, 
                Settings.PharmaTechnologySkill, 
                Settings.ComputerLiteracySkill);            
            
            _loadPeriod = new AutoResetInterval(Settings.LoadPeriodInSeconds * 1000);
            _loadPeriod.Reset();

            Client.OnUpdate += OrderProcessor.UpdateLoop;
            Client.OnUpdate += BotFinishInit;
        }

        private void BotFinishInit(object sender, double e)
        {
            if (!_loadPeriod.Elapsed)
                return;

            Logger.Information("Bot Loaded");

            foreach (var bag in Inventory.Containers.Where(x=>x.IsOpen))
            {
                foreach (var item in bag.Items)
                    item.Delete();
            }

            _loaded = true;

            Client.OnUpdate -= BotFinishInit;
        }

        private void PrivateMessageAction(PrivateMessage privMsgArgs)
        {
            Logger.Information(privMsgArgs.Message);    

            if (!CommandProcessor.TryProcess(privMsgArgs, out CommandType command, out CommandBase cmdBase))
                return;

            if (!_loaded)
            {
                Client.SendPrivateMessage(cmdBase.RequesterId, ScriptTemplate.RespondMsg(Color.Red, "I am still loading, request rejected"));
                return;
            }

            Command.Invoke(command, cmdBase);
        }
    }
}