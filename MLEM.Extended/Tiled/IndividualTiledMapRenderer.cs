using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;

namespace MLEM.Extended.Tiled {
    public class IndividualTiledMapRenderer {

        private TiledMap map;
        private TileDrawInfo[,,] drawInfos;
        public GetDepth DepthFunction = (tile, layer, layerIndex, position) => 0;

        public IndividualTiledMapRenderer(TiledMap map = null) {
            if (map != null)
                this.SetMap(map);
        }

        public void SetMap(TiledMap map) {
            if (this.map == map)
                return;
            this.map = map;

            this.drawInfos = new TileDrawInfo[map.TileLayers.Count, map.Width, map.Height];
            for (var i = 0; i < map.TileLayers.Count; i++) {
                var layer = map.TileLayers[i];
                for (var x = 0; x < map.Width; x++) {
                    for (var y = 0; y < map.Height; y++) {
                        var tile = layer.GetTile((ushort) x, (ushort) y);
                        if (tile.IsBlank)
                            continue;
                        var tileset = tile.GetTileset(map);
                        var source = tileset.GetTileRegion(tile.GetLocalIdentifier(tileset, map));
                        this.drawInfos[i, x, y] = new TileDrawInfo(this, tile, layer, i, tileset, new Point(x, y), source);
                    }
                }
            }
        }

        public void Draw(SpriteBatch batch, RectangleF? frustum = null) {
            for (var i = 0; i < this.map.TileLayers.Count; i++) {
                this.DrawLayer(batch, i, frustum);
            }
        }

        public void DrawLayer(SpriteBatch batch, int layerIndex, RectangleF? frustum = null) {
            var frust = frustum ?? new RectangleF(0, 0, float.MaxValue, float.MaxValue);
            var minX = Math.Max(0, frust.Left / this.map.TileWidth).Floor();
            var minY = Math.Max(0, frust.Top / this.map.TileHeight).Floor();
            var maxX = Math.Min(this.map.Width, frust.Right / this.map.TileWidth).Ceil();
            var maxY = Math.Min(this.map.Height, frust.Bottom / this.map.TileHeight).Ceil();
            for (var x = minX; x < maxX; x++) {
                for (var y = minY; y < maxY; y++) {
                    var info = this.drawInfos[layerIndex, x, y];
                    if (info != null)
                        info.Draw(batch);
                }
            }
        }

        public delegate float GetDepth(TiledMapTile tile, TiledMapTileLayer layer, int layerIndex, Point position);

        private class TileDrawInfo {

            private readonly IndividualTiledMapRenderer renderer;
            private readonly TiledMapTile tile;
            private readonly TiledMapTileLayer layer;
            private readonly int layerIndex;
            private readonly TiledMapTileset tileset;
            private readonly Point position;
            private readonly Rectangle source;

            public TileDrawInfo(IndividualTiledMapRenderer renderer, TiledMapTile tile, TiledMapTileLayer layer, int layerIndex, TiledMapTileset tileset, Point position, Rectangle source) {
                this.renderer = renderer;
                this.tile = tile;
                this.layer = layer;
                this.layerIndex = layerIndex;
                this.tileset = tileset;
                this.position = position;
                this.source = source;
            }

            public void Draw(SpriteBatch batch) {
                var drawPos = new Vector2(this.position.X * this.renderer.map.TileWidth, this.position.Y * this.renderer.map.TileHeight);
                var depth = this.renderer.DepthFunction(this.tile, this.layer, this.layerIndex, this.position);
                batch.Draw(this.tileset.Texture, drawPos, this.source, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, depth);
            }

        }

    }
}