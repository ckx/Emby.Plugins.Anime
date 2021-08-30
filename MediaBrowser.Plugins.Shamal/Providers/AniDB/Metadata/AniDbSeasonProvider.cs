﻿using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Plugins.Shamal.Providers.AniDB.Metadata
{
    public class AniDbSeasonProvider : IRemoteMetadataProvider<Season, SeasonInfo>
    {
        private readonly AniDbSeriesProvider _seriesProvider;

        public AniDbSeasonProvider(IHttpClient httpClient, IApplicationPaths appPaths)
        {
            _seriesProvider = new AniDbSeriesProvider(appPaths, httpClient);
        }

        public async Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Season>
            {
                HasMetadata = true,
                Item = new Season
                {
                    Name = info.Name,
                    IndexNumber = info.IndexNumber
                }
            };

            var seriesId = info.GetProviderId(ProviderNames.AniDb);
            if (string.IsNullOrEmpty(seriesId))
                return result;

            var seriesInfo = new SeriesInfo();
            seriesInfo.SetProviderId(ProviderNames.AniDb, seriesId);

            var seriesResult = await _seriesProvider.GetMetadata(seriesInfo, cancellationToken);
            if (seriesResult.HasMetadata)
            {
                result.Item.Name = seriesResult.Item.Name;
                result.Item.Overview = seriesResult.Item.Overview;
                result.Item.PremiereDate = seriesResult.Item.PremiereDate;
                result.Item.EndDate = seriesResult.Item.EndDate;
                result.Item.CommunityRating = seriesResult.Item.CommunityRating;
                result.Item.Studios = seriesResult.Item.Studios;
                result.Item.Genres = seriesResult.Item.Genres;
            }

            return result;
        }

        public string Name => "AniDB (Shamal)";

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo, CancellationToken cancellationToken)
        {
            var metadata = await GetMetadata(searchInfo, cancellationToken).ConfigureAwait(false);

            var list = new List<RemoteSearchResult>();

            if (metadata.HasMetadata)
            {
                var res = new RemoteSearchResult
                {
                    Name = metadata.Item.Name,
                    PremiereDate = metadata.Item.PremiereDate,
                    ProductionYear = metadata.Item.ProductionYear,
                    ProviderIds = metadata.Item.ProviderIds,
                    SearchProviderName = Name
                };

                list.Add(res);
            }

            return list;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}