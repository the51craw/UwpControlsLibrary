Study KnobSampleapp to understand the basic procedure for using UwpControlsLibrary.
Study SampleApplication to learn all different control types except CompoundControl.
Study CompoundExample to learn about CompoundControl controls.

Basic procedure for setting up a project using UwpControlsLibrary:
Create an empty UWP application.
Add a reference to UwpControlsLibrary.dll.
Make the images needed for the controls.
Put the images in a subdirectory 'Images' and add them to the project.

Run MakeImageList to make Image entries to paste into MainPage.xaml and paste them
as it is done in KnobSampleApp. Those images are for coopying to the controls, and
will themself never be shown. There is functionality to keep track of them and to
set them collapsed after controls are created, but since they are placed behind
the background image, they will not show anyway. The background image is used as
background, but also as a 'touch-panel' for receiving all pointer events by being
positioned on top of all controls, but is set to be transparent tom make the controls
visible anyway. The only control that does not follow this schema is the keyboard.
The keyboard uses a black key and a wnite key image that themself reacts to pointer
events, and is added above the imgClickArea.

If the application window is not forced to a fixed width-height ratio, margins might
show at the sides of the Background image, or above and under. If you want to fill
those margins you can either use a color of your chise, or add an extra background
image behind the background image and set it to stretch to fill.

Some initial work can be done at application startup, but controls needs image
size information in order to be correctly created.

The image imgClickArea is loaded last, so use the event imgClickArea_ImageOpened
to create controls.

The images are initially set to to not stretch in order to collect correct size
from them. After that, all images should be set to be stretch uniformly and to be
correctly sized by calling the following functions:

Controls.ResizeControls(gridControls, Window.Current.Bounds);
Controls.SetControlsUniform(gridControls);
UpdateLayout();

Regarding control events. The first parameter when creating a control can be used
to identify the control, but it is not always the best practice, especially not
when using CompoundControl class. All control inherits ControlBase, which has
a Tag that can be set to anything needed to identify a control, and also othe
data needed.

All events are collected via imgClickArea, which belongs to MainPage. All events
must therefore be collected in MainPage.

When recieving the event gridMain_SizeChanged, call 
Controls.ResizeControls(gridMain, Window.Current.Bounds)

When recieveing pointer events, call event handlers in Controls object to make
the controls 'live'.

MouseButton 1 and 2 are used in some controls, and any third in some.
MouseWheel is used in all controls that can be manipulated by dragging, as well
as Rotator controls.