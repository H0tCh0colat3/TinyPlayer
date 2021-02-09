using TagLib;
using TinyPlayer.IO;

namespace TinyPlayer.ViewModels
{
    public class Track
    {
        public string Filepath { get; private set; }
        public string FriendlyName { get; private set; }

        public Track(string path)
        {
            Filepath = path;

            using (var tagFile = File.Create(path))
            {
                FriendlyName = tagFile.Tag.Title;
            }

            if (string.IsNullOrWhiteSpace(FriendlyName))
            {
                FriendlyName = IOUtility.GetName(path);
            }
        }
    }
}