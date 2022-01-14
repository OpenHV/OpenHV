#region Copyright & License Information
/*
 * Copyright 2019-2022 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRA.Support;

namespace OpenRA.Mods.HV
{
	public enum ModVersionStatus { NotChecked, Latest, Outdated, Unknown, PlaytestAvailable }

	public class NewsItem
	{
		public string Title;
		public string Author;
		public DateTime DateTime;
		public string Content;
	}

	public class GitHubWebServices : IGlobalModData
	{
		public readonly string LatestRelease = "https://api.github.com/repos/OpenHV/OpenHV/releases/latest";
		public readonly string GameNewsFileName = "news.json";

		public ModVersionStatus ModVersionStatus { get; private set; }
		public NewsItem NewsItem { get; private set; }
		public bool NewsAlert { get; private set; }

		public void FetchRelease(Action onParsed)
		{
			Task.Run(async () =>
			{
				try
				{
					var httpClient = HttpClientFactory.Create();
					var metadata = Game.ModData.Manifest.Metadata;
					var header = new ProductHeaderValue(metadata.Title);
					var userAgent = new ProductInfoHeaderValue(header);
					httpClient.DefaultRequestHeaders.UserAgent.Add(userAgent);

					var url = new HttpQueryBuilder(LatestRelease).ToString();
					var response = await httpClient.GetStringAsync(url);
					var cacheFile = Path.Combine(Platform.SupportDir, GameNewsFileName);

					var oldPublishingDate = DateTime.MinValue;

					if (File.Exists(cacheFile))
					{
						var oldNews = File.ReadAllText(cacheFile);
						var oldJson = JObject.Parse(oldNews);
						oldPublishingDate = (DateTime)oldJson["published_at"];
					}

					await File.WriteAllTextAsync(cacheFile, response);

					Game.RunAfterTick(() => // run on the main thread
					{
						var data = File.ReadAllText(cacheFile);
						var json = JObject.Parse(data);
						var tagname = (int)json["tag_name"];
						var prerelease = (bool)json["prerelease"];

						if (!int.TryParse(Game.ModData.Manifest.Metadata.Version, out var version))
							Log.Write("debug", "Error parsing version number.");

						var status = ModVersionStatus.Unknown;
						if (Game.ModData.Manifest.Metadata.Version == "{DEV_VERSION}")
							status = ModVersionStatus.Unknown;
						else if (tagname > version)
						{
							if (prerelease)
								status = ModVersionStatus.PlaytestAvailable;
							else
								status = ModVersionStatus.Outdated;
						}
						else if (tagname == version)
							status = ModVersionStatus.Latest;

						ModVersionStatus = status;

						var publishingDate = (DateTime)json["published_at"];
						if (publishingDate > oldPublishingDate)
							NewsAlert = true;

						var body = (string)json["body"];
						body = StripSpecialCharacters(body);
						body = StripMarkdown(body);
						body = body.Replace("* ", "• ");

						var newsItem = new NewsItem
						{
							Title = $"Release {tagname}",
							Author = "OpenHV Team",
							DateTime = publishingDate,
							Content = body
						};

						NewsItem = newsItem;
						onParsed();
					});
				}
				catch (Exception e)
				{
					Log.Write("debug", e.StackTrace);
				}
			});
		}

		static string StripMarkdown(string content)
		{
			// remove links
			return Regex.Replace(content, "\\[(.*?)\\][\\[\\(].*?[\\]\\)]", "$1");
		}

		static string StripSpecialCharacters(string content)
		{
			return Encoding.ASCII.GetString(
				Encoding.Convert(
					Encoding.UTF8,
					Encoding.GetEncoding(
						Encoding.ASCII.EncodingName,
						new EncoderReplacementFallback(string.Empty),
						new DecoderExceptionFallback()),
					Encoding.UTF8.GetBytes(content))).
						Replace("  ", " ");
		}
	}
}
