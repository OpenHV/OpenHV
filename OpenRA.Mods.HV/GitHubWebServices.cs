#region Copyright & License Information
/*
 * Copyright 2019-2025 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
						using (var jsonDocument = JsonDocument.Parse(oldNews))
						{
							oldPublishingDate = jsonDocument.RootElement.EnumerateArray()
								.Max(d => GetPublishedAt(d));
						}
					}

					await File.WriteAllTextAsync(cacheFile, response);

					Game.RunAfterTick(() => // run on the main thread
					{
						var data = File.ReadAllText(cacheFile);

						using (var jsonDocument = JsonDocument.Parse(data))
						{
							var jsonElement = jsonDocument.RootElement.EnumerateArray()
								.OrderByDescending(d => GetPublishedAt(d))
								.FirstOrDefault();

							if (jsonElement.ValueKind == JsonValueKind.Undefined)
								return;

							var tagName = "";
							if (jsonElement.TryGetProperty("tag_name", out var tagJsonElement) &&
								tagJsonElement.ValueKind == JsonValueKind.String)
								tagName = jsonElement.GetProperty("tag_name").GetString();

							var digits = string.Concat(tagName.TakeWhile(c => char.IsDigit(c)));
							if (!int.TryParse(digits, out var parsedVersion))
								Log.Write("debug", "Error parsing tag name.");

							var prerelease = false;
							if (jsonElement.TryGetProperty("prerelease", out var prereleaseJsonElement))
								prerelease = jsonElement.GetProperty("prerelease").GetBoolean();

							var status = ModVersionStatus.Unknown;
							if (Game.ModData.Manifest.Metadata.Version == "{DEV_VERSION}")
								status = ModVersionStatus.Unknown;
							else if (int.TryParse(Game.ModData.Manifest.Metadata.Version, out var version))
							{
								if (parsedVersion > version)
									status = prerelease ? ModVersionStatus.PrereleaseAvailable : ModVersionStatus.Outdated;
								if (parsedVersion == version)
									status = ModVersionStatus.Latest;
							}
							else
								Log.Write("debug", "Error parsing version number.");

							ModVersionStatus = status;

							var publishingDate = GetPublishedAt(jsonElement);
							if (publishingDate > oldPublishingDate)
								NewsAlert = true;

							var body = string.Empty;
							if (jsonElement.TryGetProperty("body", out var bodyJsonElement) &&
								bodyJsonElement.ValueKind == JsonValueKind.String)
							{
								body = bodyJsonElement.GetString();
								body = StripSpecialCharacters(body);
								body = StripMarkdown(body);
								body = body.Replace("* ", "• ");
							}

							NewsItem = new NewsItem
							{
								Title = prerelease ? $"Pre-Release {tagName}" : $"Release {tagName}",
								Author = "OpenHV Team",
								DateTime = publishingDate,
								Content = body
							};

							onParsed();
						}
					});
				}
				catch (Exception e)
				{
					Log.Write("debug", e);
				}
			});
		}

		static DateTime GetPublishedAt(JsonElement jsonElement)
		{
			if (jsonElement.TryGetProperty("published_at", out var publishedAt) &&
				DateTime.TryParse(publishedAt.GetString(), DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var date))
				return date;

			return DateTime.MinValue;
		}

		static string StripMarkdown(string content)
		{
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
