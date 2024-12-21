#region Copyright & License Information
/*
 * Copyright 2019-2024 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.GameRules;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Warheads
{
	[Desc("Steals money from the target's owner.")]
	public class RobMoneyWarhead : ValidateTriggerWarhead
	{
		[FieldLoader.Require]
		[Desc("Amount to be taken away from the target.")]
		public readonly int Amount;

		[Desc("Percentage of the taken money granted to the firer.")]
		public readonly int PercentageGranted = 100;

		[Desc("Range of actors to be stolen from.")]
		public readonly WDist Range = new(64);

		[NotificationReference("Speech")]
		[Desc("Sound the victim will hear when they get robbed.")]
		public readonly string RobbedNotification = null;

		[FluentReference(optional: true)]
		[Desc("Text notification the victim will see when they get robbed.")]
		public readonly string RobbedTextNotification = null;

		[NotificationReference("Speech")]
		[Desc("Sound the perpetrator will hear after successful leeching.")]
		public readonly string RobNotification = null;

		[FluentReference(optional: true)]
		[Desc("Text notification the perpetrator will see after successful leeching.")]
		public readonly string RobTextNotification = null;

		[Desc("Whether to show a floating text.")]
		public readonly bool ShowTicks = true;

		public override void DoImpact(in Target target, WarheadArgs args)
		{
			if (!ValidateTrigger(target, args))
				return;

			var targetPosition = target.CenterPosition;
			var firedBy = args.SourceActor;
			var world = firedBy.World;
			var map = world.Map;

			var ownerPlayerResources = firedBy.Owner.PlayerActor.Trait<PlayerResources>();

			foreach (var actor in firedBy.World.FindActorsOnCircle(targetPosition, Range))
			{
				if (!IsValidAgainst(actor, firedBy))
					continue;

				if (actor.IsDead)
					continue;

				var activeShapes = actor.TraitsImplementing<HitShape>().Where(Exts.IsTraitEnabled);
				if (!activeShapes.Any())
					continue;

				var distance = activeShapes.Min(t => t.DistanceFromEdge(actor, targetPosition));

				if (distance > Range)
					continue;

				var targetPlayerResources = actor.Owner.PlayerActor.Trait<PlayerResources>();
				var stolen = targetPlayerResources.GetCashAndResources() > Amount ? Amount : targetPlayerResources.GetCashAndResources();

				targetPlayerResources.TakeCash(stolen, true);
				ownerPlayerResources.GiveCash(stolen * PercentageGranted / 100);

				if (RobbedNotification != null)
					Game.Sound.PlayNotification(world.Map.Rules, actor.Owner, "Speech", RobbedNotification, actor.Owner.Faction.InternalName);

				if (RobNotification != null)
					Game.Sound.PlayNotification(world.Map.Rules, firedBy.Owner, "Speech", RobNotification, firedBy.Owner.Faction.InternalName);

				TextNotificationsManager.AddTransientLine(actor.Owner, RobbedTextNotification);
				TextNotificationsManager.AddTransientLine(firedBy.Owner, RobTextNotification);

				if (ShowTicks)
				{
					world.AddFrameEndTask(w => w.Add(new FloatingText(actor.CenterPosition, firedBy.Owner.Color, FloatingText.FormatCashTick(stolen * PercentageGranted / 100), 30)));
				}
			}
		}
	}
}
