using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Cameras;
using MLEM.Extensions;
using MLEM.Misc;
using MonoGame.Extended.Tiled;
using RectangleF = MonoGame.Extended.RectangleF;

namespace MLEM.Extended.Tiled {
    /// <summary>
    /// A tiled map renderer that renders each tile individually, while optionally supplying a depth for each tile.
    /// Rendering in this manner allows for entities to be behind or in front of buildings based on their y height.
    /// </summary>
    public class IndividualTiledMapRenderer {

        private TiledMap map;
        private TileDrawInfo[,,] drawInfos;
        private GetDepth depthFunction;
        private List<TiledMapTilesetAnimatedTile> animatedTiles;

        /// <summary>
        /// Creates a new individual tiled map renderer using the given map and depth function
        /// </summary>
        /// <param name="map">The map to use</param>
        /// <param name="depthFunction">The depth function to use. Defaults to a function that assigns a depth of 0 to every tile.</param>
        public IndividualTiledMapRenderer(TiledMap map = null, GetDepth depthFunction = null) {
            if (map != null)
                this.SetMap(map, depthFunction);
        }

        /// <summary>
        /// Sets this individual tiled map renderer's map and depth function
        /// </summary>
        /// <param name="map">The map to use</param>
        /// <param name="depthFunction">The depth function to use. Defaults to a function that assigns a depth of 0 to every tile.</param>
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

        /// <summary>
        /// Updates the rendering information for the tile in the given layer, x and y.
        /// </summary>
        /// <param name="layerIndex">The index of the layer in <see cref="TiledMap.TileLayers"/></param>
        /// <param name="x">The x coordinate of the tile</param>
        /// <param name="y">The y coordinate of the tile</param>
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

        /// <summary>
        /// Draws this individual tiled map renderer.
        /// Optionally, a frustum can be supplied that determines which positions, in pixel space, are visible at this time. <see cref="Camera"/> provides <see cref="Camera.GetVisibleRectangle"/> for this purpose.
        /// </summary>
        /// <param name="batch">The sprite batch to use</param>
        /// <param name="frustum">The area that is visible, in pixel space.</param>
        /// <param name="drawFunction">The draw function to use, or null to use <see cref="DefaultDraw"/></param>
        public void Draw(SpriteBatch batch, RectangleF? frustum = null, DrawDelegate drawFunction = null) {
            for (var i = 0; i < this.map.TileLayers.Count; i++) {
                if (this.map.TileLayers[i].IsVisible)
                    this.DrawLayer(batch, i, frustum, drawFunction);
            }
        }

        /// <summary>
        /// Draws the given layer of this individual tiled map renderer.
        /// Optionally, a frustum can be supplied that determines which positions, in pixel space, are visible at this time. <see cref="Camera"/> provides <see cref="Camera.GetVisibleRectangle"/> for this purpose.
        /// </summary>
        /// <param name="batch">The sprite batch to use</param>
        /// <param name="layerIndex">The index of the layer in <see cref="TiledMap.TileLayers"/></param>
        /// <param name="frustum">The area that is visible, in pixel space.</param>
        /// <param name="drawFunction">The draw function to use, or null to use <see cref="DefaultDraw"/></param>
        public void DrawLayer(SpriteBatch batch, int layerIndex, RectangleF? frustum = null, DrawDelegate drawFunction = null) {
            var draw = drawFunction ?? DefaultDraw;
            var frust = frustum ?? new RectangleF(0, 0, float.MaxValue, float.MaxValue);
            var minX = Math.Max(0, frust.Left / this.map.TileWidth).Floor();
            var minY = Math.Max(0, frust.Top / this.map.TileHeight).Floor();
            var maxX = Math.Min(this.map.Width, frust.Right / this.map.TileWidth).Ceil();
            var maxY = Math.Min(this.map.Height, frust.Bottom / this.map.TileHeight).Ceil();
            for (var x = minX; x < maxX; x++) {
                for (var y = minY; y < maxY; y++) {
                    var info = this.drawInfos[layerIndex, x, y];
                    if (info != null)
                        draw(batch, info);
                }
            }
        }

        /// <summary>
        /// Update all of the animated tiles in this individual tiled map renderer
        /// </summary>
        /// <param name="time">The game's time</param>
        public void UpdateAnimations(GameTime time) {
            foreach (var animation in this.animatedTiles)
                animation.Update(time);
        }

        /// <summary>
        /// The default implementation of <see cref="DrawDelegate"/> that is used by <see cref="SetMap"/> if no custom draw function is passed
        /// </summary>
        /// <param name="batch">The sprite batch to use for drawing</param>
        /// <param name="info">The <see cref="TileDrawInfo"/> to draw</param>
        public static void DefaultDraw(SpriteBatch batch, TileDrawInfo info) {
            var region = info.Tileset.GetTextureRegion(info.TilesetTile);
            var effects = info.Tile.GetSpriteEffects();
            var drawPos = new Vector2(info.Position.X * info.Renderer.map.TileWidth, info.Position.Y * info.Renderer.map.TileHeight);
            batch.Draw(info.Tileset.Texture, drawPos, region, Color.White, 0, Vector2.Zero, 1, effects, info.Depth);
        }

        /// <summary>
        /// A delegate method used for <see cref="IndividualTiledMapRenderer.depthFunction"/>.
        /// The idea is to return a depth (between 0 and 1) for the given tile that determines where in the sprite batch it should be rendererd.
        /// Note that, for this depth function to take effect, the sprite batch needs to begin with <see cref="SpriteSortMode.FrontToBack"/> or <see cref="SpriteSortMode.BackToFront"/>.
        /// </summary>
        /// <param name="tile">The tile whose depth to get</param>
        /// <param name="layer">The layer the tile is on</param>
        /// <param name="layerIndex">The index of the layer in <see cref="TiledMap.TileLayers"/></param>
        /// <param name="position">The tile position of this tile</param>
        public delegate float GetDepth(TiledMapTile tile, TiledMapTileLayer layer, int layerIndex, Point position);

        /// <summary>
        /// A delegate method used for drawing an <see cref="IndividualTiledMapRenderer"/>.
        /// </summary>
        /// <param name="batch">The sprite batch to use for drawing</param>
        /// <param name="info">The <see cref="TileDrawInfo"/> to draw</param>
        public delegate void DrawDelegate(SpriteBatch batch, TileDrawInfo info);

        /// <summary>
        /// A tile draw info contains information about a tile at a given map location.
        /// It caches a lot of data that is required for drawing a tile efficiently.
        /// </summary>
        public class TileDrawInfo : GenericDataHolder {

            /// <summary>
            /// The renderer used by this info
            /// </summary>
            public readonly IndividualTiledMapRenderer Renderer;
            /// <summary>
            /// The tiled map tile to draw
            /// </summary>
            public readonly TiledMapTile Tile;
            /// <summary>
            /// The tileset that <see cref="Tile"/> is on
            /// </summary>
            public readonly TiledMapTileset Tileset;
            /// <summary>
            /// The tileset tile that corresponds to <see cref="Tile"/>
            /// </summary>
            public readonly TiledMapTilesetTile TilesetTile;
            /// <summary>
            /// The position, in tile space, of <see cref="Tile"/>
            /// </summary>
            public readonly Point Position;
            /// <summary>
            /// The depth calculated by the depth function
            /// </summary>
            public readonly float Depth;

            internal TileDrawInfo(IndividualTiledMapRenderer renderer, TiledMapTile tile, TiledMapTileset tileset, TiledMapTilesetTile tilesetTile, Point position, float depth) {
                this.Renderer = renderer;
                this.Tile = tile;
                this.Tileset = tileset;
                this.TilesetTile = tilesetTile;
                this.Position = position;
                this.Depth = depth;
            }

        }

    }
}