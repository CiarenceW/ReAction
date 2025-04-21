# ReInput

## Alternative input system for s&box

### THIS IS STILL A WIP! LOL

### Table of contents
1. [Introduction](#introduction)  
2. [How to use](#howtouse)  
	a. [Basics](#basics)  
	b. [Extra features](#extra)  
3. [Stuff that's untested/doesn't currently work](#wip)

## Introduction <a name="introduction"></a>
This is an alternative to the Sandbox.Input class, inspired by EFT and Helldiver 2's keybind systems.  

It works for whitelisted games too!  
  
This is built using raw input, via `Input.Keyboard`.
  
## How to use <a name="howtouse"></a>

### Basics <a name="basics"></a>
Under the "View" tab in the editor, you enable the suspiciously familiar ReInput Actions window.  
Then, you define a new action, you give it a name, a KeyCode, and an activation condition.  
  
This activation condition can be one of the following:  
- Press
  - Activates only when the key is pressed, when it wasn't before
- Long Press  
  - Activates after holding the key for a certain amount of time (duration configurable)
- Continuous
  - Activates while the key is pressed down
- Release
  - Activates when the key is released
- DoubleTap
  - Activates after the key is double tapped, during a certain time interval (duration configurable)
- Toggle
  - Activates after the key is pressed, deactivates after it's pressed again
- Mash
  - Activates when the key is repeatedly pressed  
 
You can give you action any, and all of these modifiers:
- LShift  
- LCtrl  
- LMeta (also called LWin)  
- LAlt  
- RAlt  
- RMeta  
- RShift (also called RWin)  
- RCtrl  

Instead of `Input.Down`, `Pressed`, or `Released`, you use `ReInput.ActionTriggered`.  

The inputs are polled via the ReInputSystem, which is a GameObjectSystem that activates at the beginning of each frame.

To save the configured actions, click the Save Actions button, this will save the actions locally, only for you. If you want to have the configured actions be the project's default, click the Set As New Game Default.

### Extra features <a name="extra"></a>
You can check if an action would be triggered with a different conditional by doing `ReInput.ActionConditionsValid(string or int, Conditional)`  

If your action has an analog GamepadInput, you can check its value with the `Analog` property  
Each action also has a configurable PositiveAxis property boolean, again, for analog inputs.

There's a button to export the indices of your actions to `public int consts`, or `public static readonly ints` in ReInputConsts.cs file, that will be placed alongside your code, this is good for preventing typos and such :) :) :)

## WIP stuff: <a name="wip"></a>  
Gamepad support is still wip, I don't even think it works, lol  

TODO:  
there's stuff on the [Bind](https://developer.valvesoftware.com/wiki/Bind) page on the valve developer wiki that probably could be used, like the joy14 and joy13 shtuff :) :) :)  
being able to switch context, basically have different sets of bindings, ie. one for walking, one for driving, one for flying a plane