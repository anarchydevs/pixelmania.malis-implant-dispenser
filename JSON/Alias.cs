using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MalisImpDispenser
{
    internal class Alias<TKey> : JsonFile<Dictionary<TKey, List<string>>>
    {
        internal Alias(string path) : base(path) { }

        internal bool TryGet(string tag, out TKey equipSlot)
        {
            equipSlot = default;
            bool foundMatch = false;

            foreach (var keyValue in Entries)
            {
                if (keyValue.Value.Contains(tag))
                {
                    equipSlot = keyValue.Key;   
                    foundMatch = true;
                    break;
                }
            }

            return foundMatch;
        }
    }
}