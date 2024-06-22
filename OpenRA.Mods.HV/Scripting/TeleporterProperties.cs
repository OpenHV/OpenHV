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

using Eluant;
using OpenRA.Mods.HV.Traits;
using OpenRA.Scripting;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Scripting
{
	[ScriptPropertyGroup("Support Powers")]
	public class TeleporterProperties : ScriptActorProperties, Requires<TeleportPowerInfo>
	{
		public TeleporterProperties(ScriptContext context, Actor self)
			: base(context, self) { }

		[Desc("Teleport a group of actors.")]
		public void Teleport(LuaTable unitLocationPairs)
		{
			foreach (var kv in unitLocationPairs)
			{
				Actor actor;
				CPos cell;
				using (kv.Key)
				using (kv.Value)
				{
					if (!kv.Key.TryGetClrValue(out actor) || !kv.Value.TryGetClrValue(out cell))
						throw new LuaException($"Teleport requires a table of Actor,CPos pairs. Received {kv.Key.WrappedClrType().Name},{kv.Value.WrappedClrType().Name}");
				}

				var teleportable = actor.TraitsImplementing<Teleportable>()
					.FirstEnabledConditionalTraitOrDefault();

				if (teleportable != null && teleportable.CanTeleportTo(actor, cell))
					teleportable.Teleport(actor, cell, Self);
			}
		}
	}
}
