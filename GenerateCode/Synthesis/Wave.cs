using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;

namespace SynthLab
{
    /// <summary>
    /// Uses a number of sample files from an instrument.
    /// Rather than using one sample for each key, some
    /// ranges are used around selected keys.
    /// FileInput.PlaybackSpeedFactor is used to make
    /// each key present a transposed sound.
    /// </summary>
    public class Wave
    {
        private MainPage mainPage;
        public StorageFolder FilesLocation;
        public int FileCount;
        public List<string> fileParsingResults;
        public List<WaveKeyFile> waveKeyFiles;
        public List<WaveKeyRange> waveKeyRanges;
        public List<StorageFile> storageFiles;
        public AudioFileInputNode[] Keys;
        private int lowKey;

        public Wave(MainPage mainPage, StorageFolder filesLocation)
        {
            this.mainPage = mainPage;
            FilesLocation = filesLocation;
            Init();
        }

        public async void Init()
        {
            fileParsingResults = new List<string>();
            waveKeyFiles = new List<WaveKeyFile>();
            waveKeyRanges = new List<WaveKeyRange>();
            storageFiles = new List<StorageFile>();
            Keys = new AudioFileInputNode[128];
            int keyDiff;
            if (FilesLocation != null)
            {
                try
                {
                    IReadOnlyList<StorageFile> files = await FilesLocation.GetFilesAsync();
                    foreach (StorageFile file in files)
                    {
                        StorageApplicationPermissions.FutureAccessList.Add(file);
                        //await GetWaveFile(file);
                        string[] parts = file.Name.Split('_');
                        int waveKey = -1; // waveKey is the key the file is played at its natural speed.
                        if (parts.Length > 1)
                        {
                            waveKey = UnpackKeyNumber(parts[0]);
                        }

                        if (waveKey > -1)
                        {
                            waveKeyFiles.Add(new WaveKeyFile(waveKey, file));
                        }
                    }

                    if (waveKeyFiles.Count > 0)
                    {
                        // Fill out missing AudioFileInputNodes with
                        // transposed AudioFileInputNodes from nearest
                        // given AudioFileInputNode:
                        int from = 0;
                        int to = 127;
                        waveKeyFiles.Sort();
                        lowKey = waveKeyFiles[0].KeyNumber;
                        for (int i = 0; i < waveKeyFiles.Count; i++)
                        {
                            from = waveKeyFiles[i].KeyNumber;
                            if (i > 0)
                            {
                                from = waveKeyRanges[i - 1].To + 1;
                            }

                            to = waveKeyFiles[i].KeyNumber;
                            if (i < waveKeyFiles.Count - 1)
                            {
                                keyDiff = waveKeyFiles[i + 1].KeyNumber - waveKeyFiles[i].KeyNumber;
                                to += (keyDiff) / 2;
                            }

                            waveKeyRanges.Add(new WaveKeyRange(waveKeyFiles[i].StorageFile, from, waveKeyFiles[i].KeyNumber, to));
                        }

                        // Add all 'missing' files:
                        foreach (WaveKeyRange waveKeyRange in waveKeyRanges)
                        {
                            for (int i = waveKeyRange.From; i <= waveKeyRange.To; i++)
                            {
                                if (waveKeyRange.Original == i)
                                {
                                    // Add the original file:
                                    storageFiles.Add(waveKeyRange.File);
                                }
                                else
                                {
                                    // Add new in between file:
                                    string name = i.ToString() + waveKeyRange.File.Path.Remove(0, waveKeyRange.File.Path.LastIndexOf('.'));
                                    storageFiles.Add(await waveKeyRange.File.CopyAsync(FilesLocation, name, NameCollisionOption.ReplaceExisting));
                                }
                            }
                        }
                    }
                }
                catch { }

                foreach (WaveKeyRange range in waveKeyRanges)
                {
                    await MakeAudioFileInputNodes(range);
                }
            }
        }

        public void Start(int key, double gain)
        {
            if (Keys[key] != null)
            {
                Keys[key].OutgoingGain = gain;
                Keys[key].Start();
            }
        }

        public void Reset(int key)
        {
            if (Keys[key] != null)
            {
                Keys[key].Reset();
            }
        }

        public void Stop(int key)
        {
            if (Keys[key] != null)
            {
                Keys[key].Stop();
            }
        }

        public async Task GetWaveFolder()
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add(".wav");
            folderPicker.FileTypeFilter.Add(".mp3");
            folderPicker.CommitButtonText = "Ok";
            folderPicker.ViewMode = PickerViewMode.Thumbnail;
            FilesLocation = await folderPicker.PickSingleFolderAsync();
            List<int> keyPositions = new List<int>();

        }

        //private async Task MakeAudioFiles(WaveKeyRange waveKeyRange)
        //{
        //    for (int i = waveKeyRange.From; i <= waveKeyRange.To; i++)
        //    {
        //        string name = waveKeyRange.File.Path.Remove(0, waveKeyRange.File.Path.LastIndexOf("\\") + 1);
        //        name = name.Remove(name.LastIndexOf("_"));
        //        name += waveKeyRange.File.Path.Remove(0, waveKeyRange.File.Path.LastIndexOf('.') + 1);
        //        await waveKeyRange.File.CopyAsync(FilesLocation, i.ToString(), NameCollisionOption.FailIfExists);
        //    }
        //}

        //private async Task MakeAudioFile(StorageFile file, int key)
        //{
        //    string name = file.Path.Remove(0, file.Path.LastIndexOf("\\") + 1);
        //    name = name.Remove(name.LastIndexOf("_"));
        //    name += file.Path.Remove(0, file.Path.LastIndexOf('.') + 1);
        //    await file.CopyAsync(FilesLocation, key.ToString(), NameCollisionOption.FailIfExists);
        //}

        private async Task MakeAudioFileInputNodes(WaveKeyRange waveKeyRange)
        {
            // FrequencyFactor
            for (int i = waveKeyRange.From; i <= waveKeyRange.To; i++)
            {
                await MakeAudioFileInputNode(storageFiles[i - lowKey], i,
                    waveKeyRange.FrequencyFactors[i - waveKeyRange.From]);
            }
        }

        private async Task MakeAudioFileInputNode(StorageFile file, int key, double frequencyFactor)
        {
            //StorageFile.CreateStreamedFileAsync(file.Name, )
            //Stream stream = new MemoryStream()
            //MemoryMappedFile mmf = MemoryMappedFile.CreateNew(key.ToString(), file.);
            //MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(file.Path);
            CreateAudioFileInputNodeResult fileInputResult = 
                await mainPage.FrameServer.theAudioGraph.CreateFileInputNodeAsync(file);
            if (fileInputResult.Status != AudioFileNodeCreationStatus.Success)
            {
                fileParsingResults.Add("The file " + file.Name + " could not be opened by the AudioGraph.");
            }
            else
            {
                fileInputResult.FileInputNode.PlaybackSpeedFactor = frequencyFactor;
                Keys[key] = fileInputResult.FileInputNode;
                Keys[key].AddOutgoingConnection(mainPage.FrameServer.Mixer);
                Keys[key].Stop();
            }
        }

        private async Task GetWaveFile(StorageFile file)
        {
            string[] parts = file.Name.Split('_');
            int key = -1;
            if (parts.Length > 1)
            {
                key = UnpackKeyNumber(parts[0]);
            }

            if (key < 0)
            {
                fileParsingResults.Add("The file " + file.Name + " does not follow the format k_name.ext nor the format K#o_name.ext " +
                    "where k = MIDI key number (0 - 127), K = Key name (C/D/E/F/G/A/B) # is optional, o is octave number 0 - 7, " +
                    "first key is the key the sample normal key, second key is the lowest pitch key, third is the highest pitch key and " +
                    "name is any text describing the sound or nothing and ext is one of wav or mp3!");
                return;
            }

            CreateAudioFileInputNodeResult fileInputResult = await mainPage.FrameServer.theAudioGraph.CreateFileInputNodeAsync(file);
            if (fileInputResult.Status != AudioFileNodeCreationStatus.Success)
            {
                fileParsingResults.Add("The file " + file.Name + " could not be opened by the AudioGraph.");
            }
            else
            {
                Keys[key] = fileInputResult.FileInputNode;
                Keys[key].AddOutgoingConnection(mainPage.FrameServer.Mixer);
                Keys[key].Stop();
            }
        }

        private int UnpackKeyNumber(string s)
        {
            string keyNames = "CCDDEFFGGAAB";
            int key = 0;
            if (!keyNames.Contains(s.Remove(1)))
            {
                int n = int.Parse(s);
                if (n > -1 && n < 128)
                {
                    return n;
                }
            }
            else
            {
                s = s.ToUpper();
                int n = keyNames.IndexOf(s.Remove(1));
                if (n > -1)
                {
                    key += n;
                    if (s.Contains("#"))
                    {
                        key++;
                    }
                    s = s.Replace("#", "");
                    s = s.Remove(0, 1);
                    n = int.Parse(s);
                    if (n > -1 && n < 10)
                    {
                        n = key + 12 * n;
                        return n > 127 ? 127 : n;
                    }
                }
            }
            return -1;
        }
    }

    public class WaveKeyFile : IComparable
    {
        public int KeyNumber { get; set; }
        public StorageFile StorageFile { get; set; }

        public WaveKeyFile(int KeyNumber, StorageFile StorageFile)
        {
            this.KeyNumber = KeyNumber;
            this.StorageFile = StorageFile;
        }

        public int CompareTo(object obj)
        {
            return (KeyNumber > ((WaveKeyFile)obj).KeyNumber) ? 1 : -1;
        }
    }

    public class WaveKeyRange : IComparable
    {
        public StorageFile File;
        public int From;
        public int Original;
        public int To;
        public List<double> FrequencyFactors;
        private double frequencyFactor;

        public WaveKeyRange(StorageFile File, int From, int Original, int To)
        {
            this.File = File;
            this.From = From;
            this.Original = Original;
            this.To = To;
            frequencyFactor = Math.Pow(2.0, 1.0 / 12.0);
            FrequencyFactors = new List<double>();
            for (int i = From; i <= To; i++)
            {
                FrequencyFactors.Add(Math.Pow(frequencyFactor, i - Original));
            }
        }

        public int CompareTo(object obj)
        {
            return (Original > ((WaveKeyRange)obj).Original) ? 1 : -1;
        }
    }
}
