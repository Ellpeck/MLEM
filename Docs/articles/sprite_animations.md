# Sprite Animations

The **MLEM** package contains a simple sprite animation system that features different-length frames as well as conditioned animations and grouping.

## Using Animations
You can create an animation like so:
```cs
var texture = this.Content.Load<Texture2D>("Textures/TestSprite");

// Two-frame animation using a frame time of 0.5 seconds
var anim1 = new SpriteAnimation(timePerFrame: 0.5F,
    new TextureRegion(texture, 0, 0, 16, 16), new TextureRegion(texture, 16, 0, 16, 16));

// Three-frame animation with varying frame times
var anim2 = new SpriteAnimation(
    new AnimationFrame(new TextureRegion(texture, 0, 0, 16, 16), 0.25F),
    new AnimationFrame(new TextureRegion(texture, 16, 0, 16, 16), 0.5F),
    new AnimationFrame(new TextureRegion(texture, 32, 0, 16, 16), 0.3F)
);
```

Additionally, you have to update the animation every update frame in your game's `Update` method:
```cs
anim1.Update(gameTime);
```

You can draw the animation's current frame as follows:
```cs
this.SpriteBatch.Draw(anim1.CurrentRegion, new Vector2(10, 10), Color.White);
```

## Using Animation Groups
Animation groups consist of multiple animations. Each animation in a group has a condition that determines if it should currently be playing.

You can create an animation group and add animations to it like so:
```cs
var group = new SpriteAnimationGroup();
// Animation 1 should always play
group.Add(anim1, () => true, priority: 0);
// Animation 2 should play if the game has been running for 10 seconds or more
// Since its priority is higher than anim1's, it will be the one that plays when its condition is true
group.Add(anim2, () => gameTime.TotalGameTime.TotalSeconds >= 10, priority: 1);
```

As with regular animations, an animation group also has to be updated each update frame:
```cs
group.Update(gameTime);
```

You can draw the group's current frame as follows:
```cs
this.SpriteBatch.Draw(group.CurrentRegion, new Vector2(10, 10), Color.White);
```
