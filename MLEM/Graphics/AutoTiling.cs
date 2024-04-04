using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Misc;
using MLEM.Textures;

namespace MLEM.Graphics {
    /// <summary>
    /// This class contains a <see cref="DrawAutoTile"/> method that allows users to easily draw a tile with automatic connections, as well as a more complex <see cref="DrawExtendedAutoTile(Microsoft.Xna.Framework.Graphics.SpriteBatch,Microsoft.Xna.Framework.Vector2,MLEM.Textures.TextureRegion,MLEM.Textures.TextureRegion,MLEM.Graphics.AutoTiling.ConnectsTo,Microsoft.Xna.Framework.Color,Microsoft.Xna.Framework.Color,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{Microsoft.Xna.Framework.Vector2},float,float)"/> method.
    /// Note that <see cref="StaticSpriteBatch"/> can also be used for drawing by using the <see cref="AddAutoTile"/> and <see cref="AddExtendedAutoTile(MLEM.Graphics.StaticSpriteBatch,Microsoft.Xna.Framework.Vector2,MLEM.Textures.TextureRegion,MLEM.Textures.TextureRegion,MLEM.Graphics.AutoTiling.ConnectsTo,Microsoft.Xna.Framework.Color,Microsoft.Xna.Framework.Color,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{Microsoft.Xna.Framework.Vector2},float,float,System.Collections.Generic.ICollection{MLEM.Graphics.StaticSpriteBatch.Item})"/> methods instead.
    /// </summary>
    public static class AutoTiling {

        /// <summary>
        /// This method allows for a tiled texture to be drawn in an auto-tiling mode.
        /// This allows, for example, a grass patch on a tilemap to have nice looking edges that transfer over into a path without any hard edges between tiles.
        ///
        /// For auto-tiling in this way to work, the tiles have to be laid out as follows: five tiles aligned horizontally within the texture file, with the following information:
        /// <list type="number">
        /// <item><description>The texture used for filling big areas</description></item>
        /// <item><description>The texture used for straight, horizontal borders, with the borders facing away from the center</description></item>
        /// <item><description>The texture used for outer corners, with the corners facing away from the center</description></item>
        /// <item><description>The texture used for straight, vertical borders, with the borders facing away from the center</description></item>
        /// <item><description>The texture used for inner corners, with the corners facing away from the center</description></item>
        /// </list>
        /// For more information and an example, see https://github.com/Ellpeck/MLEM/blob/main/Demos/AutoTilingDemo.cs and its source texture https://github.com/Ellpeck/MLEM/blob/main/Demos/Content/Textures/AutoTiling.png.
        /// </summary>
        /// <param name="batch">The sprite batch to use for drawing.</param>
        /// <param name="pos">The position to draw at.</param>
        /// <param name="texture">The texture to use for drawing, with the area set to the first texture region, as described in the summary.</param>
        /// <param name="connectsTo">A function that determines whether two positions should connect.</param>
        /// <param name="color">The color to draw with.</param>
        /// <param name="origin">The origin to draw from.</param>
        /// <param name="scale">The scale to draw with.</param>
        /// <param name="layerDepth">The layer depth to draw with.</param>
        public static void DrawAutoTile(SpriteBatch batch, Vector2 pos, TextureRegion texture, ConnectsTo connectsTo, Color color, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0) {
            var orig = origin ?? Vector2.Zero;
            var sc = scale ?? Vector2.One;
            var (p1, r1, p2, r2, p3, r3, p4, r4) = AutoTiling.CalculateAutoTile(pos, texture.Area, connectsTo, sc);
            batch.Draw(texture.Texture, p1, r1, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture.Texture, p2, r2, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture.Texture, p3, r3, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture.Texture, p4, r4, color, 0, orig, sc, SpriteEffects.None, layerDepth);
        }

        /// <inheritdoc cref="DrawAutoTile"/>
        public static void AddAutoTile(StaticSpriteBatch batch, Vector2 pos, TextureRegion texture, ConnectsTo connectsTo, Color color, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0, ICollection<StaticSpriteBatch.Item> items = null) {
            var orig = origin ?? Vector2.Zero;
            var sc = scale ?? Vector2.One;
            var (p1, r1, p2, r2, p3, r3, p4, r4) = AutoTiling.CalculateAutoTile(pos, texture.Area, connectsTo, sc);
            var a1 = batch.Add(texture.Texture, p1, r1, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            var a2 = batch.Add(texture.Texture, p2, r2, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            var a3 = batch.Add(texture.Texture, p3, r3, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            var a4 = batch.Add(texture.Texture, p4, r4, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            if (items != null) {
                items.Add(a1);
                items.Add(a2);
                items.Add(a3);
                items.Add(a4);
            }
        }

        /// <summary>
        /// This method allows for a tiled texture to be drawn in an auto-tiling mode.
        /// This allows, for example, a grass patch on a tilemap to have nice looking edges that transfer over into a path without any hard edges between tiles.
        ///
        /// This method is a more complex version of <see cref="DrawAutoTile"/> that overlays separate border textures on a background texture region, which also allows for non-rectangular texture areas to be used easily.
        /// For auto-tiling in this way to work, the overlay sections have to be laid out as follows: 16 sections aligned horizontally within the texture file, with the following information:
        /// <list type="number">
        /// <item><description>The texture used for straight, horizontal borders, with the borders facing away from the center, split up into four parts: top left, then top right, then bottom left, then bottom right</description></item>
        /// <item><description>The texture used for outer corners, with the corners facing away from the center, split up into four parts: top left, then top right, then bottom left, then bottom right</description></item>
        /// <item><description>The texture used for straight, vertical borders, with the borders facing away from the center, split up into four parts: top left, then top right, then bottom left, then bottom right</description></item>
        /// <item><description>The texture used for inner corners, with the corners facing away from the center, split up into four parts: top left, then top right, then bottom left, then bottom right</description></item>
        /// </list>
        /// For more information and an example, see https://github.com/Ellpeck/MLEM/blob/main/Demos/AutoTilingDemo.cs and its source texture https://github.com/Ellpeck/MLEM/blob/main/Demos/Content/Textures/AutoTiling.png.
        /// </summary>
        /// <param name="batch">The sprite batch to use for drawing.</param>
        /// <param name="pos">The position to draw at.</param>
        /// <param name="backgroundTexture">The background region, or null to skip drawing a background.</param>
        /// <param name="overlayTexture">The first overlay region, as described in the summary.</param>
        /// <param name="connectsTo">A function that determines whether two positions should connect.</param>
        /// <param name="backgroundColor">The color to draw the texture used for filling big areas with.</param>
        /// <param name="overlayColor">The color to draw border and corner textures with.</param>
        /// <param name="origin">The origin to draw from.</param>
        /// <param name="scale">The scale to draw with.</param>
        /// <param name="layerDepth">The layer depth to draw with.</param>
        /// <param name="overlayDepthOffset">An optional depth offset from <paramref name="layerDepth"/> that the overlay should be drawn with</param>
        public static void DrawExtendedAutoTile(SpriteBatch batch, Vector2 pos, TextureRegion backgroundTexture, TextureRegion overlayTexture, ConnectsTo connectsTo, Color backgroundColor, Color overlayColor, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0, float overlayDepthOffset = 0) {
            if (backgroundTexture != null)
                batch.Draw(backgroundTexture, pos, backgroundColor, 0, origin ?? Vector2.Zero, scale ?? Vector2.One, SpriteEffects.None, layerDepth);
            var od = layerDepth + overlayDepthOffset;
            AutoTiling.DrawExtendedAutoTileCorner(batch, pos, overlayTexture, connectsTo, overlayColor, Direction2.UpLeft, origin, scale, od);
            AutoTiling.DrawExtendedAutoTileCorner(batch, pos, overlayTexture, connectsTo, overlayColor, Direction2.UpRight, origin, scale, od);
            AutoTiling.DrawExtendedAutoTileCorner(batch, pos, overlayTexture, connectsTo, overlayColor, Direction2.DownLeft, origin, scale, od);
            AutoTiling.DrawExtendedAutoTileCorner(batch, pos, overlayTexture, connectsTo, overlayColor, Direction2.DownRight, origin, scale, od);
        }

        /// <summary>
        /// This method allows for a single corner of a tiled texture to be drawn in an auto-tiling mode.
        /// This allows, for example, a grass patch on a tilemap to have nice looking edges that transfer over into a path without any hard edges between tiles.
        ///
        /// For more information, and to draw all four corners at once, see <see cref="DrawExtendedAutoTile(Microsoft.Xna.Framework.Graphics.SpriteBatch,Microsoft.Xna.Framework.Vector2,MLEM.Textures.TextureRegion,MLEM.Textures.TextureRegion,MLEM.Graphics.AutoTiling.ConnectsTo,Microsoft.Xna.Framework.Color,Microsoft.Xna.Framework.Color,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{Microsoft.Xna.Framework.Vector2},float,float)"/>
        /// </summary>
        /// <param name="batch">The sprite batch to use for drawing.</param>
        /// <param name="pos">The position to draw at.</param>
        /// <param name="overlayTexture">The first overlay region, as described in the summary.</param>
        /// <param name="connectsTo">A function that determines whether two positions should connect.</param>
        /// <param name="overlayColor">The color to draw border and corner textures with.</param>
        /// <param name="corner">The corner of the auto-tile to draw. Can be <see cref="Direction2.UpLeft"/>, <see cref="Direction2.UpRight"/>, <see cref="Direction2.DownLeft"/> or <see cref="Direction2.DownRight"/>.</param>
        /// <param name="origin">The origin to draw from.</param>
        /// <param name="scale">The scale to draw with.</param>
        /// <param name="layerDepth">The layer depth to draw with.</param>
        public static void DrawExtendedAutoTileCorner(SpriteBatch batch, Vector2 pos, TextureRegion overlayTexture, ConnectsTo connectsTo, Color overlayColor, Direction2 corner, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0) {
            var src = AutoTiling.CalculateExtendedAutoTile(overlayTexture.Area, connectsTo, corner);
            if (src != Rectangle.Empty)
                batch.Draw(overlayTexture.Texture, pos, src, overlayColor, 0, origin ?? Vector2.Zero, scale ?? Vector2.One, SpriteEffects.None, layerDepth);
        }

        /// <inheritdoc cref="DrawExtendedAutoTile(Microsoft.Xna.Framework.Graphics.SpriteBatch,Microsoft.Xna.Framework.Vector2,MLEM.Textures.TextureRegion,MLEM.Textures.TextureRegion,MLEM.Graphics.AutoTiling.ConnectsTo,Microsoft.Xna.Framework.Color,Microsoft.Xna.Framework.Color,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{Microsoft.Xna.Framework.Vector2},float,float)"/>
        public static void DrawExtendedAutoTile(SpriteBatch batch, Vector2 pos, TextureRegion backgroundTexture, Func<int, TextureRegion> overlayTextures, ConnectsTo connectsTo, Color backgroundColor, Color overlayColor, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0, float overlayDepthOffset = 0) {
            if (backgroundTexture != null)
                batch.Draw(backgroundTexture, pos, backgroundColor, 0, origin ?? Vector2.Zero, scale ?? Vector2.One, SpriteEffects.None, layerDepth);
            var od = layerDepth + overlayDepthOffset;
            AutoTiling.DrawExtendedAutoTileCorner(batch, pos, overlayTextures, connectsTo, overlayColor, Direction2.UpLeft, origin, scale, od);
            AutoTiling.DrawExtendedAutoTileCorner(batch, pos, overlayTextures, connectsTo, overlayColor, Direction2.UpRight, origin, scale, od);
            AutoTiling.DrawExtendedAutoTileCorner(batch, pos, overlayTextures, connectsTo, overlayColor, Direction2.DownLeft, origin, scale, od);
            AutoTiling.DrawExtendedAutoTileCorner(batch, pos, overlayTextures, connectsTo, overlayColor, Direction2.DownRight, origin, scale, od);
        }

        /// <inheritdoc cref="DrawExtendedAutoTileCorner(Microsoft.Xna.Framework.Graphics.SpriteBatch,Microsoft.Xna.Framework.Vector2,MLEM.Textures.TextureRegion,MLEM.Graphics.AutoTiling.ConnectsTo,Microsoft.Xna.Framework.Color,MLEM.Misc.Direction2,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{Microsoft.Xna.Framework.Vector2},float)"/>
        public static void DrawExtendedAutoTileCorner(SpriteBatch batch, Vector2 pos, Func<int, TextureRegion> overlayTextures, ConnectsTo connectsTo, Color overlayColor, Direction2 corner, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0) {
            var src = AutoTiling.CalculateExtendedAutoTileOffset(connectsTo, corner);
            if (src >= 0)
                batch.Draw(overlayTextures(src), pos, overlayColor, 0, origin ?? Vector2.Zero, scale ?? Vector2.One, SpriteEffects.None, layerDepth);
        }

        /// <inheritdoc cref="DrawExtendedAutoTile(Microsoft.Xna.Framework.Graphics.SpriteBatch,Microsoft.Xna.Framework.Vector2,MLEM.Textures.TextureRegion,MLEM.Textures.TextureRegion,MLEM.Graphics.AutoTiling.ConnectsTo,Microsoft.Xna.Framework.Color,Microsoft.Xna.Framework.Color,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{Microsoft.Xna.Framework.Vector2},float,float)"/>
        public static void AddExtendedAutoTile(StaticSpriteBatch batch, Vector2 pos, TextureRegion backgroundTexture, TextureRegion overlayTexture, ConnectsTo connectsTo, Color backgroundColor, Color overlayColor, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0, float overlayDepthOffset = 0, ICollection<StaticSpriteBatch.Item> items = null) {
            if (backgroundTexture != null) {
                var background = batch.Add(backgroundTexture, pos, backgroundColor, 0, origin ?? Vector2.Zero, scale ?? Vector2.One, SpriteEffects.None, layerDepth);
                items?.Add(background);
            }
            var od = layerDepth + overlayDepthOffset;
            AutoTiling.AddExtendedAutoTileCorner(batch, pos, overlayTexture, connectsTo, overlayColor, Direction2.UpLeft, origin, scale, od, items);
            AutoTiling.AddExtendedAutoTileCorner(batch, pos, overlayTexture, connectsTo, overlayColor, Direction2.UpRight, origin, scale, od, items);
            AutoTiling.AddExtendedAutoTileCorner(batch, pos, overlayTexture, connectsTo, overlayColor, Direction2.DownLeft, origin, scale, od, items);
            AutoTiling.AddExtendedAutoTileCorner(batch, pos, overlayTexture, connectsTo, overlayColor, Direction2.DownRight, origin, scale, od, items);
        }

        /// <inheritdoc cref="DrawExtendedAutoTileCorner(Microsoft.Xna.Framework.Graphics.SpriteBatch,Microsoft.Xna.Framework.Vector2,MLEM.Textures.TextureRegion,MLEM.Graphics.AutoTiling.ConnectsTo,Microsoft.Xna.Framework.Color,MLEM.Misc.Direction2,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{Microsoft.Xna.Framework.Vector2},float)"/>
        public static void AddExtendedAutoTileCorner(StaticSpriteBatch batch, Vector2 pos, TextureRegion overlayTexture, ConnectsTo connectsTo, Color overlayColor, Direction2 corner, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0, ICollection<StaticSpriteBatch.Item> items = null) {
            var src = AutoTiling.CalculateExtendedAutoTile(overlayTexture.Area, connectsTo, corner);
            if (src != Rectangle.Empty) {
                var o4 = batch.Add(overlayTexture.Texture, pos, src, overlayColor, 0, origin ?? Vector2.Zero, scale ?? Vector2.One, SpriteEffects.None, layerDepth);
                items?.Add(o4);
            }
        }

        /// <inheritdoc cref="DrawExtendedAutoTile(Microsoft.Xna.Framework.Graphics.SpriteBatch,Microsoft.Xna.Framework.Vector2,MLEM.Textures.TextureRegion,Func{int,MLEM.Textures.TextureRegion},MLEM.Graphics.AutoTiling.ConnectsTo,Microsoft.Xna.Framework.Color,Microsoft.Xna.Framework.Color,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{Microsoft.Xna.Framework.Vector2},float,float)"/>
        public static void AddExtendedAutoTile(StaticSpriteBatch batch, Vector2 pos, TextureRegion backgroundTexture, Func<int, TextureRegion> overlayTextures, ConnectsTo connectsTo, Color backgroundColor, Color overlayColor, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0, float overlayDepthOffset = 0, ICollection<StaticSpriteBatch.Item> items = null) {
            if (backgroundTexture != null) {
                var background = batch.Add(backgroundTexture, pos, backgroundColor, 0, origin ?? Vector2.Zero, scale ?? Vector2.One, SpriteEffects.None, layerDepth);
                items?.Add(background);
            }
            var od = layerDepth + overlayDepthOffset;
            AutoTiling.AddExtendedAutoTileCorner(batch, pos, overlayTextures, connectsTo, overlayColor, Direction2.UpLeft, origin, scale, od, items);
            AutoTiling.AddExtendedAutoTileCorner(batch, pos, overlayTextures, connectsTo, overlayColor, Direction2.UpRight, origin, scale, od, items);
            AutoTiling.AddExtendedAutoTileCorner(batch, pos, overlayTextures, connectsTo, overlayColor, Direction2.DownLeft, origin, scale, od, items);
            AutoTiling.AddExtendedAutoTileCorner(batch, pos, overlayTextures, connectsTo, overlayColor, Direction2.DownRight, origin, scale, od, items);
        }

        /// <inheritdoc cref="DrawExtendedAutoTileCorner(Microsoft.Xna.Framework.Graphics.SpriteBatch,Microsoft.Xna.Framework.Vector2,MLEM.Textures.TextureRegion,MLEM.Graphics.AutoTiling.ConnectsTo,Microsoft.Xna.Framework.Color,MLEM.Misc.Direction2,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{Microsoft.Xna.Framework.Vector2},float)"/>
        public static void AddExtendedAutoTileCorner(StaticSpriteBatch batch, Vector2 pos, Func<int, TextureRegion> overlayTextures, ConnectsTo connectsTo, Color overlayColor, Direction2 corner, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0, ICollection<StaticSpriteBatch.Item> items = null) {
            var src = AutoTiling.CalculateExtendedAutoTileOffset(connectsTo, corner);
            if (src >= 0) {
                var o4 = batch.Add(overlayTextures(src), pos, overlayColor, 0, origin ?? Vector2.Zero, scale ?? Vector2.One, SpriteEffects.None, layerDepth);
                items?.Add(o4);
            }
        }

        private static (Vector2, Rectangle, Vector2, Rectangle, Vector2, Rectangle, Vector2, Rectangle) CalculateAutoTile(Vector2 pos, Rectangle textureRegion, ConnectsTo connectsTo, Vector2 scale) {
            var up = connectsTo(0, -1);
            var down = connectsTo(0, 1);
            var left = connectsTo(-1, 0);
            var right = connectsTo(1, 0);
            var xUl = up && left ? connectsTo(-1, -1) ? 0 : 4 : left ? 1 : up ? 3 : 2;
            var xUr = up && right ? connectsTo(1, -1) ? 0 : 4 : right ? 1 : up ? 3 : 2;
            var xDl = down && left ? connectsTo(-1, 1) ? 0 : 4 : left ? 1 : down ? 3 : 2;
            var xDr = down && right ? connectsTo(1, 1) ? 0 : 4 : right ? 1 : down ? 3 : 2;
            var (w, h) = (textureRegion.Width, textureRegion.Height);
            var (w2, h2) = (w / 2, h / 2);
            return (
                new Vector2(pos.X, pos.Y), new Rectangle(textureRegion.X + xUl * w, textureRegion.Y, w2, h2),
                new Vector2(pos.X + w2 * scale.X, pos.Y), new Rectangle(textureRegion.X + w2 + xUr * w, textureRegion.Y, w2, h2),
                new Vector2(pos.X, pos.Y + h2 * scale.Y), new Rectangle(textureRegion.X + xDl * w, textureRegion.Y + h2, w2, h2),
                new Vector2(pos.X + w2 * scale.X, pos.Y + h2 * scale.Y), new Rectangle(textureRegion.X + w2 + xDr * w, textureRegion.Y + h2, w2, h2));
        }

        private static int CalculateExtendedAutoTileOffset(ConnectsTo connectsTo, Direction2 corner) {
            switch (corner) {
                case Direction2.UpLeft: {
                    var up = connectsTo(0, -1);
                    var left = connectsTo(-1, 0);
                    return up && left ? connectsTo(-1, -1) ? -1 : 12 : left ? 0 : up ? 8 : 4;
                }
                case Direction2.UpRight: {
                    var up = connectsTo(0, -1);
                    var right = connectsTo(1, 0);
                    return up && right ? connectsTo(1, -1) ? -1 : 13 : right ? 1 : up ? 9 : 5;
                }
                case Direction2.DownLeft: {
                    var down = connectsTo(0, 1);
                    var left = connectsTo(-1, 0);
                    return down && left ? connectsTo(-1, 1) ? -1 : 14 : left ? 2 : down ? 10 : 6;
                }
                case Direction2.DownRight: {
                    var down = connectsTo(0, 1);
                    var right = connectsTo(1, 0);
                    return down && right ? connectsTo(1, 1) ? -1 : 15 : right ? 3 : down ? 11 : 7;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
            }
        }

        private static Rectangle CalculateExtendedAutoTile(Rectangle textureRegion, ConnectsTo connectsTo, Direction2 corner) {
            var off = AutoTiling.CalculateExtendedAutoTileOffset(connectsTo, corner);
            return off < 0 ? Rectangle.Empty : new Rectangle(textureRegion.X + off * textureRegion.Width, textureRegion.Y, textureRegion.Width, textureRegion.Height);
        }

        /// <summary>
        /// A delegate function that determines if a given offset position connects to an auto-tile location.
        /// </summary>
        /// <param name="xOff">The x offset</param>
        /// <param name="yOff">The y offset</param>
        public delegate bool ConnectsTo(int xOff, int yOff);

    }
}
