#region Copyright & License Information
/*
 * Copyright 2019-2021 The OpenHV Developers (see AUTHORS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.World)]
	public class LimitedResourcesInfo : TraitInfo, ILobbyOptions
	{
		[Desc("Descriptive label for the limited resources checkbox in the lobby.")]
		public readonly string CheckboxLabel = "Limited Resources";

		[Desc("Tooltip description for the limited resources checkbox in the lobby.")]
		public readonly string CheckboxDescription = "Resources will deplete.";

		[Desc("Default value of the limited resources checkbox in the lobby.")]
		public readonly bool CheckboxEnabled = true;

		[Desc("Prevent the limited resources state from being changed in the lobby.")]
		public readonly bool CheckboxLocked = false;

		[Desc("Whether to display the limited resources checkbox in the lobby.")]
		public readonly bool CheckboxVisible = true;

		[Desc("Display order for the limited resources checkbox in the lobby.")]
		public readonly int CheckboxDisplayOrder = 0;

		IEnumerable<LobbyOption> ILobbyOptions.LobbyOptions(MapPreview map)
		{
			yield return new LobbyBooleanOption("limited-resources", CheckboxLabel, CheckboxDescription, CheckboxVisible, CheckboxDisplayOrder, CheckboxEnabled, CheckboxLocked);
		}

		public override object Create(ActorInitializer init) { return new LimitedResources(this); }
	}

	public class LimitedResources : INotifyCreated
	{
		public bool Enabled { get; private set; }

		readonly LimitedResourcesInfo info;

		public LimitedResources(LimitedResourcesInfo info)
		{
			this.info = info;
		}

		void INotifyCreated.Created(Actor self)
		{
			Enabled = self.World.LobbyInfo.GlobalSettings.OptionOrDefault("limited-resources", info.CheckboxEnabled);
		}
	}
}
