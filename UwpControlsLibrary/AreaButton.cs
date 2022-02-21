using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace UwpControlsLibrary
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// AreaButton class.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class AreaButton : ControlBase
    {
        /// <summary>
        /// The AreaButton only defines a hit area with no graphics of it's own. It is meant to be defined to cover
        /// an area that is needed to accept click events. Typically there will be some graphics on the background
        /// image associated with the area.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="Id"></param>
        /// <param name="gridMain"></param>
        /// <param name="HitArea"></param>
        public AreaButton(Controls controls, Int32 Id, Grid gridMain, Rect HitArea)
        {
            this.Id = Id;
            GridControls = gridMain;
            HitTarget = true;
            this.HitArea = new Rect(HitArea.Left, HitArea.Top, HitArea.Width, HitArea.Height);
            ControlSizing = new ControlSizing(controls, this);
        }
    }
}
