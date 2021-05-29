--[[
   Copyright 2021 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

Tick = function()
	if creeps.Resources >= creeps.ResourceCapacity * 0.75 then
		creeps.Cash = creeps.Cash + creeps.Resources - creeps.ResourceCapacity * 0.25
		creeps.Resources = creeps.ResourceCapacity * 0.25
	end

	if creeps.HasNoRequiredUnits() then
		player.MarkCompletedObjective(EnemyEliminatedObjective)
	end

end

WorldLoaded = function()
	player = Player.GetPlayer("The Company")
	creeps = Player.GetPlayer("Creeps")

	InitObjectives(player)

	EnemyEliminatedObjective = player.AddPrimaryObjective("Eliminate all competitors in the area.")

	Camera.Position = Base.CenterPosition
end
