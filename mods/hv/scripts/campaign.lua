--[[
   Copyright 2019-2020 The OpenHV Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

Difficulty = Map.LobbyOption("difficulty")

IdleHunt = function(actor)
	if actor.HasProperty("Hunt") and not actor.IsDead then
		Trigger.OnIdle(actor, actor.Hunt)
	end
end

InitObjectives = function(player)
	Trigger.OnObjectiveAdded(player, function(p, id)
		Media.DisplayMessage(p.GetObjectiveDescription(id), "New " .. string.lower(p.GetObjectiveType(id)) .. " objective")
	end)

	Trigger.OnObjectiveCompleted(player, function(p, id)
		Media.DisplayMessage(p.GetObjectiveDescription(id), "Objective completed")
	end)
	Trigger.OnObjectiveFailed(player, function(p, id)
		Media.DisplayMessage(p.GetObjectiveDescription(id), "Objective failed")
	end)
end

CheckForBase = function(player, buildingTypes)
	local count = 0

	Utils.Do(buildingTypes, function(name)
		if #player.GetActorsByType(name) > 0 then
			count = count + 1
		end
	end)

	return count == #buildingTypes
end