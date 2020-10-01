using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace TempusToMomentumMapRenamer
{
    /// <summary>
    ///     Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly List<string> _steamGamePaths = new List<string>();
        private readonly string _steamPath;

        public SettingsWindow()
        {
            InitializeComponent();

            _steamPath = (string) Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", null);

            // Adds the default library path
            _steamGamePaths.Add(Path.Join(_steamPath, "steamapps", "common"));

            // Get the additional library paths
            var libraryFile = Path.Combine(_steamPath, "steamapps", "libraryfolders.vdf");
            var libraryFileText = File.ReadAllText(libraryFile);

            var libraryDirectories =
                Regex.Matches(libraryFileText, "^\\s*\"\\d*\"\\s*\"([^\"]*)\"", RegexOptions.Multiline);
            foreach (Match match in libraryDirectories)
            {
                _steamGamePaths.Add(match.Groups[1].Value);
            }

            if (TryGetLibraryPathWithGame("Team Fortress 2", out var tf2LibraryPath))
            {
                SourcePathTextBox.Text = Path.Join(tf2LibraryPath, "Team Fortress 2", "tf", "download", "maps");
            }

            if (TryGetLibraryPathWithGame("Momentum Mod", out var mmodLibraryPath))
            {
                DestinationPathTextBox.Text = Path.Join(mmodLibraryPath, "Momentum Mod", "momentum", "maps");
            }
        }

        private bool TryGetLibraryPathWithGame(string gameName, out string libraryPath)
        {
            foreach (var steamGamePath in _steamGamePaths)
            {
                if (Directory.Exists(Path.Join(steamGamePath, gameName)))
                {
                    libraryPath = steamGamePath;
                    return true;
                }
            }

            libraryPath = null;
            return false;
        }

        private void SourcePathSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Select the folder with TF2 Maps",
                UseDescriptionForTitle = true
            };

            var result = dialog.ShowDialog(this);

            if (result.HasValue && result.Value)
            {
                SourcePathTextBox.Text = dialog.SelectedPath;
            }
        }

        private void DestinationPathSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Select the folder with Momentum Mod Maps",
                UseDescriptionForTitle = true
            };

            var result = dialog.ShowDialog(this);

            if (result.HasValue && result.Value)
            {
                DestinationPathTextBox.Text = dialog.SelectedPath;
            }
        }

        private void OpenMapSelectorButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(SourcePathTextBox.Text))
            {
                MessageBox.Show("TF2 Map Path does not exist", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(DestinationPathTextBox.Text))
            {
                MessageBox.Show("Momentum Mod Map Path does not exist", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var mapSelectorWindow = new MapSelectorWindow(SourcePathTextBox.Text, DestinationPathTextBox.Text, DownloadMissingMapCheckbox.IsChecked ?? false, CopyToMomentumModCheckbox.IsChecked ?? false);
            mapSelectorWindow.Show();
            Close();
        }
    }
}