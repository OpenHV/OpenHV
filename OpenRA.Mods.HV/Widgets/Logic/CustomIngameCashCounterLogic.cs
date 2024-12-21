#region Copyright & License Information
/*
 * Copyright 2024 The OpenHV Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Globalization;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Primitives;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets.Logic
{
	public class CustomIngameCashCounterLogic : ChromeLogic
	{
		[FluentReference("revenue")]
		const string Income = "label-income";

		const float DisplayFracPerFrame = .07f;
		const int DisplayDeltaPerFrame = 37;

		readonly World world;
		readonly Player player;
		readonly PlayerResources playerResources;
		readonly LabelWithTooltipWidget cashLabel;
		readonly CachedTransform<int, string> incomeTooltipCache;

		int nextCashTickTime = 0;
		int displayResources;

		[ObjectCreator.UseCtor]
		public CustomIngameCashCounterLogic(Widget widget, ModData modData, World world)
		{
			this.world = world;
			player = world.LocalPlayer;
			playerResources = player.PlayerActor.Trait<PlayerResources>();
			displayResources = playerResources.GetCashAndResources();

			var stats = player.PlayerActor.Trait<PlayerStatistics>();
			incomeTooltipCache = new CachedTransform<int, string>(x =>
				FluentProvider.GetString(Income, "revenue", x));
			cashLabel = widget.Get<LabelWithTooltipWidget>("CASH");
			cashLabel.GetTooltipText = () => incomeTooltipCache.Update(stats.DisplayIncome);
		}

		public override void Tick()
		{
			if (nextCashTickTime > 0)
				nextCashTickTime--;

			var actual = playerResources.GetCashAndResources();

			var diff = Math.Abs(actual - displayResources);
			var move = Math.Min(Math.Max((int)(diff * DisplayFracPerFrame), DisplayDeltaPerFrame), diff);

			if (displayResources < actual)
			{
				displayResources += move;

				if (Game.Settings.Sound.CashTicks)
					Game.Sound.PlayNotification(world.Map.Rules, player, "Sounds", playerResources.Info.CashTickUpNotification, player.Faction.InternalName);
			}
			else if (displayResources > actual)
			{
				displayResources -= move;

				if (Game.Settings.Sound.CashTicks && nextCashTickTime == 0)
				{
					Game.Sound.PlayNotification(world.Map.Rules, player, "Sounds", playerResources.Info.CashTickDownNotification, player.Faction.InternalName);
					nextCashTickTime = 2;
				}
			}

			var displayResourcesText = displayResources.ToString(CultureInfo.CurrentCulture);
			cashLabel.GetText = () => displayResourcesText;
		}
	}
}
