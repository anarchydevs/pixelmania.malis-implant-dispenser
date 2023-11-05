using System;
using System.Linq;
using AOSharp.Common.GameData;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using AOSharp.Clientless.Logging;

namespace MalisImpDispenser
{
    internal class JsonFile<T>
    {
        internal T Entries;

        internal JsonFile(string path)
        {
            try
            {
                Entries = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.Message);
            }
        }
    }
}