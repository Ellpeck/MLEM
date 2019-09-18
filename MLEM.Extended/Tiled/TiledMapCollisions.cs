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
            if (this.map == map)
                return;
            this.map = map;

            this.collisionInfos = new TileCollisionInfo[map.Layers.Count, map.Width, map.Height];
            for (var i = 0; i < map.TileLayers.Count; i++) {
                var layer = map.TileLayers[i];
                for (var x = 0; x < map.Width; x++) {
                    for (var y = 0; y < map.Height; y++) {
                        var tile = layer.GetTile((ushort) x, (ushort) y);
                        if (tile.IsBlank)
                            continue;
                        var tilesetTile = tile.GetTilesetTile(map);
                        if (tilesetTile == null)
                            continue;
                        this.collisionInfos[i, x, y] = new TileCollisionInfo(map, new Vector2(x, y), tile, tilesetTile);
                    }
                }
            }
        }

        public IEnumerable<TileCollisionInfo> GetCollidingTiles(RectangleF area, Func<TileCollisionInfo, bool> included = null) {
            var inclusionFunc = included ?? (tile => tile.Collisions.Any(c => c.Intersects(area)));
            for (var i = 0; i < this.map.TileLayers.Count; i++) {
                for (var x = area.Left.Floor(); x <= area.Right; x++) {
                    for (var y = area.Top.Floor(); y <= area.Bottom; y++) {
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