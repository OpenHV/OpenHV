#region Copyright & License Information
/*
 * Copyright 2022-2023 The OpenHV Developers (see AUTHORS)
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
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Manages AI builders.")]
	public class BuilderBotModuleInfo : ConditionalTraitInfo
	{
		[Desc("Actor types that can deploy into outposts.")]
		public readonly HashSet<string> BuilderTypes = new();

		[Desc("Delay (in ticks) between looking for and giving out orders to new builders.")]
		public readonly int ScanForNewBuilderInterval = 20;

		[Desc("Minimum distance in cells from center of the base when checking for builder deployment location.")]
		public readonly int MinimumBaseRadius = 0;

		[Desc("Maximum distance in cells from center of the base when checking for builder deployment location.")]
		public readonly int MaximumBaseRadius = 50;

		public override object Create(ActorInitializer init) { return new BuilderBotModule(init.Self, this); }
	}

	public class BuilderBotModule : ConditionalTrait<BuilderBotModuleInfo>, IBotTick, IBotPositionsUpdated, IGameSaveTraitData
	{
		public CPos GetRandomBaseCenter()
		{
			var randomOutpost = world.Actors.Where(a => a.Owner == player &&
				(a.TraitOrDefault<BaseProvider>() != null))
				.RandomOrDefault(world.LocalRandom);

			return randomOutpost?.Location ?? initialBaseCenter;
		}

		readonly World world;
		readonly Player player;

		CPos initialBaseCenter;
		int scanInterval;
		bool firstTick = true;

		public BuilderBotModule(Actor self, BuilderBotModuleInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;
		}

		protected override void TraitEnabled(Actor self)
		{
			// Avoid all AIs reevaluating assignments on the same tick, randomize their initial evaluation delay.
			scanInterval = world.LocalRandom.Next(Info.ScanForNewBuilderInterval, Info.ScanForNewBuilderInterval * 2);
		}

		void IBotPositionsUpdated.UpdatedBaseCenter(CPos newLocation)
		{
			initialBaseCenter = newLocation;
		}

		void IBotPositionsUpdated.UpdatedDefenseCenter(CPos newLocation) { }

		void IBotTick.BotTick(IBot bot)
		{
			if (firstTick)
			{
				DeployBuilders(bot, false);
				firstTick = false;
			}

			if (--scanInterval <= 0)
			{
				scanInterval = Info.ScanForNewBuilderInterval;
				DeployBuilders(bot, true);
			}
		}

		void DeployBuilders(IBot bot, bool chooseLocation)
		{
			var newBuilders = world.ActorsHavingTrait<Transforms>()
				.Where(a => a.Owner == player && a.IsIdle && Info.BuilderTypes.Contains(a.Info.Name));

			foreach (var builder in newBuilders)
				DeployBuilder(bot, builder, chooseLocation);
		}

		// Find any builder and deploy at a sensible location.
		void DeployBuilder(IBot bot, Actor mcv, bool move)
		{
			if (move)
			{
				var transformsInfo = mcv.Info.TraitInfo<TransformsInfo>();
				var desiredLocation = ChooseDeployLocation(transformsInfo.IntoActor, transformsInfo.Offset);
				if (desiredLocation == null)
					return;

				bot.QueueOrder(new Order("Move", mcv, Target.FromCell(world, desiredLocation.Value), true));
			}

			bot.QueueOrder(new Order("DeployTransform", mcv, true));
		}

		CPos? ChooseDeployLocation(string actorType, CVec offset)
		{
			var actorInfo = world.Map.Rules.Actors[actorType];
			var bi = actorInfo.TraitInfoOrDefault<BuildingInfo>();
			if (bi == null)
				return null;

			// Find the buildable cell that is closest to pos and centered around center
			CPos? FindPos(CPos center, CPos target, int minRange, int maxRange)
			{
				var cells = world.Map.FindTilesInAnnulus(center, minRange, maxRange);

				// Sort by distance to target if we have one
				if (center != target)
					cells = cells.OrderBy(c => (c - target).LengthSquared);
				else
					cells = cells.Shuffle(world.LocalRandom);

				foreach (var cell in cells)
					if (world.CanPlaceBuilding(cell + offset, actorInfo, bi, null))
						return cell;

				return null;
			}

			var baseCenter = GetRandomBaseCenter();

			return FindPos(baseCenter, baseCenter, Info.MinimumBaseRadius, Info.MaximumBaseRadius);
		}

		List<MiniYamlNode> IGameSaveTraitData.IssueTraitData(Actor self)
		{
			if (IsTraitDisabled)
				return null;

			return new List<MiniYamlNode>()
			{
				new("InitialBaseCenter", FieldSaver.FormatValue(initialBaseCenter))
			};
		}

		void IGameSaveTraitData.ResolveTraitData(Actor self, MiniYaml data)
		{
			if (self.World.IsReplay)
				return;

			var initialBaseCenterNode = data.NodeWithKeyOrDefault("InitialBaseCenter");
			if (initialBaseCenterNode != null)
				initialBaseCenter = FieldLoader.GetValue<CPos>("InitialBaseCenter", initialBaseCenterNode.Value.Value);
		}
	}
}
