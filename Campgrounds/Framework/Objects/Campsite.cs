using Campgrounds.Framework.Managers;
using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Models.Enums;
using Campgrounds.Framework.Utilities;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Objects
{
    public class Campsite
    {
        public Farmer Camper { get; }
        public Character Guest { get; }

        public CampgroundData Data { get; }

        public CookingSpot CookingSpot { get; private set; }
        public CampingTentData CurrentCampTent { get; private set; }

        public bool IsTraveling { get; private set; }

        private Stack<Dialogue> _cachedDialogue;

        public Campsite(Farmer who, CampgroundData data, Character guest = null)
        {
            Camper = who;
            Data = data;
            Guest = guest;

            CookingSpot = new CookingSpot() { CanCook = true };
            CurrentCampTent = Campgrounds.tentManager.GetCurrentTent(who);
        }

        public GameLocation GetLocation()
        {
            var location = Game1.getLocationFromName(Data.Id);
            if (location is null)
            {
                Campgrounds.monitor.LogOnce($"The campgrounds map with name {Data.Id} does not exist!", LogLevel.Warn);
                return null;
            }

            return location;
        }

        public void CacheBuffs(List<Buff> buffs)
        {
            // Cache the data via CustomFields / modData
            Camper.modDataForSerialization[CampingManager.CACHED_BUFF_IDS_MOD_DATA_ID] = JsonSerializer.Serialize(buffs.Select(b => b.id));

            if (Guest is Farmer guestFarmer && guestFarmer is not null)
            {
                guestFarmer.modDataForSerialization[CampingManager.CACHED_BUFF_IDS_MOD_DATA_ID] = JsonSerializer.Serialize(buffs.Select(b => b.id));
            }

            CookingSpot.HasCookedToday = true;
        }

        public void ClearBuffs()
        {
            Camper.modDataForSerialization[CampingManager.CACHED_BUFF_IDS_MOD_DATA_ID] = string.Empty;
            if (Guest is Farmer guestFarmer && guestFarmer is not null)
            {
                guestFarmer.modDataForSerialization[CampingManager.CACHED_BUFF_IDS_MOD_DATA_ID] = string.Empty;
            }

            CookingSpot.HasCookedToday = false;
        }

        public void ApplyCachedBuffs()
        {
            var guestFarmer = Guest is Farmer farmer ? farmer : null;

            var rawBuffText = string.Empty;
            if (Camper.modDataForSerialization.ContainsKey(CampingManager.CACHED_BUFF_IDS_MOD_DATA_ID) && string.IsNullOrEmpty(Camper.modDataForSerialization[CampingManager.CACHED_BUFF_IDS_MOD_DATA_ID]) is false)
            {
                rawBuffText = Camper.modDataForSerialization[CampingManager.CACHED_BUFF_IDS_MOD_DATA_ID];
            }
            else if (guestFarmer is not null && guestFarmer.modDataForSerialization.ContainsKey(CampingManager.CACHED_BUFF_IDS_MOD_DATA_ID) && string.IsNullOrEmpty(guestFarmer.modDataForSerialization[CampingManager.CACHED_BUFF_IDS_MOD_DATA_ID]) is false)
            {
                rawBuffText = guestFarmer.modDataForSerialization[CampingManager.CACHED_BUFF_IDS_MOD_DATA_ID];
            }

            if (string.IsNullOrEmpty(rawBuffText) is false)
            {
                var buffIds = JsonSerializer.Deserialize<List<string>>(Camper.modDataForSerialization[CampingManager.CACHED_BUFF_IDS_MOD_DATA_ID]);
                foreach (var buffId in buffIds)
                {
                    var buff = new Buff(buffId);
                    buff.millisecondsDuration = 420000;

                    Camper.applyBuff(buff);
                    if (guestFarmer is not null)
                    {
                        guestFarmer.applyBuff(buff);
                    }
                }

                CookingSpot.HasCookedToday = false;
            }
        }

        public void HandleNewDay()
        {
            var location = GetLocation();
            if (location is null)
            { 
                return;
            }

            HandleCampsiteSetup(isDayAfter: true);

            CookingSpot.HasCookedToday = false;
            if (location.farmers.Any(c => c == Camper) || location.characters.Any(c => c == Guest))
            {
                CookingSpot.CanCook = false;
            }

            ApplyCachedBuffs();
        }

        public void Sanitize()
        {
            var location = GetLocation();
            if (location is null)
            {
                return;
            }

            location.largeTerrainFeatures.RemoveWhere(o => o is CampingTent);

            var cookingSpotsToRemove = location.objects.Pairs.Where(o => o.Value is CookingSpot);
            foreach (var cookingSpot in cookingSpotsToRemove)
            {
                location.objects.Remove(cookingSpot.Key);
            }
        }

        public bool HandleCampsiteSetup(bool isDayAfter = false)
        {
            var location = GetLocation();
            if (location is null)
            {
                return false;
            }

            // Get tent tiles
            var layer = location.Map.GetLayer("Back");

            Vector2? playerTentTile = null;
            Vector2? guestTentTile = null;

            Direction playerTentDirection = Direction.South;
            Direction guestTentDirection = Direction.South;

            Vector2? cookingSpotTile = null;

            for (int x = 0; x < layer.LayerWidth; x++)
            {
                for (int y = 0; y < layer.LayerHeight; y++)
                {
                    if (location.doesTileHaveProperty(x, y, "IsCampingSpot", "Back") != null)
                    {
                        if (location.doesTileHaveProperty(x, y, "IsForGuest", "Back") == "T")
                        {
                            if (Enum.TryParse<Direction>(location.doesTileHaveProperty(x, y, "CampingDirection", "Back"), out var direction))
                            {
                                guestTentDirection = direction;
                            }

                            guestTentTile = new Vector2(x, y);
                        }
                        else
                        {
                            if (Enum.TryParse<Direction>(location.doesTileHaveProperty(x, y, "CampingDirection", "Back"), out var direction))
                            {
                                playerTentDirection = direction;
                            }

                            playerTentTile = new Vector2(x, y);
                        }
                    }

                    if (location.doesTileHaveProperty(x, y, "IsCookingSpot", "Back") != null)
                    {
                        cookingSpotTile = new Vector2(x, y);
                    }
                }
            }

            if (playerTentTile is null)
            {
                Campgrounds.monitor.LogOnce($"The campgrounds map with name {Data.Id} is missing the player's tent spot (IsCampingSpot tile property on Back layer)", LogLevel.Warn);
                return false;
            }
            if (guestTentTile is null)
            {
                Campgrounds.monitor.LogOnce($"The campgrounds map with name {Data.Id} is missing the guest's tent spot (IsCampingSpot and IsForGuest tile property on Back layer)", LogLevel.Warn);
                return false;
            }
            if (cookingSpotTile is null)
            {
                Campgrounds.monitor.LogOnce($"The campgrounds map with name {Data.Id} is missing a cooking spot (IsCookingSpot tile property on Back layer)", LogLevel.Warn);
                return false;
            }

            // Place the tents
            if (!location.isTerrainFeatureAt((int)playerTentTile.Value.X, (int)playerTentTile.Value.Y))
            {
                location.largeTerrainFeatures.Add(new CampingTent(playerTentTile.Value, playerTentDirection, this, CurrentCampTent));
            }
            if (Guest is not null && !location.isTerrainFeatureAt((int)guestTentTile.Value.X, (int)guestTentTile.Value.Y))
            {
                CampingTentData guestCampingTentData = null;
                if (Guest is Farmer farmer && farmer is not null)
                {
                    guestCampingTentData = Campgrounds.tentManager.GetCurrentTent(farmer);
                }
                else if (Guest is NPC npc && npc is not null)
                {
                    var villagerData = Campgrounds.villagerManager.GetVillagerData(npc);
                    guestCampingTentData = Campgrounds.tentManager.GetTentDataById(villagerData.TentId);
                }

                if (guestCampingTentData is null)
                {
                    guestCampingTentData = Campgrounds.tentManager.GetStarterTent();
                }
                location.largeTerrainFeatures.Add(new CampingTent(guestTentTile.Value, guestTentDirection, this, guestCampingTentData));
            }

            // Place the cooking spot
            if (!location.objects.ContainsKey(cookingSpotTile.Value))
            {
                location.objects.Add(cookingSpotTile.Value, CookingSpot);
            }

            // Warp the Guest (if player, skip since we warp them via message)
            if (Guest is NPC guestNPC && guestNPC is not null && Data.GuestSpawnTile is not null)
            {
                // Cache previous dialogue
                _cachedDialogue = new Stack<Dialogue>(guestNPC.CurrentDialogue.Reverse());

                var dialogue = Campgrounds.villagerManager.GetGameReadyDialogue(Campgrounds.villagerManager.GetCampsiteDialogue(Data, guestNPC, isDayAfter));
                NPCHelper.WarpAndSetDialogue(guestNPC, GetLocation(), Data.GuestSpawnTile.Value, dialogue);
            }

            return true;
        }

        public void HandleExit()
        {
            Sanitize();

            // Clear invited NPC
            Campgrounds.villagerManager.SetInvitedCharacter(Camper, null);

            if (Guest is NPC npc && npc is not null)
            {
                // Restore previous dialogue
                npc.CurrentDialogue.Clear();
                if (_cachedDialogue is not null)
                {
                    foreach (Dialogue d in _cachedDialogue.Reverse())
                    {
                        npc.CurrentDialogue.Push(d);
                    }

                    _cachedDialogue = null;
                }

                // Reset schedule
                NPCHelper.ReturnNPCToSchedule(npc);
            }
        }

        internal void Sleep()
        {
            var location = GetLocation();
            if (location is null)
            {
                return;
            }
            Camper.modDataForSerialization[CampingManager.LAST_CAMPSITE_SLEPT_MOD_DATA_ID] = Data.Id;

            Game1.player.isInBed.Value = true;
            Game1.player.sleptInTemporaryBed.Value = true;
            Game1.displayFarmer = false;
            Game1.playSound("sandyStep");
            DelayedAction.playSoundAfterDelay("sandyStep", 500);

            Campgrounds.modHelper.Reflection.GetMethod(location, "startSleep").Invoke();
        }
    }
}
