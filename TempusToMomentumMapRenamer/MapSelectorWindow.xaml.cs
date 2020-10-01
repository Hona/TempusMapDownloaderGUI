using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using TempusToMomentumMapRenamer.Models;
using TempusToMomentumMapRenamer.Utilities;

namespace TempusToMomentumMapRenamer
{
    /// <summary>
    ///     Interaction logic for MapSelectorWindow.xaml
    /// </summary>
    public partial class MapSelectorWindow : Window
    {
        private readonly string _destinationMapPath;
        private readonly string _sourceMapPath;
        private bool _isCopying;
        private int _mapCopyCount;
        private bool _downloadMissingMaps;
        private bool _copyToMomentumMod;

        private delegate void UpdateLogText(string log);
        public MapSelectorWindow(string sourceMapPath, string destinationMapPath, bool downloadMissingMaps, bool copyToMomentumMod)
        {
            InitializeComponent();

            _sourceMapPath = sourceMapPath;
            _destinationMapPath = destinationMapPath;
            _downloadMissingMaps = downloadMissingMaps;
            _copyToMomentumMod = copyToMomentumMod;

            DataContext = this;
        }

        public ObservableCollection<MapData> MapData { get; set; } = new ObservableCollection<MapData>();

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var data = await MapDataUtility.GetMapDataAsync();

            data = data.OrderBy(x => x.Name).ToList();

            if (!_downloadMissingMaps)
            {
                var files = Directory.GetFiles(_sourceMapPath).Select(x => x.Split(Path.DirectorySeparatorChar).Last())
                    .ToList();
                var mapsNotFound = data.Where(x => !files.Contains(x.Name + ".bsp")).ToList();
                MessageBox.Show(
                    $"{mapsNotFound.Count} Tempus Maps not found, not copying {Environment.NewLine + Environment.NewLine}{string.Join(" ", mapsNotFound.Select(x => x.Name))}");
            }

            foreach (var entry in data)
            {
                // User might not have every map
                if (_downloadMissingMaps || File.Exists(Path.Join(_sourceMapPath, entry.Name + ".bsp")))
                {
                    MapData.Add(entry);
                }
            }
        }

        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isCopying)
            {
                return;
            }

            _isCopying = true;
            _mapCopyCount = 0;

            var selectedData = MapData.Where(x => x.ToCopy).ToList();

            var total = selectedData.Count;

            await MapRenamerUtility.RenameMapsAsync(selectedData, _sourceMapPath, _destinationMapPath, _downloadMissingMaps, _copyToMomentumMod, finishedMap =>
            {
                _mapCopyCount++;
                CopyStatusLabel.Text = $"Copied {_mapCopyCount} out of {total} | {finishedMap}";
            }, log =>
            {
                if (LoggerLabel.Dispatcher.CheckAccess())
                {
                    LoggerLabel.Text = log;
                }
                else
                {
                    LoggerLabel.Dispatcher.Invoke(() => LoggerLabel.Text = log);
                }
            });

            LoggerLabel.Dispatcher.DisableProcessing();
            CopyStatusLabel.Text = "Done!";
            _isCopying = false;
        }
    }
}