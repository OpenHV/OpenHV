--[[
   Copyright 2023 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

IdleHunt = function(actor)
	if actor.HasProperty("Hunt") and not actor.IsDead then
		Trigger.OnIdle(actor, actor.Hunt)
	end
end

---Adds a new mandatory objective, translates it and announces it via in-game chat message.
---@param player player recipient of the objective
---@param description string key of the translation string
---@return number id used to query for the objective later
AddPrimaryObjective = function(player, description)
	local translation = UserInterface.GetFluentMessage(description)
	Media.DisplayMessage(translation, UserInterface.GetFluentMessage("new-primary-objective"))
	return player.AddObjective(translation, UserInterface.GetFluentMessage("primary"), true)
end

---Adds a new optional objective, translates it and announces it via in-game chat message.
---@param player player recipient of the objective
---@param description string key of the translation string
---@return number id used to query for the objective later
AddSecondaryObjective = function(player, description)
	local translation = UserInterface.GetFluentMessage(description)
	Media.DisplayMessage(translation, UserInterface.GetFluentMessage("new-secondary-objective"))
	return player.AddObjective(translation, UserInterface.GetFluentMessage("secondary"), false)
end
