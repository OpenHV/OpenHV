#region Copyright & License Information
/*
 * Copyright 2024-2025 The OpenHV Developers (see CREDITS)
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
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Passenger can do drive by shootings with separate firing offsets ignoring facing.")]
	public class AttackOpenToppedInfo : AttackFollowInfo, IRulesetLoaded
	{
		[FieldLoader.Require]
		[Desc("Fire port offsets in local coordinates.")]
		public readonly WVec[] PortOffsets = null;

		public override object Create(ActorInitializer init) { return new AttackOpenTopped(init.Self, this); }
		public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			if (PortOffsets.Length == 0)
				throw new YamlException("PortOffsets must have at least one entry.");

			base.RulesetLoaded(rules, ai);
		}
	}

	public class AttackOpenTopped : AttackFollow, IRender, INotifyPassengerEntered, INotifyPassengerExited
	{
		readonly AttackOpenToppedInfo info;
		readonly Lazy<BodyOrientation> coords;
		readonly List<Actor> actors;
		readonly List<Armament> armaments;
		readonly HashSet<(AnimationWithOffset Animation, string Sequence)> muzzles;
		readonly Dictionary<Actor, IFacing> passengerFacings;
		readonly Dictionary<Actor, IPositionable> passengerPositions;
		readonly Dictionary<Actor, RenderSprites> passengerRenders;

		public AttackOpenTopped(Actor self, AttackOpenToppedInfo info)
			: base(self, info)
		{
			this.info = info;
			coords = Exts.Lazy(self.Trait<BodyOrientation>);
			actors = [];
			armaments = [];
			muzzles = [];
			passengerFacings = [];
			passengerPositions = [];
			passengerRenders = [];
		}

		protected override Func<IEnumerable<Armament>> InitializeGetArmaments(Actor self)
		{
			return () => armaments;
		}

		void INotifyPassengerEntered.OnPassengerEntered(Actor self, Actor passenger)
		{
			actors.Add(passenger);
			passengerFacings.Add(passenger, passenger.Trait<IFacing>());
			passengerPositions.Add(passenger, passenger.Trait<IPositionable>());
			passengerRenders.Add(passenger, passenger.Trait<RenderSprites>());
			armaments.AddRange(
				passenger.TraitsImplementing<Armament>()
				.Where(a => info.Armaments.Contains(a.Info.Name)));
		}

		void INotifyPassengerExited.OnPassengerExited(Actor self, Actor passenger)
		{
			actors.Remove(passenger);
			passengerFacings.Remove(passenger);
			passengerPositions.Remove(passenger);
			passengerRenders.Remove(passenger);
			armaments.RemoveAll(a => a.Actor == passenger);
		}

		WVec SelectFirePort(Actor firer)
		{
			var passengerIndex = actors.IndexOf(firer);
			if (passengerIndex == -1)
				return new WVec(0, 0, 0);

			var portIndex = passengerIndex % info.PortOffsets.Length;

			return info.PortOffsets[portIndex];
		}

		WVec PortOffset(Actor self, WVec offset)
		{
			var bodyOrientation = coords.Value.QuantizeOrientation(self.Orientation);
			return coords.Value.LocalToWorld(offset.Rotate(bodyOrientation));
		}

		public override void DoAttack(Actor self, in Target target)
		{
			if (!CanAttack(self, target))
				return;

			var targetedPosition = GetTargetPosition(self.CenterPosition, target);
			var targetYaw = (targetedPosition - self.CenterPosition).Yaw;

			foreach (var armament in Armaments)
			{
				if (armament.IsTraitDisabled)
					continue;

				var port = SelectFirePort(armament.Actor);

				passengerFacings[armament.Actor].Facing = targetYaw;
				passengerPositions[armament.Actor].SetCenterPosition(armament.Actor, self.CenterPosition + PortOffset(self, port));

				if (!armament.CheckFire(armament.Actor, facing, target))
					continue;

				if (armament.Info.MuzzleSequence != null)
				{
					// Muzzle facing is fixed once the firing starts
					var muzzleAnimation = new Animation(self.World, passengerRenders[armament.Actor].GetImage(armament.Actor), () => targetYaw);
					var sequence = armament.Info.MuzzleSequence;
					var palette = armament.Info.MuzzlePalette;

					var muzzleFlash = new AnimationWithOffset(muzzleAnimation,
						() => PortOffset(self, port),
						() => false,
						p => RenderUtils.ZOffsetFromCenter(self, p, 1024));

					var pair = (muzzleFlash, palette);
					muzzles.Add(pair);
					muzzleAnimation.PlayThen(sequence, () => muzzles.Remove(pair));
				}
			}
		}

		IEnumerable<IRenderable> IRender.Render(Actor self, WorldRenderer wr)
		{
			foreach (var muzzle in muzzles)
				foreach (var renderable in muzzle.Animation.Render(self, wr.Palette(muzzle.Sequence)))
					yield return renderable;
		}

		IEnumerable<Rectangle> IRender.ScreenBounds(Actor self, WorldRenderer wr)
		{
			yield break;
		}

		protected override void Tick(Actor self)
		{
			base.Tick(self);

			// Take a copy so that Tick() can remove animations
			foreach (var m in muzzles.ToArray())
				m.Animation.Animation.Tick();
		}
	}
}
