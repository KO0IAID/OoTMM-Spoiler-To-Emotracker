using Microsoft.Win32;
using SpoilerToTrackerConverter.Emotracker.Controller;
using SpoilerToTrackerConverter.SpoilerLog.Controller;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace SpoilerToTracker
{
    public partial class MainWindow : Window
    {
        Tracker tracker = new();
        Spoiler spoiler = new();
        private IEnumerable<object>? currentCategoryData;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Window
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging with left mouse button
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    // Double-click to toggle maximize/restore
                    this.WindowState = this.WindowState == WindowState.Maximized
                        ? WindowState.Normal
                        : WindowState.Maximized;
                }
                else
                {
                    this.DragMove();
                }
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
        private void MainGrid_DragOver(object sender, DragEventArgs e)
        {
            // Only allow if data contains files
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy; // Show copy cursor
            }
            else
            {
                e.Effects = DragDropEffects.None; // No drop allowed
            }

            e.Handled = true;
        }
        #endregion
        #region Search Box
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;

            if (textbox != null && textbox.Text == "Search...")
            {
                textbox.Foreground = new SolidColorBrush(Colors.White);
                textbox.Text = string.Empty;
            }
        }
        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;

            if (textbox != null && textbox.Text == "")
            {
                textbox.Foreground = new SolidColorBrush(Colors.DimGray);
                textbox.Text = "Search...";
            }
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentCategoryData == null)
                return;

            string query = SearchTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                // Reset to full list if search box is cleared
                SpoilerDataGrid.ItemsSource = new ObservableCollection<object>(currentCategoryData);
                return;
            }

            // Case-insensitive search on all public string properties
            var filtered = currentCategoryData.Where(item =>
            {
                var props = item.GetType().GetProperties()
                    .Where(p => p.PropertyType == typeof(string));

                foreach (var prop in props)
                {
                    var value = prop.GetValue(item) as string;
                    if (!string.IsNullOrEmpty(value) &&
                        value.Contains(query, StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                return false;
            });

            SpoilerDataGrid.ItemsSource = new ObservableCollection<object>(filtered);
        }

        #endregion
        #region Upload File
        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Select a spoiler file";
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            openFileDialog.Multiselect = false;

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                spoiler = new();
                bool isSpoiler = await spoiler.IsSpoilerAsync(openFileDialog.FileName);

                if (isSpoiler == true)
                {
                    await spoiler.AddFileContents(openFileDialog.FileName);

                    if (spoiler.Parsed)
                    {
                        StatusImage.Source = (BitmapImage)FindResource("Check");
                        FilePathTextBox.Text = openFileDialog.FileName;
                        ConvertButton.IsEnabled = true;
                        ResetButton.IsEnabled = true;
                        CategoryComboBox.IsEnabled = true;
                        CategoryComboBox.SelectedIndex = 0;
                        SearchTextBox.IsEnabled = true;
                        SpoilerDataGrid.Items.Clear();
                        SpoilerDataGrid.Columns.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Unable to parse data from spoiler");
                    }
                }
                else
                {
                    StatusImage.Source = (BitmapImage)FindResource("Cross");
                    FilePathTextBox.Text = openFileDialog.FileName;
                    ConvertButton.IsEnabled = false;
                    ResetButton.IsEnabled = false;
                    CategoryComboBox.IsEnabled = false;
                    CategoryComboBox.SelectedIndex = 0;
                    SearchTextBox.IsEnabled = true;
                    SearchTextBox.Text = "Search...";
                    SpoilerDataGrid.Items.Clear();
                    SpoilerDataGrid.Columns.Clear();

                }
            }



        }
        private async void MainGrid_Drop(object sender, DragEventArgs e)
        {
            // Verify the data actually contains files
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            // Get the dropped files
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);


            string filePath = files.FirstOrDefault();
            if (filePath == null)
                return;

            bool isSpoiler = await spoiler.IsSpoilerAsync(filePath);


            if (isSpoiler == true)
            {
                spoiler = new();
                await spoiler.AddFileContents(filePath);

                if (spoiler.Parsed)
                {
                    StatusImage.Source = (BitmapImage)FindResource("Check");

                    StatusImage.Source = (BitmapImage)FindResource("Check");
                    FilePathTextBox.Text = filePath;
                    ConvertButton.IsEnabled = true;
                    ResetButton.IsEnabled = true;
                    CategoryComboBox.IsEnabled = true;
                    CategoryComboBox.SelectedIndex = 0;
                    SearchTextBox.IsEnabled = true;
                    SpoilerDataGrid.Items.Clear();
                    SpoilerDataGrid.Columns.Clear();
                }
            }
            else
            {
                StatusImage.Source = (BitmapImage)FindResource("Cross");
                ConvertButton.IsEnabled = false;
                ResetButton.IsEnabled = false;
                CategoryComboBox.IsEnabled = false;
                CategoryComboBox.SelectedIndex = 0;
                SearchTextBox.IsEnabled = true;
                SearchTextBox.Text = "Search...";
                SpoilerDataGrid.Items.Clear();
                SpoilerDataGrid.Columns.Clear();
            }

            FilePathTextBox.Text = filePath;

            e.Handled = true;
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            spoiler = new();
            tracker = new();
            ResetButton.IsEnabled = false;
            ConvertButton.IsEnabled = false;
            FilePathTextBox.Text = "No file selected";
            StatusImage.Source = (BitmapImage)FindResource("Search");
            CategoryComboBox.IsEnabled = false;
            CategoryComboBox.SelectedIndex = 0;
            SearchTextBox.IsEnabled = false;
            SearchTextBox.Text = "Search...";
            SpoilerDataGrid.ItemsSource = null;
            SpoilerDataGrid.Columns.Clear();

        }
        #endregion
        #region Export File
        private async void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FilePathTextBox.Text;
            string fileName = System.IO.Path.GetFileName(filePath);

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = ".json",

                // Strip original extension to avoid .txt or others leaking in filename here:
                FileName = $"Tracker - {System.IO.Path.GetFileNameWithoutExtension(fileName)}.json"
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            string outputPath = saveFileDialog.FileName;

            tracker = new();
            if (spoiler.Parsed)
            {
                await tracker.ConvertSpoilerToEmotracker(spoiler, outputPath);
            }
            else
            {
                MessageBox.Show("Unable to parse data from spoiler.");
            }

            if (tracker.Converted)
            {
                StatusImage.Source = (BitmapImage)FindResource("Saved");
            }
        }

        #endregion
        #region Categories
        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!spoiler.Parsed)
                return;

            if (e.AddedItems.Count == 0)
                return;

            var selectedItem = e.AddedItems[0];


            SearchTextBox.Foreground = new SolidColorBrush(Colors.DimGray);
            SearchTextBox.Text = "Search...";


            string category = (selectedItem as ComboBoxItem)?.Content.ToString() ?? selectedItem.ToString();

            switch (category)
            {
                case "Seed Info":
                    if (spoiler.SeedInfo == null)
                        return;

                    UpdateDataGrid(spoiler.SeedInfo, grid =>
                    {
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Name",
                            Binding = new Binding("Name")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Value",
                            Binding = new Binding("Value")
                        });
                    });
                    break;

                case "Game Settings":
                    if (spoiler.GameSettings == null)
                        return;

                    UpdateDataGrid(spoiler.GameSettings, grid =>
                    {
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Name",
                            Binding = new Binding("Name")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Value",
                            Binding = new Binding("Value")
                        });
                    });
                    break;

                case "Starting Items":
                    if (spoiler.StartingItems == null)
                        return;

                    UpdateDataGrid(spoiler.StartingItems, grid =>
                    {
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Item",
                            Binding = new Binding("Name")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Count",
                            Binding = new Binding("Count")
                        });
                    });
                    break;

                case "Conditions":
                    if (spoiler.SpecialConditions == null)
                        return;

                    UpdateDataGrid(spoiler.SpecialConditions, grid =>
                    {
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Type",
                            Binding = new Binding("Type")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Name",
                            Binding = new Binding("Name")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Value",
                            Binding = new Binding("Value")
                        });
                    });
                    break;

                case "World Flags":
                    if (spoiler.WorldFlags == null)
                        return;

                    UpdateDataGrid(spoiler.WorldFlags, grid =>
                    {
                        // Check if any 'World' values exist
                        bool hasWorldValues = spoiler.WorldFlags.Any(wf => !string.IsNullOrWhiteSpace(wf.World));

                        if (hasWorldValues)
                        {
                            grid.Columns.Add(new DataGridTextColumn
                            {
                                Header = "World",
                                Binding = new Binding("World")
                            });
                        }

                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Condition",
                            Binding = new Binding("Condition")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Value",
                            Binding = new Binding("Value")
                        });
                    });
                    break;

                case "Junk Locations":
                    if (spoiler.JunkLocations == null)
                        return;

                    UpdateDataGrid(spoiler.JunkLocations, grid =>
                    {
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Location",
                            Binding = new Binding("Location")
                        });
                    });
                    break;

                case "Glitches":
                    if (spoiler.Glitches == null)
                        return;

                    UpdateDataGrid(spoiler.Glitches, grid =>
                    {
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Description",
                            Binding = new Binding("Description")
                        });
                    });
                    break;

                case "Entrances":
                    if (spoiler.Entrances == null)
                        return;

                    UpdateDataGrid(spoiler.Entrances, grid =>
                    {
                        // Check if any 'World' values exist
                        bool hasWorldValues = spoiler.Entrances.Any(wf => !string.IsNullOrWhiteSpace(wf.World));

                        if (hasWorldValues)
                        {
                            grid.Columns.Add(new DataGridTextColumn
                            {
                                Header = "World",
                                Binding = new Binding("World")
                            });
                        }

                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "From",
                            Binding = new Binding("FromGame")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Entrance",
                            Binding = new Binding("LongEntrance")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "To",
                            Binding = new Binding("ToGame")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Destination",
                            Binding = new Binding("LongDestination")
                        });
                    });
                    break;

                case "Regional Hints":
                    if (spoiler.RegionalHints == null)
                        return;

                    UpdateDataGrid(spoiler.RegionalHints, grid =>
                    {
                        bool hasMultipleWorlds = spoiler.RegionalHints
                            .Where(wf => !string.IsNullOrWhiteSpace(wf.World))
                            .Select(wf => wf.World)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count() > 1;

                        if (hasMultipleWorlds)
                        {
                            grid.Columns.Add(new DataGridTextColumn
                            {
                                Header = "World",
                                Binding = new Binding("World")
                            });
                        }

                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Gossip Stone",
                            Binding = new Binding("GossipStone")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Region",
                            Binding = new Binding("Region")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Item",
                            Binding = new Binding("Item")
                        });
                    });
                    break;

                case "Way of the Hero Hints":
                    if (spoiler.WayOfTheHeroHints == null)
                        return;

                    UpdateDataGrid(spoiler.WayOfTheHeroHints, grid =>
                    {
                        // Check if any 'World' values exist
                        bool hasMultipleWorlds = spoiler.WayOfTheHeroHints
                            .Where(wf => !string.IsNullOrWhiteSpace(wf.World))
                            .Select(wf => wf.World)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count() > 1;

                        if (hasMultipleWorlds)
                        {
                            grid.Columns.Add(new DataGridTextColumn
                            {
                                Header = "World",
                                Binding = new Binding("World")
                            });
                        }

                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Gossip Stone",
                            Binding = new Binding("GossipStone")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Location",
                            Binding = new Binding("Location")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Item",
                            Binding = new Binding("Item")
                        });
                    });
                    break;

                case "Foolish Hints":
                    if (spoiler.FoolishHints == null)
                        return;

                    UpdateDataGrid(spoiler.FoolishHints, grid =>
                    {
                        // Check if any 'World' values exist
                        bool hasMultipleWorlds = spoiler.FoolishHints
                            .Where(wf => !string.IsNullOrWhiteSpace(wf.World))
                            .Select(wf => wf.World)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count() > 1;

                        if (hasMultipleWorlds)
                        {
                            grid.Columns.Add(new DataGridTextColumn
                            {
                                Header = "World",
                                Binding = new Binding("World")
                            });
                        }

                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Gossip Stone",
                            Binding = new Binding("GossipStone")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Location",
                            Binding = new Binding("Location")
                        });
                    });
                    break;

                case "Specific Hints":
                    if (spoiler.SpecificHints == null)
                        return;

                    UpdateDataGrid(spoiler.SpecificHints, grid =>
                    {
                        // Check if any 'World' values exist
                        bool hasMultipleWorlds = spoiler.SpecificHints
                            .Where(wf => !string.IsNullOrWhiteSpace(wf.World))
                            .Select(wf => wf.World)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count() > 1;

                        if (hasMultipleWorlds)
                        {
                            grid.Columns.Add(new DataGridTextColumn
                            {
                                Header = "World",
                                Binding = new Binding("World")
                            });
                        }

                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Gossip Stone",
                            Binding = new Binding("GossipStone")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Location",
                            Binding = new Binding("Location")
                        });
                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Item",
                            Binding = new Binding("Item")
                        });
                    });
                    break;

                case "Way of the Hero Paths":
                    if (spoiler.WayOfTheHeroPaths == null)
                        return;

                    UpdateDataGrid(spoiler.WayOfTheHeroPaths, grid =>
                    {
                        // Check if any 'World' values exist
                        bool hasMultipleWorlds = spoiler.WayOfTheHeroPaths
                            .Where(wf => !string.IsNullOrWhiteSpace(wf.World))
                            .Select(wf => wf.World)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count() > 1;

                        // Check if any 'World' values exist
                        bool hasMultiplePlayers = spoiler.WayOfTheHeroPaths
                            .Where(wf => !string.IsNullOrWhiteSpace(wf.Player))
                            .Select(wf => wf.Player)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count() > 1;


                        if (hasMultipleWorlds)
                        {
                            grid.Columns.Add(new DataGridTextColumn
                            {
                                Header = "World",
                                Binding = new Binding("World")
                            });
                        }

                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Description",
                            Binding = new Binding("Description")
                        });

                        if (hasMultiplePlayers)
                        {
                            grid.Columns.Add(new DataGridTextColumn
                            {
                                Header = "Player",
                                Binding = new Binding("Player")
                            });
                        }

                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Item",
                            Binding = new Binding("Item")
                        });
                    });
                    break;

                case "Spheres":
                    if (spoiler.Spheres == null)
                        return;

                    UpdateDataGrid(spoiler.Spheres, grid =>
                    {
                        // Check if any 'World' values exist
                        bool hasMultipleWorlds = spoiler.Spheres
                            .Where(wf => !string.IsNullOrWhiteSpace(wf.World))
                            .Select(wf => wf.World)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count() > 1;

                        // Check if any 'World' values exist
                        bool hasMultiplePlayers = spoiler.Spheres
                            .Where(wf => !string.IsNullOrWhiteSpace(wf.Player))
                            .Select(wf => wf.Player)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count() > 1;


                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Sphere",
                            Binding = new Binding("Number")
                        });

                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Type",
                            Binding = new Binding("Type")
                        });


                        if (hasMultipleWorlds)
                        {
                            grid.Columns.Add(new DataGridTextColumn
                            {
                                Header = "World",
                                Binding = new Binding("World")
                            });
                        }

                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Location",
                            Binding = new Binding("Location")
                        });

                        if (hasMultiplePlayers)
                        {
                            grid.Columns.Add(new DataGridTextColumn
                            {
                                Header = "Player",
                                Binding = new Binding("Player")
                            });
                        }

                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Item",
                            Binding = new Binding("Item")
                        });
                    });
                    break;

                case "Item Locations":
                    if (spoiler.Spheres == null)
                        return;

                    UpdateDataGrid(spoiler.ItemLocations, grid =>
                    {
                        // Check if any 'World' values exist
                        bool hasMultipleWorlds = spoiler.ItemLocations
                            .Where(wf => !string.IsNullOrWhiteSpace(wf.World))
                            .Select(wf => wf.World)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count() > 1;

                        // Check if any 'World' values exist
                        bool hasMultiplePlayers = spoiler.ItemLocations
                            .Where(wf => !string.IsNullOrWhiteSpace(wf.Player))
                            .Select(wf => wf.Player)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count() > 1;

                        if (hasMultipleWorlds)
                        {
                            grid.Columns.Add(new DataGridTextColumn
                            {
                                Header = "World",
                                Binding = new Binding("World")
                            });
                        }


                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Game",
                            Binding = new Binding("Game")
                        });

                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Region",
                            Binding = new Binding("Region")
                        });

                        if (hasMultiplePlayers)
                        {
                            grid.Columns.Add(new DataGridTextColumn
                            {
                                Header = "Player",
                                Binding = new Binding("Player")
                            });
                        }

                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Description",
                            Binding = new Binding("Description")
                        });

                        grid.Columns.Add(new DataGridTextColumn
                        {
                            Header = "Item",
                            Binding = new Binding("Item")
                        });
                    });
                    break;

                default:
                    SpoilerDataGrid.ItemsSource = null;
                    SpoilerDataGrid.Columns.Clear();
                    break;
            }
        }
        private void UpdateDataGrid<T>(IEnumerable<T> items, Action<DataGrid> configureColumns)
        {
            currentCategoryData = items.Cast<object>().ToList();

            SpoilerDataGrid.ItemsSource = null;
            SpoilerDataGrid.Columns.Clear();
            SpoilerDataGrid.AutoGenerateColumns = false;

            configureColumns(SpoilerDataGrid);

            SpoilerDataGrid.ColumnWidth = new DataGridLength(1, DataGridLengthUnitType.Auto);
            SpoilerDataGrid.ItemsSource = new ObservableCollection<T>(items);
            SpoilerDataGrid.Items.Refresh();
        }
        #endregion
        #region Search
        #endregion
    }
}