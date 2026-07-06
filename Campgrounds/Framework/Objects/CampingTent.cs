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

        public CampingTent(Vector2 tile, Direction direction, Campsite campsite, CampingTentData campingTentData) : base(true)
        {
            Tile = tile;

            Direction = direction;

            _campsite = campsite;
            _campingTentData = campingTentData;

            isDestroyedByNPCTrample = false;
        }

        private Vector2 GetOffsetTile()
        {
            switch (Direction)
            {
                case Direction.North:
                    return new Vector2(Tile.X, Tile.Y - (_campingTentData.NorthSprite.DisplayRectangle.Height / 16) + 1);
                case Direction.East:
                    return new Vector2(Tile.X, Tile.Y - (_campingTentData.EastSprite.DisplayRectangle.Height / 16) + 1);
                case Direction.South:
                    return new Vector2(Tile.X, Tile.Y - (_campingTentData.SouthSprite.DisplayRectangle.Height / 16) + 1);
                case Direction.West:
                    return new Vector2(Tile.X, Tile.Y - (_campingTentData.WestSprite.DisplayRectangle.Height / 16) + 1);
            }

            return Tile;
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
            Vector2 tileLocation = GetOffsetTile();
            switch (Direction)
            {
                case Direction.North:
                    tileLocation = new Vector2(tileLocation.X, tileLocation.Y + _campingTentData.NorthSprite.BoundaryRectangle.Y / 16);
                    break;
                case Direction.East:
                    tileLocation = new Vector2(tileLocation.X, tileLocation.Y + _campingTentData.EastSprite.BoundaryRectangle.Y / 16);
                    break;
                case Direction.South:
                    tileLocation = new Vector2(tileLocation.X, tileLocation.Y + _campingTentData.SouthSprite.BoundaryRectangle.Y / 16);
                    break;
                case Direction.West:
                    tileLocation = new Vector2(tileLocation.X, tileLocation.Y + _campingTentData.WestSprite.BoundaryRectangle.Y / 16);
                    break;
            }
            return new Rectangle((int)(tileLocation.X) * 64, (int)(tileLocation.Y) * 64, boundaryRectangle.Width * 4, boundaryRectangle.Height * 4);
        }

        public override bool isPassable(Character c = null)
        {
            return c != null;
        }

        public Vector2 GetEntranceTile()
        {
            Vector2 tilePosition = GetOffsetTile();
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
            tilePosition = (tilePosition / 16) + GetOffsetTile();

            return tilePosition;
        }

        public override bool performUseAction(Vector2 tileLocation)
        {
            Vector2 tilePosition = GetEntranceTile();
            Vector2 playerGrab = Game1.player.GetGrabTile();

            if ((playerGrab == tilePosition || (playerGrab.X == tilePosition.X && playerGrab.Y >= tilePosition.Y)) && !Game1.newDay && Game1.shouldTimePass() && Game1.player.hasMoved && !Game1.player.passedOut)
            {
                if (Campgrounds.campManager.GetLastCampsiteSleptIn(Game1.player) == _campsite.Data.Id)
                {
                    Game1.activeClickableMenu = new DialogueBox("Time to head back.");
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
            Vector2 tileLocation = GetOffsetTile();
            var spriteTexture = Campgrounds.modHelper.GameContent.Load<Texture2D>(_campingTentData.TexturePath);

            Rectangle? spriteDisplayRectangle = null;
            Rectangle? shadowDisplayRectangle = null;
            Vector2 shadowOffset = Vector2.Zero;
            int layerHeightOffset = 0;
            switch (Direction)
            {
                case Direction.North:
                    spriteDisplayRectangle = _campingTentData.NorthSprite.DisplayRectangle;
                    shadowDisplayRectangle = _campingTentData.NorthSprite.ShadowRectangle;
                    shadowOffset = _campingTentData.NorthSprite.ShadowOffset;
                    layerHeightOffset = _campingTentData.NorthSprite.DisplayRectangle.Height / 16;
                    break;
                case Direction.East:
                    spriteDisplayRectangle = _campingTentData.EastSprite.DisplayRectangle;
                    shadowDisplayRectangle = _campingTentData.EastSprite.ShadowRectangle;
                    shadowOffset = _campingTentData.EastSprite.ShadowOffset;
                    layerHeightOffset = _campingTentData.EastSprite.DisplayRectangle.Height / 16;
                    break;
                case Direction.South:
                    spriteDisplayRectangle = _campingTentData.SouthSprite.DisplayRectangle;
                    shadowDisplayRectangle = _campingTentData.SouthSprite.ShadowRectangle;
                    shadowOffset = _campingTentData.SouthSprite.ShadowOffset;
                    layerHeightOffset = _campingTentData.SouthSprite.DisplayRectangle.Height / 16;
                    break;
                case Direction.West:
                    spriteDisplayRectangle = _campingTentData.WestSprite.DisplayRectangle;
                    shadowDisplayRectangle = _campingTentData.WestSprite.ShadowRectangle;
                    shadowOffset = _campingTentData.WestSprite.ShadowOffset;
                    layerHeightOffset = _campingTentData.WestSprite.DisplayRectangle.Height / 16;
                    break;
            }
            shadowOffset = new Vector2(16, 32);

            spriteBatch.Draw(spriteTexture, Game1.GlobalToLocal(tileLocation * 64), shadowDisplayRectangle, Color.White, 0f, new Vector2(shadowOffset.X, -shadowOffset.Y), 4f, SpriteEffects.None, 0.0001f);
            spriteBatch.Draw(spriteTexture, Game1.GlobalToLocal(tileLocation * 64), spriteDisplayRectangle, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y + layerHeightOffset) * 64f / 10000f);
        }
    }
}
