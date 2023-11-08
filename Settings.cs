using AOSharp.Clientless.Logging;
using AOSharp.Common.GameData;
using AOSharp.Common.Unmanaged.Imports;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MalisItemFinder
{
    public class Settings
    {
        public int NanoProgrammingSkill;
        public int QuantumFieldSkill; 
        public int PsychologySkill; 
        public int WeaponSmithingSkill; 
        public int PharmaTechnologySkill;
        public int ComputerLiteracySkill;
        public int LoadPeriodInSeconds;
        public int TradeExpireTimeInSeconds;
        public int OrderExpireTimeInSeconds;

        public Dictionary<int, int> Blacklist;

        [JsonIgnore]
        private string _path;

        public Settings(string rootFolder)
        {
            _path = $"{rootFolder}\\Settings.json";
        }

        internal void Load()
        {
            try
            {
                if (File.Exists(_path))
                {
                    Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(_path));
                    PopulateSettings(settings.NanoProgrammingSkill,
                        settings.QuantumFieldSkill,
                        settings.PsychologySkill,
                        settings.WeaponSmithingSkill,
                        settings.PharmaTechnologySkill,
                        settings.ComputerLiteracySkill,
                        settings.LoadPeriodInSeconds,
                        settings.TradeExpireTimeInSeconds,
                        settings.OrderExpireTimeInSeconds,
                        settings.Blacklist);
                    Logger.Information("Settings loaded.");

                    return;
                }

                Logger.Information("Settings file not found, creating defaults.");

                string directoryPath = Path.GetDirectoryName(_path);

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                PopulateSettings(0, 0, 0, 0, 0, 0, 10, 60, 60 * 30, new Dictionary<int, int>());
                Save();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        private void PopulateSettings(int nanoProgrammingSkill, int quantumFieldSkill, int psychologySkill, int weaponSmithingSkill, int pharmaTechnologySkill, int computerLiteracySkill, int loadPeriodInSeconds, int tradeExpireTimeInSeconds, int orderExpireTimeInSeconds, Dictionary<int, int> blacklist)
        {
            NanoProgrammingSkill = nanoProgrammingSkill;
            QuantumFieldSkill = quantumFieldSkill;
            PsychologySkill = psychologySkill;
            WeaponSmithingSkill = weaponSmithingSkill;
            PharmaTechnologySkill = pharmaTechnologySkill;
            ComputerLiteracySkill = computerLiteracySkill;
            LoadPeriodInSeconds = loadPeriodInSeconds;
            TradeExpireTimeInSeconds = tradeExpireTimeInSeconds;
            OrderExpireTimeInSeconds = orderExpireTimeInSeconds;
            Blacklist = blacklist;
        }

        internal void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(_path, json);
        }
    }
}