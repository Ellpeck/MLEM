using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Misc;
using MonoGame.Extended.Tiled;
using RectangleF = MonoGame.Extended.RectangleF;

namespace MLEM.Extended.Tiled {
    public class IndividualTiledMapRenderer {

        private TiledMap map;
        private TileDrawInfo[,,] drawInfos;
        private GetDepth depthFunction;
        private List<TiledMapTilesetAnimatedTile> animatedTiles;

        public IndividualTiledMapRenderer(TiledMap map = null, GetDepth depthFunction = null) {
            if (map != null)
                this.SetMap(map, depthFunction);
        }

        public void SetMap(TiledMap map, GetDepth depthFunction = null) {
            this.map = map;
            this.depthFunction = depthFunction ?? ((tile, layer, layerIndex, position) => 0);

            this.drawInfos = new TileDrawInfo[map.TileLayers.Count, map.Width, map.Height];
            for (var i = 0; i < map.TileLayers.Count; i++) {
                for (var x = 0; x < map.Width; x++) {
                    for (var y = 0; y < map.Height; y++) {
                        this.UpdateDrawInfo(i, x, y);
                    }
                }
            }

            this.animatedTiles = new List<TiledMapTilesetAnimatedTile>();
            foreach (var tileset in map.Tilesets) {
                foreach (var tile in tileset.Tiles) {
                    if (tile is TiledMapTilesetAnimatedTile animated) {
                        this.animatedTiles.Add(animated);
                    }
                }
            }
        }

        public void UpdateDrawInfo(int layerIndex, int x, int y) {
            var layer = this.map.TileLayers[layerIndex];
            var tile = layer.GetTile(x, y);
            if (tile.IsBlank) {
                this.drawInfos[layerIndex, x, y] = null;
                return;
            }
            var tileset = tile.GetTileset(this.map);
            var tilesetTile = tileset.GetTilesetTile(tile, this.map);
            var pos = new Point(x, y);
            var depth = this.depthFunction(tile, layer, layerIndex, pos);
            this.drawInfos[layerIndex, x, y] = new TileDrawInfo(this, tile, tileset, tilesetTile, pos, depth);
        }

        public void Draw(SpriteBatch batch, RectangleF? frustum = null, DrawDelegate drawFunction = null) {
            for (var i = 0; i < this.map.TileLayers.Count; i++)
                this.DrawLayer(batch, i, frustum, drawFunction);
        }

        public void DrawLayer(SpriteBatch batch, int layerIndex, RectangleF? frustum = null, DrawDelegate drawFunction = null) {
            var frust = frustum ?? new RectangleF(0, 0, float.MaxValue, float.MaxValue);
            var minX = Math.Max(0, frust.Left / this.map.TileWidth).Floor();
            var minY = Math.Max(0, frust.Top / this.map.TileHeight).Floor();
            var maxX = Math.Min(this.map.Width, frust.Right / this.map.TileWidth).Ceil();
            var maxY = Math.Min(this.map.Height, frust.Bottom / this.map.TileHeight).Ceil();
            for (var x = minX; x < maxX; x++) {
                for (var y = minY; y < maxY; y++) {
                    var info = this.drawInfos[layerIndex, x, y];
                    if (info != null)
                        info.Draw(batch, drawFunction);
                }
            }
        }

        public void UpdateAnimations(GameTime time) {
            foreach (var animation in this.animatedTiles)
                animation.Update(time);
        }

        public delegate float GetDepth(TiledMapTile tile, TiledMapTileLayer layer, int layerIndex, Point position);

        public delegate void DrawDelegate(SpriteBatch batch, TileDrawInfo info);

        public class TileDrawInfo : GenericDataHolder {

            public readonly IndividualTiledMapRenderer Renderer;
            public readonly TiledMapTile Tile;
            public readonly TiledMapTileset Tileset;
            public readonly TiledMapTilesetTile TilesetTile;
            public readonly Point Position;
            public readonly float Depth;

            public TileDrawInfo(IndividualTiledMapRenderer renderer, TiledMapTile tile, TiledMapTileset tileset, TiledMapTilesetTile tilesetTile, Point position, float depth) {
                this.Renderer = renderer;
                this.Tile = tile;
                this.Tileset = tileset;
                this.TilesetTile = tilesetTile;
                this.Position = position;
                this.Depth = depth;
            }

            public void Draw(SpriteBatch batch, DrawDelegate drawFunction) {
                if (drawFunction == null) {
                    var region = this.Tileset.GetTextureRegion(this.TilesetTile);
                    var effects = this.Tile.GetSpriteEffects();
                    var drawPos = new Vector2(this.Position.X * this.Renderer.map.TileWidth, this.Position.Y * this.Renderer.map.TileHeight);
                    batch.Draw(this.Tileset.Texture, drawPos, region, Color.White, 0, Vector2.Zero, 1, effects, this.Depth);
                } else {
                    drawFunction(batch, this);
                }
            }

        }

    }
}