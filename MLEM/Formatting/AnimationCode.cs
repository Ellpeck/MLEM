using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Formatting {
    public class AnimationCode : FormattingCode {

        public readonly FormatState.DrawCharacter Draw;
        public TimeSpan Time;

        public AnimationCode(int startIndex, FormatState.DrawCharacter draw) : base(startIndex, Type.Animation) {
            this.Draw = draw;
        }

        public override void Update(GameTime time) {
            this.Time += time.ElapsedGameTime;
        }

        public override void Reset() {
            this.Time = TimeSpan.Zero;
        }

        public override void ModifyFormatting(ref FormatState state) {
            state.DrawBehavior = this.Draw;
        }

    }
}