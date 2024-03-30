#region Copyright & License Information
/*
 * Copyright 2007-2024 The OpenHV Developers (see AUTHORS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Manages the initial base.")]
	public class BaseBotModuleInfo : ConditionalTraitInfo
	{
		public override object Create(ActorInitializer init) { return new BaseBotModule(init.Self, this); }
	}

	public class BaseBotModule : ConditionalTrait<BaseBotModuleInfo>, IBotTick, IBotPositionsUpdated, IGameSaveTraitData
	{
		readonly World world;
		readonly Player player;

		IBotPositionsUpdated[] notifyPositionsUpdated;

		CPos initialBaseCenter;
		bool initialized;

		public BaseBotModule(Actor self, BaseBotModuleInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;
		}

		protected override void Created(Actor self)
		{
			notifyPositionsUpdated = self.Owner.PlayerActor.TraitsImplementing<IBotPositionsUpdated>().ToArray();
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (!initialized)
			{
				foreach (var baseBuilding in world.ActorsHavingTrait<BaseBuilding>().Where(a => a.Owner == player))
					SetCenter(baseBuilding.Location);

				initialized = true;
			}
		}

		void SetCenter(CPos location)
		{
			foreach (var notify in notifyPositionsUpdated)
			{
				notify.UpdatedBaseCenter(location);
				notify.UpdatedDefenseCenter(location);
			}
		}

		void IBotPositionsUpdated.UpdatedBaseCenter(CPos newLocation)
		{
			initialBaseCenter = newLocation;
		}

		void IBotPositionsUpdated.UpdatedDefenseCenter(CPos newLocation) { }

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
