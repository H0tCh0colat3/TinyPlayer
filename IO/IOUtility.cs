using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using TinyPlayer.Enums;

namespace TinyPlayer.IO
{
    public static class IOUtility
    {
        private static readonly Dictionary<FileDialogFilter, (string name, string extensions)> _fileDialogFilterValues = new Dictionary<FileDialogFilter, (string name, string extensions)>()
        {
            { FileDialogFilter.AllFiles, ("All Files", "*.*") },
            { FileDialogFilter.AudioFiles, ("Audio files", "*.wav; *.mp3; *.wma; *.ogg; *.flac") }
        };

        private static string OpenDialog(string initialDirectory, DependencyObject currentElement, string title, bool isFolderPicker, params FileDialogFilter[] filters)
        {
            string result = "";
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Title = title;
                dialog.IsFolderPicker = isFolderPicker;
                dialog.InitialDirectory = initialDirectory;

                if (!isFolderPicker && filters?.Any() == true)
                {
                    foreach (var item in filters)
                    {
                        var (name, extensions) = _fileDialogFilterValues[item];
                        dialog.Filters.Add(new CommonFileDialogFilter(name, extensions));
                    }
                }

                var dialogResult = dialog.ShowDialog();
                if (currentElement != null)
                {
                    if (currentElement is Window w)
                    {
                        w.Focus();
                    }
                    else
                    {
                        Window.GetWindow(currentElement).Focus();
                    }
                }
                if (dialogResult == CommonFileDialogResult.Ok)
                {
                    result = dialog.FileName;
                }
            }
            return result;
        }

        public static string OpenFolderDialog(string initialDirectory, DependencyObject currentElement = null, string title = "Select a folder")
        {
            return OpenDialog(initialDirectory, currentElement, title, true);
        }

        public static string OpenFileDialog(string initialDirectory, DependencyObject currentElement = null, string title = "Select a File", params FileDialogFilter[] filters)
        {
            return OpenDialog(initialDirectory, currentElement, title, false, filters);
        }
    }
}