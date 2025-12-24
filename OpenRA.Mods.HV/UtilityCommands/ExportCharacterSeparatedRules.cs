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
using System.IO;

namespace OpenRA.Mods.HV.UtilityCommands
{
	sealed class ExportCharacterSeparatedRules : IUtilityCommand
	{
		string IUtilityCommand.Name { get { return "--generate-dps-table"; } }

		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			return true;
		}

		[Desc("Export the damage per second evaluation into a CSV file for inspection.")]
		void IUtilityCommand.Run(Utility utility, string[] args)
		{
			// HACK: The engine code assumes that Game.ModData is set.
			var modData = Game.ModData = utility.ModData;
			var table = ActorStatsExport.GenerateTable(modData);
			var filename = $"{modData.Manifest.Id}-mod-dps.csv";

			try
			{
				using (var outfile = new StreamWriter(filename))
					outfile.Write(table.ToCharacterSeparatedValues(";", true));
				Console.WriteLine($"{filename} has been saved.");
				Console.WriteLine("Open as values separated by semicolon.");
			}
			catch (UnauthorizedAccessException e)
			{
				Console.WriteLine(e.Message);
			}
		}
	}
}
