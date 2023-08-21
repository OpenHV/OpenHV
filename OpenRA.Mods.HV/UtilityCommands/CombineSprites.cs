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
using System.Collections.Generic;
using System.IO;
using OpenRA.FileFormats;
using OpenRA.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Mods.Common.UtilityCommands
{
	sealed class CombineSprites : IUtilityCommand
	{
		string IUtilityCommand.Name { get { return "--combine-png"; } }

		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			return args.Length >= 2;
		}

		[Desc("PATH INPUT-FILENAME-PATTERN OUTPUT-FILENAME",
			  "Combine a series of indexed PNGs described.")]
		void IUtilityCommand.Run(Utility utility, string[] args)
		{
			// HACK: The engine code assumes that Game.modData is set.
			Game.ModData = utility.ModData;

			var width = 0;
			var height = 0;
			var data = Array.Empty<byte>();
			var palette = Array.Empty<Color>();
			var embeddedData = new Dictionary<string, string>();

			var sprites = Directory.GetFiles(args[1], args[2], SearchOption.TopDirectoryOnly);
			Array.Sort(sprites);
			foreach (var sprite in sprites)
			{
				using (var s = File.OpenRead(sprite))
				{
					var png = new Png(s);

					if (png.Palette == null)
						throw new InvalidDataException("Non-Palette indexed PNG are not supported.");

					width = png.Width;
					height += png.Height;

					var combined = new byte[data.Length + png.Data.Length];
					Array.Copy(data, combined, data.Length);
					Array.Copy(png.Data, 0, combined, data.Length, png.Data.Length);

					data = combined;

					palette = png.Palette;

					embeddedData = png.EmbeddedData;
				}
			}

			var output = new Png(data, SpriteFrameType.Indexed8, width, height, palette, embeddedData);
			var path = Path.Combine(args[1], args[3]);
			output.Save(path);
		}
	}
}
