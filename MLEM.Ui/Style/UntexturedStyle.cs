using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Textures;

namespace MLEM.Ui.Style {
    public class UntexturedStyle : UiStyle {

        public UntexturedStyle(SpriteBatch batch) {
            this.ButtonTexture = GenerateTexture(batch, Color.CadetBlue);
            this.ButtonHoveredColor = Color.LightGray;
            this.PanelTexture = GenerateTexture(batch, Color.Gray);
            this.TextFieldTexture = GenerateTexture(batch, Color.MediumBlue);
            this.TextFieldHoveredColor = Color.LightGray;
            this.ScrollBarBackground = GenerateTexture(batch, Color.LightBlue);
            this.ScrollBarScrollerTexture = GenerateTexture(batch, Color.Blue);
            this.CheckboxTexture = GenerateTexture(batch, Color.LightBlue);
            this.CheckboxHoveredColor = Color.LightGray;
            this.CheckboxCheckmark = GenerateTexture(batch, Color.Blue).Region;
            this.RadioTexture = GenerateTexture(batch, Color.AliceBlue);
            this.RadioHoveredColor = Color.LightGray;
            this.RadioCheckmark = GenerateTexture(batch, Color.CornflowerBlue).Region;
            this.TooltipBackground = GenerateTexture(batch, Color.DarkGray);
            this.TooltipBackgroundColor = new Color(Color.Black, 0.65F);
            this.Font = new EmptyFont();
        }

        private static NinePatch GenerateTexture(SpriteBatch batch, Color color) {
            var tex = new Texture2D(batch.GraphicsDevice, 3, 3);
            tex.SetData(new[] {
                Color.Black, Color.Black, Color.Black,
                Color.Black, color, Color.Black,
                Color.Black, Color.Black, Color.Black
            });
            batch.Disposing += (sender, args) => {
                if (tex != null) {
                    tex.Dispose();
                    tex = null;
                }
            };
            return new NinePatch(tex, 1);
        }

        private class EmptyFont : IGenericFont {

            public Vector2 MeasureString(string text) {
                return Vector2.One;
            }

            public Vector2 MeasureString(StringBuilder text) {
                return Vector2.One;
            }

            public void DrawString(SpriteBatch batch, string text, Vector2 position, Color color) {
            }

            public void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            }

            public void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            }

            public void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color) {
            }

            public void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            }

            public void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            }

            public void DrawCenteredString(SpriteBatch batch, string text, Vector2 position, float scale, Color color, bool horizontal = true, bool vertical = false, float addedScale = 0) {
            }

            public IEnumerable<string> SplitString(string text, float width, float scale) {
                yield break;
            }

        }

    }
}