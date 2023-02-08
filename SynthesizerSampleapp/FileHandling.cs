using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.Data.Json;
using System.Runtime.Serialization.Json;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.Serialization;
using UwpControlsLibrary;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;

namespace SynthesizerSampleapp
{
    public sealed partial class MainPage : Page
    {
        public Folder Patches;
        private int menuId;

        private async void InitPatches()
        {
            Patches = new Folder("Root", 0);
            menuId = menu.Id + 1;
            Folder tempPatches = null;
            for (int i = 0; i < 3; i++) 
            {
                tempPatches = new Folder("Patchlist " + i.ToString(), i);
                for (int j = 0; j < 3; j++)
                {
                    Patch patch = new Patch();
                    patch.Id = i * 3 +j;
                    patch.Name = "Patch " + i.ToString() + j.ToString();
                    patch.KeyFollow = 127;
                    patch.Gain = 63;
                    patch.Sustain = 127;
                    tempPatches.PatchList.Add(patch);
                }
                Patches.FolderList.Add(tempPatches);
            }
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            String fileContent = JsonConvert.SerializeObject(Patches, 
                Newtonsoft.Json.Formatting.Indented);
            StorageFile sampleFile = await localFolder.CreateFileAsync("Patches.json",
                   CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sampleFile, fileContent);


            //FileStream stream = File.OpenWrite("Patches");
            //string patches = JsonSerializer.DeserializeAsync<Patches>(;
            //string patches = JsonSerializer.Serialize(Patches);
            //DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(Patches.GetType());
            //string patches =  dataContractJsonSerializer.Read
        }

        private void ClearPatchMenus(PopupMenuButton menu)
        {
            foreach (List<PopupMenuButton> folders in menu.Children)
            {
                foreach (PopupMenuButton folder in folders)
                {

                }
            }
        }

        private async Task<int> LoadPatches()
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile sampleFile = await localFolder.GetFileAsync("Patches.json");
                String jsonString = await FileIO.ReadTextAsync(sampleFile);
                if (string.IsNullOrEmpty(jsonString))
                {
                    InitPatches();
                }
                else
                {
                    Patches = JsonConvert.DeserializeObject<Folder>(jsonString);
                    if (Patches == null
                        || Patches.FolderList == null
                        || Patches.PatchList == null
                        || (Patches.FolderList.Count == 0 && Patches.PatchList.Count == 0))
                    {
                        InitPatches();
                    }
                    else
                    {
                        menuId = menu.Id + 1;
                        ConnectParents(Patches);
                        CreatePatchMenuItems(Patches, menu);
                    }
                }
            }
            catch
            {
                InitPatches();
            }
            return 0;
        }

        private async Task<int> StorePatches()
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile sampleFile = await localFolder.GetFileAsync("Patches.json");
                String jsonString = JsonConvert.SerializeObject(Patches, Newtonsoft.Json.Formatting.Indented);
                await FileIO.WriteTextAsync(sampleFile, jsonString);
            }
            catch
            {
            }
            return 0;
        }

        private void ConnectParents(Folder patches)
        {
            foreach (Folder subPatches in patches.FolderList)
            {
                ConnectParents(subPatches);
            }

            foreach (Patch patch in patches.PatchList)
            {
                patch.Parent = patches;
            }
        }

        /// <summary>
        /// From PATCHES button, travels the tree of foldres and patches
        /// to build a menu tree. Each tree item is marked in its Tag
        /// with corresponding folder or patch
        /// </summary>
        /// <param name="patches"></param>
        /// <param name="menuItem"></param>
        private void CreatePatchMenuItems(Folder patches, PopupMenuButton menuItem)
        {
            int subMenuId;
            int itemNumber = 0;
            if (patches.FolderList.Count > 0)
            {
                subMenuId = menuItem.AddMenu();
                foreach (Folder subPatches in patches.FolderList)
                {
                    PopupMenuButton subMenu = AddFolderPopupMenuButton(menuItem, subMenuId, itemNumber++, subPatches);
                    CreatePatchMenuItems(subPatches, subMenu);
                }
            }

            if (patches.PatchList.Count > 0)
            {
                subMenuId = menuItem.AddMenu();
                foreach (Patch patch in patches.PatchList)
                {
                    AddPatchPopupMenuButton(patch, menuItem, subMenuId, itemNumber++);
                }
            }
        }

        private PopupMenuButton AddFolderPopupMenuButton(PopupMenuButton menuItem, int subMenuId, int itemNumber, Folder subPatches)
        {
            PopupMenuButton subMenu =
                menuItem.AddMenuItem(subMenuId, itemNumber, new Image[] { imgMenuButton },
                ControlBase.PopupMenuButtonStyle.MENU,
                new ControlBase.PointerButton[] { ControlBase.PointerButton.LEFT, ControlBase.PointerButton.RIGHT, ControlBase.PointerButton.OTHER },
                subPatches.Name, 14, true, ControlBase.ControlTextWeight.BOLD, ControlBase.ControlTextAlignment.LEFT,
                -0.97, 0.5, -0.1,
                new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)), new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)));
            subMenu.Tag = subPatches;
            subMenu.Parent = menuItem;
            return subMenu;
        }

        private PopupMenuButton AddPatchPopupMenuButton(Patch patch, PopupMenuButton menuItem, int subMenuId, int itemNumber)
        {
            PopupMenuButton patchButton =
                menuItem.AddMenuItem(subMenuId, itemNumber, new Image[] { imgPatchButton },
                ControlBase.PopupMenuButtonStyle.MENU,
                new ControlBase.PointerButton[] { ControlBase.PointerButton.LEFT, ControlBase.PointerButton.RIGHT, ControlBase.PointerButton.OTHER },
                patch.Name, 14, true, ControlBase.ControlTextWeight.BOLD, ControlBase.ControlTextAlignment.LEFT,
                -0.97, 0.5, -0.1,
                new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)), new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)));
            patchButton.Tag = patch;
            patchButton.Parent = menuItem;
            patchButton.Id = menuId++;
            patch.Id = subMenuId;
            return patchButton;
        }

        /// <summary>
        /// Adds a folder to given folder.
        /// </summary>
        /// <param name="popupMenuButton">Folder to contain the new patch or folder.</param>
        private async Task AddFolder(PopupMenuButton popupMenuButton)
        {
            NewItem folderName = new NewItem(NewItem.Type.FOLDER);
            await folderName.ShowAsync();
            if (folderName.Ok && !string.IsNullOrEmpty(folderName.Text))
            {
                if (popupMenuButton.Tag == null)
                {
                    // Add a folder to the root folder:
                    Folder patches = new Folder(folderName.Text);
                    patches.Parent = Patches;
                    Patches.FolderList.Add(patches);
                }
                else
                {
                    // Add a folder to a sub folder:
                    ((Folder)popupMenuButton.Tag).FolderList.Add(new Folder(folderName.Text));
                }
                await StorePatches();
                await LoadPatches();
            }
        }

        /// <summary>
        /// Adds a patch to given folder.
        /// </summary>
        /// <param name="popupMenuButton">Folder to contain the new patch or folder.</param>
        private async void AddPatch(PopupMenuButton popupMenuButton)
        {
            NewItem patchName = new NewItem(NewItem.Type.PATCH);
            await patchName.ShowAsync();
            if (patchName.Ok && !string.IsNullOrEmpty(patchName.Text))
            {
                if (popupMenuButton.Tag == null)
                {
                    // Add a patch to the root folder:
                    Patches.PatchList.Add(CreatePatch());
                }
                else
                {
                    // Add a patch to a sub folder:
                    Patch patch = CreatePatch();
                    patch.Name = patchName.Text;
                    patch.Id = ((Folder)popupMenuButton.Tag).PatchList[((Folder)popupMenuButton.Tag).PatchList.Count - 1].Id + 1;
                    ((Folder)popupMenuButton.Tag).PatchList.Add(patch);

                    // Add a PopupMenuButton item:
                    PopupMenuButton button = 
                        AddPatchPopupMenuButton(patch, popupMenuButton,
                        popupMenuButton.Id, popupMenuButton.Children[0].Count);
                    Controls.ResizeControls(gridMain, Window.Current.Bounds);
                    UpdatePatch(patch);
                }
                await StorePatches();
            }
        }

        public async void SavePatch(Patch patch)
        {
            Confirm confirm = new Confirm("Are you sure you want to overwrite this patch?");
            await confirm.ShowAsync();
            if (confirm.Ok)
            {
                //UpdatePatch(patch);
                await StorePatches();
            }
        }

        public async Task<bool> DeletePatch(Patch patch)
        {
            Confirm confirm = new Confirm("Are you sure you want to delete this patch?");
            await confirm.ShowAsync();
            if (confirm.Ok)
            {
                patch.Parent.PatchList.Remove(patch);
                await StorePatches();
                await CoreApplication.RequestRestartAsync("Application Restart Programmatically ");
            }
            return confirm.Ok;
        }

        public async void DeleteFolder(Folder folder)
        {
            Confirm confirm = new Confirm("Are you sure you want to delete this folder?");
            await confirm.ShowAsync();
            if (confirm.Ok)
            {
                ((Folder)folder.Parent).FolderList.Remove(folder);
                await StorePatches();
                await LoadPatches();
            }
        }

        private void UpdatePatch(Patch patch)
        {
            patch.Vibrato = slVibrato.Value;
            patch.Vibrato = slVibrato.Value;
            patch.Tremolo = slTremolo.Value;
            patch.Speed = slLFOFrequency.Value;
            patch.Phase = slPhase.Value;
            patch.LfoPhase = slLfoPhase.Value;
            patch.Q = slFilterQ.Value;
            patch.Frequency = slFilterFreq.Value;
            patch.KeyFollow = slFilterKeyFollow.Value;
            patch.Gain = slFilterGain.Value;
            patch.Attack = slADSR_A.Value;
            patch.Decay = slADSR_D.Value;
            patch.Sustain = slADSR_S.Value;
            patch.Release = slADSR_R.Value;
            patch.Reverb = slReverb.Value;
            patch.Waveform = waveform.Selection;
            patch.Filter = filter.Selection;
            patch.Chorus = ibChorus.IsOn ? 1 : 0;
            patch.UseAdsr = useAdsr.Selection;
        }

        private Patch CreatePatch()
        {
            Patch patch = new Patch();
            UpdatePatch(patch);
            return patch;
        }
    }
}
