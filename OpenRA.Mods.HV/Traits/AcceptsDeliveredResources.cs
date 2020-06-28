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

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Tag trait for actors with `ResourceCollector`.")]
	public class AcceptsDeliveredResourcesInfo : TraitInfo
	{
		public override object Create(ActorInitializer init) { return new AcceptsDeliveredResources(); }
	}

	public class AcceptsDeliveredResources
	{
		public AcceptsDeliveredResources() { }
	}
}
