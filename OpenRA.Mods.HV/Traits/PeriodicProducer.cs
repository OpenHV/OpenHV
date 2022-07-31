#region Copyright & License Information
/*
 * Copyright 2022 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Produces an actor without using the standard production queue.")]
	public class PeriodicProducerInfo : PausableConditionalTraitInfo
	{
		[ActorReference]
		[FieldLoader.Require]
		[Desc("Actors to produce.")]
		public readonly string[] Actors = null;

		[FieldLoader.Require]
		[Desc("Production queue type to use")]
		public readonly string Type = null;

		[Desc("Notification played when production is activated.",
			"The filename of the audio is defined per faction in notifications.yaml.")]
		public readonly string ReadyAudio = null;

		[Desc("Notification played when the exit is jammed.",
			"The filename of the audio is defined per faction in notifications.yaml.")]
		public readonly string BlockedAudio = null;

		[Desc("Duration between productions.")]
		public readonly int ChargeDuration = 1000;

		[Desc("Reset the countdown when the traits gets enabled.")]
		public readonly bool ResetTraitOnEnable = false;

		public readonly bool ShowSelectionBar = false;
		public readonly Color ChargeColor = Color.SkyBlue;

		[Desc("Defines to which players the bar is to be shown.")]
		public readonly PlayerRelationship SelectionBarDisplayRelationships = PlayerRelationship.Ally;

		public override object Create(ActorInitializer init) { return new PeriodicProducer(init, this); }
	}

	public class PeriodicProducer : PausableConditionalTrait<PeriodicProducerInfo>, ISelectionBar, ITick, ISync
	{
		readonly PeriodicProducerInfo info;
		readonly Actor self;

		[Sync]
		int ticks;

		public PeriodicProducer(ActorInitializer init, PeriodicProducerInfo info)
			: base(info)
		{
			this.info = info;
			self = init.Self;
			ticks = info.ChargeDuration;
		}

		void ITick.Tick(Actor self)
		{
			if (IsTraitPaused)
				return;

			if (!IsTraitDisabled && --ticks < 0)
			{
				var sp = self.TraitsImplementing<Production>()
				.FirstOrDefault(p => !p.IsTraitDisabled && !p.IsTraitPaused && p.Info.Produces.Contains(info.Type));

				var activated = false;

				if (sp != null)
				{
					foreach (var name in info.Actors)
					{
						var inits = new TypeDictionary
						{
							new OwnerInit(self.Owner),
							new FactionInit(sp.Faction)
						};

						activated |= sp.Produce(self, self.World.Map.Rules.Actors[name.ToLowerInvariant()], info.Type, inits, 0);
					}
				}

				if (activated)
					Game.Sound.PlayNotification(self.World.Map.Rules, self.Owner, "Speech", info.ReadyAudio, self.Owner.Faction.InternalName);
				else
					Game.Sound.PlayNotification(self.World.Map.Rules, self.Owner, "Speech", info.BlockedAudio, self.Owner.Faction.InternalName);

				ticks = info.ChargeDuration;
			}
		}

		protected override void TraitEnabled(Actor self)
		{
			if (info.ResetTraitOnEnable)
				ticks = info.ChargeDuration;
		}

		float ISelectionBar.GetValue()
		{
			if (!info.ShowSelectionBar || IsTraitDisabled)
				return 0f;

			var viewer = self.World.RenderPlayer ?? self.World.LocalPlayer;
			if (viewer != null && !Info.SelectionBarDisplayRelationships.HasRelationship(self.Owner.RelationshipWith(viewer)))
				return 0f;

			return (float)(info.ChargeDuration - ticks) / info.ChargeDuration;
		}

		Color ISelectionBar.GetColor()
		{
			return info.ChargeColor;
		}

		bool ISelectionBar.DisplayWhenEmpty
		{
			get
			{
				var viewer = self.World.RenderPlayer ?? self.World.LocalPlayer;
				if (viewer != null && !Info.SelectionBarDisplayRelationships.HasRelationship(self.Owner.RelationshipWith(viewer)))
					return false;

				return info.ShowSelectionBar && !IsTraitDisabled;
			}
		}
	}
}
