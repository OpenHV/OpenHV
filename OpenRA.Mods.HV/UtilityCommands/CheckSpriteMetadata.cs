#region Copyright & License Information
/*
 * Copyright 2021-2026 The OpenHV Developers (see CREDITS)
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
using System.Text;
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
				var fileName = Path.GetFileName(pngPath);
				var png = new Png(spriteStream);

				var yamlPath = Path.ChangeExtension(pngPath, "yaml");
				if (!File.Exists(yamlPath))
				{
					Console.WriteLine(yamlPath + " does not exist.");
					Environment.Exit(1);
				}

				var embeddedNodes = png.EmbeddedData.Select(m => new MiniYamlNode(m.Key, m.Value));
				var embeddedYaml = MiniYaml.FromString(embeddedNodes.WriteToString(), fileName);

				Console.WriteLine("Checking " + fileName);

				var reader = new StreamReader(yamlPath, encoding: Encoding.UTF8);
				var externalYaml = MiniYaml.FromStream(reader.BaseStream, Path.GetFileName(yamlPath));

				var n = 0;
				foreach (var node in externalYaml)
				{
					var embeddedNode = embeddedYaml.ElementAt(n);
					if (embeddedNode.Key != node.Key || embeddedNode.Value.Value != node.Value.Value)
					{
						var foregroundColor = Console.ForegroundColor;
						Console.WriteLine($"Embedded metadata {embeddedNode.Location} does not match {node.Location}.");
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("+ " + node.Key + ": " + node.Value.Value);
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("- " + embeddedNode.Key + ": " + embeddedNode.Value.Value);
						Console.ForegroundColor = foregroundColor;
					}

					n++;
				}
			}
		}
	}
}
