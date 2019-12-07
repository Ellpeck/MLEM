using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Extensions;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;

namespace MLEM.Extended.Tiled {
    public class TiledMapCollisions {

        private TiledMap map;
        private TileCollisionInfo[,,] collisionInfos;

        public TiledMapCollisions(TiledMap map = null) {
            if (map != null)
                this.SetMap(map);
        }

        public void SetMap(TiledMap map) {
            this.map = map;
            this.collisionInfos = new TileCollisionInfo[map.Layers.Count, map.Width, map.Height];
            for (var i = 0; i < map.TileLayers.Count; i++) {
                for (var x = 0; x < map.Width; x++) {
                    for (var y = 0; y < map.Height; y++) {
                        this.UpdateCollisionInfo(i, x, y);
                    }
                }
            }
        }

        public void UpdateCollisionInfo(int layerIndex, int x, int y) {
            var layer = this.map.TileLayers[layerIndex];
            var tile = layer.GetTile(x, y);
            if (tile.IsBlank) {
                this.collisionInfos[layerIndex, x, y] = null;
                return;
            }
            var tilesetTile = tile.GetTilesetTile(this.map);
            if (tilesetTile == null) {
                this.collisionInfos[layerIndex, x, y] = null;
                return;
            }
            this.collisionInfos[layerIndex, x, y] = new TileCollisionInfo(this.map, new Vector2(x, y), tile, tilesetTile);
        }

        public IEnumerable<TileCollisionInfo> GetCollidingTiles(RectangleF area, Func<TileCollisionInfo, bool> included = null) {
            var inclusionFunc = included ?? (tile => tile.Collisions.Any(c => c.Intersects(area)));
            var minX = Math.Max(0, area.Left.Floor());
            var maxX = Math.Min(this.map.Width - 1, area.Right);
            var minY = Math.Max(0, area.Top.Floor());
            var maxY = Math.Min(this.map.Height - 1, area.Bottom);
            for (var i = 0; i < this.map.TileLayers.Count; i++) {
                for (var x = minX; x <= maxX; x++) {
                    for (var y = minY; y <= maxY; y++) {
                        var tile = this.collisionInfos[i, x, y];
                        if (tile == null)
                            continue;
                        if (inclusionFunc(tile))
                            yield return tile;
                    }
                }
            }
        }

        public bool IsColliding(RectangleF area, Func<TileCollisionInfo, bool> included = null) {
            return this.GetCollidingTiles(area, included).Any();
        }

        public class TileCollisionInfo {

            public readonly TiledMapTile Tile;
            public readonly TiledMapTilesetTile TilesetTile;
            public readonly ReadOnlyCollection<RectangleF> Collisions;

            public TileCollisionInfo(TiledMap map, Vector2 position, TiledMapTile tile, TiledMapTilesetTile tilesetTile) {
                this.TilesetTile = tilesetTile;
                this.Tile = tile;

                var collisions = new List<RectangleF>();
                foreach (var obj in tilesetTile.Objects)
                    collisions.Add(obj.GetArea(map, position));
                this.Collisions = collisions.AsReadOnly();
            }

        }

    }
}