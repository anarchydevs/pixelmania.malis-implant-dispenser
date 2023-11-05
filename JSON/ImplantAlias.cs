using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MalisImpDispenser
{
    internal class ImplantAlias : Alias<ImplantSlot>
    {
        internal ImplantAlias(string path) : base(path) { }
    }
}