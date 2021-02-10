using System;
using System.Collections.Generic;
using System.IO;
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
            { FileDialogFilter.AudioFiles, ("Audio files", "*.wav; *.mp3; *.m4a; *.wma; *.ogg; *.flac") }
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

        public static string GetName(string path, bool includeExtention = true)
        {
            var separatorIndex = path.LastIndexOf(Path.DirectorySeparatorChar);

            var nameOnly = path.Substring(separatorIndex + 1);
            if (!includeExtention)
            {
                if (PathIsFile(path) == true)
                {
                    return Path.GetFileNameWithoutExtension(nameOnly);
                }
            }
            return nameOnly;
        }

        public static bool? PathIsFile(string path)
        {
            try
            {
                var attributes = File.GetAttributes(path);
                return !attributes.HasFlag(FileAttributes.Directory);
            }
            catch
            {
                return null;
            }
        }

        public static string GetFileExtension(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "";
            }

            var pathIsFile = PathIsFile(path);
            if (pathIsFile == true)
            {
                return Path.GetExtension(path).ToLower();
            }
            if (pathIsFile == false)
            {
                return "";
            }
            else
            {
                return path.Substring(path.LastIndexOf('.'));
            }
        }

        public static string[] GetAllFiles(string path, string[] ofType = null)
        {
            //assumes user has access to desired folder/files. This will break if they don't
            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);

            var final = new List<string>();
            if (ofType != null)
            {
                foreach (string file in files)
                {
                    string ext = GetFileExtension(file);
                    if (ofType.Contains(ext))
                    {
                        final.Add(file);
                    }
                }
            }
            else
            {
                final.AddRange(files);
            }

            return final.ToArray();
        }
    }
}