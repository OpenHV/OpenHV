#region Copyright & License Information
/*
 * Copyright 2019-2020 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("For use with the ForestLayer.")]
	class TreeInfo : TraitInfo
	{
		[FieldLoader.Require]
		[Desc("Terrain tile to match.")]
		public readonly ushort Template = 0;

		public override object Create(ActorInitializer init) { return new Tree(this); }
	}

	class Tree
	{
		public readonly TreeInfo Info;

		public Tree(TreeInfo info)
		{
			Info = info;
		}
	}
}
