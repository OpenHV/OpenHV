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

using System;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("This actor can be compromised by other actors.")]
	public class HackableInfo : PausableConditionalTraitInfo
	{
		[Desc("Condition to grant while under influence.")]
		[GrantedConditionReference]
		public readonly string Condition = null;

		[Desc("The sound played when the influence is revoked.")]
		public readonly string[] RevokeControlSounds = Array.Empty<string>();

		[Desc("Map player to transfer this actor to if the owner lost the game.")]
		public readonly string FallbackOwner = "Creeps";

		public override object Create(ActorInitializer init) { return new Hackable(this); }
	}

	public class Hackable : PausableConditionalTrait<HackableInfo>, INotifyKilled, INotifyActorDisposing, INotifyOwnerChanged
	{
		readonly HackableInfo info;
		Player creatorOwner;
		bool controlChanging;

		int token = Actor.InvalidConditionToken;

		public Actor Attacker { get; private set; }

		public Hackable(HackableInfo info)
			: base(info)
		{
			this.info = info;
		}

		public void LinkAttacker(Actor self, Actor attacker)
		{
			self.CancelActivity();

			if (Attacker == null)
				creatorOwner = self.Owner;

			controlChanging = true;

			var oldOwner = self.Owner;
			self.ChangeOwner(attacker.Owner);

			DisconnectAttacker(self, Attacker);
			Attacker = attacker;

			if (token == Actor.InvalidConditionToken && !string.IsNullOrEmpty(Info.Condition))
				token = self.GrantCondition(Info.Condition);

			if (attacker.Owner == creatorOwner)
				DisconnectAttacker(self, attacker);

			self.World.AddFrameEndTask(_ => controlChanging = false);
		}

		public void DisconnectAttacker(Actor self, Actor attacker)
		{
			if (attacker == null)
				return;

			self.World.AddFrameEndTask(_ =>
			{
				if (attacker.IsDead || attacker.Disposed)
					return;

				attacker.Trait<Hacker>().DisconnectVictim(attacker, self);
			});

			Attacker = null;

			if (token != Actor.InvalidConditionToken)
				token = self.RevokeCondition(token);
		}

		public void RevokeHack(Actor self)
		{
			self.CancelActivity();

			controlChanging = true;

			if (creatorOwner.WinState == WinState.Lost)
				self.ChangeOwner(self.World.Players.First(p => p.InternalName == info.FallbackOwner));
			else
				self.ChangeOwner(creatorOwner);

			DisconnectAttacker(self, Attacker);

			if (info.RevokeControlSounds.Length != 0)
				Game.Sound.Play(SoundType.World, info.RevokeControlSounds.Random(self.World.SharedRandom), self.CenterPosition);

			self.World.AddFrameEndTask(_ => controlChanging = false);
		}

		void INotifyKilled.Killed(Actor self, AttackInfo e)
		{
			DisconnectAttacker(self, Attacker);
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			DisconnectAttacker(self, Attacker);
		}

		void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			if (!controlChanging)
				DisconnectAttacker(self, Attacker);
		}

		protected override void TraitDisabled(Actor self)
		{
			if (Attacker != null)
				RevokeHack(self);
		}
	}
}
