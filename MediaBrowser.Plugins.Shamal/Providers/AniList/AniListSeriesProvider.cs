﻿using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Plugins.Shamal.Providers.AniList.MediaBrowser.Plugins.Anime.Providers.AniList;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Configuration;

//API v2
namespace MediaBrowser.Plugins.Shamal.Providers.AniList
{
    public class AniListSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
    {
        private readonly IHttpClient _httpClient;
        private readonly IApplicationPaths _paths;
        private readonly ILogger _log;
        private readonly Api _api;
        public int Order => 0;
        public string Name => "AniList (Shamal)";

        public AniListSeriesProvider(IApplicationPaths appPaths, IHttpClient httpClient, ILogManager logManager, IJsonSerializer jsonSerializer)
        {
            _log = logManager.GetLogger("AniList (Shamal)");
            _httpClient = httpClient;
            _api = new Api(jsonSerializer);
            _paths = appPaths;
        }

        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Series>();

            var aid = info.GetProviderId(ProviderNames.AniList);
            if (string.IsNullOrEmpty(aid))
            {
                _log.Info("Start AniList... Searching(" + info.Name + ")");
                aid = await _api.FindSeries(info.Name, cancellationToken);
            }

            if (!string.IsNullOrEmpty(aid))
            {
                RootObject  WebContent = await _api.WebRequestAPI(_api.AniList_anime_link.Replace("{0}",aid));
                result.Item = new Series();
                result.HasMetadata = true;
               
                result.People = await _api.GetPersonInfo(WebContent.data.Media.id, cancellationToken);
                result.Item.SetProviderId(ProviderNames.AniList, aid);
                result.Item.Overview = WebContent.data.Media.description;
                result.Item.OriginalTitle = WebContent.data.Media.title.native;
                result.Item.ProductionYear = WebContent.data.Media.startDate.year;
                //try
                //{
                //    //AniList has a max rating of 5
                //    result.Item.CommunityRating = (WebContent.data.Media.averageScore/10);
                //}
                //catch (Exception) { }
                //foreach (var genre in _api.Get_Genre(WebContent))
                //    result.Item.AddGenre(genre);
                //GenreHelper.CleanupGenres(result.Item);
                StoreImageUrl(aid, WebContent.data.Media.coverImage.extraLarge, "image");
                StoreImageUrl(aid, WebContent.data.Media.bannerImage, "bannerImage");
            }
            return result;
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            var results = new Dictionary<string, RemoteSearchResult>();

            var aid = searchInfo.GetProviderId(ProviderNames.AniList);
            if (!string.IsNullOrEmpty(aid))
            {
                if (!results.ContainsKey(aid))
                    results.Add(aid, await _api.GetAnime(aid));
            }

            if (!string.IsNullOrEmpty(searchInfo.Name))
            {
                List<string> ids = await _api.Search_GetSeries_list(searchInfo.Name, cancellationToken);
                foreach (string a in ids)
                {
                    results.Add(a, await _api.GetAnime(a));
                }
            }

            return results.Values;
        }

        private void StoreImageUrl(string series, string url, string type)
        {
            var path = Path.Combine(_paths.CachePath, "anilist", type, series + ".txt");
            var directory = Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);

            File.WriteAllText(path, url);
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClient.GetResponse(new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
                Url = url
            });
        }
    }

    public class AniListSeriesImageProvider : IRemoteImageProvider
    {
        private readonly IHttpClient _httpClient;
        private readonly IApplicationPaths _appPaths;
        private readonly Api _api;
        public AniListSeriesImageProvider(IHttpClient httpClient, IApplicationPaths appPaths, IJsonSerializer jsonSerializer)
        {
            _httpClient = httpClient;
            _appPaths = appPaths;
            _api = new Api(jsonSerializer);
        }

        public string Name => "AniList (Shamal)";

        public bool Supports(BaseItem item) => item is Series || item is Season;

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new[] { ImageType.Primary, ImageType.Banner };
        }

        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken)
        {
            var seriesId = item.GetProviderId(ProviderNames.AniList);
            return GetImages(seriesId, cancellationToken);
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(string aid, CancellationToken cancellationToken)
        {
            var list = new List<RemoteImageInfo>();

            if (!string.IsNullOrEmpty(aid))
            {
                var primary =  _api.Get_ImageUrl(await _api.WebRequestAPI(_api.AniList_anime_link.Replace("{0}", aid)));
                list.Add(new RemoteImageInfo
                {
                    ProviderName = Name,
                    Type = ImageType.Primary,
                    Url = primary
                });
                var banner = _api.Get_ImageBannerUrl(await _api.WebRequestAPI(_api.AniList_anime_link.Replace("{0}", aid)));
                list.Add(new RemoteImageInfo
                {
                    ProviderName = Name,
                    Type = ImageType.Banner,
                    Url = banner
                });
            }
            return list;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClient.GetResponse(new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
                Url = url
            });
        }
    }
}