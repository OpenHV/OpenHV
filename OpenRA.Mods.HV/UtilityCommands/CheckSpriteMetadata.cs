#region Copyright & License Information
/*
 * Copyright 2021-2024 The OpenHV Developers (see CREDITS)
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
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using OpenRA.FileFormats;

namespace OpenRA.Mods.HV.UtilityCommands
{
	sealed class CheckSpriteMetadata : IUtilityCommand
	{
		string IUtilityCommand.Name { get { return "--check-sprite-metadata"; } }

		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			return args.Length == 2;
		}

		[Desc("PNGFILE", "Check if .png and .yaml metadata still match up.")]
		void IUtilityCommand.Run(Utility utility, string[] args)
		{
			var pngPath = args[1];
			if (!File.Exists(pngPath))
			{
				Console.WriteLine(pngPath + " does not exist.");
				Environment.Exit(1);
			}

			using (var spriteStream = File.OpenRead(pngPath))
			{
				var png = new Png(spriteStream);
				var embeddedYaml = png.EmbeddedData.Select(m => new MiniYamlNode(m.Key, m.Value))
					.ToList()
					.WriteToString();

				var yamlPath = Path.ChangeExtension(pngPath, "yaml");
				if (!File.Exists(yamlPath))
				{
					Console.WriteLine(yamlPath + " does not exist.");
					Environment.Exit(1);
				}

				Console.WriteLine("Checking " + Path.GetFileName(pngPath));

				using (var metadataStream = File.OpenRead(yamlPath))
				{
					var externalYaml = metadataStream.ReadAllText()
						.Replace('ä', '?') // TODO: fix this in the engine
						.Replace('ö', '?')
						.Replace('ü', '?');

					if (!externalYaml.Equals(embeddedYaml))
					{
						Console.WriteLine($"{pngPath} embedded metadata does not match {yamlPath} file.");
						var diff = InlineDiffBuilder.Diff(embeddedYaml, externalYaml);

						var savedColor = Console.ForegroundColor;
						foreach (var line in diff.Lines)
						{
							switch (line.Type)
							{
								case ChangeType.Inserted:
									Console.ForegroundColor = ConsoleColor.Green;
									Console.Write("+ ");
									break;
								case ChangeType.Deleted:
									Console.ForegroundColor = ConsoleColor.Red;
									Console.Write("- ");
									break;
								default:
									Console.ForegroundColor = ConsoleColor.Gray;
									Console.Write("  ");
									break;
							}

							Console.WriteLine(line.Text);
						}

						Console.ForegroundColor = savedColor;
						Environment.Exit(1);
					}
				}
			}
		}
	}
}
