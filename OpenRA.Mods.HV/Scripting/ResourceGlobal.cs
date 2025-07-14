#region Copyright & License Information
/*
 * Copyright 2025 The OpenHV Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Mods.HV.Traits;
using OpenRA.Scripting;

namespace OpenRA.Mods.HV.Scripting
{
	[ScriptGlobal("Resource")]
	public class ResourceGlobal : ScriptGlobal
	{
		readonly UndergroundResourceLayer resourceLayer;

		public ResourceGlobal(ScriptContext context)
			: base(context)
		{
			resourceLayer = context.World.WorldActor.Trait<UndergroundResourceLayer>();
		}

		[Desc("How many resource deposits are on the map.")]
		public int TotalResourceCells => resourceLayer.TotalResourceCells;
	}
}
