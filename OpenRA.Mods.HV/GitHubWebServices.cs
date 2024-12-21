#region Copyright & License Information
/*
 * Copyright 2019-2024 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRA.Support;

namespace OpenRA.Mods.HV
{
	public enum ModVersionStatus { NotChecked, Latest, Outdated, Unknown, PrereleaseAvailable }

	public class NewsItem
	{
		public string Title;
		public string Author;
		public DateTime DateTime;
		public string Content;
	}

	public class GitHubWebServices : IGlobalModData
	{
		public readonly string LatestRelease = "https://api.github.com/repos/OpenHV/OpenHV/releases";
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
					var header = new ProductHeaderValue("OpenHV");
					var userAgent = new ProductInfoHeaderValue(header);
					httpClient.DefaultRequestHeaders.UserAgent.Add(userAgent);

					var url = new HttpQueryBuilder(LatestRelease).ToString();
					var response = await httpClient.GetStringAsync(url);
					var cacheFile = Path.Combine(Platform.SupportDir, GameNewsFileName);

					var oldPublishingDate = DateTime.MinValue;

					if (File.Exists(cacheFile))
					{
						var oldNews = await File.ReadAllTextAsync(cacheFile);
						oldPublishingDate = JToken.Parse(oldNews)
							.Where(p => p["published_at"] != null)
							.Max(d => d["published_at"].Value<DateTime>());
					}

					await File.WriteAllTextAsync(cacheFile, response);

					Game.RunAfterTick(() => // run on the main thread
					{
						var data = File.ReadAllText(cacheFile);

						var json = JToken.Parse(data)
							.Where(j => j["published_at"] != null)
							.OrderByDescending(n => n["published_at"].Value<DateTime>())
							.FirstOrDefault();

						if (json == null)
							return;

						var tagname = json["tag_name"].Value<string>();
						var digits = string.Concat(tagname.TakeWhile(c => char.IsDigit(c)));
						if (!int.TryParse(digits, out var parsedVersion))
							Log.Write("debug", "Error parsing tag name.");

						var prerelease = json["prerelease"].Value<bool>();

						if (!int.TryParse(Game.ModData.Manifest.Metadata.Version, out var version))
							Log.Write("debug", "Error parsing version number.");

						var status = ModVersionStatus.Unknown;
						if (Game.ModData.Manifest.Metadata.Version == "{DEV_VERSION}")
							status = ModVersionStatus.Unknown;
						else if (parsedVersion > version)
							status = prerelease ? ModVersionStatus.PrereleaseAvailable : ModVersionStatus.Outdated;
						else if (parsedVersion == version)
							status = ModVersionStatus.Latest;

						ModVersionStatus = status;

						var publishingDate = json["published_at"].Value<DateTime>();
						if (publishingDate > oldPublishingDate)
							NewsAlert = true;

						var body = json["body"].Value<string>();
						body = StripSpecialCharacters(body);
						body = StripMarkdown(body);
						body = body.Replace("* ", "• ");

						var newsItem = new NewsItem
						{
							Title = prerelease ? $"Pre-Release {tagname}" : $"Release {tagname}",
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
					Log.Write("debug", e);
				}
			});
		}

		static string StripMarkdown(string content)
		{
			content = content.Replace("https://github.com/OpenRA/OpenRA/wiki/Changelog-(bleed)/", "");
			content = Regex.Replace(content, "\\[(.*?)\\][\\[\\(].*?[\\]\\)]", "$1"); // remove links
			return Regex.Replace(content, @":\w+:", string.Empty); // remove Emojis
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
