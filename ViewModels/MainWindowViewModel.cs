using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using TinyPlayer.Audio;
using TinyPlayer.Enums;
using TinyPlayer.Extensions;
using TinyPlayer.IO;

namespace TinyPlayer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public class Track
        {
            public string Filepath { get; set; }
            public string FriendlyName { get; set; }

            public Track(string path, string name)
            {
                Filepath = path;
                FriendlyName = name;
            }
        }

        private string _title;
        private double _currentTrackLength;
        private double _currentTrackPosition;
        private string _playPauseImageSource;
        private float _currentVolume;

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

        public ICommand RewindToStartCommand { get; set; }
        public ICommand StartPlaybackCommand { get; set; }
        public ICommand StopPlaybackCommand { get; set; }
        public ICommand ForwardToEndCommand { get; set; }
        public ICommand ShuffleCommand { get; set; }

        public ICommand TrackControlMouseDownCommand { get; set; }
        public ICommand TrackControlMouseUpCommand { get; set; }
        public ICommand VolumeControlValueChangedCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private enum PlaybackState
        {
            Playing, Stopped, Paused
        }

        private PlaybackState _playbackState;

        public MainWindowViewModel()
        {
            InitCommands();

            Playlist = new ObservableCollection<Track>();

            Title = "Tiny Player";

            _playbackState = PlaybackState.Stopped;
            CurrentVolume = 1;
        }

        private void InitCommands()
        {
            ExitApplicationCommand = new RelayCommand(ExitApplication, CanExitApplication);
            AddFileToPlaylistCommand = new RelayCommand(AddFileToPlaylist, CanAddFileToPlaylist);
            AddFolderToPlaylistCommand = new RelayCommand(AddFolderToPlaylist, CanAddFolderToPlaylist);
            SavePlaylistCommand = new RelayCommand(SavePlaylist, CanSavePlaylist);
            LoadPlaylistCommand = new RelayCommand(LoadPlaylist, CanLoadPlaylist);

            RewindToStartCommand = new RelayCommand(RewindToStart, CanRewindToStart);
            StartPlaybackCommand = new RelayCommand(StartPlayback, CanStartPlayback);
            StopPlaybackCommand = new RelayCommand(StopPlayback, CanStopPlayback);
            ForwardToEndCommand = new RelayCommand(ForwardToEnd, CanForwardToEnd);
            ShuffleCommand = new RelayCommand(Shuffle, CanShuffle);

            TrackControlMouseDownCommand = new RelayCommand(TrackControlMouseDown, CanTrackControlMouseDown);
            TrackControlMouseUpCommand = new RelayCommand(TrackControlMouseUp, CanTrackControlMouseUp);
            VolumeControlValueChangedCommand = new RelayCommand(VolumeControlValueChanged, CanVolumeControlValueChanged);
        }

        private void ExitApplication(object p)
        {
            _audioPlayer?.Dispose();
            Application.Current.Shutdown();
        }

        private bool CanExitApplication(object p)
        {
            return true;
        }

        private void AddFileToPlaylist(object p)
        {
            var result = IOUtility.OpenFileDialog(Assembly.GetEntryAssembly().Location, null, "Select a File", FileDialogFilter.AudioFiles);
            if (!string.IsNullOrWhiteSpace(result))
            {
                var track = new Track(result, result);
                Playlist.Add(track);
            }
        }

        private bool CanAddFileToPlaylist(object p)
        {
            return _playbackState == PlaybackState.Stopped;
        }

        private void AddFolderToPlaylist(object p)
        {

        }

        private bool CanAddFolderToPlaylist(object p)
        {
            return true;
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

        private void StartPlayback(object p)
        {
            if (CurrentlySelectedTrack != null)
            {
                if (_playbackState == PlaybackState.Stopped)
                {
                    _audioPlayer = new AudioPlayer(CurrentlySelectedTrack.Filepath, CurrentVolume);
                    _audioPlayer.PlaybackStopType = PlaybackStopType.EndOfFile;
                    _audioPlayer.PlaybackPaused += _audioPlayer_PlaybackPaused;
                    _audioPlayer.PlaybackResumed += _audioPlayer_PlaybackResumed;
                    _audioPlayer.PlaybackStopped += _audioPlayer_PlaybackStopped;
                    _audioPlayer.PositionUpdated += _audioPlayer_PositionUpdated;
                    CurrentTrackLength = _audioPlayer.LengthInSeconds;
                    CurrentlyPlayingTrack = CurrentlySelectedTrack;
                }
                if (CurrentlySelectedTrack == CurrentlyPlayingTrack)
                {
                    _audioPlayer.TogglePlayPause(CurrentVolume);
                }
            }
        }

        private bool CanStartPlayback(object p)
        {
            return CurrentlySelectedTrack != null;
        }

        private void StopPlayback(object p)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.PlaybackStopType = PlaybackStopType.User;
                _audioPlayer.Stop();
            }
        }

        private bool CanStopPlayback(object p)
        {
            if (_playbackState == PlaybackState.Playing || _playbackState == PlaybackState.Paused)
            {
                return true;
            }
            return false;
        }

        private void ForwardToEnd(object p)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.PlaybackStopType = PlaybackStopType.EndOfFile;
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
            Playlist = Playlist.Shuffle();
        }

        private bool CanShuffle(object p)
        {
            return true;
        }

        private void TrackControlMouseDown(object p)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Pause();
            }
        }

        private void TrackControlMouseUp(object p)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.PositionInSeconds = CurrentTrackPosition;
                _audioPlayer.Play(NAudio.Wave.PlaybackState.Paused, CurrentVolume);
            }
        }

        private bool CanTrackControlMouseDown(object p)
        {
            if (_playbackState == PlaybackState.Playing)
            {
                return true;
            }
            return false;
        }

        private bool CanTrackControlMouseUp(object p)
        {
            if (_playbackState == PlaybackState.Paused)
            {
                return true;
            }
            return false;
        }

        private void VolumeControlValueChanged(object p)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Volume = CurrentVolume;
            }
        }

        private bool CanVolumeControlValueChanged(object p)
        {
            return true;
        }

        private void _audioPlayer_PlaybackStopped()
        {
            _playbackState = PlaybackState.Stopped;
            CommandManager.InvalidateRequerySuggested();
            CurrentTrackPosition = 0;

            if (_audioPlayer.PlaybackStopType == PlaybackStopType.EndOfFile)
            {
                CurrentlySelectedTrack = Playlist.NextItem(CurrentlyPlayingTrack);
                StartPlayback(null);
            }
        }

        private void _audioPlayer_PlaybackResumed()
        {
            _playbackState = PlaybackState.Playing;
        }

        private void _audioPlayer_PlaybackPaused()
        {
            _playbackState = PlaybackState.Paused;
        }

        private void _audioPlayer_PositionUpdated()
        {
            CurrentTrackPosition = _audioPlayer?.PositionInSeconds ?? 0.0;
        }
    }
}