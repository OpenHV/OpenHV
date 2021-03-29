#region Copyright & License Information
/*
 * Copyright 2019-2021 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using OpenHV;

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
			Action<DownloadDataCompletedEventArgs> onComplete = i =>
			{
				if (i.Error != null)
				{
					Log.Write("debug", i.Error.StackTrace);
					return;
				}

				try
				{
					var oldPublishingDate = DateTime.MinValue;

					var cacheFile = Path.Combine(Platform.SupportDir, GameNewsFileName);
					if (File.Exists(cacheFile))
					{
						var oldNews = File.ReadAllText(cacheFile);
						var oldJson = JObject.Parse(oldNews);
						oldPublishingDate = (DateTime)oldJson["published_at"];
					}

					File.WriteAllBytes(cacheFile, i.Result);

					var data = Encoding.UTF8.GetString(i.Result);
					var json = JObject.Parse(data);
					var tagname = (int)json["tag_name"];
					var prerelease = (bool)json["prerelease"];

					int.TryParse(Game.ModData.Manifest.Metadata.Version, out var version);

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

					Game.RunAfterTick(() => ModVersionStatus = status);

					var publishingDate = (DateTime)json["published_at"];
					if (publishingDate > oldPublishingDate)
						NewsAlert = true;

					var body = (string)json["body"];
					body = StripSpecialCharacters(body);
					body = StripMarkdown(body);
					body = body.Replace("* ", "• ");

					var newsItem = new NewsItem
					{
						Title = "Release {0}".F(tagname),
						Author = "OpenHV Team",
						DateTime = publishingDate,
						Content = body
					};

					Game.RunAfterTick(() => NewsItem = newsItem);

					Game.RunAfterTick(onParsed);
				}
				catch (Exception e)
				{
					Log.Write("debug", e.StackTrace);
				}
			};

			new DownloadWithAgent(LatestRelease, _ => { }, onComplete);
		}

		string StripMarkdown(string content)
		{
			// remove links
			return Regex.Replace(content, "\\[(.*?)\\][\\[\\(].*?[\\]\\)]", "$1");
		}

		string StripSpecialCharacters(string content)
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
