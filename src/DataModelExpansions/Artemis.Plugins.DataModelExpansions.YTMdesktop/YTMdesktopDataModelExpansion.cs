using Artemis.Plugins.DataModelExpansions.YTMdesktop.DataModels;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Artemis.Core.Modules;
using Artemis.Core;
using Artemis.Core.ColorScience;

namespace Artemis.Plugins.DataModelExpansions.YTMdesktop
{
    [PluginFeature(Name = "Youtube Music Desktop Player", AlwaysEnabled = true)]
    public class YTMdesktopDataModelExpansion : Module<YTMdesktopDataModel>
    {
        #region Variables declarations

        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly ConcurrentDictionary<string, ColorSwatch> _albumArtColorCache;
        private const string YTMD_PROCESS_NAME = "YouTube Music Desktop App";
        private YTMDesktopClient _YTMDesktopClient;
        private RootInfo _rootInfo;
        private string _trackId;
        private string _albumArtUrl;

        #endregion

        #region Constructor

        public YTMdesktopDataModelExpansion(ILogger logger)
        {
            _logger = logger;

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(1)
            };
            _albumArtColorCache = new ConcurrentDictionary<string, ColorSwatch>();
        }

        public override List<IModuleActivationRequirement> ActivationRequirements => null;

        #endregion

        #region Plugin Methods

        public override void Enable()
        {
            AddTimedUpdate(TimeSpan.FromSeconds(1), UpdateData);
            _YTMDesktopClient = new YTMDesktopClient();
        }

        private bool YoutubeIsRunning()
        {
            try
            {
                // Match by process name (without .exe)
                return Process.GetProcessesByName("YouTube Music Desktop App").Any();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to query processes: {ex}");
                return false;
            }
        }

        public override void Disable()
        {
            _YTMDesktopClient = null;
            _trackId = null;
            _albumArtUrl = null;
        }

        public override void Update(double deltaTime)
        {
            if (DataModel.Player.State != State.Closed)
                return;

            if (DataModel.Track.Duration == 0)
                return;

            DataModel.Player.SeekbarCurrentPositionHuman = DataModel.Player.SeekbarCurrentPositionHuman.Add(TimeSpan.FromMilliseconds(deltaTime * 1000));
            DataModel.Player.SeekbarCurrentPosition = DataModel.Player.SeekbarCurrentPositionHuman.TotalSeconds;
            DataModel.Player.StatePercent = DataModel.Player.SeekbarCurrentPosition / DataModel.Track.Duration;
        }

        #endregion

        #region DataModel update methods

        private async Task UpdateData(double deltaTime)
        {
            await UpdateYTMDekstopInfo();
        }

        private async Task UpdateYTMDekstopInfo()
        {
            if (!YoutubeIsRunning())
            {
                DataModel.Empty();
                return;
            }

            try
            {
                _YTMDesktopClient?.Update();
                _rootInfo = _YTMDesktopClient?.Data;

                if (_rootInfo != null)
                    await UpdateInfo(_rootInfo);
                else
                    DataModel.Empty();
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
            }
        }

        private async Task UpdateInfo(RootInfo data)
        {
            UpdatePlayerInfo(data.Player);

            if (data.Player.HasSong && data.Track != null)
                await UpdateTrackInfo(data.Track);
            else
                DataModel.Track.Empty();
        }

        private async Task UpdateTrackInfo(TrackInfo track)
        {
            if (track.Id != _trackId)
            {
                UpdateBasicTrackInfo(track);

                if (track.Cover != _albumArtUrl)
                {
                    if (string.IsNullOrEmpty(track.Cover))
                        return;

                    await UpdateAlbumArtColors(track.Cover);
                    _albumArtUrl = track.Cover;
                }

                _trackId = track.Id;
            }
        }

        private void UpdatePlayerInfo(PlayerInfo player)
        {
            if (player.HasSong && !player.IsPaused)
                DataModel.Player.State = State.Playing;
            else if (player.HasSong && player.IsPaused)
                DataModel.Player.State = State.Paused;
            else if (!player.HasSong)
                DataModel.Player.State = State.Stopped;
            else
                DataModel.Player.State = State.Closed;

            DataModel.Player.VolumePercent = player.VolumePercent;
            DataModel.Player.SeekbarCurrentPosition = player.SeekbarCurrentPosition;
            DataModel.Player.SeekbarCurrentPositionHuman = TimeSpan.FromSeconds(player.SeekbarCurrentPosition);
            DataModel.Player.StatePercent = player.StatePercent;
            DataModel.Player.LikeStatus = player.LikeStatus;
            DataModel.Player.RepeatType = Enum.Parse<RepeatState>(player.RepeatType, true);
        }

        private async Task UpdateAlbumArtColors(string albumArtUrl)
        {
            if (!_albumArtColorCache.ContainsKey(albumArtUrl))
            {
                try
                {
                    using Stream stream = await _httpClient.GetStreamAsync(albumArtUrl);
                    using SKBitmap skbm = SKBitmap.Decode(stream);
                    SKColor[] skClrs = ColorQuantizer.Quantize(skbm.Pixels, 256);
                    _albumArtColorCache[albumArtUrl] = ColorQuantizer.FindAllColorVariations(skClrs, true);
                }
                catch (Exception e)
                {
                    _logger.Error($"Failed to get album art colors. Exception: {e}");
                }
            }

            if (_albumArtColorCache.TryGetValue(albumArtUrl, out var colorDataModel))
                DataModel.Track.Colors = colorDataModel;
        }

        private void UpdateBasicTrackInfo(TrackInfo track)
        {
            DataModel.Track.Author = track.Author;
            DataModel.Track.Title = track.Title;
            DataModel.Track.Album = track.Album;
            DataModel.Track.Cover = track.Cover;
            DataModel.Track.Duration = track.Duration;
            DataModel.Track.DurationHuman = TimeSpan.FromSeconds(track.Duration);
            DataModel.Track.Url = track.Url;
            DataModel.Track.Id = track.Id;
            DataModel.Track.IsVideo = track.IsVideo;
            DataModel.Track.IsAdvertisement = track.IsAdvertisement;
            DataModel.Track.InLibrary = track.InLibrary;
        }

        #endregion
    }
}
