﻿using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Plugins.Shamal.Configuration;
using MediaBrowser.Plugins.Shamal.Providers.AniDB.Identity;
using System;
using System.Collections.Generic;
using MediaBrowser.Model.Drawing;
using System.IO;

namespace MediaBrowser.Plugins.Shamal
{
    public class Plugin
        : BasePlugin<PluginConfiguration>, IHasWebPages, IHasThumbImage
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILogger logger) : base(applicationPaths, xmlSerializer)
        {
            Instance = this;

            AniDbTitleMatcher.DefaultInstance = new AniDbTitleMatcher(logger, new AniDbTitleDownloader(logger, applicationPaths));
        }

        public override string Name
        {
            get { return "Shamal"; }
        }

        public static Plugin Instance { get; private set; }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "shamal",
                    EmbeddedResourcePath = "MediaBrowser.Plugins.Shamal.Configuration.configPage.html"
                }
            };
        }

        private Guid _id = new Guid("1d0dddf7-1877-4473-8d7b-03f7dac1e559");

        public override Guid Id
        {
            get { return _id; }
        }

        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.png");
        }

        public ImageFormat ThumbImageFormat
        {
            get
            {
                return ImageFormat.Png;
            }
        }
    }
}