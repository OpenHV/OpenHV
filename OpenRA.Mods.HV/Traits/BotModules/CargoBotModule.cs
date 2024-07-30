#region Copyright & License Information
/*
 * Copyright 2023-2024 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.Player)]
	[Desc("Manages AI load unit related with " + nameof(Cargo) + " and " + nameof(Passenger) + " traits.")]
	public class CargoBotModuleInfo : ConditionalTraitInfo
	{
		[Desc("Actor types that can be targeted for load, must have " + nameof(Cargo) + ".")]
		public readonly HashSet<string> TransportTypes = default;

		[Desc("Actor types that used for loading, must have " + nameof(Passenger) + ".")]
		public readonly HashSet<string> PassengerTypes = default;

		[Desc("Allow enter allied transport.")]
		public readonly bool OnlyEnterOwnerPlayer = true;

		[Desc("Scan suitable actors and target in this interval.")]
		public readonly int ScanTick = 317;

		[Desc("Don't load passengers to this actor if damage state is worse than this.")]
		public readonly DamageState ValidDamageState = DamageState.Heavy;

		[Desc("Unload passengers to this actor if damage state is worse than this.")]
		public readonly DamageState UnloadDamageState = DamageState.Heavy;

		[Desc("Don't load passengers that are further than this distance to this actor.")]
		public readonly WDist MaxDistance = WDist.FromCells(20);

		public override object Create(ActorInitializer init) { return new CargoBotModule(init.Self, this); }
	}

	public class CargoBotModule : ConditionalTrait<CargoBotModuleInfo>, IBotTick, IBotRespondToAttack
	{
		readonly World world;
		readonly Player player;
		readonly Predicate<Actor> unitCannotBeOrdered;
		readonly Predicate<Actor> unitCannotBeOrderedOrIsBusy;
		readonly Predicate<Actor> invalidTransport;

		int minAssignRoleDelayTicks;

		public CargoBotModule(Actor self, CargoBotModuleInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;

			if (info.OnlyEnterOwnerPlayer)
				invalidTransport = a => a == null || a.IsDead || !a.IsInWorld || a.Owner != player;
			else
				invalidTransport = a => a == null || a.IsDead || !a.IsInWorld || a.Owner.RelationshipWith(player) != PlayerRelationship.Ally;

			unitCannotBeOrdered = a => a == null || a.IsDead || !a.IsInWorld || a.Owner != player;
			unitCannotBeOrderedOrIsBusy = a => unitCannotBeOrdered(a) || (!a.IsIdle && a.CurrentActivity is not FlyIdle);
		}

		protected override void TraitEnabled(Actor self)
		{
			// Avoid all AIs reevaluating assignments on the same tick, randomize their initial evaluation delay.
			minAssignRoleDelayTicks = world.LocalRandom.Next(0, Info.ScanTick);
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (--minAssignRoleDelayTicks <= 0)
			{
				minAssignRoleDelayTicks = Info.ScanTick;

				var transporters = world.ActorsWithTrait<Cargo>().Where(at =>
				{
					var health = at.Actor.TraitOrDefault<IHealth>()?.DamageState;
					return Info.TransportTypes.Contains(at.Actor.Info.Name) && !invalidTransport(at.Actor)
						&& at.Trait.HasSpace(1) && (health == null || health < Info.ValidDamageState);
				}).ToArray();

				if (transporters.Length == 0)
					return;

				var transporter = transporters.Random(world.LocalRandom);
				var cargo = transporter.Trait;
				var transport = transporter.Actor;
				var spaceTaken = 0;

				var passengers = world.ActorsWithTrait<Passenger>().Where(at => !unitCannotBeOrderedOrIsBusy(at.Actor) && Info.PassengerTypes.Contains(at.Actor.Info.Name) && cargo.HasSpace(at.Trait.Info.Weight) && (at.Actor.CenterPosition - transport.CenterPosition).HorizontalLengthSquared <= Info.MaxDistance.LengthSquared)
					.OrderBy(at => (at.Actor.CenterPosition - transport.CenterPosition).HorizontalLengthSquared);

				var orderedActors = new List<Actor>();

				foreach (var passenger in passengers)
				{
					var mobile = passenger.Actor.TraitOrDefault<Mobile>();
					if (mobile == null || !mobile.PathFinder.PathExistsForLocomotor(mobile.Locomotor, passenger.Actor.Location, transport.Location))
						continue;

					if (cargo.HasSpace(spaceTaken + passenger.Trait.Info.Weight))
					{
						spaceTaken += passenger.Trait.Info.Weight;
						orderedActors.Add(passenger.Actor);
					}

					if (!cargo.HasSpace(spaceTaken + 1))
						break;
				}

				if (orderedActors.Count > 0)
					bot.QueueOrder(new Order("EnterTransport", null, Target.FromActor(transport), false, groupedActors: orderedActors.ToArray()));
			}
		}

		void IBotRespondToAttack.RespondToAttack(IBot bot, Actor self, AttackInfo e)
		{
			if (!Info.TransportTypes.Contains(self.Info.Name))
				return;

			if (unitCannotBeOrdered(self))
				return;

			var damageState = self.TraitOrDefault<IHealth>()?.DamageState;
			if (damageState == null || damageState >= Info.UnloadDamageState)
			{
				var cargo = self.TraitOrDefault<Cargo>();
				if (cargo != null && !cargo.IsEmpty())
					bot.QueueOrder(new Order("Unload", self, false));
			}
		}
	}
}
