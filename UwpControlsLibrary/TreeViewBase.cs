using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace UwpControlsLibrary
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// TreeView class. Handles a list of TreeViewList and a list of TreeViewItem. <summary>
    /// Holds prefered max number of lines in the TreeView.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class TreeViewBase : ControlBase
    {
        public List<TreeViewFolder> TreeViewFolders { get; set; }
        public List<TreeViewItem> TreeViewItems { get; set; }

        /// <summary>
        /// Use SubType to distinguish between your own types of Compound controls when
        /// creating more than one type of CompoundControl, like different types of
        /// panels of controls.
        /// </summary>
        public int SubType { get; set; }

        private Controls controls;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="AppSize"></param>
        /// <param name="ClickArea"></param>
        /// <param name="Id"></param>
        /// <param name="gridControls"></param>
        /// <param name="imageList">ImageList contains background image of the TreeView (may be transparent), TreeViewList icon,
        /// TreeViewItem icon and optional icons for lines indicating parent-child relations: Down line,
        /// down-right line and T-line. Used if present. All icons must be the same size.</param>
        public TreeViewBase(Controls controls, int Id, Grid gridControls, Image[] imageList, Point position)
        {
            this.Id = Id;
            GridControls = gridControls;
            this.controls = controls;

            if (imageList == null)
            {
                throw new Exception("ImageList can not be null!");
            }

            if (imageList.Length < 3)
            {
                throw new Exception("ImageList must contain a background image and at least icons for List and Item!");
            }

            if (imageList[0].ActualWidth < imageList[1].ActualWidth || imageList[0].ActualHeight < imageList[1].ActualHeight)
            {
                throw new Exception("Icon images must be smaller than the backbround image!");
            }

            for (int i = 2; i < imageList.Length; i++)
            {
                if (imageList[i].ActualWidth != imageList[1].ActualWidth || imageList[i].ActualHeight != imageList[1].ActualHeight)
                {
                    throw new Exception("Icon images, all images except imageList[0], must be of the same size.");
                }
            }

            if (imageList.Length == 3)
            {
                // No indentation image supplied, create one by copying the first icon image:
                ImageCopy ImageCopy = new ImageCopy(imageList[1]);
                imageList.Append(ImageCopy.Image);
            }

            this.HitArea = new Rect(position.X, position.Y, imageList[0].ActualWidth, imageList[0].ActualHeight);

            CopyImages(imageList);
            TreeViewFolders = new List<TreeViewFolder>();
            TreeViewItems = new List<TreeViewItem>();
            ControlSizing = new ControlSizing(controls, this);
        }

        //public TreeView AddTreeView(Controls controls, int Id, Grid gridControls, Image[] imageList, Point position)
        //{
        //    TreeView treeView = new TreeView(controls, Id, gridControls, imageList, position);
        //    return treeView;
        //}

        public void AddTreeViewFolder()
        {

        }

        public void AddTreeViewItem()
        {

        }

        public void ExpandTreeViewFolder()
        {

        }

        public void CollapseTreeViewFolder()
        {

        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// TreeViewList class. Handles a list of TreeViewList and a list of TreeViewItem.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class TreeViewFolder : ControlBase
    {
        public List<TreeViewFolder> TreeViewFolders { get; set; }
        public List<TreeViewItem> TreeViewItems { get; set; }

        private Controls controls;

        public TreeViewFolder(Controls controls, Rect AppSize, Image ClickArea, int Id, Grid gridControls, Image[] imageList, Rect HitArea)
        {
            this.Id = Id;
            GridControls = gridControls;
            this.controls = controls;

            if (imageList != null)
            {
                this.HitArea = new Rect(HitArea.Left, HitArea.Top, imageList[0].ActualWidth, imageList[0].ActualHeight);
            }
            else
            {
                this.HitArea = new Rect(HitArea.Left, HitArea.Top, HitArea.Width, HitArea.Height);
            }

            CopyImages(imageList);
            ControlSizing = new ControlSizing(controls, this);
        }

        //public TreeViewFolder AddTreeViewFolder(int Id, Grid gridControls, Rect HitArea)
        //{
        //    TreeViewFolder treeViewFolder = new TreeViewFolder(controls, Id, gridControls,
        //        new Rect(this.HitArea.Left + HitArea.Left, this.HitArea.Top + HitArea.Top, HitArea.Width, HitArea.Height));
        //    return treeViewFolder;
        //}

    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// TreeViewItem class. Handles a list of TreeViewItem.
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class TreeViewItem : ControlBase
    {
        public TreeViewItem(Controls controls, Rect AppSize, Image ClickArea, int Id, int subType, Grid gridControls, Image[] imageList, Rect HitArea)
        {
            this.Id = Id;
            GridControls = gridControls;

            if (imageList != null)
            {
                this.HitArea = new Rect(HitArea.Left, HitArea.Top, imageList[0].ActualWidth, imageList[0].ActualHeight);
            }
            else
            {
                this.HitArea = new Rect(HitArea.Left, HitArea.Top, HitArea.Width, HitArea.Height);
            }

            CopyImages(imageList);
            ControlSizing = new ControlSizing(controls, this);
        }

        //public AreaButton AddAreaButton(int Id, Grid gridControls, Rect HitArea)
        //{
        //    AreaButton areaButton = new AreaButton(controls, Id, gridControls,
        //        new Rect(this.HitArea.Left + HitArea.Left, this.HitArea.Top + HitArea.Top, HitArea.Width, HitArea.Height));
        //    SubControls.ControlsList.Add(areaButton);
        //    return areaButton;
        //}

    }
}
