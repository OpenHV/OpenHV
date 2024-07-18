--[[
   Copyright 2021-2023 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

Tick = function()
	if Enemy.Resources >= Enemy.ResourceCapacity * 0.75 then
		Enemy.Cash = Enemy.Cash + Enemy.Resources - Enemy.ResourceCapacity * 0.25
		Enemy.Resources = Enemy.ResourceCapacity * 0.25
	end

	if Enemy.HasNoRequiredUnits() then
		Human.MarkCompletedObjective(EnemyEliminatedObjective)
	end

	if Human.HasNoRequiredUnits() then
		Human.MarkFailedObjective(EnemyEliminatedObjective)
	end
end

WorldLoaded = function()
	Human = Player.GetPlayer("Yuruki Industries")
	Enemy = Player.GetPlayer("Synapol Corporation")

	InitObjectives(Human)

	EnemyEliminatedObjective = AddPrimaryObjective(Human, "eliminate-all-competitors")

	Camera.Position = Base.CenterPosition
end
