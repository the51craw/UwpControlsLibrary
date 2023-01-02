# UwpControlsLibrary
Use UwpControlsLibrary to create image based resizable UWP applications.
The solution contains in addition to UwpControlsLibrary a code generator application called GenerateCode and a number of sample applications.
UwpControlsLibrary is a work inprogress and has been enhanced while developing some UWP applications that are now available in Windows store.
My Synthesizer Laboratory is all based on an early version of UwpControlsLibrary while Roland VT-4 Companion application (needs a Roland VT-4 to run)
inpired me to create PopupMenuButton, a multi menu button showing different menus (scrollable) depending on which mouse button was pressed.

The controls are:
CompoundControl is a control containing sub controls of any type from UwpControlsLibrary. Most part of My Synthesizer Laboratory use CompoundControl.
DigitalDisplay uses images of digits 0 - 9 a minus and a period plus an optional background image to create a digital display. My Synthesizer Laboratory
  uses DigitalDisplay to display frequencies.
Graph creates a graph by drawing lines between suppied Point objects. My Synthesizer Laboratory uses this to display wave shapes live while playing and 
  for making a Pitch Envelope shape.
HorizontalSlider and VerticalSlider are image based sliders. The VT-4 has four vertical sliders and My Synthesizer Laboratory uses sliders for modulation
  wheel, pitch bend wheel and reverb depth wheel. Those are sliders, but using creative imaging makes them look like wheel controls.
ImageButton is a common button based on images.
Indicator is a control that shows an alternate image to indicate some condition. I use it e.g. for emulating leds going on and off, but also as sockets
  where cables are plugged in in My Synthesizer Laboratory.
Joystick acts as an X-Y control. It is possible to create a shaft that follows the knob. See SampleApplication or JoyStickSampleApp (not yet written)
  to see how to use it.
Keyboard uses one black and one white piano key to make a keyboard with a number of octaves.
Knob displays a knob image that can be rotated using mouse move in combination with left button, or the mouse wheel, where left and right mose buttons
  changes the speed the knob rotates.
Rotator, or selector as it was previosly called, works as a multi state switch and can be applied using a number of images or one background image and
  a number of text strings to select from. Left click advances one step while right key moves backwards. Both wraps around, while the mouse wheel can
  be used to scroll among the selections stops at first and last selection. Agan My Synthesizer Laboratory uses this control a lot.
Static Image is used to place an image with no further functionality.
ToolTips displays a tooltip after hovering a selectable number of seconds over a control.
TouchpadKeyboard was an idea for emulating devices that has touch panels for input. This control is not finished by a long shot.
TreeViewBase is an idea from my application 'Mp3 player and music explorer' where the user gets a tree view of mp3 files on disk. Xaml did not have
  a tree view when I wrote the application, but I think it could be useful also in other applications. I will probably implement it later.

All theese controls are resizable and most has a lot of optional settings allowing you to customize them beyond what you can do with your graphics.
Many controls can have its own background image but that could also be part of the common backgorund image. If you emulate a device you may want to 
take a picture of its front panel and remove only slider handles and such. Then you supply the rest of the image as a background image and uses e.g.
an image of a slider handle when creating the slider. 
