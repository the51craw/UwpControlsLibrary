using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace UwpControlsLibrary
{
    public partial class Controls
    {
        public void OnPointerPressed(object sender, PointerEventHandler e)
        {

        }

        public void OnPointerReleased(object sender, PointerEventHandler e)
        {

        }

        public void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            controlsHit = FindControlsUnderPointer(e.GetCurrentPoint(imgClickArea).Position);
        }

        public void OnTapped(object sender, TappedRoutedEventArgs e)
        {

        }

        public void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {

        }

        public void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {

        }

        private ControlsUnderPointer FindControlsUnderPointer(Point point)
        {
            ControlsUnderPointer controlsUnderPointer = new ControlsUnderPointer();
            foreach (object control in ControlsList)
            {
                if (control.GetType() == typeof(UwpControlsLibrary.CompoundControl))
                {
                    CompoundControlsUnderPointer compoundControl = 
                        new CompoundControlsUnderPointer(((ControlBase)control).Id, ((CompoundControl)control).SubType);
                    foreach (object subControl in ((UwpControlsLibrary.CompoundControl)control).SubControls.ControlsList)
                    {
                        if (((ControlBase)subControl).ControlSizing.IsHit(point))
                        {
                            compoundControl.Controls.Add(((ControlBase)subControl).Id);
                        }
                    }
                    if (compoundControl.Controls.Count > 0)
                    {
                        controlsUnderPointer.CompoundControls.Add(compoundControl);
                    }
                }
                else
                {
                    if (((ControlBase)control).ControlSizing.IsHit(point))
                    {
                        controlsUnderPointer.Controls.Add(((ControlBase)control).Id);
                    }
                }
            }
            return controlsUnderPointer;
        }

        public class ControlsUnderPointer
        {
            public List<CompoundControlsUnderPointer> CompoundControls;
            public List<Int32> Controls;

            public ControlsUnderPointer()
            {
                CompoundControls = new List<CompoundControlsUnderPointer>();
                Controls = new List<Int32>();
            }
        }

        public class CompoundControlsUnderPointer
        {
            public Int32 Id;
            public Int32 SubType;
            public List<Int32> Controls;

            public CompoundControlsUnderPointer(Int32 Id, Int32 SubType)
            {
                this.Id = Id;
                this.SubType = SubType;
                Controls = new List<Int32>();
            }
        }
    }
}
