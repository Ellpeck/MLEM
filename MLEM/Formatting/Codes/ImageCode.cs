using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Animations;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Misc;
using MLEM.Textures;

namespace MLEM.Formatting.Codes {
    public class ImageCode : Code {

        private readonly SpriteAnimation image;
        private string replacement;
        private float gapSize;

        public ImageCode(Match match, Regex regex, SpriteAnimation image) : base(match, regex) {
            this.image = image;
        }

        public override bool EndsHere(Code other) {
            return true;
        }

        public override string GetReplacementString(GenericFont font) {
            if (this.replacement == null) {
                // use non-breaking space so that the image won't be line-splitted
                var strg = font.GetWidthString(font.LineHeight, '\u00A0');
                this.replacement = strg.Remove(strg.Length - 1) + ' ';
                this.gapSize = font.MeasureString(this.replacement).X;
            }
            return this.replacement;
        }

        public override void Update(GameTime time) {
            this.image.Update(time);
        }

        public override void DrawSelf(GameTime time, SpriteBatch batch, Vector2 pos, GenericFont font, Color color, float scale, float depth) {
            var position = pos + new Vector2(this.gapSize - font.LineHeight, 0) / 2 * scale;
            batch.Draw(this.image.CurrentRegion, new RectangleF(position, new Vector2(font.LineHeight * scale)), Color.White.CopyAlpha(color));
        }

    }

    public static class ImageCodeExtensions {

        public static void AddImage(this TextFormatter formatter, string name, TextureRegion image) {
            formatter.AddImage(name, new SpriteAnimation(1, image));
        }

        public static void AddImage(this TextFormatter formatter, string name, SpriteAnimation image) {
            formatter.Codes.Add(new Regex($"<i {name}>"), (f, m, r) => new ImageCode(m, r, image));
        }

    }
}