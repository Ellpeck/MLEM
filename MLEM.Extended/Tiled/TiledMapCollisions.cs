using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Extended.Extensions;
using MLEM.Extensions;
using MLEM.Misc;
using MonoGame.Extended.Tiled;
using RectangleF = MonoGame.Extended.RectangleF;

namespace MLEM.Extended.Tiled {
    /// <summary>
    /// A collision handler for a MonoGame.Extended tiled tile map.
    /// The idea behind this collision handler is that, on the map's tileset, each tile is assigned a certain rectangular area. That area is converted into a collision map that is dealt with in tile units, where each tile's covered area is 1x1 units big.
    /// </summary>
    public class TiledMapCollisions {

        private TiledMap map;
        private TileCollisionInfo[,,] collisionInfos;
        private CollectCollisions collisionFunction;

        /// <summary>
        /// Creates a new tiled map collision handler for the given map
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="collisionFunction">The function used to collect the collision info of a tile, or null to use <see cref="DefaultCollectCollisions"/></param>
        public TiledMapCollisions(TiledMap map = null, CollectCollisions collisionFunction = null) {
            if (map != null)
                this.SetMap(map, collisionFunction);
        }

        /// <summary>
        /// Sets this collision handler's handled map
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="collisionFunction">The function used to collect the collision info of a tile, or null to use <see cref="DefaultCollectCollisions"/></param>
        public void SetMap(TiledMap map, CollectCollisions collisionFunction = null) {
            this.map = map;
            this.collisionFunction = collisionFunction ?? TiledMapCollisions.DefaultCollectCollisions;
            this.collisionInfos = new TileCollisionInfo[map.Layers.Count, map.Width, map.Height];
            for (var i = 0; i < map.TileLayers.Count; i++) {
                for (var x = 0; x < map.Width; x++) {
                    for (var y = 0; y < map.Height; y++) {
                        this.UpdateCollisionInfo(i, x, y);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the collision info for the tile at the given position.
        /// </summary>
        /// <param name="layerIndex">The index of the tile's layer in <see cref="TiledMap.TileLayers"/></param>
        /// <param name="x">The tile's x coordinate</param>
        /// <param name="y">The tile's y coordinate</param>
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

        /// <summary>
        /// Returns an enumerable of tile collision infos that intersect the given area.
        /// Optionally, a predicate can be supplied that narrows the search.
        /// </summary>
        /// <param name="area">The area to check for collisions in</param>
        /// <param name="included">A function that determines if a certain info should be included or not</param>
        /// <returns>An enumerable of collision infos for that area</returns>
        public IEnumerable<TileCollisionInfo> GetCollidingTiles(RectangleF area, Func<TileCollisionInfo, bool> included = null) {
            bool DefaultInclusion(TileCollisionInfo tile) {
                foreach (var c in tile.Collisions) {
                    if (c.Intersects(area))
                        return true;
                }
                return false;
            }

            var inclusionFunc = included ?? DefaultInclusion;
            var minX = Math.Max(0, area.Left.Floor());
            var maxX = Math.Min(this.map.Width - 1, area.Right.Floor());
            var minY = Math.Max(0, area.Top.Floor());
            var maxY = Math.Min(this.map.Height - 1, area.Bottom.Floor());
            for (var i = 0; i < this.map.TileLayers.Count; i++) {
                for (var y = maxY; y >= minY; y--) {
                    for (var x = minX; x <= maxX; x++) {
                        var tile = this.collisionInfos[i, x, y];
                        if (tile == null)
                            continue;
                        if (inclusionFunc(tile))
                            yield return tile;
                    }
                }
            }
        }

        /// <summary>
        /// Returns whether there are any colliding tiles in the given area.
        /// Optionally, a predicate can be supplied that narrows the search.
        /// </summary>
        /// <param name="area">The area to check for collisions in</param>
        /// <param name="included">A function that determines if a certain info should be included or not</param>
        /// <returns>True if there are any colliders in the area, false otherwise</returns>
        public bool IsColliding(RectangleF area, Func<TileCollisionInfo, bool> included = null) {
            return this.GetCollidingTiles(area, included).Any();
        }

        /// <summary>
        /// Returns an enumerable of all of the <see cref="TileCollisionInfo.Collisions"/> of the colliding tiles in the given area.
        /// This method is a convenience method based on <see cref="GetCollidingTiles"/>.
        /// </summary>
        /// <param name="area">The area to check for collisions in</param>
        /// <param name="included">A function that determines if a certain info should be included or not</param>
        /// <returns>An enumerable of collision rectangles for that area</returns>
        public IEnumerable<RectangleF> GetCollidingAreas(RectangleF area, Func<TileCollisionInfo, bool> included = null) {
            foreach (var tile in this.GetCollidingTiles(area, included)) {
                foreach (var col in tile.Collisions)
                    yield return col;
            }
        }

        /// <summary>
        /// Returns an enumerable of normals and penetration amounts for each <see cref="TileCollisionInfo"/> that intersects with the given <see cref="RectangleF"/> area.
        /// The normals and penetration amounts are based on <see cref="MLEM.Extensions.NumberExtensions.Penetrate"/>.
        /// Note that all x penetrations are returned before all y penetrations, which improves collision detection in sidescrolling games with gravity. Note that this behavior can be inverted using <paramref name="prioritizeX"/>.
        /// </summary>
        /// <param name="getArea">The area to penetrate</param>
        /// <param name="included">A function that determines if a certain info should be included or not</param>
        /// <param name="prioritizeX">Whether all x penetrations should be prioritized (returned first). If this is false, all y penetrations are prioritized instead.</param>
        /// <returns>A set of normals and penetration amounts</returns>
        public IEnumerable<(Vector2, float)> GetPenetrations(Func<RectangleF> getArea, Func<TileCollisionInfo, bool> included = null, bool prioritizeX = true) {
            foreach (var col in this.GetCollidingAreas(getArea(), included)) {
                if (getArea().Penetrate(col, out var normal, out var penetration) && normal.X != 0 == prioritizeX)
                    yield return (normal, penetration);
            }
            foreach (var col in this.GetCollidingAreas(getArea(), included)) {
                if (getArea().Penetrate(col, out var normal, out var penetration) && normal.X == 0 == prioritizeX)
                    yield return (normal, penetration);
            }
        }

        /// <summary>
        /// The default implementation of <see cref="CollectCollisions"/> which is used by <see cref="SetMap"/> if no custom collision collection function is passed
        /// </summary>
        /// <param name="collisions">The list of collisions to add to</param>
        /// <param name="tile">The tile's collision information</param>
        public static void DefaultCollectCollisions(List<RectangleF> collisions, TileCollisionInfo tile) {
            foreach (var obj in tile.TilesetTile.Objects)
                collisions.Add(obj.GetArea(tile.Map, tile.Position, tile.Tile.Flags));
        }

        /// <summary>
        /// A delegate method used to override the default collision checking behavior.
        /// </summary>
        /// <param name="collisions">The list of collisions to add to</param>
        /// <param name="tile">The tile's collision information</param>
        public delegate void CollectCollisions(List<RectangleF> collisions, TileCollisionInfo tile);

        /// <summary>
        /// A tile collision info stores information about a tile at a given location on a given layer, including its objects and their bounds.
        /// </summary>
        public class TileCollisionInfo : GenericDataHolder {

            /// <summary>
            /// The map the tile is on
            /// </summary>
            public readonly TiledMap Map;
            /// <summary>
            /// The position of the tile, in tile units
            /// </summary>
            public readonly Vector2 Position;
            /// <summary>
            /// The tiled map tile
            /// </summary>
            public readonly TiledMapTile Tile;
            /// <summary>
            /// The layer that this tile is on
            /// </summary>
            public readonly TiledMapTileLayer Layer;
            /// <summary>
            /// The tileset tile for this tile
            /// </summary>
            public readonly TiledMapTilesetTile TilesetTile;
            /// <summary>
            /// The list of colliders for this tile
            /// </summary>
            public readonly List<RectangleF> Collisions;

            internal TileCollisionInfo(TiledMap map, Vector2 position, TiledMapTile tile, TiledMapTileLayer layer, TiledMapTilesetTile tilesetTile, CollectCollisions collisionFunction) {
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
