using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using AOSharp.Clientless;
using AOSharp.Clientless.Logging;
using System.Globalization;

namespace MalisImpDispenser
{
    public class UniqueContainerIds
    {
        public const int RobustBackpack = 158790;
        public const int ERobustBackpack = 275381;
        public const int PioneerBackpack = 152039;
        public const int MPioneerBackpack = 156831;

        public static bool Contains(int id) => 
            id == RobustBackpack || 
            id == ERobustBackpack || 
            id == PioneerBackpack || 
            id == MPioneerBackpack;
    }

    public class Utils
    {
        internal static void OpenBags()
        {
            Logger.Information($"Peeking all inventory bags.");

            foreach (Item item in Inventory.Items.Where(x=>x.UniqueIdentity.Type == IdentityType.Container && x.Slot.Type == IdentityType.Inventory))
            {
                item.Use();
                item.Use();
            }
        }

        public static string ToTitleCase(string text) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());

        public static Item LastInventoryItem()
        {
            var items = Inventory.Items.Where(x => x.Slot.Type == IdentityType.Inventory);

            if (items.Count() == 1)
                return items.FirstOrDefault();

           return Inventory.Items.OrderByDescending(x => x.Slot.Instance).FirstOrDefault();
        }

        public static Item SecondLastInventoryItem()
        {
            var items = Inventory.Items.Where(x => x.Slot.Type == IdentityType.Inventory);

            if (items.Count() == 1)
                return items.FirstOrDefault();

            return Inventory.Items.OrderByDescending(x => x.Slot.Instance).Skip(1).FirstOrDefault();
        }

        public static string ConvertToPercentage(double value) => (value * 100).ToString("0.00");

        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;
            else if (value.CompareTo(max) > 0)
                return max;
            else
                return value;
        }
    }
}