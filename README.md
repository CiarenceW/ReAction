# ReInput

## Alternative input system for s&box

### THIS IS STILL A WIP! LOL

This is an alternative to the Sandbox.Input class, inspired by EFT and Helldiver 2's keybind systems.  

It works for whitelisted games too!  
  
It functions like this:  
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

If, for instance, you have an action called "Use", with KeyCode 'F' and Condition Press, but you also want to use that action for QTEs, you can simply check `ReInput.ActionConditionsValid("Use", Conditional.Mash)` when it is relevant to do so.  
  
This is built using the Input.Keyboard raw input method.