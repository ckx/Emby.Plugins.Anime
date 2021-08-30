using System.Text.RegularExpressions;

namespace MediaBrowser.Plugins.Shamal.Providers.AniDB.Converter
{
    public struct AnidbEpisodeIdentity
    {
        private static readonly Regex Regex = new Regex(@"(?<series>\d+):(?<type>[S])?(?<epno>\d+)(-(?<epnoend>\d+))?");

        public string SeriesId { get; private set; }
        public int EpisodeNumber { get; private set; }
        public int? EpisodeNumberEnd { get; private set; }
        public string EpisodeType { get; private set; }

        public AnidbEpisodeIdentity(string id)
        {
            this = Parse(id).Value;
        }

        public AnidbEpisodeIdentity(string seriesId, int episodeNumber, int? episodeNumberEnd, string episodeType)
        {
            SeriesId = seriesId;
            EpisodeNumber = episodeNumber;
            EpisodeNumberEnd = episodeNumberEnd;
            EpisodeType = episodeType;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}{2}",
                SeriesId,
                EpisodeType ?? "",
                EpisodeNumber + (EpisodeNumberEnd != null ? "-" + EpisodeNumberEnd.Value.ToString() : ""));
        }

        public static AnidbEpisodeIdentity? Parse(string id)
        {
            var match = Regex.Match(id);
            if (match.Success)
            {
                return new AnidbEpisodeIdentity(
                    match.Groups["series"].Value,
                    int.Parse(match.Groups["epno"].Value),
                    match.Groups["epnoend"].Success ? int.Parse(match.Groups["epnoend"].Value) : (int?)null,
                    match.Groups["type"].Success ? match.Groups["type"].Value : null);
            }

            return null;
        }
    }
}