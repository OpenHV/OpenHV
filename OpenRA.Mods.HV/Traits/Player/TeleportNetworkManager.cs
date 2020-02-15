﻿#region Copyright & License Information
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
	[Desc("This must be attached to player in order for TeleportNetwork to work.")]
	public class TeleportNetworkManagerInfo : ITraitInfo
	{
		[FieldLoader.Require]
		[Desc("Type of TeleportNetwork that pairs up, in order for it to work.")]
		public string Type;

		public object Create(ActorInitializer init) { return new TeleportNetworkManager(init, this); }
	}

	public class TeleportNetworkManager
	{
		public readonly string Type;
		public int Count = 0;
		public Actor PrimaryActor = null;

		public TeleportNetworkManager(ActorInitializer init, TeleportNetworkManagerInfo info)
		{
			Type = info.Type;
		}
	}
}
