using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;

namespace MLEM.Extended.Tiled {
    public static class TiledExtensions {

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

        public static TiledMapTilesetTile GetTilesetTile(this TiledMapTileset tileset, TiledMapTile tile, TiledMap map) {
            var localId = tile.GetLocalIdentifier(tileset, map);
            return tileset.Tiles.FirstOrDefault(t => t.LocalTileIdentifier == localId);
        }

        public static TiledMapTilesetTile GetTilesetTile(this TiledMapTile tile, TiledMap map) {
            var tileset = tile.GetTileset(map);
            return tileset.GetTilesetTile(tile, map);
        }

        public static TiledMapTile GetTile(this TiledMap map, string layerName, int x, int y) {
            var layer = map.GetLayer<TiledMapTileLayer>(layerName);
            return layer != null ? layer.GetTile((ushort) x, (ushort) y) : new TiledMapTile(0, (ushort) x, (ushort) y);
        }

        public static RectangleF GetArea(this TiledMapObject obj, TiledMap map, Vector2 position) {
            var tileSize = new Vector2(map.TileWidth, map.TileHeight);
            return new RectangleF(obj.Position / tileSize + position, new Size2(obj.Size.Width, obj.Size.Height) / tileSize);
        }

    }
}