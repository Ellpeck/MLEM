using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using ColorHelper = MLEM.Extensions.ColorHelper;

namespace MLEM.Extended.Tiled {
    /// <summary>
    /// A set of extensions for dealing with MonoGame.Extended tiled maps
    /// </summary>
    public static class TiledExtensions {

        private static readonly Dictionary<int, TiledMapTilesetTile> StubTilesetTiles = new Dictionary<int, TiledMapTilesetTile>();

        /// <summary>
        /// Gets the property with the given key, or null if there is none.
        /// </summary>
        /// <param name="properties">The set of properties</param>
        /// <param name="key">The key by which to get a property</param>
        /// <returns>The property, or null if there is none</returns>
        public static string Get(this TiledMapProperties properties, string key) {
            properties.TryGetValue(key, out var val);
            return val;
        }

        /// <summary>
        /// Gets a boolean property with the given key, or null if there is none.
        /// </summary>
        /// <param name="properties">The set of properties</param>
        /// <param name="key">The key by which to get a property</param>
        /// <returns>The boolean property, or false if there is none</returns>
        public static bool GetBool(this TiledMapProperties properties, string key) {
            bool.TryParse(properties.Get(key), out var val);
            return val;
        }

        /// <summary>
        /// Gets a Color property with the given key, or null if there is none.
        /// </summary>
        /// <param name="properties">The set of properties</param>
        /// <param name="key">The key by which to get a property</param>
        /// <returns>The color property</returns>
        public static Color GetColor(this TiledMapProperties properties, string key) {
            return ColorHelper.FromHexString(properties.Get(key));
        }

        /// <summary>
        /// Gets a float property with the given key, or null if there is none.
        /// </summary>
        /// <param name="properties">The set of properties</param>
        /// <param name="key">The key by which to get a property</param>
        /// <returns>The float property, or 0 if there is none</returns>
        public static float GetFloat(this TiledMapProperties properties, string key) {
            float.TryParse(properties.Get(key), NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var val);
            return val;
        }

        /// <summary>
        /// Gets an int property with the given key, or null if there is none.
        /// </summary>
        /// <param name="properties">The set of properties</param>
        /// <param name="key">The key by which to get a property</param>
        /// <returns>The int property, or 0 if there is none</returns>
        public static int GetInt(this TiledMapProperties properties, string key) {
            int.TryParse(properties.Get(key), NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var val);
            return val;
        }

        /// <summary>
        /// Gets the tileset for the given map tile on the given map.
        /// </summary>
        /// <param name="tile">The tile</param>
        /// <param name="map">The map the tile is on</param>
        /// <returns>The tileset that the tile came from</returns>
        public static TiledMapTileset GetTileset(this TiledMapTile tile, TiledMap map) {
            return map.GetTilesetByTileGlobalIdentifier(tile.GlobalIdentifier);
        }

        /// <summary>
        /// Gets the local tile identifier for the given tiled map tile.
        /// The local tile identifier is the identifier within the tile's tileset.
        /// </summary>
        /// <param name="tile">The tile whose identifier to get</param>
        /// <param name="tileset">The tileset the tile is from</param>
        /// <param name="map">The map the tile is on</param>
        /// <returns>The local identifier</returns>
        public static int GetLocalIdentifier(this TiledMapTile tile, TiledMapTileset tileset, TiledMap map) {
            return tile.GlobalIdentifier - map.GetTilesetFirstGlobalIdentifier(tileset);
        }

        /// <summary>
        /// Gets the global tile identifier for the given tiled map tileset tile.
        /// The global tile identifier is the identifier within all of the tile sets that the map has.
        /// </summary>
        /// <param name="tile">The tile whose global identifier to get</param>
        /// <param name="tileset">The tileset the tile is from</param>
        /// <param name="map">The map the tile is on</param>
        /// <returns>The global identifier</returns>
        public static int GetGlobalIdentifier(this TiledMapTilesetTile tile, TiledMapTileset tileset, TiledMap map) {
            return map.GetTilesetFirstGlobalIdentifier(tileset) + tile.LocalTileIdentifier;
        }

        /// <summary>
        /// Gets the tileset tile on the given tileset for the given tile.
        /// </summary>
        /// <param name="tileset">The tileset</param>
        /// <param name="tile">The tile</param>
        /// <param name="map">The map the tile is on</param>
        /// <param name="createStub">If a tileset tile has no special properties, there is no pre-made object for it. If this boolean is true, a stub object with no extra data will be created instead of returning null.</param>
        /// <returns>null if the tile is blank or the tileset tile if there is one or createStub is true</returns>
        public static TiledMapTilesetTile GetTilesetTile(this TiledMapTileset tileset, TiledMapTile tile, TiledMap map, bool createStub = true) {
            if (tile.IsBlank)
                return null;
            var localId = tile.GetLocalIdentifier(tileset, map);
            return tileset.GetTilesetTile(localId, createStub);
        }

        /// <summary>
        /// Gets the tileset tile on the given tileset for the given tile.
        /// If the tileset is already known, you should use <see cref="GetTilesetTile(MonoGame.Extended.Tiled.TiledMapTileset,MonoGame.Extended.Tiled.TiledMapTile,MonoGame.Extended.Tiled.TiledMap,bool)"/> instead for performance.
        /// </summary>
        /// <param name="tile">The tile</param>
        /// <param name="map">The map the tile is on</param>
        /// <param name="createStub">If a tileset tile has no special properties, there is no pre-made object for it. If this boolean is true, a stub object with no extra data will be created instead of returning null.</param>
        /// <returns>null if the tile is blank or the tileset tile if there is one or createStub is true</returns>
        public static TiledMapTilesetTile GetTilesetTile(this TiledMapTile tile, TiledMap map, bool createStub = true) {
            if (tile.IsBlank)
                return null;
            var tileset = tile.GetTileset(map);
            return tileset.GetTilesetTile(tile, map, createStub);
        }

        /// <summary>
        /// Gets the tileset tile on the given tileset for the given local id.
        /// </summary>
        /// <param name="tileset">The tileset</param>
        /// <param name="localId">The tile's local id</param>
        /// <param name="createStub">If a tileset tile has no special properties, there is no pre-made object for it. If this boolean is true, a stub object with no extra data will be created instead of returning null.</param>
        /// <returns>null if the tile is blank or the tileset tile if there is one or createStub is true</returns>
        public static TiledMapTilesetTile GetTilesetTile(this TiledMapTileset tileset, int localId, bool createStub = true) {
            var tilesetTile = tileset.Tiles.FirstOrDefault(t => t.LocalTileIdentifier == localId);
            if (tilesetTile == null && createStub) {
                if (!StubTilesetTiles.TryGetValue(localId, out tilesetTile)) {
                    tilesetTile = new TiledMapTilesetTile(localId);
                    StubTilesetTiles.Add(localId, tilesetTile);
                }
            }
            return tilesetTile;
        }

        /// <summary>
        /// Gets the layer index of the layer with the given name in the <see cref="TiledMap.Layers"/> array.
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="layerName">The name of the layer</param>
        /// <returns>The resulting index</returns>
        public static int GetTileLayerIndex(this TiledMap map, string layerName) {
            var layer = map.GetLayer<TiledMapTileLayer>(layerName);
            return map.TileLayers.IndexOf(layer);
        }

        /// <summary>
        /// Returns the tiled map tile at the given location on the layer with the given name.
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="layerName">The name of the layer the tile is on</param>
        /// <param name="x">The x coordinate of the tile</param>
        /// <param name="y">The y coordinate of the tile</param>
        /// <returns>The tile at the given location, or default if the layer does not exist</returns>
        public static TiledMapTile GetTile(this TiledMap map, string layerName, int x, int y) {
            var layer = map.GetLayer<TiledMapTileLayer>(layerName);
            return layer != null ? layer.GetTile(x, y) : default;
        }

        /// <summary>
        /// Returns the tiled map tile at the given location on the layer with the given name.
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="pos">The layer position to get the tile at</param>
        /// <returns>The tile at the given location, or default if the layer does not exist</returns>
        public static TiledMapTile GetTile(this TiledMap map, LayerPosition pos) {
            return map.GetTile(pos.Layer, pos.X, pos.Y);
        }

        /// <summary>
        /// Sets the tiled map tile at the given location to the given global tile identifier.
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="layerName">The name of the layer</param>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <param name="globalTile">The tile's global identifier to set</param>
        public static void SetTile(this TiledMap map, string layerName, int x, int y, int globalTile) {
            var layer = map.GetLayer<TiledMapTileLayer>(layerName);
            if (layer != null)
                layer.SetTile((ushort) x, (ushort) y, (uint) globalTile);
        }

        /// <summary>
        /// Sets the tiled map tile at the given location to the given tile from the given tileset.
        /// If the passed <paramref name="tileset"/> or <paramref name="tile"/> is null, the tile at the location is removed instead.
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="layerName">The name of the layer</param>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <param name="tileset">The tileset to use, or null to remove the tile</param>
        /// <param name="tile">The tile to place, from the given tileset, or null to remove the tile</param>
        public static void SetTile(this TiledMap map, string layerName, int x, int y, TiledMapTileset tileset, TiledMapTilesetTile tile) {
            map.SetTile(layerName, x, y, tileset != null && tile != null ? tile.GetGlobalIdentifier(tileset, map) : 0);
        }

        /// <summary>
        /// Sets the tiled map tile at the given location to the given global tile identifier.
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="pos">The layer position</param>
        /// <param name="globalTile">The tile's global identifier to set</param>
        public static void SetTile(this TiledMap map, LayerPosition pos, int globalTile) {
            map.SetTile(pos.Layer, pos.X, pos.Y, globalTile);
        }

        /// <summary>
        /// Sets the tiled map tile at the given location to the given tile from the given tileset.
        /// If the passed <paramref name="tileset"/> or <paramref name="tile"/> is null, the tile at the location is removed instead.
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="pos">The layer position</param>
        /// <param name="tileset">The tileset to use, or null to remove the tile</param>
        /// <param name="tile">The tile to place, from the given tileset, or null to remove the tile</param>
        public static void SetTile(this TiledMap map, LayerPosition pos, TiledMapTileset tileset, TiledMapTilesetTile tile) {
            map.SetTile(pos.Layer, pos.X, pos.Y, tileset, tile);
        }

        /// <summary>
        /// For an x and y coordinate, returns an enumerable of all of the tiles on each of the map's <see cref="TiledMap.TileLayers"/>.
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns>All of the tiles on the map at the given location</returns>
        public static IEnumerable<TiledMapTile> GetTiles(this TiledMap map, int x, int y) {
            foreach (var layer in map.TileLayers) {
                var tile = layer.GetTile(x, y);
                if (!tile.IsBlank)
                    yield return tile;
            }
        }

        /// <summary>
        /// Returns the tiled map at the given location on the given layer
        /// </summary>
        /// <param name="layer">The layer to get the tile from</param>
        /// <param name="x">The tile's x coordinate</param>
        /// <param name="y">The tile's y coordinate</param>
        /// <returns>The tiled map tile at the location, or default if the location is out of bounds</returns>
        public static TiledMapTile GetTile(this TiledMapTileLayer layer, int x, int y) {
            return !layer.IsInBounds(x, y) ? default : layer.GetTile((ushort) x, (ushort) y);
        }

        /// <summary>
        /// Returns the area that a tiled map object covers.
        /// The area returned is in percent, meaning that an area that covers a full tile has a size of 1,1.
        /// </summary>
        /// <param name="obj">The object whose area to get</param>
        /// <param name="map">The map</param>
        /// <param name="position">The position to add to the object's position</param>
        /// <returns>The area that the tile covers</returns>
        public static RectangleF GetArea(this TiledMapObject obj, TiledMap map, Vector2? position = null) {
            var tileSize = map.GetTileSize();
            var pos = position ?? Vector2.Zero;
            return new RectangleF(obj.Position / tileSize + pos, obj.Size / tileSize);
        }

        /// <summary>
        /// Returns the width and height of a tile on the given map, as a vector.
        /// </summary>
        /// <param name="map">The map</param>
        /// <returns>The width and height of a tile</returns>
        public static Vector2 GetTileSize(this TiledMap map) {
            return new Vector2(map.TileWidth, map.TileHeight);
        }

        /// <summary>
        /// Returns whether the given position is in the bounds of the layer (that is, if each coordinate is >= 0 and if they are both smaller than the layer's width and height).
        /// </summary>
        /// <param name="layer">The layer</param>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns>Whether the position is in bounds of the layer</returns>
        public static bool IsInBounds(this TiledMapTileLayer layer, int x, int y) {
            return x >= 0 && y >= 0 && x < layer.Width && y < layer.Height;
        }

        /// <summary>
        /// Returns all of the objects by the given name, or by the given type, in an object layer.
        /// </summary>
        /// <param name="layer">The layer whose objects to search</param>
        /// <param name="id">The name or type name of the objects to find</param>
        /// <param name="searchName">Whether to search object names</param>
        /// <param name="searchType">Whether to search object types</param>
        /// <returns>An enumerable of tiled map objects that match the search</returns>
        public static IEnumerable<TiledMapObject> GetObjects(this TiledMapObjectLayer layer, string id, bool searchName = true, bool searchType = false) {
            foreach (var obj in layer.Objects) {
                if (searchName && obj.Name == id || searchType && obj.Type == id)
                    yield return obj;
            }
        }

        /// <summary>
        /// Returns all of the objects by the given name, or by the given type, on the given map.
        /// </summary>
        /// <param name="map">The layer whose objects to search</param>
        /// <param name="id">The name or type name of the objects to find</param>
        /// <param name="searchName">Whether to search object names</param>
        /// <param name="searchType">Whether to search object types</param>
        /// <returns>An enumerable of tiled map objects that match the search</returns>
        public static IEnumerable<TiledMapObject> GetObjects(this TiledMap map, string id, bool searchName = true, bool searchType = false) {
            foreach (var layer in map.ObjectLayers) {
                foreach (var obj in layer.GetObjects(id, searchName, searchType))
                    yield return obj;
            }
        }

        /// <summary>
        /// Returns the texture region, as a rectangle, that the given tile uses for rendering.
        /// </summary>
        /// <param name="tileset">The tileset the tile is on</param>
        /// <param name="tile">The tile</param>
        /// <returns>The tile's texture region, in pixels.</returns>
        public static Rectangle GetTextureRegion(this TiledMapTileset tileset, TiledMapTilesetTile tile) {
            var id = tile.LocalTileIdentifier;
            if (tile is TiledMapTilesetAnimatedTile animated)
                id = animated.CurrentAnimationFrame.LocalTileIdentifier;
            return tileset.GetTileRegion(id);
        }

        /// <summary>
        /// Converts a tile's flip settings into <see cref="SpriteEffects"/>.
        /// </summary>
        /// <param name="tile">The tile whose flip settings to convert</param>
        /// <returns>The tile's flip settings as sprite effects</returns>
        public static SpriteEffects GetSpriteEffects(this TiledMapTile tile) {
            var flipping = SpriteEffects.None;
            if (tile.IsFlippedHorizontally)
                flipping |= SpriteEffects.FlipHorizontally;
            if (tile.IsFlippedVertically)
                flipping |= SpriteEffects.FlipVertically;
            return flipping;
        }

    }
}