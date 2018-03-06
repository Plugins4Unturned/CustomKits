using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Rocket.Unturned.Player;
using Rocket.Unturned.Items;
using Rocket.Unturned.Chat;
using Rocket.Unturned;
using Rocket.API;
using SDG.Unturned;
using UnityEngine;
using Steamworks;
using Newtonsoft.Json;
using Logger = Rocket.Core.Logging.Logger;


namespace Teyhota.CustomKits
{
    public class InventoryManager
    {
        public class Inventory
        {
            public List<Item> items;
            public Clothing clothes;

            public Inventory(List<Item> items, Clothing clothes)
            {
                this.items = items;
                this.clothes = clothes;
            }
        }

        public class Item
        {
            public ushort id;
            public byte[] meta;
            public byte page;
            public byte x;
            public byte y;
            public byte rot;

            public Item(ushort id, byte[] meta, byte page, byte x, byte y, byte rot)
            {
                this.id = id;
                this.meta = meta;
                this.page = page;
                this.x = x;
                this.y = y;
                this.rot = rot;
            }
        }

        public class ClothingData
        {
            public ushort id;
            public byte quality;
            public byte[] state;

            public ClothingData(ushort id, byte quality, byte[] state)
            {
                this.id = id;
                this.quality = quality;
                this.state = state;
            }
        }

        public class Clothing
        {
            public Hat hat;
            public Mask mask;
            public Shirt shirt;
            public Vest vest;
            public Backpack backpack;
            public Pants pants;

            public Clothing(Hat hat, Mask mask, Shirt shirt, Vest vest, Backpack backpack, Pants pants)
            {
                this.hat = hat;
                this.mask = mask;
                this.shirt = shirt;
                this.vest = vest;
                this.backpack = backpack;
                this.pants = pants;
            }
        }

        #region Clothing
        public class Backpack : ClothingData
        {
            public Backpack(ushort id, byte quality, byte[] state) : base(id, quality, state) { }
        }

        public class Hat : ClothingData
        {
            public Hat(ushort id, byte quality, byte[] state) : base(id, quality, state) { }
        }

        public class Mask : ClothingData
        {
            public Mask(ushort id, byte quality, byte[] state) : base(id, quality, state) { }
        }

        public class Pants : ClothingData
        {
            public Pants(ushort id, byte quality, byte[] state) : base(id, quality, state) { }
        }

        public class Shirt : ClothingData
        {
            public Shirt(ushort id, byte quality, byte[] state) : base(id, quality, state) { }
        }

        public class Vest : ClothingData
        {
            public Vest(ushort id, byte quality, byte[] state) : base(id, quality, state) { }
        }
        #endregion

        public static List<Item> ListItems(UnturnedPlayer player)
        {
            List<Item> itemList = new List<Item>();

            for (byte page = 0; page < PlayerInventory.PAGES - 1; page++)
            {
                for (byte index = 0; index < player.Inventory.getItemCount(page); index++)
                {
                    ItemJar iJar = player.Inventory.getItem(page, index);

                    itemList.Add(new Item(iJar.item.id, iJar.item.metadata, page, iJar.x, iJar.y, iJar.rot));
                }
            }

            return itemList;
        }

        public static void AddItem(UnturnedPlayer player, Item item)
        {
            SDG.Unturned.Item uItem = new SDG.Unturned.Item(item.id, true)
            {
                metadata = item.meta
            };

            if (item.page == 0 && item.x == 0 && item.y == 0 && item.rot == 0)
            {
                player.Inventory.tryAddItem(uItem, true, true);
            }
            else
            {
                player.Inventory.tryAddItem(uItem, item.x, item.y, item.page, item.rot);
            }
        }

        public static void AddClothing(UnturnedPlayer player, Backpack backpack, Clothing clothing = null)
        {
            if (clothing != null)
            {
                Hat hat = clothing.hat;
                Mask mask = clothing.mask;
                Shirt shirt = clothing.shirt;
                Vest vest = clothing.vest;
                Pants pants = clothing.pants;

                if (hat != null)
                {
                    player.Player.clothing.askWearHat(hat.id, hat.quality, hat.state, true);
                }
                if (mask != null)
                {
                    player.Player.clothing.askWearMask(mask.id, mask.quality, mask.state, true);
                }
                if (shirt != null)
                {
                    player.Player.clothing.askWearShirt(shirt.id, shirt.quality, shirt.state, true);
                }
                if (vest != null)
                {
                    player.Player.clothing.askWearVest(vest.id, vest.quality, vest.state, true);
                }
                if (pants != null)
                {
                    player.Player.clothing.askWearPants(pants.id, pants.quality, pants.state, true);
                }
            }

            if (backpack != null)
            {
                player.Player.clothing.askWearBackpack(backpack.id, backpack.quality, backpack.state, true);
            }
        }
        
        public static void Clear(UnturnedPlayer player, bool clothes)
        {
            // inventory...
            try
            {
                player.Player.equipment.dequip();
                for (byte p = 0; p < (PlayerInventory.PAGES - 1); p++)
                {
                    byte itemc = player.Player.inventory.getItemCount(p);
                    if (itemc > 0)
                    {
                        for (byte p1 = 0; p1 < itemc; p1++)
                        {
                            player.Player.inventory.removeItem(p, 0);
                        }
                    }
                }
                player.Player.channel.send("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                {
                    (byte)0,
                    (byte)0,
                    new byte[0]
                });
                player.Player.channel.send("tellSlot", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                {
                    (byte)1,
                    (byte)0,
                    new byte[0]
                });
            }
            catch (Exception e)
            {
                Logger.LogError("There was an error clearing " + player.CharacterName + "'s inventory.  Here is the error.");
                Logger.LogException(e);
            }

            // clothes...
            if (clothes)
            {
                try
                {
                    player.Player.clothing.askWearBackpack(0, 0, new byte[0], true);
                    for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                    {
                        player.Player.inventory.removeItem(2, 0);
                    }
                    player.Player.clothing.askWearGlasses(0, 0, new byte[0], true);
                    for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                    {
                        player.Player.inventory.removeItem(2, 0);
                    }
                    player.Player.clothing.askWearHat(0, 0, new byte[0], true);
                    for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                    {
                        player.Player.inventory.removeItem(2, 0);
                    }
                    player.Player.clothing.askWearMask(0, 0, new byte[0], true);
                    for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                    {
                        player.Player.inventory.removeItem(2, 0);
                    }
                    player.Player.clothing.askWearPants(0, 0, new byte[0], true);
                    for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                    {
                        player.Player.inventory.removeItem(2, 0);
                    }
                    player.Player.clothing.askWearShirt(0, 0, new byte[0], true);
                    for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                    {
                        player.Player.inventory.removeItem(2, 0);
                    }
                    player.Player.clothing.askWearVest(0, 0, new byte[0], true);
                    for (byte p2 = 0; p2 < player.Player.inventory.getItemCount(2); p2++)
                    {
                        player.Player.inventory.removeItem(2, 0);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError("There was an error clearing " + player.CharacterName + "'s inventory.  Here is the error.");
                    Logger.LogException(e);
                }
            }
            
            Events.InvokeClearInventory(player);
        }

        public static void Copy(UnturnedPlayer fromPlayer, UnturnedPlayer toPlayer, bool clothes)
        {
            PlayerClothing clothing = fromPlayer.Player.clothing;
            Hat hat = new Hat(clothing.hat, clothing.hatQuality, clothing.hatState);
            Mask mask = new Mask(clothing.mask, clothing.maskQuality, clothing.maskState);
            Shirt shirt = new Shirt(clothing.shirt, clothing.shirtQuality, clothing.shirtState);
            Vest vest = new Vest(clothing.vest, clothing.vestQuality, clothing.vestState);
            Backpack backpack = new Backpack(clothing.backpack, clothing.backpackQuality, clothing.backpackState);
            Pants pants = new Pants(clothing.pants, clothing.pantsQuality, clothing.pantsState);
            Clothing clothesList = new Clothing(hat, mask, shirt, vest, backpack, pants);

            List<Item> itemList = ListItems(fromPlayer);
            int inventoryCount = itemList.Count;

            Clear(toPlayer, clothes);

            if (clothes == true)
            {
                AddClothing(toPlayer, backpack, clothesList);
            }
            else
            {
                AddClothing(toPlayer, backpack);
            }

            for (int i = 0; i < itemList.Count; i++)
            {
                AddItem(toPlayer, itemList[i]);
            }
        }

        internal static void AutoCopy(UnturnedPlayer player)
        {
            if (Commands.Command_AutoCopy.Murdered.ContainsKey(player.CSteamID))
            {
                UnturnedPlayer murderer = UnturnedPlayer.FromCSteamID(Commands.Command_AutoCopy.Murdered[player.CSteamID]);
                
                if (murderer.HasPermission("ck.copyinventory.bypass"))
                {
                    UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("copy_bypass", murderer.CharacterName), Color.red);
                    return;
                }

                Copy(murderer, player, Plugin.CustomKitsPlugin.Instance.Configuration.Instance.IncludeClothingInKits);

                UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("copied", murderer.CharacterName), Color.green);
            }
        }
    }

    public class KitManager
    {
        public static Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> Kits;
        public static Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> AutoSaveKits;

        internal static void AutoSave(UnturnedPlayer player)
        {
            string kitName = Commands.Command_AutoSave.AutoSave[player.CSteamID];
            InventoryManager.Inventory inventory = AutoSaveKits[player.CSteamID.m_SteamID][kitName];
            int inventoryCount = inventory.items.Count;

            if (!player.IsAdmin)
            {
                string[] blackList = new string[] { };
                int itemLimit = int.MaxValue;

                foreach (Plugin.CustomKitsConfig.Preset Preset in Plugin.CustomKitsPlugin.Instance.Configuration.Instance.Presets)
                {
                    if (player.HasPermission(Plugin.CustomKitsPlugin.PERMISSION + Preset.Name))
                    {
                        if (Preset.Blacklist != "")
                        {
                            blackList = Preset.Blacklist.Split(',');
                            break;
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("set_permissions"), Color.red);
                        return;
                    }
                }

                if (KitCount(player, Kits) >= SlotManager.SlotCount(player))
                {
                    UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("no_kits_left"), Color.red);
                    return;
                }

                var v = KitCount(player, Kits);
                var slot = SlotManager.Slots[player.CSteamID.m_SteamID][v];

                itemLimit = slot.itemLimit;

                if (blackList.Length > 0)
                {
                    foreach (InventoryManager.Item item in inventory.items)
                    {
                        List<int> bList = new List<int>();
                        foreach (var itemID in blackList)
                        {
                            bList.Add(int.Parse(itemID));
                        }

                        if (bList.Contains(item.id))
                        {
                            UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("blacklisted", UnturnedItems.GetItemAssetById(item.id)), Color.red);
                        }
                    }
                }

                if (inventoryCount > itemLimit)
                {
                    UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("item_limit", itemLimit), Color.red);
                    return;
                }
            }

            if (inventoryCount < 1 || inventory.items == null)
            {
                UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("empty_inventory"), Color.red);
                return;
            }

            if (KitManager.HasKit(player, kitName, KitManager.Kits))
            {
                KitManager.DeleteKit(player, kitName, KitManager.Kits);
            }

            KitManager.SaveKit(player, player, kitName, KitManager.Kits);
            UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("kit_saved", kitName), Color.green);

            // Auto off
            Commands.Command_AutoSave.AutoSave.Remove(player.CSteamID);
            UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("autosave_off"), Color.green);
        }

        public static void SaveKit(UnturnedPlayer fromPlayer, UnturnedPlayer toPlayer, string kitName, Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> database)
        {
            PlayerClothing clothing = fromPlayer.Player.clothing;
            InventoryManager.Hat hat = new InventoryManager.Hat(clothing.hat, clothing.hatQuality, clothing.hatState);
            InventoryManager.Mask mask = new InventoryManager.Mask(clothing.mask, clothing.maskQuality, clothing.maskState);
            InventoryManager.Shirt shirt = new InventoryManager.Shirt(clothing.shirt, clothing.shirtQuality, clothing.shirtState);
            InventoryManager.Vest vest = new InventoryManager.Vest(clothing.vest, clothing.vestQuality, clothing.vestState);
            InventoryManager.Backpack backpack = new InventoryManager.Backpack(clothing.backpack, clothing.backpackQuality, clothing.backpackState);
            InventoryManager.Pants pants = new InventoryManager.Pants(clothing.pants, clothing.pantsQuality, clothing.pantsState);
            InventoryManager.Clothing clothesList = new InventoryManager.Clothing(hat, mask, shirt, vest, backpack, pants);

            List<InventoryManager.Item> itemList = new List<InventoryManager.Item>();
            int inventoryCount = 0;

            string[] blackList = new string[] { };
            foreach (Plugin.CustomKitsConfig.Preset Preset in Plugin.CustomKitsPlugin.Instance.Configuration.Instance.Presets)
            {
                if (toPlayer.HasPermission(Plugin.CustomKitsPlugin.PERMISSION + Preset.Name))
                {
                    if (Preset.Blacklist != "")
                    {
                        blackList = Preset.Blacklist.Split(',');
                        break;
                    }
                }
            }

            List<int> bList = new List<int>();
            if (blackList.Length > 0)
            {
                foreach (var itemID in blackList)
                {
                    bList.Add(int.Parse(itemID));
                }
            }

            for (byte page = 0; page < PlayerInventory.PAGES - 1; page++)
            {
                for (byte index = 0; index < fromPlayer.Inventory.getItemCount(page); index++)
                {
                    ItemJar iJar = fromPlayer.Inventory.getItem(page, index);

                    if (!toPlayer.IsAdmin)
                    {
                        if (bList.Contains(iJar.item.id))
                        {
                            continue;
                        }
                    }

                    itemList.Add(new InventoryManager.Item(iJar.item.id, iJar.item.metadata, page, iJar.x, iJar.y, iJar.rot));

                    inventoryCount = itemList.Count;
                }
            }

            if (database.ContainsKey(toPlayer.CSteamID.m_SteamID))
            {
                if (database[toPlayer.CSteamID.m_SteamID].ContainsKey(kitName))
                {
                    database[toPlayer.CSteamID.m_SteamID][kitName] = new InventoryManager.Inventory(itemList, clothesList);
                }
                else
                {
                    database[toPlayer.CSteamID.m_SteamID].Add(kitName, new InventoryManager.Inventory(itemList, clothesList));
                }
            }
            else
            {
                Dictionary<string, InventoryManager.Inventory> kit = new Dictionary<string, InventoryManager.Inventory>
                {
                    { kitName, new InventoryManager.Inventory(itemList, clothesList) }
                };

                database.Add(toPlayer.CSteamID.m_SteamID, kit);
            }
            
            Events.InvokeSaveKit(fromPlayer, toPlayer, kitName);
        }

        public static void LoadKit(UnturnedPlayer fromPlayer, UnturnedPlayer toPlayer, string kitName, bool clothes, Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> database)
        {
            if (clothes == true)
            {
                InventoryManager.AddClothing(toPlayer, database[fromPlayer.CSteamID.m_SteamID][kitName].clothes.backpack, database[fromPlayer.CSteamID.m_SteamID][kitName].clothes);
            }
            else
            {
                InventoryManager.AddClothing(toPlayer, database[fromPlayer.CSteamID.m_SteamID][kitName].clothes.backpack);
            }

            for (int i = 0; i < database[fromPlayer.CSteamID.m_SteamID][kitName].items.Count; i++)
            {
                InventoryManager.AddItem(toPlayer, database[fromPlayer.CSteamID.m_SteamID][kitName].items[i]);
            }

            Events.InvokeLoadKit(fromPlayer, toPlayer, kitName, clothes);
        }

        public static void DeleteKit(UnturnedPlayer player, string kitName, Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> database)
        {
            if (kitName == "*")
            {
                database[player.CSteamID.m_SteamID].Clear();
            }
            else
            {
                database[player.CSteamID.m_SteamID].Remove(kitName);
            }

            Events.InvokeDelKit(player, kitName);
        }

        public static int KitCount(UnturnedPlayer player, Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> database)
        {
            return database[player.CSteamID.m_SteamID].Count;
        }

        public static bool HasKit(UnturnedPlayer player, string kitName, Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> database)
        {
            if (database[player.CSteamID.m_SteamID].ContainsKey(kitName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool HasSavedKits(UnturnedPlayer player, Dictionary<ulong, Dictionary<string, InventoryManager.Inventory>> database)
        {
            if (database[player.CSteamID.m_SteamID].Count < 0 || !database.ContainsKey(player.CSteamID.m_SteamID))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void TryStoreKits()
        {
            if (Plugin.CustomKitsPlugin.Instance.Configuration.Instance.KeepKitsOnRestart == true)
            {
                string json = "";

                try
                {
                    json = JsonConvert.SerializeObject(Kits);
                }
                catch
                {
                    Logger.LogError("An error has occured while serializing kits");
                    return;
                }

                if (File.Exists(Plugin.CustomKitsPlugin.ThisDirectory + "StoredKits.json"))
                {
                    File.WriteAllText(Plugin.CustomKitsPlugin.ThisDirectory + "StoredKits.json", json);
                }
                else
                {
                    File.Create(Plugin.CustomKitsPlugin.ThisDirectory + "StoredKits.json").Close();
                    File.WriteAllText(Plugin.CustomKitsPlugin.ThisDirectory + "StoredKits.json", json);
                }
            }
            else
            {
                Logger.LogError("KeepKitsOnRestart must be disabled!");
            }
        }

        internal static IEnumerator DelayedLoad(UnturnedPlayer player, string kitName, float delay)
        {
            yield return new WaitForSeconds(delay);

            LoadKit(player, player, kitName, Plugin.CustomKitsPlugin.Instance.Configuration.Instance.IncludeClothingInKits, Kits);
            UnturnedChat.Say(player, Plugin.CustomKitsPlugin.Instance.Translate("kit_loaded", kitName), Color.green);
        }

        internal static IEnumerator AutoStoreKits()
        {
            int time = U.Settings.Instance.AutomaticSave.Interval;

            if (time <= 30)
            {
                time = 30;
            }

            while (U.Settings.Instance.AutomaticSave.Enabled)
            {
                yield return new WaitForSeconds(time);

                TryStoreKits();

                Logger.Log(Plugin.CustomKitsPlugin.Instance.Translate("auto_stored"), ConsoleColor.Green);
            }
        }
    }

    public class SlotManager
    {
        public static Dictionary<ulong, List<Slot>> Slots;

        public class Slot
        {
            public int itemLimit;

            public Slot(int itemLimit)
            {
                this.itemLimit = itemLimit;
            }
        }

        public static int SlotCount(UnturnedPlayer player)
        {
            return Slots[player.CSteamID.m_SteamID].Count;
        }
        
        public static void AddSlot(UnturnedPlayer player, int amount, int itemLimit)
        {
            if (Slots.ContainsKey(player.CSteamID.m_SteamID))
            {
                for (int i = 0; i < amount; i++)
                {
                    Slots[player.CSteamID.m_SteamID].Add(new Slot(itemLimit));
                }
            }
            else
            {
                Slots.Add(player.CSteamID.m_SteamID, new List<Slot>());

                for (int i = 0; i < amount; i++)
                {
                    Slots[player.CSteamID.m_SteamID].Add(new Slot(itemLimit));
                }
            }
        }
        
        public static void RemoveSlot(UnturnedPlayer player, int amount)
        {
            if (!Slots.ContainsKey(player.CSteamID.m_SteamID)) return;

            for (int i = 0; i < amount; i++)
            {
                foreach (Slot slot in Slots[player.CSteamID.m_SteamID])
                {
                    Slots[player.CSteamID.m_SteamID].Remove(slot);
                }
            }
        }
        
        public static void ClearSlots(UnturnedPlayer player)
        {
            if (!Slots.ContainsKey(player.CSteamID.m_SteamID)) return;

            Slots.Remove(player.CSteamID.m_SteamID);
        }
    }

    public class VehicleManager
    {
        public static Dictionary<CSteamID, List<InteractableVehicle>> CurrentVehicles;

        public static IEnumerator LimitVehicles(UnturnedPlayer player)
        {
            yield return new WaitForSeconds(2.5f);

            if (CurrentVehicles.ContainsKey(player.CSteamID))
            {
                if (CurrentVehicles[player.CSteamID].Count > 0)
                {
                    foreach (var car in CurrentVehicles[player.CSteamID])
                    {
                        if (!car.isEmpty)
                        {
                            car.forceRemoveAllPlayers();
                        }

                        SDG.Unturned.VehicleManager.instance.channel.send("tellVehicleDestroy", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[] { car.instanceID });
                    }

                    CurrentVehicles[player.CSteamID].Clear();
                }
            }
            else
            {
                CurrentVehicles.Add(player.CSteamID, new List<InteractableVehicle>());
            }
            
            foreach (var car in SDG.Unturned.VehicleManager.vehicles)
            {
                if ((car.transform.position - player.Position).magnitude < 24f)
                {
                    CurrentVehicles[player.CSteamID].Add(car);
                }
            }
        }
    }

    public class Events
    {
        // Kit saved
        public delegate void SaveKitDelegate(UnturnedPlayer player, UnturnedPlayer toPlayer, string kitName);
        public static event SaveKitDelegate OnKitSaved;

        public static void InvokeSaveKit(UnturnedPlayer player, UnturnedPlayer toPlayer, string kitName)
        {
            if (OnKitSaved != null)
            {
                OnKitSaved.Invoke(player, toPlayer, kitName);
            }
        }
        
        // Kit loaded
        public delegate void LoadKitDelegate(UnturnedPlayer player, UnturnedPlayer toPlayer, string kitName, bool clothes);
        public static event LoadKitDelegate OnKitLoaded;

        public static void InvokeLoadKit(UnturnedPlayer player, UnturnedPlayer toPlayer, string kitName, bool clothes)
        {
            if (OnKitLoaded != null)
            {
                OnKitLoaded.Invoke(player, toPlayer, kitName, clothes);
            }
        }
        
        // Kit deleted
        public delegate void DelKitDelegate(UnturnedPlayer player, string kitName);
        public static event DelKitDelegate OnKitDeleted;

        public static void InvokeDelKit(UnturnedPlayer player, string kitName)
        {
            if (OnKitDeleted != null)
            {
                OnKitDeleted.Invoke(player, kitName);
            }
        }

        // Inventory cleared
        public delegate void ClearInventoryDelegate(UnturnedPlayer player);
        public static event ClearInventoryDelegate OnInventoryCleared;

        public static void InvokeClearInventory(UnturnedPlayer player)
        {
            if (OnInventoryCleared != null)
            {
                OnInventoryCleared.Invoke(player);
            }
        }
    }
}