--[[
   Copyright 2021-2023 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

PatrolTroopEasy = { "mgpodpatrol", "rocketpodpatrol", "mbt2patrol" }
PatrolTroopNormal = { "mgpodpatrol", "mgpodpatrol", "rocketpodpatrol", "rocketpodpatrol", "mbt2patrol" }
PatrolTroopHard = { "mgpodpatrol", "mgpodpatrol", "rocketpodpatrol", "flamepodpatrol", "rocketpodpatrol", "mbt2patrol", "mbt2patrol" }
PatrolPoints = { PatrolPoint1.Location, PatrolPoint2.Location, PatrolPoint3.Location, PatrolPoint4.Location }

SynapolPatrolInit = function()
	if Difficulty == "easy" then
		TroopFirst = Reinforcements.Reinforce(Enemy, PatrolTroopEasy, PatrolPoints)
		TroopSecond = Reinforcements.Reinforce(Enemy, PatrolTroopEasy, PatrolPoints)
	end
	if Difficulty == "normal" then
		TroopFirst = Reinforcements.Reinforce(Enemy, PatrolTroopNormal, PatrolPoints)
		TroopSecond = Reinforcements.Reinforce(Enemy, PatrolTroopNormal, PatrolPoints)
	end
	if Difficulty == "hard" then
		TroopFirst = Reinforcements.Reinforce(Enemy, PatrolTroopHard, PatrolPoints)
		TroopSecond = Reinforcements.Reinforce(Enemy, PatrolTroopHard, PatrolPoints)
	end

	Utils.Do(TroopFirst, function(a)
		if not a.IsDead then
			a.Patrol(PatrolPoints, true, 0)
		end
	end)
	Utils.Do(TroopSecond, function(a)
		if not a.IsDead then
			a.Patrol(PatrolPoints, true, 0)
		end
	end)
end

ProduceUnits = function()
	if Difficulty == "easy" then
		Enemy.Build({ "mgpod", "mgpod", "mgpod", "rocketpod", "mbt2", "mbt2" })
	end
	if Difficulty == "normal" then
		Enemy.Build({ "mgpod", "mgpod", "mgpod", "rocketpod", "mortarpod", "mbt2", "mbt2", "artillery" })
	end
	if Difficulty == "hard" then
		Enemy.Build({ "mgpod", "mgpod", "mgpod", "rocketpod", "rocketpod", "mortarpod", "flamepod", "mbt2", "mbt2", "missiletank", "artillery" })
	end

	if not Difficulty == "easy" and #Enemy.GetActorsByType("miner") == 0 then
		Enemy.Build({ "miner" })
	end
end

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

	if DateTime.GameTime % DateTime.Seconds(90) and DateTime.GameTime > DateTime.Seconds(150) then
		ProduceUnits() -- every 01min30sec, starting from 02min30sec, produce a basic Synapol squad
	end
end

WorldLoaded = function()
	Human = Player.GetPlayer("Yuruki Industries")
	Enemy = Player.GetPlayer("Synapol Corporation")

	InitObjectives(Human)
	SynapolPatrolInit()

	EnemyEliminatedObjective = AddPrimaryObjective(Human, "eliminate-all-competitors")

	Camera.Position = Base.CenterPosition
end
