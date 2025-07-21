#region Copyright & License Information
/*
 * Copyright 2019-2025 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Spawn an actor of the same value immediately upon death when the 'Scrap' trait is enabled.")]
	public class SpawnScrapOnDeathInfo : ConditionalTraitInfo
	{
		[ActorReference]
		[FieldLoader.Require]
		[Desc("Random actor to spawn on death.")]
		public readonly string[] Actors = null;

		[Desc("Probability the actor spawns.")]
		public readonly int Probability = 100;

		[Desc("Allowed to spawn on.")]
		public readonly HashSet<string> TerrainTypes = [];

		[Desc("Map player to use when 'InternalName' is defined on 'OwnerType'.")]
		public readonly string InternalOwner = "Neutral";

		[Desc("DeathType that triggers the actor spawn.",
			"Leave empty to spawn an actor ignoring the DeathTypes.")]
		public readonly string DeathType = null;

		[Desc("Offset of the spawned actor relative to the dying actor's position.",
			"Warning: Spawning an actor outside the parent actor's footprint/influence might",
			"lead to unexpected behaviour.")]
		public readonly CVec Offset = CVec.Zero;

		public override object Create(ActorInitializer init) { return new SpawnScrapOnDeath(init, this); }
	}

	public class SpawnScrapOnDeath : ConditionalTrait<SpawnScrapOnDeathInfo>, INotifyKilled, INotifyRemovedFromWorld
	{
		readonly string faction;
		readonly bool enabled;

		Player attackingPlayer;

		public SpawnScrapOnDeath(ActorInitializer init, SpawnScrapOnDeathInfo info)
			: base(info)
		{
			enabled = init.Self.World.WorldActor.Trait<ScrapOptions>().Enabled;
			faction = init.GetValue<FactionInit, string>(info, init.Self.Owner.Faction.InternalName);
		}

		void INotifyKilled.Killed(Actor self, AttackInfo e)
		{
			if (!enabled || IsTraitDisabled)
				return;

			if (!self.IsInWorld)
				return;

			if (self.World.SharedRandom.Next(100) > Info.Probability)
				return;

			if (Info.DeathType != null && !e.Damage.DamageTypes.Contains(Info.DeathType))
				return;

			var type = self.World.Map.GetTerrainInfo(self.Location).Type;
			if (!Info.TerrainTypes.Contains(type))
				return;

			attackingPlayer = e.Attacker.Owner;
		}

		// Don't add the new scrap actor to the world before all RemovedFromWorld callbacks have run.
		void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
		{
			if (attackingPlayer == null)
				return;

			var td = new TypeDictionary
			{
				new ParentActorInit(self),
				new LocationInit(self.Location + Info.Offset),
				new CenterPositionInit(self.CenterPosition),
				new FactionInit(faction),
				new ValueInit(self.GetSellValue()),
				new OwnerInit(self.World.Players.First(p => p.InternalName == Info.InternalOwner)),
				new SkipMakeAnimsInit()
			};

			self.World.AddFrameEndTask(w => w.CreateActor(Info.Actors.Random(self.World.SharedRandom), td));
		}
	}
}
