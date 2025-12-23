using Newtonsoft.Json;
using RestSharp;

namespace Artemis.Plugins.DataModelExpansions.YTMdesktop
{
    // ReSharper disable once InconsistentNaming
    public class YTMDesktopClient
    {
        private readonly RestClient _client;
        private readonly RestRequest _queryRootInfoRequest;

        private RootInfo? _rootInfo;

        public YTMDesktopClient()
        {
            _client = new RestClient("http://127.0.0.1:9863");
            // In modern RestSharp, use Method.Get instead of Method.GET
            _queryRootInfoRequest = new RestRequest("query", Method.Get);
        }

        public RootInfo? Data => _rootInfo;

        public void Update()
        {
            var response = _client.Execute(_queryRootInfoRequest);
            if (response.Content != null)
                _rootInfo = JsonConvert.DeserializeObject<RootInfo>(response.Content);
        }
    }

    public class PlayerInfo
    {
        public bool HasSong { get; set; }
        public bool IsPaused { get; set; }
        public float VolumePercent { get; set; }
        public int SeekbarCurrentPosition { get; set; }
        public string SeekbarCurrentPositionHuman { get; set; } = string.Empty;
        public double StatePercent { get; set; }
        public string LikeStatus { get; set; } = string.Empty;
        public string RepeatType { get; set; } = string.Empty;
    }

    public class TrackInfo
    {
        public string Author { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public string Cover { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string DurationHuman { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public bool IsVideo { get; set; }
        public bool IsAdvertisement { get; set; }
        public bool InLibrary { get; set; }
    }

    public class RootInfo
    {
        public PlayerInfo Player { get; set; } = new PlayerInfo();
        public TrackInfo Track { get; set; } = new TrackInfo();
    }
}
