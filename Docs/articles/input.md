# Input Handler

The **MLEM** base package features an extended `InputHandler` class that allows for finer control over inputs, like the ability to query a new *pressed* state as well as a repeat events implementation for both keyboard and gamepad input.

Rather than using an event-based structure, the MLEM input handler relies on the game's `Update` frames: To query input through the input handler, you have to query it every Update frame, and input information will only be available for a single update frame in most situations.

## Setting it up
To set it up, all you have to do is create a new instance. The constructor optionally accepts parameters to enable or disable certain kinds of input.
```cs
this.InputHandler = new InputHandler();
```
Additionally, you will have to call the input handler's `Update` method each update call of your game:
```cs
this.InputHandler.Update();
```

## Querying pressed keys
A *pressed* key is a key that wasn't down the last update but is held down the current update. This behavior can be useful for things like ui buttons, where holding down the mouse button shouldn't constantly keep triggering the button.

You can query if any key, mouse button or gamepad button is pressed as follows:
```cs
var mouse = this.InputHandler.IsPressed(MouseButton.Left);
var key = this.InputHandler.IsPressed(Keys.A);

// Is any gamepad's A button pressed
var gamepad = this.InputHandler.IsPressed(Buttons.A);
// Is the 2nd gamepad's A button pressed
var gamepad2 = this.InputHandler.IsPressed(Buttons.A, 2);
```

### Repeat events
Keyboard and gamepad repeat events can be enabled or disabled through the `HandleKeyboardRepeats` and `HandleGamepadRepeats` properties in the input handler. Additionally, you can configure the time that it takes until the first repeat is triggered through the `KeyRepeatDelay` property, and you can configure the delay between repeat events through the `KeyRepeatRate` property.

When enabled, repeat events for *pressing* are automatically triggered. This means that calling `IsPressed` every update call would return `true` for a control that is being held down every `KeyRepeatRate` seconds after `KeyRepeatDelay` seconds have passed once.

## Gesture handling
MonoGame's default touch handling can be a bit wonky to deal with, so the input handler also provides a much better user experience for touch gesture input.

To enable touch input, the gestures you want to use first have to be enabled:
```cs
InputHandler.EnableGestures(GestureType.Tap);
```

When enabled, a `GestureSample` will be available for the requested gesture type *the frame it is finished*. It can be accessed like so:
```cs
if (this.InputHandler.GetGesture(GestureType.Tap, out var sample)) {
    // The gesture happened this frame
    Console.WriteLine(sample.Position);
} else {
    // The gesture did not happen this frame
}
```

### External gesture handling
If your game already handles gestures through some other means, you might notice that one of the gesture handling methods stops working correctly. This is due to the fact that MonoGame's gesture querying system only supports each gesture to be queried once before it is removed from the queue.

If you want to continue using your own gesture handling, but still allow the `InputHandler` to use gestures (for [MLEM.Ui](ui.md), for example), you can set `GesturesExternal` to true in your `InputHandler`. Then, you can use `AddExternalGesture` to make the input handler aware of a gesture for the duration of the update frame that you added it on.
