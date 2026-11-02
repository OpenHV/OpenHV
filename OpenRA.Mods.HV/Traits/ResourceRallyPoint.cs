#region Copyright & License Information
/*
 * Copyright 2026 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Frozen;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.Effects;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Rally point that colors the line by the resource at the final waypoint.")]
	public class ResourceRallyPointInfo : RallyPointInfo
	{
		[Desc("Line color per resource type at the final waypoint. Falls back to the owner color.")]
		public readonly FrozenDictionary<string, Color> Colors = FrozenDictionary<string, Color>.Empty;

		public override object Create(ActorInitializer init) { return new ResourceRallyPoint(init.Self, this); }
	}

	public class ResourceRallyPoint : RallyPoint, INotifyCreated, INotifyAddedToWorld, INotifyRemovedFromWorld
	{
		readonly ResourceRallyPointInfo info;
		readonly IResourceLayer resourceLayer;
		ResourceRallyPointIndicator effect;

		public ResourceRallyPoint(Actor self, ResourceRallyPointInfo info)
			: base(self, info)
		{
			this.info = info;
			resourceLayer = self.World.WorldActor.TraitOrDefault<IResourceLayer>();
		}

		public Color LineColor(Actor self)
		{
			if (resourceLayer != null && Path.Count > 0)
			{
				var resourceType = resourceLayer.GetResource(Path[^1]).Type;
				if (resourceType != null && info.Colors.TryGetValue(resourceType, out var color))
					return color;
			}

			return self.OwnerColor();
		}

		void INotifyCreated.Created(Actor self)
		{
			effect = new ResourceRallyPointIndicator(self, this);
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			self.World.AddFrameEndTask(w => w.Add(effect));
		}

		void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
		{
			self.World.AddFrameEndTask(w => w.Remove(effect));
		}
	}
}
