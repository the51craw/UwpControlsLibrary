using System;
using UwpControlsLibrary;
using Windows.UI.Xaml.Controls;

namespace CompositeExample
{
    public sealed partial class MainPage : Page
    {
        private void Actions()
        {
            foreach (Int32 controlUnderPointer in Controls.ControlsHit.Controls)
            {
                Actions(controlUnderPointer);
            }
            foreach (Controls.CompoundControlsUnderPointer compoundControlUnderPointer in Controls.ControlsHit.CompoundControls)
            {
                foreach (Int32 controlUnderPointer in compoundControlUnderPointer.Controls)
                {
                    Actions(controlUnderPointer, compoundControlUnderPointer.Id, compoundControlUnderPointer.SubType);
                }
            }
        }

        private void Actions( Int32 Id, Int32 CompoundId = -1, Int32 CompoundType = -1)
        {
            if (CompoundId == -1)
            {
                switch ((_controlId)Id)
                {
                    case _controlId.SELECT_LAYOUT:
                        layout++;
                        layout %= 4;
                        CreateControls();
                        break;
                }
            }
        }
    }
}
