#region Copyright & License Information
/*
 * Copyright 2019-2023 The OpenHV Developers (see AUTHORS)
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
	[RequireExplicitImplementation]
	public interface INotifyEnterTeleporter { void Charging(Actor self, Actor teleporter); }
	public interface INotifyExitTeleporter { void Arrived(Actor self); }

	[RequireExplicitImplementation]
	public interface INotifyResourceTransport { void Delivered(Actor sender, Actor receiver); }

	[RequireExplicitImplementation]
	public interface INotifyResourceCollection { void Mining(Actor self); void Suspended(Actor self); void Depletion(Actor self); }

	[RequireExplicitImplementation]
	public interface INotifyEnterCarrier
	{
		void Approaching(Actor self, Actor child);
		void Landed(Actor self, Actor child);
	}

	[RequireExplicitImplementation]
	public interface INotifyExitCarrier
	{
		void Opening(Actor self, Actor child);
		void Closing(Actor self, Actor child);
	}

	[RequireExplicitImplementation]
	public interface INotifyMissileSpawn { void Launching(Actor self, Target target); }

	[RequireExplicitImplementation]
	public interface INotifyTeleportation { void Teleporting(WPos from, WPos to); }
}
