using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using AOSharp.Clientless;
using AOSharp.Clientless.Logging;

namespace MalisImpDispenser
{
    public static class BagOwners
    {
        private static Dictionary<int, Identity> _owners = new Dictionary<int, Identity>();

        internal static List<Identity> UsedBagIdentities() => _owners.Values.ToList();

        private static List<Identity> _availableBagIdentities => Inventory.Items
                    .Where(x => x.Slot.Type == IdentityType.Inventory && x.UniqueIdentity.Type == IdentityType.Container)
                    .Select(x => x.UniqueIdentity)
                    .Except(_owners.Values).ToList();

        internal static bool HasAvailableBags => _availableBagIdentities.Count() > 0;

        internal static void TryRemoveOwner(int requesterId, out Identity bagIdentity)
        {
            if (_owners.TryGetValue(requesterId, out bagIdentity))
                _owners.Remove(requesterId);
        }

        internal static Identity AddOrGetOwner(int requesterId)
        {
            if (_owners.ContainsKey(requesterId))
            {
                return _owners[requesterId];
            }
            else
            {
                if (_availableBagIdentities.Count() == 0)
                    return Identity.None;

                var bag = _availableBagIdentities.FirstOrDefault();

                _owners.Add(requesterId, bag);
                Logger.Information($"Assigning bag with Identity: {bag} to CharId: {requesterId}");
                return bag;
            }
        }
    }
}