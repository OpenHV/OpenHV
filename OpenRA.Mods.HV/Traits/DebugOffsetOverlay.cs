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
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Displays a developer offset point, controllable via chat commands.",
		"Select the unit first.",
		"Rendering is enabled automatically with the first valid command.",
		"Available commands:",
		"`body`: Sets the reference point to the actor's center position.",
		"`turret X`: where X is the turret index whose center the reference point should be set. Falls back to actor center position on wrong index.",
		"`set X,Y,Z`: Sets the offset. No spaces are supported between the values.",
		"`add X,Y,Z`: Adds the value to the current offset. Negative values function to subtract. No spaces are supported between the values.",
		"`query`: Returns the current offset value in the chat.",
		"`disable`: Disables rendering of the offset.")]
	public class DebugOffsetOverlayInfo : ITraitInfo
	{
		public object Create(ActorInitializer init) { return new DebugOffsetOverlay(init.Self); }
	}

	public class DebugOffsetOverlay : Requires<BodyOrientationInfo>, IRenderAnnotations, INotifyCreated
	{
		static readonly WVec TargetPosHLine = new WVec(0, 128, 0);
		static readonly WVec TargetPosVLine = new WVec(128, 0, 0);

		readonly BodyOrientation coords;

		Turreted[] turrets;
		WVec devOffset;
		int turret = -1;
		bool enabled;

		public DebugOffsetOverlay(Actor self)
		{
			coords = self.Trait<BodyOrientation>();
		}

		void INotifyCreated.Created(Actor self)
		{
			turrets = self.TraitsImplementing<Turreted>().ToArray();
		}

		IEnumerable<IRenderable> IRenderAnnotations.RenderAnnotations(Actor self, WorldRenderer wr)
		{
			if (!enabled || self.World.FogObscures(self))
				yield break;

			var referencePoint = self.CenterPosition;
			var bodyOrientation = coords.QuantizeOrientation(self, self.Orientation);
			var devRenderOffset = devOffset;

			if (turret != -1)
			{
				var turretOrientation = turrets[turret].WorldOrientation(self) - bodyOrientation;
				devRenderOffset = devRenderOffset.Rotate(turretOrientation);
				referencePoint += coords.LocalToWorld(turrets[turret].Offset.Rotate(bodyOrientation));
			}

			devRenderOffset = coords.LocalToWorld(devRenderOffset.Rotate(bodyOrientation));

			yield return new LineAnnotationRenderable(referencePoint - TargetPosHLine, referencePoint + TargetPosHLine, 1, Color.Magenta);
			yield return new LineAnnotationRenderable(referencePoint - TargetPosVLine, referencePoint + TargetPosVLine, 1, Color.Magenta);

			var devPoint = referencePoint + devRenderOffset;
			var devOrientation = turret != -1 ? turrets[turret].WorldOrientation(self) :
				coords.QuantizeOrientation(self, self.Orientation);
			var dirOffset = new WVec(0, -224, 0).Rotate(devOrientation);
			yield return new LineAnnotationRenderable(devPoint, devPoint + dirOffset, 1, Color.Magenta);
		}

		bool IRenderAnnotations.SpatiallyPartitionable { get { return true; } }

		public void ParseCommand(Actor self, string message)
		{
			var command = message.Split(' ')[0].ToLowerInvariant();
			switch (command)
			{
				case "body":
					turret = -1;
					enabled = true;
					break;

				case "turret":
					int turretIndex;
					var parse = int.TryParse(message.Split(' ')[1], out turretIndex);
					if (parse = false || turretIndex >= turrets.Length)
						turret = -1;
					else
						turret = turretIndex;
					enabled = true;
					break;

				case "set":
					var setoffsets = message.Split(' ')[1].Split(',');
					if (setoffsets.Length != 3)
						break;

					int[] setoffset = new int[3];
					for (int i = 0; i < setoffsets.Length; i++)
						int.TryParse(setoffsets[i], out setoffset[i]);

					devOffset = new WVec(setoffset[0], setoffset[1], setoffset[2]);
					enabled = true;
					break;

				case "add":
					var addoffsets = message.Split(' ')[1].Split(',');
					if (addoffsets.Length != 3)
						break;

					int[] addoffset = new int[3];
					for (int i = 0; i < addoffsets.Length; i++)
						int.TryParse(addoffsets[i], out addoffset[i]);

					devOffset += new WVec(addoffset[0], addoffset[1], addoffset[2]);
					enabled = true;
					break;

				case "query":
					Game.Debug("The current DevOffset on actor {0} {1} is: {2},{3},{4}", self.Info.Name, self.ActorID, devOffset.X, devOffset.Y, devOffset.Z);
					break;

				case "disable":
					enabled = false;
					break;

				default:
					break;
			}
		}
	}
}
