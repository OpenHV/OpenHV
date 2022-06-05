#region Copyright & License Information
/*
 * Copyright 2019-2022 The OpenHV Developers (see CREDITS)
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
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Attach this to the world actor.")]
	[TraitLocation(SystemActors.World)]
	public class ForestLayerInfo : TraitInfo, IOccupySpaceInfo, IRulesetLoaded
	{
		[Desc("Palette to render the layer sprites in.")]
		public readonly string Palette = TileSet.TerrainPaletteInternalName;

		[Desc("Who should be allowed to crush a tree.")]
		public readonly BitSet<CrushClass> CrushClasses = new BitSet<CrushClass>("Tree");

		[Desc("Initial max amount per tile.")]
		public readonly int Hitpoints = 100000;

		[Desc("At which health level to display flames.")]
		public readonly int DamagedHitpoints = 75000;

		[Desc("Time in ticks to apply damage.")]
		public readonly int Interval = 8;

		[Desc("How much damage to apply to neighboring tiles when on fire.")]
		public readonly int Damage = 10;

		[Desc("Player that spawns the flame actors.")]
		public readonly string FlameOwner = "Creeps";

		[FieldLoader.Require]
		[Desc("Fake actor required for targeting.")]
		[ActorReference]
		public readonly string FlameActor = "";

		[FieldLoader.Require]
		[Desc("Terrain types a tree can change into.")]
		public readonly string[] TransformedTerrain = Array.Empty<string>();

		[FieldLoader.Require]
		[Desc("Which tile ID to replace with which munched variant")]
		public readonly Dictionary<ushort, ushort> CrushedTiles = new Dictionary<ushort, ushort>();

		[FieldLoader.Require]
		[Desc("Which tile ID to replace with which scorched variant")]
		public readonly Dictionary<ushort, ushort> BurnedTiles = new Dictionary<ushort, ushort>();

		public void RulesetLoaded(Ruleset rules, ActorInfo actorInfo)
		{
			var distinctCrushedTiles = CrushedTiles.Keys.Distinct().Count();
			if (CrushedTiles.Keys.Count > distinctCrushedTiles)
				throw new YamlException($"Duplicate tile in {nameof(CrushedTiles)}.");

			var distinctBurnedTiles = BurnedTiles.Keys.Distinct().Count();
			if (BurnedTiles.Keys.Count > distinctBurnedTiles)
				throw new YamlException($"Duplicate tile in {nameof(BurnedTiles)}.");

			if (distinctCrushedTiles != distinctBurnedTiles)
				throw new YamlException($"{nameof(CrushedTiles)} does not match {nameof(BurnedTiles)}.");
		}

		public IReadOnlyDictionary<CPos, SubCell> OccupiedCells(ActorInfo info, CPos location, SubCell subCell = SubCell.Any)
		{
			return new Dictionary<CPos, SubCell>() { { location, SubCell.FullCell } };
		}

		bool IOccupySpaceInfo.SharesCell => false;

		public override object Create(ActorInitializer init) { return new ForestLayer(init.Self, this); }
	}

	public class ForestLayer : IWorldLoaded, IOccupySpace, ICrushable, INotifyCrushed, ITick, ITickRender, IRenderOverlay, IRadarTerrainLayer
	{
		public int TotalTrees;
		public int TreesLeft => occupied.Count;
		public int TreesBurning;

		readonly ForestLayerInfo info;
		readonly World world;
		readonly CellLayer<int> hitpoints;
		readonly ITiledTerrainRenderer terrainRenderer;
		readonly CellLayer<(Color, Color)> radarColor;
		readonly Dictionary<CPos, TerrainTile?> dirty;
		readonly List<(CPos Cell, SubCell SubCell)> occupied;

		TerrainSpriteLayer render;
		PaletteReference paletteReference;

		int ticks;

		public ForestLayer(Actor self, ForestLayerInfo info)
		{
			this.info = info;
			world = self.World;
			hitpoints = new CellLayer<int>(world.Map);
			terrainRenderer = self.Trait<ITiledTerrainRenderer>();
			radarColor = new CellLayer<(Color, Color)>(world.Map);
			world.UpdateMaps(self, this);
			occupied = new List<(CPos, SubCell)>();
			dirty = new Dictionary<CPos, TerrainTile?>();
			ticks = info.Interval;
		}

		public void WorldLoaded(World w, WorldRenderer wr)
		{
			render = new TerrainSpriteLayer(w, wr, terrainRenderer.MissingTile, BlendMode.Alpha, wr.World.Type != WorldType.Editor);
			paletteReference = wr.Palette(info.Palette);

			var treeTiles = info.CrushedTiles.Keys;
			foreach (var forestCell in w.Map.AllCells.Where(cell => treeTiles.Contains(w.Map.Tiles[cell].Type)))
			{
				hitpoints[forestCell] = info.Hitpoints;
				occupied.Add((forestCell, SubCell.FullCell));
				TotalTrees++;
			}

			w.ActorMap.AddInfluence(w.WorldActor, this);
		}

		public CPos TopLeft => CPos.Zero;
		public WPos CenterPosition => WPos.Zero;
		public (CPos Cell, SubCell SubCell)[] OccupiedCells() { return occupied.ToArray(); }

		bool ICrushable.CrushableBy(Actor self, Actor crusher, BitSet<CrushClass> crushClasses)
		{
			return info.CrushClasses.Overlaps(crushClasses);
		}

		LongBitSet<PlayerBitMask> ICrushable.CrushableBy(Actor self, BitSet<CrushClass> crushClasses)
		{
			if (!info.CrushClasses.Overlaps(crushClasses))
				return self.World.NoPlayersMask;

			return self.World.AllPlayersMask;
		}

		void INotifyCrushed.WarnCrush(Actor self, Actor crusher, BitSet<CrushClass> crushClasses) { }
		void INotifyCrushed.OnCrush(Actor self, Actor crusher, BitSet<CrushClass> crushClasses)
		{
			var cell = crusher.Location;
			var originalTile = self.World.Map.Tiles[cell];
			var replacement = info.CrushedTiles[originalTile.Type];
			var replacementTile = new TerrainTile(replacement, 0x00);
			ReplaceTile(cell, replacementTile);
		}

		void ReplaceTile(CPos cell, TerrainTile replacementTile)
		{
			dirty[cell] = replacementTile;

			var uv = cell.ToMPos(world.Map);
			var tileInfo = world.Map.Rules.TerrainInfo.GetTerrainInfo(replacementTile);
			world.Map.CustomTerrain[uv] = tileInfo.TerrainType;
			hitpoints[uv] = 0;
			radarColor[uv] = (tileInfo.GetColor(world.LocalRandom), tileInfo.GetColor(world.LocalRandom));

			occupied.RemoveAll(o => o.Cell == cell);
			world.ActorMap.UpdateOccupiedCells(this);
		}

		public void Hit(CPos cell, int damage)
		{
			if (!hitpoints.Contains(cell) || hitpoints[cell] <= 0)
				return;

			hitpoints[cell] = hitpoints[cell] - damage;

			if (hitpoints[cell] < 1)
			{
				var originalTile = world.Map.Tiles[cell];
				var replacement = info.BurnedTiles[originalTile.Type];
				var replacementTile = new TerrainTile(replacement, 0x00);
				ReplaceTile(cell, replacementTile);
			}
		}

		void ITick.Tick(Actor self)
		{
			if (--ticks > 0)
				return;

			foreach (var cell in world.Map.AllCells)
			{
				var hp = hitpoints[cell];
				if (hp > 0 && hp < info.DamagedHitpoints)
				{
					if (!world.ActorMap.GetActorsAt(cell).Any(a => a.Info.Name == info.FlameActor))
					{
						var typeDictionary = new TypeDictionary { new OwnerInit(info.FlameOwner), new LocationInit(cell) };
						world.CreateActor(info.FlameActor, typeDictionary);
						TreesBurning++;
					}

					Hit(cell, info.Damage);
					foreach (var direction in CVec.Directions)
						Hit(cell + direction, info.Damage);
				}
				else
				{
					var flame = world.ActorMap.GetActorsAt(cell).FirstOrDefault(a => a.Info.Name == info.FlameActor);
					if (flame != null)
					{
						world.Remove(flame);
						TreesBurning--;
					}
				}
			}

			ticks = info.Interval;
		}

		void ITickRender.TickRender(WorldRenderer wr, Actor self)
		{
			var remove = new List<CPos>();
			foreach (var kv in dirty)
			{
				if (!self.World.FogObscures(kv.Key))
				{
					var tile = kv.Value;
					if (tile.HasValue)
					{
						// Terrain tiles define their origin at the topleft
						var s = terrainRenderer.TileSprite(tile.Value);
						var sprite = new Sprite(s.Sheet, s.Bounds, s.ZRamp, float2.Zero, s.Channel, s.BlendMode);
						render.Update(kv.Key, sprite, paletteReference);
					}
					else
						render.Clear(kv.Key);

					remove.Add(kv.Key);
				}
			}

			foreach (var r in remove)
				dirty.Remove(r);
		}

		void IRenderOverlay.Render(WorldRenderer wr)
		{
			render.Draw(wr.Viewport);
		}

		event Action<CPos> IRadarTerrainLayer.CellEntryChanged
		{
			add => radarColor.CellEntryChanged += value;
			remove => radarColor.CellEntryChanged -= value;
		}

		bool IRadarTerrainLayer.TryGetTerrainColorPair(MPos uv, out (Color Left, Color Right) value)
		{
			value = default;

			if (world.Map.CustomTerrain[uv] == byte.MaxValue)
				return false;

			var cell = uv.ToCPos(world.Map);
			if (!info.TransformedTerrain.Contains(world.Map.GetTerrainInfo(cell).Type))
				return false;

			value = radarColor[uv];
			return true;
		}
	}
}
