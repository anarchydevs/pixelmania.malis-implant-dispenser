using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MalisImpDispenser
{
    internal class ItemRange<TKey> : JsonFile<TKey>
    {
        internal ItemRange(string path) : base(path) { }

        protected int Interpolate(int ql, int lowMod, int highMod) => (int)Math.Round(lowMod + ((float)ql - 1) * (highMod - lowMod) / (200 - 1));
    }
}