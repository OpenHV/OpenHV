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
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Launches an attack with the actors weapons.")]
	public class AttackOrderPowerInfo : SupportPowerInfo, Requires<AttackBaseInfo>
	{
		[Desc("Range circle color.")]
		public readonly Color CircleColor = Color.Red;

		[Desc("Range circle line width.")]
		public readonly float CircleWidth = 1;

		[Desc("Range circle border color.")]
		public readonly Color CircleBorderColor = Color.FromArgb(96, Color.Black);

		[Desc("Range circle border width.")]
		public readonly float CircleBorderWidth = 3;

		[GrantedConditionReference]
		[Desc("The condition to grant to self while attacking.")]
		public readonly string AttackingCondition = null;

		public override object Create(ActorInitializer init) { return new AttackOrderPower(init.Self, this); }
	}

	sealed class AttackOrderPower : SupportPower, INotifyCreated, INotifyBurstComplete
	{
		readonly AttackOrderPowerInfo info;

		AttackBase attack;
		int attackToken = Actor.InvalidConditionToken;

		public AttackOrderPower(Actor self, AttackOrderPowerInfo info)
			: base(self, info)
		{
			this.info = info;
		}

		public override void SelectTarget(Actor self, string order, SupportPowerManager manager)
		{
			self.World.OrderGenerator = new SelectAttackPowerTarget(self, order, manager, info.Cursor, info.BlockedCursor, MouseButton.Left, attack);
		}

		public override void Activate(Actor self, Order order, SupportPowerManager manager)
		{
			base.Activate(self, order, manager);
			PlayLaunchSounds();

			if (!string.IsNullOrEmpty(info.AttackingCondition) && attackToken == Actor.InvalidConditionToken)
				attackToken = self.GrantCondition(info.AttackingCondition);

			attack.AttackTarget(order.Target, AttackSource.Default, false, false, true);
		}

		protected override void Created(Actor self)
		{
			attack = self.Trait<AttackBase>();

			base.Created(self);
		}

		void INotifyBurstComplete.FiredBurst(Actor self, in Target target, Armament a)
		{
			self.World.IssueOrder(new Order("Stop", self, false));

			if (attackToken != Actor.InvalidConditionToken)
				attackToken = self.RevokeCondition(attackToken);
		}
	}

	public class SelectAttackPowerTarget : OrderGenerator
	{
		readonly SupportPowerManager manager;
		readonly SupportPowerInstance instance;
		readonly string order;
		readonly string cursor;
		readonly string cursorBlocked;
		readonly MouseButton expectedButton;
		readonly AttackBase attack;

		public SelectAttackPowerTarget(Actor self, string order, SupportPowerManager manager,
			string cursor, string cursorBlocked, MouseButton button, AttackBase attack)
		{
			// Clear selection if using Left-Click Orders
			if (Game.Settings.Game.UseClassicMouseStyle)
				manager.Self.World.Selection.Clear();

			instance = manager.GetPowersForActor(self).FirstOrDefault();
			this.manager = manager;
			this.order = order;
			this.cursor = cursor;
			this.cursorBlocked = cursorBlocked;
			expectedButton = button;
			this.attack = attack;
		}

		bool IsValidTarget(World world, CPos cell)
		{
			var pos = world.Map.CenterOfCell(cell);
			var minRange = attack.GetMinimumRange().LengthSquared;
			var maxRange = attack.GetMaximumRange().LengthSquared;

			return world.Map.Contains(cell) && instance.Instances.Any(a => !a.IsTraitPaused
				&& (a.Self.CenterPosition - pos).HorizontalLengthSquared > minRange
				&& (a.Self.CenterPosition - pos).HorizontalLengthSquared < maxRange);
		}

		protected override IEnumerable<Order> OrderInner(World world, CPos cell, int2 worldPixel, MouseInput mi)
		{
			world.CancelInputMode();
			if (mi.Button == expectedButton && IsValidTarget(world, cell))
				yield return new Order(order, manager.Self, Target.FromCell(world, cell), false)
				{
					SuppressVisualFeedback = true
				};
		}

		protected override void Tick(World world)
		{
			// Cancel the OG if we can't use the power
			if (!manager.Powers.TryGetValue(order, out var p) || !p.Active || !p.Ready)
				world.CancelInputMode();
		}

		protected override IEnumerable<IRenderable> Render(WorldRenderer wr, World world) { yield break; }
		protected override IEnumerable<IRenderable> RenderAboveShroud(WorldRenderer wr, World world) { yield break; }

		protected override IEnumerable<IRenderable> RenderAnnotations(WorldRenderer wr, World world)
		{
			var info = instance.Info as AttackOrderPowerInfo;
			foreach (var a in instance.Instances.Where(i => !i.IsTraitPaused))
			{
				yield return new RangeCircleAnnotationRenderable(
					a.Self.CenterPosition,
					attack.GetMinimumRange(),
					0,
					info.CircleColor,
					info.CircleWidth,
					info.CircleBorderColor,
					info.CircleBorderWidth);

				yield return new RangeCircleAnnotationRenderable(
					a.Self.CenterPosition,
					attack.GetMaximumRange(),
					0,
					info.CircleColor,
					info.CircleWidth,
					info.CircleBorderColor,
					info.CircleBorderWidth);
			}
		}

		protected override string GetCursor(World world, CPos cell, int2 worldPixel, MouseInput mi)
		{
			return IsValidTarget(world, cell) ? cursor : cursorBlocked;
		}
	}
}
