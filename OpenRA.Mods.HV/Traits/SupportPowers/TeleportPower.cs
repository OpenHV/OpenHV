#region Copyright & License Information
/*
 * Copyright 2024 The OpenHV Developers (see CREDITS)
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
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Changes the location of a group of units.")]
	sealed class TeleportPowerInfo : SupportPowerInfo
	{
		[FieldLoader.Require]
		[Desc("Size of the footprint of the affected area.")]
		public readonly CVec Dimensions = CVec.Zero;

		[FieldLoader.Require]
		[Desc("Actual footprint. Cells marked as x will be affected.")]
		public readonly string Footprint = string.Empty;

		[PaletteReference]
		public readonly string TargetOverlayPalette = TileSet.TerrainPaletteInternalName;

		public readonly string FootprintImage = "overlay";

		[SequenceReference(nameof(FootprintImage), prefix: true)]
		public readonly string ValidFootprintSequence = "target-valid";

		[SequenceReference(nameof(FootprintImage))]
		public readonly string InvalidFootprintSequence = "target-invalid";

		[SequenceReference(nameof(FootprintImage))]
		public readonly string SourceFootprintSequence = "target-select";

		[CursorReference]
		[Desc("Cursor to display when selecting targets for the teleport.")]
		public readonly string SelectionCursor = "select";

		[CursorReference]
		[Desc("Cursor to display when targeting an area for the teleport.")]
		public readonly string TargetCursor = "move";

		[CursorReference]
		[Desc("Cursor to display when the targeted area is blocked.")]
		public readonly string TargetBlockedCursor = "move-blocked";

		public override object Create(ActorInitializer init) { return new TeleportPower(init.Self, this); }
	}

	sealed class TeleportPower : SupportPower
	{
		readonly char[] footprint;
		readonly CVec dimensions;

		public TeleportPower(Actor self, TeleportPowerInfo info)
			: base(self, info)
		{
			footprint = info.Footprint.Where(c => !char.IsWhiteSpace(c)).ToArray();
			dimensions = info.Dimensions;
		}

		public override void SelectTarget(Actor self, string order, SupportPowerManager manager)
		{
			self.World.OrderGenerator = new SelectTeleportTarget(Self.World, order, manager, this);
		}

		public override void Activate(Actor self, Order order, SupportPowerManager manager)
		{
			base.Activate(self, order, manager);
			PlayLaunchSounds();

			foreach (var notify in self.TraitsImplementing<INotifyTeleportation>())
				notify.Teleporting(self.World.Map.CenterOfCell(order.ExtraLocation), order.Target.CenterPosition);

			var targetDelta = self.World.Map.CellContaining(order.Target.CenterPosition) - order.ExtraLocation;
			foreach (var target in UnitsInRange(order.ExtraLocation))
			{
				var teleportable = target.TraitsImplementing<Teleportable>()
					.FirstEnabledConditionalTraitOrDefault();

				if (teleportable == null)
					continue;

				var targetCell = target.Location + targetDelta;

				if (self.Owner.Shroud.IsVisible(targetCell) && teleportable.CanTeleportTo(target, targetCell))
					teleportable.Teleport(target, targetCell, self);
			}
		}

		public IEnumerable<Actor> UnitsInRange(CPos xy)
		{
			var tiles = CellsMatching(xy, footprint, dimensions);
			var units = new HashSet<Actor>();
			foreach (var t in tiles)
				units.UnionWith(Self.World.ActorMap.GetActorsAt(t));

			return units.Where(a => a.TraitsImplementing<Teleportable>().Any(cs => !cs.IsTraitDisabled));
		}

		public bool SimilarTerrain(CPos xy, CPos sourceLocation)
		{
			if (!Self.Owner.Shroud.IsVisible(xy))
				return false;

			var sourceTiles = CellsMatching(xy, footprint, dimensions);
			var destTiles = CellsMatching(sourceLocation, footprint, dimensions);

			if (!sourceTiles.Any() || !destTiles.Any())
				return false;

			using (var se = sourceTiles.GetEnumerator())
			using (var de = destTiles.GetEnumerator())
				while (se.MoveNext() && de.MoveNext())
				{
					var a = se.Current;
					var b = de.Current;

					if (!Self.Owner.Shroud.IsVisible(a) || !Self.Owner.Shroud.IsVisible(b))
						return false;

					if (Self.World.Map.GetTerrainIndex(a) != Self.World.Map.GetTerrainIndex(b))
						return false;
				}

			return true;
		}

		sealed class SelectTeleportTarget : OrderGenerator
		{
			readonly TeleportPower power;
			readonly char[] footprint;
			readonly CVec dimensions;
			readonly Sprite tile;
			readonly float alpha;
			readonly SupportPowerManager manager;
			readonly string order;

			public SelectTeleportTarget(World world, string order, SupportPowerManager manager, TeleportPower power)
			{
				// Clear selection if using Left-Click Orders
				if (Game.Settings.Game.UseClassicMouseStyle)
					manager.Self.World.Selection.Clear();

				this.manager = manager;
				this.order = order;
				this.power = power;

				var info = (TeleportPowerInfo)power.Info;
				var s = world.Map.Sequences.GetSequence(info.FootprintImage, info.SourceFootprintSequence);
				footprint = info.Footprint.Where(c => !char.IsWhiteSpace(c)).ToArray();
				dimensions = info.Dimensions;
				tile = s.GetSprite(0);
				alpha = s.GetAlpha(0);
			}

			protected override IEnumerable<Order> OrderInner(World world, CPos cell, int2 worldPixel, MouseInput mi)
			{
				world.CancelInputMode();
				if (mi.Button == MouseButton.Left)
					world.OrderGenerator = new SelectDestination(world, order, manager, power, cell);

				yield break;
			}

			protected override void Tick(World world)
			{
				// Cancel the OG if we can't use the power
				if (!manager.Powers.TryGetValue(order, out var p) || !p.Active || !p.Ready)
					world.CancelInputMode();
			}

			protected override IEnumerable<IRenderable> RenderAboveShroud(WorldRenderer wr, World world) { yield break; }

			protected override IEnumerable<IRenderable> RenderAnnotations(WorldRenderer wr, World world)
			{
				var xy = wr.Viewport.ViewToWorld(Viewport.LastMousePos);
				foreach (var unit in power.UnitsInRange(xy).Where(a => !world.FogObscures(a)))
				{
					if (unit.CanBeViewedByPlayer(manager.Self.Owner))
					{
						var decorations = unit.TraitsImplementing<ISelectionDecorations>().FirstEnabledTraitOrDefault();
						if (decorations != null)
							foreach (var d in decorations.RenderSelectionAnnotations(unit, wr, Color.Red))
								yield return d;
					}
				}
			}

			protected override IEnumerable<IRenderable> Render(WorldRenderer wr, World world)
			{
				var xy = wr.Viewport.ViewToWorld(Viewport.LastMousePos);
				var tiles = power.CellsMatching(xy, footprint, dimensions);
				var palette = wr.Palette(((TeleportPowerInfo)power.Info).TargetOverlayPalette);
				foreach (var t in tiles)
					yield return new SpriteRenderable(tile, wr.World.Map.CenterOfCell(t), WVec.Zero, -511, palette, 1f, alpha, float3.Ones, TintModifiers.IgnoreWorldTint, true);
			}

			protected override string GetCursor(World world, CPos cell, int2 worldPixel, MouseInput mi)
			{
				return ((TeleportPowerInfo)power.Info).SelectionCursor;
			}
		}

		sealed class SelectDestination : OrderGenerator
		{
			readonly TeleportPower power;
			readonly CPos sourceLocation;
			readonly char[] footprint;
			readonly CVec dimensions;
			readonly Sprite validTile, invalidTile, sourceTile;
			readonly float validAlpha, invalidAlpha, sourceAlpha;
			readonly SupportPowerManager manager;
			readonly string order;

			public SelectDestination(World world, string order, SupportPowerManager manager, TeleportPower power, CPos sourceLocation)
			{
				this.manager = manager;
				this.order = order;
				this.power = power;
				this.sourceLocation = sourceLocation;

				var info = (TeleportPowerInfo)power.Info;
				footprint = info.Footprint.Where(c => !char.IsWhiteSpace(c)).ToArray();
				dimensions = info.Dimensions;

				var sequences = world.Map.Sequences;
				var tilesetValid = info.ValidFootprintSequence + "-" + world.Map.Tileset.ToLowerInvariant();
				if (sequences.HasSequence(info.FootprintImage, tilesetValid))
				{
					var validSequence = sequences.GetSequence(info.FootprintImage, tilesetValid);
					validTile = validSequence.GetSprite(0);
					validAlpha = validSequence.GetAlpha(0);
				}
				else
				{
					var validSequence = sequences.GetSequence(info.FootprintImage, info.ValidFootprintSequence);
					validTile = validSequence.GetSprite(0);
					validAlpha = validSequence.GetAlpha(0);
				}

				var invalidSequence = sequences.GetSequence(info.FootprintImage, info.InvalidFootprintSequence);
				invalidTile = invalidSequence.GetSprite(0);
				invalidAlpha = invalidSequence.GetAlpha(0);

				var sourceSequence = sequences.GetSequence(info.FootprintImage, info.SourceFootprintSequence);
				sourceTile = sourceSequence.GetSprite(0);
				sourceAlpha = sourceSequence.GetAlpha(0);
			}

			protected override IEnumerable<Order> OrderInner(World world, CPos cell, int2 worldPixel, MouseInput mi)
			{
				if (mi.Button == MouseButton.Right)
				{
					world.CancelInputMode();
					yield break;
				}

				var ret = OrderInner(cell).FirstOrDefault();
				if (ret == null)
					yield break;

				world.CancelInputMode();
				yield return ret;
			}

			IEnumerable<Order> OrderInner(CPos xy)
			{
				// Cannot Teleport into unexplored location
				if (IsValidTarget(xy))
					yield return new Order(order, manager.Self, Target.FromCell(manager.Self.World, xy), false)
					{
						ExtraLocation = sourceLocation,
						SuppressVisualFeedback = true
					};
			}

			protected override void Tick(World world)
			{
				// Cancel the OG if we can't use the power
				if (!manager.Powers.TryGetValue(order, out var p) || !p.Active || !p.Ready)
					world.CancelInputMode();
			}

			protected override IEnumerable<IRenderable> RenderAboveShroud(WorldRenderer wr, World world)
			{
				var xy = wr.Viewport.ViewToWorld(Viewport.LastMousePos);
				var palette = wr.Palette(power.Info.IconPalette);

				// Destination tiles
				var delta = xy - sourceLocation;
				foreach (var t in power.CellsMatching(sourceLocation, footprint, dimensions))
				{
					var isValid = manager.Self.Owner.Shroud.IsVisible(t + delta);
					var tile = isValid ? validTile : invalidTile;
					var alpha = isValid ? validAlpha : invalidAlpha;
					yield return new SpriteRenderable(tile, wr.World.Map.CenterOfCell(t + delta), WVec.Zero, -511, palette, 1f, alpha, float3.Ones, TintModifiers.IgnoreWorldTint, true);
				}

				// Unit previews
				foreach (var unit in power.UnitsInRange(sourceLocation))
				{
					if (unit.CanBeViewedByPlayer(manager.Self.Owner))
					{
						var targetCell = unit.Location + (xy - sourceLocation);
						var canEnter = manager.Self.Owner.Shroud.IsVisible(targetCell) &&
							unit.Trait<Teleportable>().CanTeleportTo(unit, targetCell);
						var tile = canEnter ? validTile : invalidTile;
						var alpha = canEnter ? validAlpha : invalidAlpha;
						yield return new SpriteRenderable(tile, wr.World.Map.CenterOfCell(targetCell), WVec.Zero, -511, palette, 1f, alpha, float3.Ones, TintModifiers.IgnoreWorldTint, true);
					}

					var offset = world.Map.CenterOfCell(xy) - world.Map.CenterOfCell(sourceLocation);
					if (unit.CanBeViewedByPlayer(manager.Self.Owner))
						foreach (var r in unit.Render(wr))
							yield return r.OffsetBy(offset);
				}
			}

			protected override IEnumerable<IRenderable> RenderAnnotations(WorldRenderer wr, World world)
			{
				foreach (var unit in power.UnitsInRange(sourceLocation))
				{
					if (unit.CanBeViewedByPlayer(manager.Self.Owner))
					{
						var decorations = unit.TraitsImplementing<ISelectionDecorations>().FirstEnabledTraitOrDefault();
						if (decorations != null)
							foreach (var d in decorations.RenderSelectionAnnotations(unit, wr, Color.Red))
								yield return d;
					}
				}
			}

			protected override IEnumerable<IRenderable> Render(WorldRenderer wr, World world)
			{
				var palette = wr.Palette(power.Info.IconPalette);

				// Source tiles
				foreach (var t in power.CellsMatching(sourceLocation, footprint, dimensions))
					yield return new SpriteRenderable(sourceTile, wr.World.Map.CenterOfCell(t), WVec.Zero, -511, palette, 1f, sourceAlpha, float3.Ones, TintModifiers.IgnoreWorldTint, true);
			}

			bool IsValidTarget(CPos xy)
			{
				var canTeleport = false;
				var anyUnitsInRange = false;
				foreach (var unit in power.UnitsInRange(sourceLocation))
				{
					anyUnitsInRange = true;
					var targetCell = unit.Location + (xy - sourceLocation);
					if (manager.Self.Owner.Shroud.IsVisible(targetCell) && unit.Trait<Teleportable>().CanTeleportTo(unit, targetCell))
					{
						canTeleport = true;
						break;
					}
				}

				// Don't teleport if there are no units in range (either all moved out of range, or none yet moved into range)
				if (!anyUnitsInRange)
					return false;

				if (!canTeleport)
				{
					// Check the terrain types. This will allow Teleports to occur on empty terrain to terrain of
					// a similar type. This also keeps the cursor from changing in non-visible property, alerting the
					// Teleporter of enemy unit presence
					canTeleport = power.SimilarTerrain(sourceLocation, xy);
				}

				return canTeleport;
			}

			protected override string GetCursor(World world, CPos cell, int2 worldPixel, MouseInput mi)
			{
				var powerInfo = (TeleportPowerInfo)power.Info;
				return IsValidTarget(cell) ? powerInfo.TargetCursor : powerInfo.TargetBlockedCursor;
			}
		}
	}
}
