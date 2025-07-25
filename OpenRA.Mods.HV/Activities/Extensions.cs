#region Copyright & License Information
/*
 * Copyright 2019-2025 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Activities;

namespace OpenRA.Mods.HV.Activities
{
	public static class Extensions
	{
		public static void InsertActivityInQueue(Actor actor, Activity insertAfterThisActivity, Activity activityToInsert)
		{
			if (insertAfterThisActivity == null)
				actor.QueueActivity(activityToInsert);
			if (insertAfterThisActivity.NextActivity == null)
				insertAfterThisActivity.Queue(activityToInsert);

			var activityToReplace = insertAfterThisActivity.NextActivity;
			var followingActivities = activityToReplace.NextActivity;
			activityToReplace.Cancel(actor);
			insertAfterThisActivity.Queue(activityToInsert);
			insertAfterThisActivity.Queue(followingActivities);
		}
	}
}
