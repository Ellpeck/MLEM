# Input Handler

The **MLEM** base package features an extended `InputHandler` class that allows for finer control over inputs, like the ability to query a new *pressed* state as well as a repeat events implementation for both keyboard and gamepad input.

Rather than using an event-based structure, the MLEM input handler relies on the game's `Update` frames: To query input through the input handler, you have to query it every Update frame, and input information will only be available for a single update frame in most situations.

The input handler makes use of the `GenericInput` struct, which is a MLEM wrapper around the three main types of input that MonoGame and FNA provide: `Keys`, `Buttons` and `MouseButton` (the latter of which is a MLEM abstraction of mouse buttons). Values of all of these types can be converted into `GenericInput` implicitly, and a `GenericInput` can be converted back implicitly as well, so you will rarely ever have to interact with the `GenericInput` type manually.

## Setting it up
To set it up, all you have to do is create a new instance. The constructor optionally accepts parameters to enable or disable certain kinds of input.
```cs
this.InputHandler = new InputHandler(gameInstance);
```
Additionally, you will have to call the input handler's `Update` method each update call of your game:
```cs
this.InputHandler.Update();
```

## Querying pressed keys
A *pressed* key is a key that wasn't down the last update but is held down the current update, which is essentially a positive-edge-triggered press. This behavior can be useful for things like ui buttons, where holding down the mouse button shouldn't constantly keep triggering the button.

You can query if any key, mouse button or gamepad button is pressed as follows:
```cs
var mouse = this.InputHandler.IsPressed(MouseButton.Left);
var key = this.InputHandler.IsPressed(Keys.A);

// Is any gamepad's A button pressed
var gamepad = this.InputHandler.IsPressed(Buttons.A);
// Is the 2nd gamepad's A button pressed
var gamepad2 = this.InputHandler.IsPressed(Buttons.A, 2);
```

Using the `InvertPressBehavior` flag, you can invert this behavior: If it is set to `true`, a key is considered pressed if it was down the last update, but is up in the current update. This is essentially a negative-edge-triggered press.

### Repeat events
Keyboard and gamepad repeat events can be enabled or disabled through the `HandleKeyboardRepeats` and `HandleGamepadRepeats` properties in the input handler. Additionally, you can configure the time that it takes until the first repeat is triggered through the `KeyRepeatDelay` property, and you can configure the delay between repeat events through the `KeyRepeatRate` property.

When enabled, repeat events for *pressing* are automatically triggered. This means that calling `IsPressed` every update call would return `true` for a control that is being held down every `KeyRepeatRate` seconds after `KeyRepeatDelay` seconds have passed once.

## Consuming inputs
Due to the fact that the input handler is query-based (rather than event-based), multiple pieces of code might query the same input each update, causing a single press to be misconstrued as multiple distinct inputs. 

The input handler provides the methods `IsPressConsumed`, `IsPressedAvailable`, and `TryConsumePressed`. Calling `TryConsumePressed` on an input means that subsequent calls to `IsPressConsumed` and `IsPressedAvailable` will not return `true` until the next update frame:
```cs 
// is this update frame's Up press consumed yet?
var consumed = this.InputHandler.IsPressConsumed(Keys.Up);
// is the Up key pressed, and its press not consumed yet?
var available = this.InputHandler.IsPressedAvailable(Keys.Up);

// check whether the Up key is pressed and its press is available, and consume it
if (this.InputHandler.TryConsumePressed(Keys.Up)) {
    // the press has been consumed by us, now do something with the press!
}
```

## Input metrics
The input handler tracks additional data related to keyboard, gamepad, and mouse inputs, such as the amount of times that they have been down for. These metrics can be useful for implementing short-press and long-press behavior.

```cs 
// how long has the A key been up (or down) for the last time it was up (or down)?
var upTime = this.InputHandler.GetUpTime(Keys.A);
var downTime = this.InputHandler.GetDownTime(Keys.A);

// how long has it been since the A key was pressed?
var timeSincePress = this.InputHandler.TryGetTimeSincePress(Keys.A);
```

## Gesture handling
MonoGame's default gesture handling (which is inherited from XNA) can be a little difficult to deal with. This is mainly due to the fact that gestures stay in the queue until they are queried (so they might be very old), and the fact that they can't be queried without being permanently removed from the queue.

Because of this, MLEM's input handler also provides a much more streamlined user experience for touch gesture input.

To enable touch input, the gestures you want to use first have to be enabled:
```cs
InputHandler.EnableGestures(GestureType.Tap);
```

When enabled, a `GestureSample` will be available for the requested gesture type *the update frame it is finished*. It can be accessed like so:
```cs
if (this.InputHandler.GetGesture(GestureType.Tap, out var sample)) {
    // The gesture happened this frame
    Console.WriteLine(sample.Position);
} else {
    // The gesture did not happen this frame
}
```

### External gesture handling
If your game already handles gestures through some other means, you might notice that one of the gesture handling methods stops working correctly. This is due to the fact that MonoGame's gesture querying system only supports each gesture being queried once before it is removed from the queue, which causes any additional queries for that gesture to fail.

The input handler's gesture handling does not have this problem, since gestures are kept around for an entire update frame no matter how many times they are queried, and gestures can be queried from multiple sources based on the expected gesture type. Because of this, it's generally recommended that you use the input handler's gesture system instead of the default one.

However, if you want to continue using your own gesture handling, but still allow the `InputHandler` to have access to gestures (for [MLEM.Ui](ui.md), for example), you can set `ExternalGestureHandling` to true in your `InputHandler`. Then, you can use `AddExternalGesture` to make the input handler aware of a gesture for the duration of the update frame that you added it on. As an example, you could modify your game's existing gesture handling like this:
```cs 
while (TouchPanel.IsGestureAvailable) {
    var gesture = TouchPanel.ReadGesture();
    
    // your game's existing gesture handling ...
    bool gestureConsumed = this.HandleGestureSomeWay(gesture);
    if (gestureConsumed)
        continue;
    
    // pass the gesture onto the input handler if we didn't make use of it
    this.InputHandler.AddExternalGesture(gesture);
}
```
