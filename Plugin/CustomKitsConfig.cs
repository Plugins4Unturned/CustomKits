using System.Collections.Generic;
using System.Xml.Serialization;
using Rocket.API;

namespace Teyhota.CustomKits.Plugin
{
    public class CustomKitsConfig : IRocketPluginConfiguration
    {
        public static CustomKitsConfig Instance;
        
        public string DisableAutoUpdate;
        public string DefaultKitName;
        public bool KeepKitsOnRestart;
        public bool KeepKitsOnDeath;
        public bool KeepKitsOnDisconnect;
        public bool IncludeClothing;
        public bool DisableItemDrops;

        [XmlArray("Presets"), XmlArrayItem(ElementName = "Preset")]
        public List<Preset> Presets;
        
        public class Preset
        {
            public Preset() { }

            internal Preset(string name, int maxKits, int itemLimit, string blackList)
            {
                Name = name;
                SlotCount = maxKits;
                ItemLimit = itemLimit;
                Blacklist = blackList;
            }

            [XmlAttribute]
            public string Name;
            [XmlAttribute]
            public int SlotCount;
            [XmlAttribute]
            public int ItemLimit;
            [XmlAttribute]
            public string Blacklist;
        }
        
        public void LoadDefaults()
        {
            Instance = this;
            
            DefaultKitName = "Default"; // or "preset_name"
            KeepKitsOnRestart = true;
            KeepKitsOnDeath = true;
            KeepKitsOnDisconnect = true;
            IncludeClothing = true;
            DisableItemDrops = false;

            Presets = new List<Preset>()
            {
                new Preset("Default", 1, 15, "261,262,263,264,265,266,267,268"),
                new Preset("Member", 2, 30, "1244,1372,1373"),
                new Preset("VIP", 3, 45, "1441"),
                new Preset("*", 0, 60, "")
            };
        }
    }
}