#region Copyright & License Information
/*
 * Copyright 2019-2020 The OpenHV Developers (see CREDITS)
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
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits.Render
{
	[Desc("Emerge from water animation.")]
	class WithSeaMonsterBodyInfo : WithSpriteBodyInfo, Requires<BodyOrientationInfo>, Requires<IFacingInfo>
	{
		[SequenceReference]
		public readonly string LeftSequence = "left";

		[SequenceReference]
		public readonly string RightSequence = "right";

		[SequenceReference]
		[Desc("Placeholder sequence to use in the map editor.")]
		public readonly string EditorSequence = "left";

		public readonly int EmergeDuration = 0;

		public override IEnumerable<IActorPreview> RenderPreviewSprites(ActorPreviewInitializer init, string image, int facings, PaletteReference p)
		{
			if (!EnabledByDefault)
				yield break;

			var anim = new Animation(init.World, image);
			var sequence = init.World.Type == WorldType.Editor ? EditorSequence : Sequence;
			anim.PlayFetchIndex(RenderSprites.NormalizeSequence(anim, init.GetDamageState(), sequence), () => 0);
			yield return new SpriteActorPreview(anim, () => WVec.Zero, () => 0, p);
		}

		public override object Create(ActorInitializer init) { return new WithSeaMonsterBody(init, this); }
	}

	class WithSeaMonsterBody : WithSpriteBody, ITick
	{
		static readonly WAngle Left = new WAngle(256);
		static readonly WAngle Right = new WAngle(768);

		readonly WithSeaMonsterBodyInfo info;
		readonly RenderSprites rs;
		readonly IFacing facing;
		readonly IMove move;

		public WithSeaMonsterBody(ActorInitializer init, WithSeaMonsterBodyInfo info)
			: base(init, info)
		{
			this.info = info;
			rs = init.Self.Trait<RenderSprites>();
			facing = init.Self.Trait<IFacing>();
			move = init.Self.Trait<IMove>();
			rs.GetImage(init.Self);
		}

		void ITick.Tick(Actor self)
		{
			if (IsTraitDisabled || IsTraitPaused)
				return;

			if (self.CurrentActivity == null)
				return;

			var targets = self.CurrentActivity.GetTargets(self);
			if (!targets.Any())
				return;

			var positions = targets.First().Positions;
			if (!positions.Any())
				return;

			var moveDuration = move.EstimatedMoveDuration(self, self.CenterPosition, positions.First());
			if (moveDuration < info.EmergeDuration)
				return;

			if (facing.Facing == Left)
			{
				var left = NormalizeSequence(self, info.LeftSequence);
				if (DefaultAnimation.CurrentSequence.Name != left)
					DefaultAnimation.PlayThen(left, () => CancelCustomAnimation(self));
			}
			else if (facing.Facing == Right)
			{
				var right = NormalizeSequence(self, info.RightSequence);
				if (DefaultAnimation.CurrentSequence.Name != right)
					DefaultAnimation.PlayBackwardsThen(right, () => CancelCustomAnimation(self));
			}
		}
	}
}
