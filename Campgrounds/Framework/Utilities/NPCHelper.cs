using Campgrounds.Framework.Models.Enums;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campgrounds.Framework.Utilities
{
    public static class NPCHelper
    {
        public static void WarpAndSetDialogue(NPC npc, GameLocation location, Vector2 tile, string dialogue = null, Direction? faceDirection = null, bool freeze = false)
        {
            Game1.warpCharacter(npc, location, tile);

            // Assign dialogue to character
            if (string.IsNullOrEmpty(dialogue) is false)
            {
                npc.setNewDialogue(new Dialogue(npc, null, dialogue));
            }

            if (faceDirection is not null)
            {
                npc.faceDirection((int)faceDirection.Value);
            }

            // Stop movement, if needed
            if (freeze)
            {
                npc.Halt();
                npc.ignoreScheduleToday = true;
                npc.controller = null;
            }
        }
        public static void ReturnNPCToSchedule(NPC npc)
        {
            // Enable schedule
            npc.ignoreScheduleToday = false;

            if (npc.Schedule != null && npc.Schedule.Count > 0)
            {
                // Find the latest schedule entry
                var validKeys = npc.Schedule.Keys.Where(t => t <= Game1.timeOfDay);
                if (validKeys is null || validKeys.Count() == 0)
                {
                    // No valid schedule (send home)
                    Game1.warpCharacter(npc, npc.DefaultMap, npc.DefaultPosition / 64f);
                    return;
                }

                SchedulePathDescription entry = npc.Schedule[validKeys.Min()];
                Game1.warpCharacter(npc, entry.targetLocationName, entry.targetTile);
                npc.faceDirection(entry.facingDirection);

                // Let the schedule system resync so they continue on to later entries
                npc.checkSchedule(entry.time);
            }
            else
            {
                // No valid schedule (send home)
                Game1.warpCharacter(npc, npc.DefaultMap, npc.DefaultPosition / 64f);
            }
        }
    }
}
