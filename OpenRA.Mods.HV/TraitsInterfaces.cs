#region Copyright & License Information
/*
 * Copyright 2019-2020 The OpenHV Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[RequireExplicitImplementation]
	public interface INotifyEnterTeleporter { void Charging(Actor self, Actor teleporter); }
	public interface INotifyExitTeleporter { void Arrived(Actor self); }

	[RequireExplicitImplementation]
	public interface INotifyResourceTransport { void Delivered(Actor sender, Actor receiver); }
}
