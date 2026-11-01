#region Copyright & License Information
/*
 * Copyright 2026 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenRA.Mods.HV.UtilityCommands
{
	sealed class ReplaceTile : IUtilityCommand
	{
		const byte BinaryFormat = 2;
		const int TileBytes = 3;

		string IUtilityCommand.Name { get { return "--replace-tile"; } }

		ushort oldId;
		ushort newId;

		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			return args.Length == 3
				&& Exts.TryParseUInt16Invariant(args[1], out oldId)
				&& Exts.TryParseUInt16Invariant(args[2], out newId)
				&& oldId != newId;
		}

		[Desc("OLDID", "NEWID", "Replace a tile ID in all maps that are shipped with the mod.")]
		void IUtilityCommand.Run(Utility utility, string[] args)
		{
			// HACK: The engine code assumes that Game.modData is set.
			var modData = Game.ModData = utility.ModData;
			var terrainInfo = modData.DefaultTerrainInfo.First().Value;

			// Check all maps first so that a tile mismatch doesn't leave them half converted.
			foreach (var package in modData.MapCache.EnumerateMapPackagesWithoutCaching())
			{
				using (package)
				{
					var data = package.GetStream("map.bin").ReadAllBytes();
					foreach (var offset in FindTiles(data))
					{
						var tile = new TerrainTile(newId, data[offset + 2]);
						if (!terrainInfo.TryGetTerrainInfo(tile, out _))
						{
							Console.WriteLine($"{package.Name} needs tile {tile} which does not exist in tileset {terrainInfo.Id}.");
							Environment.Exit(1);
						}
					}
				}
			}

			var replacedMaps = 0;
			foreach (var package in modData.MapCache.EnumerateMapPackagesWithoutCaching())
			{
				using (package)
				{
					var data = package.GetStream("map.bin").ReadAllBytes();
					var offsets = FindTiles(data);
					if (offsets.Count == 0)
						continue;

					// Map.Tiles can't be used as loading a map replaces tiles missing from the tileset with the default tile.
					foreach (var offset in offsets)
						BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(offset), newId);

					package.Update("map.bin", data);

					using (var map = new Map(modData, package))
						if (!map.LockPreview)
							package.Update("map.png", map.SavePreview());

					Console.WriteLine($"Replaced {offsets.Count} tiles in {package.Name}.");
					replacedMaps++;
				}
			}

			Console.WriteLine($"Replaced tile ID {oldId} with {newId} in {replacedMaps} maps.");
		}

		List<int> FindTiles(byte[] data)
		{
			if (data[0] != BinaryFormat)
				throw new InvalidDataException($"Unknown binary map format '{data[0]}'");

			var width = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(1));
			var height = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(3));
			var tilesOffset = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(5));

			var offsets = new List<int>();
			for (var i = 0; i < width * height; i++)
			{
				var offset = tilesOffset + i * TileBytes;
				if (BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset)) == oldId)
					offsets.Add(offset);
			}

			return offsets;
		}
	}
}
