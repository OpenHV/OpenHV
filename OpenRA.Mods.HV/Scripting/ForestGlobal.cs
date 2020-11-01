#region Copyright & License Information
/*
 * Copyright 2021, 2022 The OpenHV Developers (see AUTHORS)
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
	[ScriptGlobal("Forest")]
	public class ForestGlobal : ScriptGlobal
	{
		readonly ForestLayer forestLayer;

		public ForestGlobal(ScriptContext context)
			: base(context)
		{
			forestLayer = context.World.WorldActor.Trait<ForestLayer>();
		}

		[Desc("How many trees when the map was initially loaded.")]
		public int TotalTrees => forestLayer.TotalTrees;

		[Desc("Remaining trees alive.")]
		public int TreesLeft => forestLayer.TreesLeft;

		[Desc("Trees that are on fire.")]
		public int TreesBurning => forestLayer.TreesBurning;

		[Desc("Damage an individual tree cell.")]
		public void Hit(CPos cell, int damage)
		{
			forestLayer.Hit(cell, damage);
		}
	}
}
