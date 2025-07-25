#region Copyright & License Information
/*
 * Copyright 2019-2021 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Data;
using System.Text;
using OpenRA.Activities;

namespace OpenRA.Mods.HV.UtilityCommands
{
	public static class Extensions
	{
		public static string ToCharacterSeparatedValues(this DataTable table, string delimiter, bool includeHeader)
		{
			var result = new StringBuilder();

			if (includeHeader)
			{
				foreach (DataColumn column in table.Columns)
				{
					result.Append(column.ColumnName);
					result.Append(delimiter);
				}

				result.Remove(result.Length, 0);
				result.AppendLine();
			}

			foreach (DataRow row in table.Rows)
			{
				for (var x = 0; x < table.Columns.Count; x++)
				{
					if (x != 0)
						result.Append(delimiter);

					result.Append(row[table.Columns[x]]);
				}

				result.AppendLine();
			}

			result.Remove(result.Length, 0);
			result.AppendLine();

			return result.ToString();
		}

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
