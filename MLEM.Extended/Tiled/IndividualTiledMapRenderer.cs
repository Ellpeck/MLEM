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
        private GetDepth depthFunction;

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
        }

        public void UpdateDrawInfo(int layerIndex, int x, int y) {
            var layer = this.map.TileLayers[layerIndex];
            var tile = layer.GetTile(x, y);
            if (tile.IsBlank) {
                this.drawInfos[layerIndex, x, y] = null;
                return;
            }
            var tileset = tile.GetTileset(this.map);
            var source = tileset.GetTileRegion(tile.GetLocalIdentifier(tileset, this.map));
            var pos = new Point(x, y);
            var depth = this.depthFunction(tile, layer, layerIndex, pos);
            this.drawInfos[layerIndex, x, y] = new TileDrawInfo(this, tileset, pos, source, depth);
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
            private readonly TiledMapTileset tileset;
            private readonly Point position;
            private readonly Rectangle source;
            private readonly float depth;

            public TileDrawInfo(IndividualTiledMapRenderer renderer, TiledMapTileset tileset, Point position, Rectangle source, float depth) {
                this.renderer = renderer;
                this.tileset = tileset;
                this.position = position;
                this.source = source;
                this.depth = depth;
            }

            public void Draw(SpriteBatch batch) {
                var drawPos = new Vector2(this.position.X * this.renderer.map.TileWidth, this.position.Y * this.renderer.map.TileHeight);
                batch.Draw(this.tileset.Texture, drawPos, this.source, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, this.depth);
            }

        }

    }
}