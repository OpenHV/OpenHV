#region Copyright & License Information
/*
 * Copyright 2021-2025 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Spawns aerial units at the target location.")]
	public class DropPodsPowerInfo : SupportPowerInfo, IRulesetLoaded
	{
		[FieldLoader.Require]
		[Desc("Drop pod unit")]
		[ActorReference([typeof(AircraftInfo), typeof(FallsToEarthInfo)])]
		public readonly string[] UnitTypes = null;

		[Desc("Number of drop pods spawned.")]
		public readonly int2 Drops = new(3, 5);

		[Desc("Sets the approach direction.")]
		public readonly WAngle PodFacing = WAngle.Zero;

		[Desc("Maximum offset from targetLocation")]
		public readonly int PodScatter = 3;

		[ActorReference]
		[Desc("Actor to spawn when the attack starts")]
		public readonly string CameraActor = null;

		[Desc("Number of ticks to keep the camera alive")]
		public readonly int CameraRemoveDelay = 25;

		public readonly HashSet<string> AllowedTerrainTypes = [];

		public override object Create(ActorInitializer init) { return new DropPodsPower(init.Self, this); }
	}

	public class DropPodsPower : SupportPower
	{
		readonly DropPodsPowerInfo info;
		readonly AircraftInfo aircraftInfo;
		readonly FallsToEarthInfo fallsToEarthInfo;
		public readonly int DropAmount = 10;

		public DropPodsPower(Actor self, DropPodsPowerInfo info)
			: base(self, info)
		{
			this.info = info;

			var actorInfo = self.World.Map.Rules.Actors[info.UnitTypes[0].ToLowerInvariant()];
			aircraftInfo = actorInfo.TraitInfo<AircraftInfo>();
			fallsToEarthInfo = actorInfo.TraitInfo<FallsToEarthInfo>();
		}

		public override void SelectTarget(Actor self, string order, SupportPowerManager manager)
		{
			self.World.OrderGenerator = new SelectDropPodsTarget(order, manager, this, MouseButton.Left);
		}

		public bool Validate(World world, CPos cell)
		{
			if (!world.Map.Contains(cell))
				return false;

			if (!info.AllowedTerrainTypes.Contains(world.Map.GetTerrainInfo(cell).Type))
				return false;

			return true;
		}

		public override void Activate(Actor self, Order order, SupportPowerManager manager)
		{
			base.Activate(self, order, manager);

			SendDropPods(self, order, info.PodFacing);
		}

		public void SendDropPods(Actor self, Order order, WAngle facing)
		{
			var altitude = aircraftInfo.CruiseAltitude.Length;
			var approachRotation = WRot.FromYaw(facing);

			var delta = new WVec(0, -altitude * aircraftInfo.Speed / fallsToEarthInfo.Velocity.Length, 0).Rotate(approachRotation);

			self.World.AddFrameEndTask(w =>
			{
				var target = order.Target.CenterPosition;
				var targetCell = self.World.Map.CellContaining(target);
				var podLocations = self.World.Map.FindTilesInCircle(targetCell, info.PodScatter)
					.Where(c => aircraftInfo.LandableTerrainTypes.Contains(w.Map.GetTerrainInfo(c).Type)
						&& !self.World.ActorMap.GetActorsAt(c).Any());

				if (!podLocations.Any())
					return;

				if (info.CameraActor != null)
				{
					var camera = w.CreateActor(info.CameraActor,
					[
						new LocationInit(targetCell),
						new OwnerInit(self.Owner),
					]);

					camera.QueueActivity(new Wait(info.CameraRemoveDelay));
					camera.QueueActivity(new RemoveSelf());
				}

				PlayLaunchSounds();

				var dropTypes = info.UnitTypes.Length;

				for (var i = 0; i < DropAmount; i++)
				{
					var podLocation = podLocations.Random(self.World.SharedRandom);
					var location = self.World.Map.CenterOfCell(podLocation) - delta + new WVec(0, 0, altitude);

					var pod = w.CreateActor(false, info.UnitTypes[i % dropTypes],
					[
						new CenterPositionInit(location),
						new OwnerInit(self.Owner),
						new FacingInit(facing)
					]);

					var aircraft = pod.Trait<Aircraft>();
					if (!aircraft.CanLand(podLocation))
						pod.Dispose();
					else
						w.Add(pod);
				}
			});
		}
	}

	public class SelectDropPodsTarget : OrderGenerator
	{
		readonly SupportPowerManager manager;
		readonly DropPodsPower power;
		readonly MouseButton expectedButton;

		public string OrderKey { get; }

		public SelectDropPodsTarget(string order, SupportPowerManager manager, DropPodsPower power, MouseButton button)
		{
			if (Game.Settings.Game.UseClassicMouseStyle)
				manager.Self.World.Selection.Clear();

			this.manager = manager;
			this.power = power;
			OrderKey = order;
			expectedButton = button;
		}

		protected override IEnumerable<Order> OrderInner(World world, CPos cell, int2 worldPixel, MouseInput mi)
		{
			world.CancelInputMode();

			if (!power.Validate(world, cell))
				yield break;

			if (mi.Button == expectedButton)
				yield return new Order(OrderKey, manager.Self, Target.FromCell(world, cell), false) { SuppressVisualFeedback = true };
		}

		protected override void Tick(World world)
		{
			if (!manager.Powers.TryGetValue(OrderKey, out var p) || !p.Active || !p.Ready)
				world.CancelInputMode();
		}

		protected override IEnumerable<IRenderable> Render(WorldRenderer wr, World world) { yield break; }
		protected override IEnumerable<IRenderable> RenderAboveShroud(WorldRenderer wr, World world) { yield break; }
		protected override IEnumerable<IRenderable> RenderAnnotations(WorldRenderer wr, World world) { yield break; }
		protected override string GetCursor(World world, CPos cell, int2 worldPixel, MouseInput mi)
		{
			var info = (DropPodsPowerInfo)power.Info;
			return power.Validate(world, cell) ? power.Info.Cursor : info.BlockedCursor;
		}
	}
}
