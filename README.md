# ReAction

## Alternative input system for s&box & Unity

### THIS IS STILL A WIP! LOL

### Table of contents
1. [Introduction](#introduction)  
2. [How to use](#howtouse)  
	a. [Basics](#basics)  
	b. [Extra features](#extra)  
3. [Stuff that's untested/doesn't currently work](#wip)

## Introduction <a name="introduction"></a>
This is an alternative to the Input class, inspired by EFT and Helldiver 2's keybind systems.  

<details>
<summary>s&box related stuff</summary>
It works for whitelisted games too!  
  
This is built using raw input, via `Input.Keyboard`.
</details>
  
## How to use <a name="howtouse"></a>

### Basics <a name="basics"></a>
Under the "View" tab in the editor, you enable the suspiciously familiar ReAction Actions window.  
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

Instead of `Input.Down`, `Pressed`, or `Released`, you use `ReAction.ActionTriggered`.  

<details>
<summary>s&box related stuff</summary>
If you really really reallyyyyyyyyyyyy miss those methods, you can use `ReAction.KeyDown`, `ReAction.KeyPressed` or `ReAction.KeyReleased` respectively. They're basically the `Input.Keyboard` methods, but you don't have to remember the source bind names.
</details>
  
The inputs are polled via the ReActionSystem, which is a GameObjectSystem (or a MonoBehaviour on Unity, that you manually place in your scene) that activates at the beginning of each frame.

To save the configured actions, click the Save Actions button, this will save the actions locally, only for you. If you want to have the configured actions be the project's default, click the Set As New Game Default button.

### Extra features <a name="extra"></a>
You can check if an action would be triggered with a different conditional by doing `ReAction.ActionConditionsValid(string or int, Conditional)`  

<details>
<summary>more s&box related stuff</summary>
If your action has an analog GamepadInput, you can check its value with the `Analog` property  
Each action also has a configurable PositiveAxis property boolean, again, for analog inputs.  
</details>

There's a button to export the names and indices of your actions to `public const string/int`s, or `public static readonly string/int`s in ReActionConsts.cs file, that will be placed alongside your code (in the root of your Assets folder, for unity), this is good for preventing typos and such :) :) :)  

<details>
<summary>even more s&box related stuff</summary>
You can use the `ReAction_debug` command line to enable/disable the ReAction overlay, it's not a cheat by default, is that bad?  
</details>

## WIP stuff: <a name="wip"></a>  
Gamepad support is still wip, I don't even think it works, lol  

TODO SBOX:  
there's stuff on the [Bind](https://developer.valvesoftware.com/wiki/Bind) page on the valve developer wiki that probably could be used, like the joy14 and joy13 shtuff :) :) :)  
being able to switch context, basically have different sets of bindings, ie. one for walking, one for driving, one for flying a plane  
wish very very hard for facepunch to add the SDL_GameControllerGetTouchpadFinger method :)  
One could probably port this to Unity? I heard the Input system they have is dogshit, lol  
Find a way to maybe properly handle different keyboard layouts? I have an azerty keyboard and because everyone who's working at facepunch is racist I couldn't rebind my forward key when I was playing sandbox (as in the game). The main problem is checking the current keyboard layout, you can use Windows.Forms.Input or something like that to get it, but A: only available on windows, and I want this to be cross-platform (s&box linux version coming tomorrow, trust) and B: I don't think it's in the whitelist :(  

TODO UNITY:
use SDL instead of whatever the fuck the Input system does for controllers  
make a debug thingie to debug thingies :)