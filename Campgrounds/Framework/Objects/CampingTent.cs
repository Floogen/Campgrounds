using Campgrounds.Framework.Models.Common;
using Campgrounds.Framework.Models.Data;
using Campgrounds.Framework.Models.Enums;
using Campgrounds.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Tiles;

namespace Campgrounds.Framework.Objects
{
    public class CampingTent : LargeTerrainFeature
    {
        public Direction Direction { get; set; }

        private Campsite _campsite;
        private CampingTentData _campingTentData;
        private Character _owner;

        public CampingTent(Vector2 tile, Direction direction, Campsite campsite, CampingTentData campingTentData, Character owner = null) : base(true)
        {
            Tile = tile;

            Direction = direction;

            _campsite = campsite;
            _campingTentData = campingTentData;
            _owner = owner;

            isDestroyedByNPCTrample = false;
        }

        public bool IsOwner(Farmer who)
        {
            return _owner == who;
        }

        private Vector2 GetActualTile()
        {
            var tile = Tile;
            switch (Direction)
            {
                case Direction.North:
                    tile = new Vector2(Tile.X - (_campingTentData.NorthSprite.EntranceTile.X / 16), Tile.Y - (_campingTentData.NorthSprite.EntranceTile.X / 16));
                    break;
                case Direction.East:
                    tile = new Vector2(Tile.X - (_campingTentData.EastSprite.EntranceTile.X / 16), Tile.Y - (_campingTentData.EastSprite.EntranceTile.Y / 16));
                    break;
                case Direction.South:
                    tile = new Vector2(Tile.X - (_campingTentData.SouthSprite.EntranceTile.X / 16), Tile.Y - (_campingTentData.SouthSprite.EntranceTile.Y / 16));
                    break;
                case Direction.West:
                    tile = new Vector2(Tile.X + (_campingTentData.WestSprite.EntranceTile.X / 16), Tile.Y - (_campingTentData.WestSprite.EntranceTile.Y / 16));
                    break;
            }

            return tile + (GetTileOffset() / 16f);
        }

        private Vector2 GetTileOffset()
        {
            switch (Direction)
            {
                case Direction.North:
                    return _campingTentData.NorthSprite.TileOffset;
                case Direction.East:
                    return _campingTentData.EastSprite.TileOffset;
                case Direction.South:
                    return _campingTentData.SouthSprite.TileOffset;
                case Direction.West:
                    return _campingTentData.WestSprite.TileOffset;
            }

            return Vector2.Zero;
        }

        public override Rectangle getBoundingBox()
        {
            switch (Direction)
            {
                case Direction.North:
                    return GetScaledBoundary(_campingTentData.NorthSprite.BoundaryRectangle);
                case Direction.East:
                    return GetScaledBoundary(_campingTentData.EastSprite.BoundaryRectangle);
                case Direction.South:
                    return GetScaledBoundary(_campingTentData.SouthSprite.BoundaryRectangle);
                case Direction.West:
                    return GetScaledBoundary(_campingTentData.WestSprite.BoundaryRectangle);
            }

            return base.getBoundingBox();
        }

        private Rectangle GetScaledBoundary(Rectangle boundaryRectangle)
        {
            Vector2 tileLocation = GetActualTile();
            switch (Direction)
            {
                case Direction.North:
                    tileLocation = new Vector2(tileLocation.X + boundaryRectangle.X / 16, tileLocation.Y + boundaryRectangle.Y / 16);
                    break;
                case Direction.East:
                    tileLocation = new Vector2(tileLocation.X + boundaryRectangle.X / 16, tileLocation.Y + boundaryRectangle.Y / 16);
                    break;
                case Direction.South:
                    tileLocation = new Vector2(tileLocation.X + boundaryRectangle.X / 16, tileLocation.Y + boundaryRectangle.Y / 16);
                    break;
                case Direction.West:
                    tileLocation = new Vector2(tileLocation.X + boundaryRectangle.X / 16, tileLocation.Y + boundaryRectangle.Y / 16);
                    break;
            }

            int tileOffsetX = (int)(Math.Abs(GetTileOffset().X) + 15) / 16 * 16;
            int tileOffsetY = (int)(Math.Abs(GetTileOffset().Y) + 15) / 16 * 16;
            return new Rectangle((int)(tileLocation.X) * 64, (int)(tileLocation.Y) * 64, (boundaryRectangle.Width * 4) + tileOffsetX, (boundaryRectangle.Height * 4) + tileOffsetY);
        }

        public override bool isPassable(Character c = null)
        {
            return c != null;
        }

        public Vector2 GetEntranceTile()
        {
            Vector2 tilePosition = GetActualTile();
            switch (Direction)
            {
                case Direction.North:
                    tilePosition = _campingTentData.NorthSprite.EntranceTile;
                    break;
                case Direction.East:
                    tilePosition = _campingTentData.EastSprite.EntranceTile;
                    break;
                case Direction.South:
                    tilePosition = _campingTentData.SouthSprite.EntranceTile;
                    break;
                case Direction.West:
                    tilePosition = _campingTentData.WestSprite.EntranceTile;
                    break;
            }
            tilePosition = (tilePosition / 16) + GetActualTile();

            return tilePosition;
        }

        public override bool performUseAction(Vector2 tileLocation)
        {
            Vector2 tilePosition = GetEntranceTile();
            Vector2 playerGrab = Game1.player.GetGrabTile();

            if ((playerGrab == tilePosition || (playerGrab.X == tilePosition.X && playerGrab.Y >= tilePosition.Y)) && !Game1.newDay && Game1.shouldTimePass() && Game1.player.hasMoved && !Game1.player.passedOut && IsOwner(Game1.player))
            {
                if (Campgrounds.campManager.GetLastCampsiteSleptIn(Game1.player) == _campsite.Data.Id)
                {
                    Location.createQuestionDialogue(Campgrounds.modHelper.Translation.Get("dialogues.general.tentClickAfterSleep"), Location.createYesNoResponses(), CampingHelper.OnTentAfterSleepResponse, null);
                }
                else
                {
                    Location.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:FarmHouse_Bed_GoToSleep"), Location.createYesNoResponses(), CampingHelper.OnTentSleepResponse, null);
                }
            }

            return base.performUseAction(tileLocation);
        }

        public override bool forceDraw()
        {
            return true;
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            Vector2 tileLocation = GetActualTile();
            var spriteTexture = Campgrounds.modHelper.GameContent.Load<Texture2D>(_campingTentData.TexturePath);

            Rectangle? spriteDisplayRectangle = null;
            Rectangle? shadowDisplayRectangle = null;
            Vector2 shadowOffset = Vector2.Zero;
            int layerHeightOffset = 0;

            DirectionalSpriteModel directionalSprite = null;
            switch (Direction)
            {
                case Direction.North:
                    directionalSprite = _campingTentData.NorthSprite;
                    break;
                case Direction.East:
                    directionalSprite = _campingTentData.EastSprite;
                    break;
                case Direction.South:
                    directionalSprite = _campingTentData.SouthSprite;
                    break;
                case Direction.West:
                    directionalSprite = _campingTentData.WestSprite;
                    break;
            }

            if (directionalSprite is null)
            {
                return;
            }

            spriteDisplayRectangle = directionalSprite.DisplayRectangle;
            shadowDisplayRectangle = directionalSprite.ShadowRectangle;
            shadowOffset = directionalSprite.ShadowOffset;
            layerHeightOffset = directionalSprite.DisplayRectangle.Height / 16;

            var spriteEffects = (directionalSprite.FlipHorizontally ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (directionalSprite.FlipVertically ? SpriteEffects.FlipVertically : SpriteEffects.None);

            if (shadowDisplayRectangle is not null)
            {
                spriteBatch.Draw(spriteTexture, Game1.GlobalToLocal(tileLocation * 64), shadowDisplayRectangle, Color.White, 0f, -shadowOffset, 4f, spriteEffects, 0.0001f);
            }

            var layerOffset = (tileLocation.Y + layerHeightOffset) * 64f / 10000f;
            spriteBatch.Draw(spriteTexture, Game1.GlobalToLocal(tileLocation * 64), spriteDisplayRectangle, Color.White, 0f, Vector2.Zero, 4f, spriteEffects, layerOffset);

            if (_owner is Farmer)
            {
                var tentColor = Campgrounds.tentManager.GetTentColor(Game1.player, _campingTentData.Id);
                if (tentColor is not null)
                {
                    Texture2D spriteTextureGrayscale = Campgrounds.modHelper.GameContent.Load<Texture2D>(_campingTentData.GrayscaleTexturePath);
                    spriteBatch.Draw(spriteTextureGrayscale, Game1.GlobalToLocal(tileLocation * 64), spriteDisplayRectangle, tentColor.Value, 0f, Vector2.Zero, 4f, spriteEffects, layerOffset + 0.00001f);
                }
            }
        }
    }
}
