using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private string selectedFilePath;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTreeView();
        }

        private void InitializeTreeView()
        {
            LoadRootDirectories();
        }

        // Funkcja ładująca dyski
        private void LoadRootDirectories()
        {
            foreach (var drive in Directory.GetLogicalDrives())
            {
                var item = new TreeViewItem() { Header = drive, Tag = drive };
                item.Expanded += Folder_Expanded;
                item.Items.Add(null);
                FileTreeView.Items.Add(item);
                item.IsExpanded = true;
            }
        }

        // Funkcja umożliwiająca rozwijanie folderów
        private async void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == null)
            {
                item.Items.Clear();
                string fullPath = (string)item.Tag;

                try
                {
                    foreach (var dir in await Task.Run(() => Directory.GetDirectories(fullPath)))
                    {
                        var subItem = new TreeViewItem() { Header = System.IO.Path.GetFileName(dir), Tag = dir };
                        subItem.Expanded += Folder_Expanded;
                        subItem.Items.Add(null);
                        item.Items.Add(subItem);
                    }

                    foreach (var file in await Task.Run(() => Directory.GetFiles(fullPath)))
                    {
                        var subItem = new TreeViewItem() { Header = System.IO.Path.GetFileName(file), Tag = file };
                        item.Items.Add(subItem);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Nie można wczytać: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (e.NewValue is TreeViewItem selectedItem)
                {
                    string? path = selectedItem.Tag as string;
                    if (File.Exists(path))
                    {
                        selectedFilePath = path;
                        ShowFile(path);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd pliku: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Funkcja podglądu obrazów i plików tekstowych
        private void ShowFile(string path)
        {
            try
            {
                string extension = System.IO.Path.GetExtension(path).ToLower();
                if (extension == ".txt" || extension == ".csv" || extension == ".log" || extension == ".ini")
                {
                    TextBoxPreview.Text = File.ReadAllText(path);
                    TextBoxPreview.Visibility = Visibility.Visible;
                    ImagePreview.Visibility = Visibility.Collapsed;
                }
                else if (extension == ".jpg" || extension == ".png")
                {
                    ImagePreview.Source = new BitmapImage(new Uri(path));
                    ImagePreview.Visibility = Visibility.Visible;
                    TextBoxPreview.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd wczytywania pliku: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Funkcja kopiowania plików
        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFilePath != null)
            {
                var dialog = new SaveFileDialog();
                dialog.InitialDirectory = Path.GetDirectoryName(selectedFilePath);
                dialog.FileName = Path.GetFileName(selectedFilePath);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        File.Copy(selectedFilePath, dialog.FileName);
                        MessageBox.Show("Pomyślnie skopiowano plik (wymaga ponownego uruchomienia programu).", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Nie można skopiować pliku: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Nie wybrano pliku.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Funkcja przenoszenia plików
        private void MoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFilePath != null)
            {
                var dialog = new SaveFileDialog();
                dialog.InitialDirectory = Path.GetDirectoryName(selectedFilePath);
                dialog.FileName = Path.GetFileName(selectedFilePath);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        File.Move(selectedFilePath, dialog.FileName);
                        MessageBox.Show("Pomyślnie przeniesiono plik (wymaga ponownego uruchomienia programu).", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Nie można przenieść pliku: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Nie wybrano pliku.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Funkcja usuwająca pliki
        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFilePath != null)
            {
                var result = MessageBox.Show("Czy na pewno chcesz usunąć ten plik?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        File.Delete(selectedFilePath);
                        MessageBox.Show("Pomyślnie usunięto plik (wymaga ponownego uruchomienia programu).", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Nie można usunąć pliku: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Nie wybrano pliku.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}

