using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using TinyPlayer.Audio;
using TinyPlayer.Enums;
using TinyPlayer.Extensions;
using TinyPlayer.IO;
using NAudio.Wave;
using TinyPlayer.Commands;

namespace TinyPlayer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _title;
        private double _currentTrackLength;
        private double _currentTrackPosition;
        private string _playPauseImageSource;
        private float _currentVolume;
        private bool _isSeeking;

        private ObservableCollection<Track> _playlist;
        private Track _currentlyPlayingTrack;
        private Track _currentlySelectedTrack;
        private AudioPlayer _audioPlayer;

        public string Resource_Previous { get; } = @"pack://application:,,,/TinyPlayer;component/Resources/previous.png";
        public string Resource_Next { get; } = @"pack://application:,,,/TinyPlayer;component/Resources/next.png";
        public string Resource_Pause { get; } = @"pack://application:,,,/TinyPlayer;component/Resources/pause.png";
        public string Resource_Play { get; } = @"pack://application:,,,/TinyPlayer;component/Resources/play.png";
        public string Resource_Stop { get; } = @"pack://application:,,,/TinyPlayer;component/Resources/stop.png";
        public string Resource_Shuffle { get; } = @"pack://application:,,,/TinyPlayer;component/Resources/shuffle.png";
        public string Resource_Volume { get; } = @"pack://application:,,,/TinyPlayer;component/Resources/volume.png";

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public string PlayPauseImageSource
        {
            get => _playPauseImageSource;
            set
            {
                _playPauseImageSource = value;
                OnPropertyChanged(nameof(PlayPauseImageSource));
            }
        }

        public float CurrentVolume
        {
            get => _currentVolume;
            set
            {
                _currentVolume = value;
                OnPropertyChanged(nameof(CurrentVolume));
            }
        }

        public double CurrentTrackLength
        {
            get => _currentTrackLength;
            set
            {
                _currentTrackLength = value;
                OnPropertyChanged(nameof(CurrentTrackLength));
            }
        }

        public double CurrentTrackPosition
        {
            get => _currentTrackPosition;
            set
            {
                _currentTrackPosition = value;
                OnPropertyChanged(nameof(CurrentTrackPosition));
            }
        }

        public Track CurrentlySelectedTrack
        {
            get => _currentlySelectedTrack;
            set
            {
                _currentlySelectedTrack = value;
                OnPropertyChanged(nameof(CurrentlySelectedTrack));
            }
        }

        public Track CurrentlyPlayingTrack
        {
            get => _currentlyPlayingTrack;
            set
            {
                _currentlyPlayingTrack = value;
                OnPropertyChanged(nameof(CurrentlyPlayingTrack));
            }
        }

        public ObservableCollection<Track> Playlist
        {
            get => _playlist;
            set
            {
                _playlist = value;
                OnPropertyChanged(nameof(Playlist));
            }
        }

        public ICommand ExitApplicationCommand { get; set; }
        public ICommand AddFileToPlaylistCommand { get; set; }
        public ICommand AddFolderToPlaylistCommand { get; set; }
        public ICommand SavePlaylistCommand { get; set; }
        public ICommand LoadPlaylistCommand { get; set; }

        public ICommand BackCommand { get; set; }
        public ICommand TogglePlaybackCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand FowardCommand { get; set; }
        public ICommand ShuffleCommand { get; set; }

        public ICommand SeekControlMouseDownCommand { get; set; }
        public ICommand SeekControlMouseUpCommand { get; set; }
        public ICommand VolumeControlChangedCommand { get; set; }

        public ICommand PlaylistDragDropCommand { get; set; }
        public ICommand ExcludeFromMasterListCommand { get; set; }
        public ICommand RemoveFromPlaylistCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private PlaybackState _playbackState;

        public MainWindowViewModel()
        {
            InitCommands();

            Playlist = new ObservableCollection<Track>();

            Title = "Tiny Player";

            _playbackState = PlaybackState.Stopped;
            CurrentVolume = 1;
            CurrentTrackLength = 1;
            PlayPauseImageSource = Resource_Play;
        }

        private void InitCommands()
        {
            ExitApplicationCommand = new RelayCommand(ExitApplication);
            AddFileToPlaylistCommand = new RelayCommand(AddFileToPlaylist);
            AddFolderToPlaylistCommand = new RelayCommand(AddFolderToPlaylist);
            SavePlaylistCommand = new RelayCommand(SavePlaylist, CanSavePlaylist);
            LoadPlaylistCommand = new RelayCommand(LoadPlaylist, CanLoadPlaylist);

            BackCommand = new RelayCommand(RewindToStart, CanRewindToStart);
            TogglePlaybackCommand = new RelayCommand(TogglePlayback, CanTogglePlayback);
            StopCommand = new RelayCommand(StopPlayback, CanStopPlayback);
            FowardCommand = new RelayCommand(ForwardToEnd, CanForwardToEnd);
            ShuffleCommand = new RelayCommand(Shuffle);

            SeekControlMouseDownCommand = new RelayCommand(BeginSeek, CanBeginSeek);
            SeekControlMouseUpCommand = new RelayCommand(EndSeek, CanEndSeek);
            VolumeControlChangedCommand = new RelayCommand(VolumeControlValueChanged);

            PlaylistDragDropCommand = new RelayCommand(PlaylistDragDrop);
            ExcludeFromMasterListCommand = new RelayCommand(ExcludeFromMasterList);
            RemoveFromPlaylistCommand = new RelayCommand(RemoveFromPlaylist);
        }

        private void ExitApplication(object p)
        {
            _audioPlayer?.Dispose();
            Application.Current.Shutdown();
        }

        private void AddFileToPlaylist(object p)
        {
            var result = IOUtility.OpenFileDialog(Assembly.GetEntryAssembly().Location, null, "Select a File", FileDialogFilter.AudioFiles);
            AddTrackFromFile(result);
        }

        private void AddFolderToPlaylist(object p)
        {
            var result = IOUtility.OpenFolderDialog(Assembly.GetEntryAssembly().Location, null, "Select a Folder");
            AddTracksFromFolder(result);
        }

        private void SavePlaylist(object p)
        {

        }

        private bool CanSavePlaylist(object p)
        {
            return true;
        }

        private void LoadPlaylist(object p)
        {

        }

        private bool CanLoadPlaylist(object p)
        {
            return true;
        }

        private void RewindToStart(object p)
        {
            _audioPlayer.PositionInSeconds = 0;
        }

        private bool CanRewindToStart(object p)
        {
            return _playbackState == PlaybackState.Playing;
        }

        private void TogglePlayback(object p)
        {
            if (CurrentlySelectedTrack == null) return;

            if (_audioPlayer == null || _playbackState == PlaybackState.Stopped)
            {
                _audioPlayer?.Dispose();
                _audioPlayer = null;

                _audioPlayer = new AudioPlayer(CurrentlySelectedTrack.Filepath, CurrentVolume);
                _audioPlayer.PlaybackPaused += PlaybackPaused;
                _audioPlayer.PlaybackResumed += PlaybackResumed;
                _audioPlayer.PlaybackStopped += PlaybackStopped;
                _audioPlayer.PositionUpdated += PositionUpdated;
            }

            if (_playbackState == PlaybackState.Stopped)
            {
                CurrentTrackPosition = 0;
                CurrentTrackLength = _audioPlayer.LengthInSeconds;
                CurrentlyPlayingTrack = CurrentlySelectedTrack;

                _audioPlayer.Play();
            }
            else if (_playbackState == PlaybackState.Playing)
            {
                _audioPlayer.Pause();
            }
            else
            {
                _audioPlayer.Play();
            }
        }

        private bool CanTogglePlayback(object p)
        {
            return CurrentlySelectedTrack != null;
        }

        private void StopPlayback(object p)
        {
            _audioPlayer?.Stop();
        }

        private bool CanStopPlayback(object p)
        {
            return _playbackState == PlaybackState.Playing || _playbackState == PlaybackState.Paused;
        }

        private void ForwardToEnd(object p)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.PositionInSeconds = _audioPlayer.LengthInSeconds;
            }
        }

        private bool CanForwardToEnd(object p)
        {
            if (_playbackState == PlaybackState.Playing)
            {
                return true;
            }
            return false;
        }

        private void Shuffle(object p)
        {
            Playlist.Shuffle();
        }

        private void BeginSeek(object p)
        {
            _isSeeking = true;
        }

        private void EndSeek(object p)
        {
            _isSeeking = false;
            _audioPlayer.PositionInSeconds = CurrentTrackPosition;
        }

        private bool CanBeginSeek(object p)
        {
            return _playbackState == PlaybackState.Playing && _audioPlayer != null;
        }

        private bool CanEndSeek(object p)
        {
            return _audioPlayer != null;
        }

        private void VolumeControlValueChanged(object p)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Volume = CurrentVolume;
            }
        }

        private void PlaylistDragDrop(object droppedObject)
        {
            if (droppedObject is DragEventArgs args)
            {
                var paths = args.Data.GetData(DataFormats.FileDrop) as string[];
                if (paths == null) return;

                foreach(var path in paths)
                {
                    if (IOUtility.PathIsFile(path) == true)
                    {
                        AddTrackFromFile(path);
                    }
                    if (IOUtility.PathIsFile(path) == false)
                    {
                        AddTracksFromFolder(path);
                    }
                }
            }
        }

        private void AddTrackFromFile(string file)
        {
            if (string.IsNullOrWhiteSpace(file)) return;
            Playlist.Add(new Track(file));
        }

        private void AddTracksFromFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath)) return;

            var files = IOUtility.GetFilesRecursive(folderPath, new[] { ".wav", ".mp3", ".m4a", ".wma", ".ogg", ".flac" });
            foreach (var file in files)
            {
                Playlist.Add(new Track(file));
            }
        }

        private void RemoveFromPlaylist(object t)
        {
            if (!(t is Track track)) return;


        }

        private void ExcludeFromMasterList(object t)
        {
            if (!(t is Track track)) return;


        }

        private void PlaybackStopped()
        {
            _playbackState = PlaybackState.Stopped;
            PlayPauseImageSource = Resource_Play;

            CommandManager.InvalidateRequerySuggested();
            CurrentTrackPosition = 0;

            if (_audioPlayer.EndOfFile)
            {
                var nextTrack = Playlist.NextItem(CurrentlyPlayingTrack);
                if (nextTrack == null) return;

                CurrentlySelectedTrack = nextTrack;
                TogglePlayback(null);
            }

        }

        private void PlaybackResumed()
        {
            _playbackState = PlaybackState.Playing;
            PlayPauseImageSource = Resource_Pause;
        }

        private void PlaybackPaused()
        {
            _playbackState = PlaybackState.Paused;
            PlayPauseImageSource = Resource_Play;
        }

        private void PositionUpdated()
        {
            if (_isSeeking) return;

            CurrentTrackPosition = _audioPlayer?.PositionInSeconds ?? 0.0;
        }
    }
}