using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;

namespace MLEM.Extended.Tiled {
    public static class TiledExtensions {

        private static readonly Dictionary<int, TiledMapTilesetTile> StubTilesetTiles = new Dictionary<int, TiledMapTilesetTile>();

        public static string Get(this TiledMapProperties properties, string key) {
            properties.TryGetValue(key, out var val);
            return val;
        }

        public static bool GetBool(this TiledMapProperties properties, string key) {
            bool.TryParse(properties.Get(key), out var val);
            return val;
        }

        public static Color GetColor(this TiledMapProperties properties, string key) {
            return ColorHelper.FromHex(properties.Get(key));
        }

        public static float GetFloat(this TiledMapProperties properties, string key) {
            float.TryParse(properties.Get(key), out var val);
            return val;
        }

        public static int GetInt(this TiledMapProperties properties, string key) {
            int.TryParse(properties.Get(key), out var val);
            return val;
        }

        public static TiledMapTileset GetTileset(this TiledMapTile tile, TiledMap map) {
            return map.GetTilesetByTileGlobalIdentifier(tile.GlobalIdentifier);
        }

        public static int GetLocalIdentifier(this TiledMapTile tile, TiledMapTileset tileset, TiledMap map) {
            return tile.GlobalIdentifier - map.GetTilesetFirstGlobalIdentifier(tileset);
        }

        public static TiledMapTilesetTile GetTilesetTile(this TiledMapTileset tileset, TiledMapTile tile, TiledMap map, bool createStub = true) {
            if (tile.IsBlank)
                return null;
            var localId = tile.GetLocalIdentifier(tileset, map);
            var tilesetTile = tileset.Tiles.FirstOrDefault(t => t.LocalTileIdentifier == localId);
            if (tilesetTile == null && createStub) {
                var id = tile.GetLocalIdentifier(tileset, map);
                if (!StubTilesetTiles.TryGetValue(id, out tilesetTile)) {
                    tilesetTile = new TiledMapTilesetTile(id);
                    StubTilesetTiles.Add(id, tilesetTile);
                }
            }
            return tilesetTile;
        }

        public static TiledMapTilesetTile GetTilesetTile(this TiledMapTile tile, TiledMap map, bool createStub = true) {
            if (tile.IsBlank)
                return null;
            var tileset = tile.GetTileset(map);
            return tileset.GetTilesetTile(tile, map, createStub);
        }

        public static int GetTileLayerIndex(this TiledMap map, string layerName) {
            var layer = map.GetLayer<TiledMapTileLayer>(layerName);
            return map.TileLayers.IndexOf(layer);
        }

        public static TiledMapTile GetTile(this TiledMap map, string layerName, int x, int y) {
            var layer = map.GetLayer<TiledMapTileLayer>(layerName);
            return layer != null ? layer.GetTile(x, y) : default;
        }

        public static TiledMapTile GetTile(this TiledMapTileLayer layer, int x, int y) {
            return !layer.IsInBounds(x, y) ? default : layer.GetTile((ushort) x, (ushort) y);
        }

        public static RectangleF GetArea(this TiledMapObject obj, TiledMap map, Vector2? position = null) {
            var tileSize = map.GetTileSize();
            var pos = position ?? Vector2.Zero;
            return new RectangleF(obj.Position / tileSize + pos, obj.Size / tileSize);
        }

        public static Vector2 GetTileSize(this TiledMap map) {
            return new Vector2(map.TileWidth, map.TileHeight);
        }

        public static bool IsInBounds(this TiledMapTileLayer layer, int x, int y) {
            return x >= 0 && y >= 0 && x < layer.Width && y < layer.Height;
        }

    }
}