using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Extensions;
using MLEM.Misc;
using MonoGame.Extended.Tiled;
using RectangleF = MonoGame.Extended.RectangleF;

namespace MLEM.Extended.Tiled {
    public class TiledMapCollisions {

        private TiledMap map;
        private TileCollisionInfo[,,] collisionInfos;
        private CollectCollisions collisionFunction;

        public TiledMapCollisions(TiledMap map = null) {
            if (map != null)
                this.SetMap(map);
        }

        public void SetMap(TiledMap map, CollectCollisions collisionFunction = null) {
            this.map = map;
            this.collisionFunction = collisionFunction ?? ((collisions, tile) => {
                foreach (var obj in tile.TilesetTile.Objects) {
                    var area = obj.GetArea(tile.Map);
                    if (tile.Tile.IsFlippedHorizontally)
                        area.X = 1 - area.X - area.Width;
                    if (tile.Tile.IsFlippedVertically)
                        area.Y = 1 - area.Y - area.Height;
                    area.Offset(tile.Position);
                    collisions.Add(area);
                }
            });

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
            this.collisionInfos[layerIndex, x, y] = new TileCollisionInfo(this.map, new Vector2(x, y), tile, layer, tilesetTile, this.collisionFunction);
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

        public delegate void CollectCollisions(List<RectangleF> collisions, TileCollisionInfo tile);

        public class TileCollisionInfo : GenericDataHolder {

            public readonly TiledMap Map;
            public readonly Vector2 Position;
            public readonly TiledMapTile Tile;
            public readonly TiledMapTileLayer Layer;
            public readonly TiledMapTilesetTile TilesetTile;
            public readonly List<RectangleF> Collisions;

            public TileCollisionInfo(TiledMap map, Vector2 position, TiledMapTile tile, TiledMapTileLayer layer, TiledMapTilesetTile tilesetTile, CollectCollisions collisionFunction) {
                this.TilesetTile = tilesetTile;
                this.Layer = layer;
                this.Tile = tile;
                this.Map = map;
                this.Position = position;

                this.Collisions = new List<RectangleF>();
                collisionFunction(this.Collisions, this);
            }

        }

    }
}