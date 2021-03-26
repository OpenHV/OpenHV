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

using System.Collections.Generic;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Controls the 'Scrap' checkbox in the lobby options.")]
	public class ScrapOptionsInfo : TraitInfo, ILobbyOptions
	{
		[Desc("Descriptive label for the scrap checkbox in the lobby.")]
		public readonly string CheckboxLabel = "Scrap";

		[Desc("Tooltip description for the scrap checkbox in the lobby.")]
		public readonly string CheckboxDescription = "Collectible junk after unit destruction";

		[Desc("Default value of the scrap checkbox in the lobby.")]
		public readonly bool CheckboxEnabled = true;

		[Desc("Prevent the scrap state from being changed in the lobby.")]
		public readonly bool CheckboxLocked = false;

		[Desc("Whether to display the scrap checkbox in the lobby.")]
		public readonly bool CheckboxVisible = true;

		[Desc("Display order for the scrap checkbox in the lobby.")]
		public readonly int CheckboxDisplayOrder = 0;

		IEnumerable<LobbyOption> ILobbyOptions.LobbyOptions(Ruleset rules)
		{
			yield return new LobbyBooleanOption("scrap", CheckboxLabel, CheckboxDescription, CheckboxVisible, CheckboxDisplayOrder, CheckboxEnabled, CheckboxLocked);
		}

		public override object Create(ActorInitializer init) { return new ScrapOptions(this); }
	}

	public class ScrapOptions : INotifyCreated
	{
		readonly ScrapOptionsInfo info;
		public bool Enabled { get; private set; }

		public ScrapOptions(ScrapOptionsInfo info)
		{
			this.info = info;
		}

		void INotifyCreated.Created(Actor self)
		{
			Enabled = self.World.LobbyInfo.GlobalSettings.OptionOrDefault("scrap", info.CheckboxEnabled);
		}
	}
}
