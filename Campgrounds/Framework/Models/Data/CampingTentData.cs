using Campgrounds.Framework.Models.Common;
using Campgrounds.Framework.Models.Enums;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace Campgrounds.Framework.Models.Data
{
    public class CampingTentData : BaseModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public string TexturePath { get; set; }
        public string? GrayscaleTexturePath { get; set; }
        public Vector2 PreviewOffset { get; set; } = Vector2.Zero;
        public DirectionalSpriteModel NorthSprite { get; set; }
        public DirectionalSpriteModel EastSprite { get; set; }
        public DirectionalSpriteModel SouthSprite { get; set; }
        public DirectionalSpriteModel WestSprite { get; set; }

        /// <summary>
        /// Determines how long the food buffs last by default. Can be overriden by food's Buffs.Duration.
        /// </summary>
        public int FoodBuffDuration { get; set; } = BuffData.DEFAULT_DURATION;
        /// <summary>
        /// Determines how long the resting buffs last by default. Can be overriden by resting buff's Buffs.Duration.
        /// </summary>
        public int RestingBuffDuration { get; set; } = BuffData.DEFAULT_DURATION;
        public List<BuffData> RestingBuffs { get; set; } = new List<BuffData>();

        public int NumberOfAllowedCampfireMeals { get { return _numberOfAllowedCampfireMeals; } set { _numberOfAllowedCampfireMeals = Math.Min(5, Math.Max(value, 1)); } }
        private int _numberOfAllowedCampfireMeals = 1;

        public string UnlockCondition { get; set; }
        public string UnlockHint { get; set; }

        /// <summary>
        /// If true, the campsite will be hidden from the CampListMenu until the player unlocks it (UnlockHint will be ignored).
        /// </summary>
        public bool HideUntilUnlocked { get; set; }

        public List<Buff> GetBuffs()
        {
            List<Buff> buffs = new List<Buff>();
            foreach (var buffData in RestingBuffs)
            {
                var buff = new Buff(buffData.Id);
                buff.millisecondsDuration = buffData.Duration != 0 ? buffData.Duration : RestingBuffDuration;

                buffs.Add(buff);
            }

            return buffs;
        }

        public SizeModel GetTileSize(Direction direction)
        {
            var size = new SizeModel();
            switch (direction)
            {
                case Direction.North:
                    size.Height = NorthSprite.DisplayRectangle.Height / 16;
                    size.Width = NorthSprite.DisplayRectangle.Width / 16;
                    break;
                case Direction.East:
                    size.Height = EastSprite.DisplayRectangle.Height / 16;
                    size.Width = EastSprite.DisplayRectangle.Width / 16;
                    break;
                case Direction.South:
                    size.Height = SouthSprite.DisplayRectangle.Height / 16;
                    size.Width = SouthSprite.DisplayRectangle.Width / 16;
                    break;
                case Direction.West:
                    size.Height = WestSprite.DisplayRectangle.Height / 16;
                    size.Width = WestSprite.DisplayRectangle.Width / 16;
                    break;
            }

            return size;
        }

        public bool IsUnlocked()
        {
            if (string.IsNullOrEmpty(UnlockCondition))
            {
                return true;
            }

            return GameStateQuery.CheckConditions(UnlockCondition);
        }

        public override (bool Result, string Error) IsValid()
        {
            if (string.IsNullOrEmpty(DisplayName))
            {
                return (false, "DisplayName needs to be given!");
            }

            if (string.IsNullOrEmpty(TexturePath))
            {
                return (false, "TexturePath needs to be given!");
            }

            if (NorthSprite is null)
            {
                return (false, "Missing NorthSprite!");
            }

            if (EastSprite is null)
            {
                return (false, "Missing EastSprite!");
            }
            if (SouthSprite is null)
            {
                return (false, "Missing SouthSprite!");
            }
            if (WestSprite is null)
            {
                return (false, "Missing WestSprite!");
            }

            return (true, string.Empty);
        }
    }
}
