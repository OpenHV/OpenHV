#region Copyright & License Information
/*
 * Copyright 2022 The OpenHV Developers (see AUTHORS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Manages bot defensive support power handling.")]
	public class ExternalConditionPowerBotModuleInfo : ConditionalTraitInfo, Requires<SupportPowerManagerInfo>
	{
		[FieldLoader.Require]
		[Desc("Which support power to use.")]
		public readonly string OrderName = null;

		[Desc("How many friendlies should at least be affected?")]
		public readonly int MinimumTargets = 4;

		public override object Create(ActorInitializer init) { return new ExternalConditionPowerBotModule(init.Self, this); }
	}

	public class ExternalConditionPowerBotModule : ConditionalTrait<ExternalConditionPowerBotModuleInfo>, IBotRespondToAttack
	{
		readonly World world;
		readonly Player player;
		readonly ExternalConditionPowerBotModuleInfo info;

		SupportPowerManager supportPowerManager;

		public ExternalConditionPowerBotModule(Actor self, ExternalConditionPowerBotModuleInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;
			this.info = info;
		}

		protected override void Created(Actor self)
		{
			supportPowerManager = player.PlayerActor.Trait<SupportPowerManager>();
		}

		void IBotRespondToAttack.RespondToAttack(IBot bot, Actor self, AttackInfo e)
		{
			if (e.Attacker == null || e.Attacker.Disposed)
				return;

			if (e.Attacker.Owner.RelationshipWith(self.Owner) != PlayerRelationship.Enemy)
				return;

			foreach (var sp in supportPowerManager.Powers.Values)
			{
				if (sp.Disabled)
					continue;

				if (info.OrderName != sp.Info.OrderName)
					continue;

				if (!sp.Ready)
					continue;

				var externalConditionPower = sp.Instances[0] as GrantExternalConditionPower;
				var possibleTargets = externalConditionPower.UnitsInRange(self.Location);
				if (possibleTargets.Any(p => !p.Owner.IsAlliedWith(player)))
					continue;

				if (possibleTargets.Count() < info.MinimumTargets)
					continue;

				bot.QueueOrder(new Order(sp.Key, supportPowerManager.Self, Target.FromCell(world, self.Location), false) { SuppressVisualFeedback = true });
			}
		}
	}
}
