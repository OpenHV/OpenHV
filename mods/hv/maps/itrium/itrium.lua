--[[
   Copyright 2023-2025 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

BaseBuildings = { "base", "generator", "miner2", "module" }

ColonistsBuildings = { Miner1, Miner2, Generator1, Generator2, Generator3, Generator4, Storage }
Colonists = { rifleman, rocketeer, Scout3, Scout4, Scout5, Scout6, Scout7, Scout8, Scout9, Ballon }

Reminder = UserInterface.GetFluentMessage("reminder")
Tick = function()
	if (Human.PowerProvided <= 20 or Human.PowerState ~= "Normal") and DateTime.GameTime % DateTime.Seconds(10) == 0 then
		HasPower = false
		Media.DisplayMessage(UserInterface.GetFluentMessage("build-power-plant"), Reminder)
	else
		HasPower = true
	end

	if not HasMiner and not Mining and HasPower and DateTime.GameTime % DateTime.Seconds(20) == 0 then
		local miners = Utils.Where(Map.ActorsInWorld, function(actor) return (actor.Type == "storage") and actor.Owner == Human end)
		if #miners == 0 then
			Media.DisplayMessage(UserInterface.GetFluentMessage("build-storage"), Reminder)
			HasMiner = false
		else
			HasMiner = true
		end
	end

	if HasPower and HasMiner and DateTime.GameTime % DateTime.Seconds(30) == 0 then
		local deployedMiners = Utils.Where(Map.ActorsInWorld, function(actor) return actor.Type == "miner2" and actor.Owner == Human end)
		if #deployedMiners == 0 then
			Media.DisplayMessage(UserInterface.GetFluentMessage("deploy-miner"), Reminder)
			Mining = false
		else
			Mining = true
		end
	end

	if HasPower and Mining and DateTime.GameTime % DateTime.Seconds(20) == 0 then
		local modules = Utils.Where(Map.ActorsInWorld, function(actor) return actor.Type == "module" and actor.Owner == Human end)
		if #modules == 0 then
			Media.DisplayMessage(UserInterface.GetFluentMessage("build-module"), Reminder)
		end
	end

	if DateTime.GameTime % DateTime.Seconds(1) == 0 and not Human.IsObjectiveCompleted(Bridgehead) and CheckForBase(Human, BaseBuildings) then
		Human.MarkCompletedObjective(Bridgehead)
	end

	if Enemy.Resources >= Enemy.ResourceCapacity * 0.75 then
		Enemy.Cash = Enemy.Cash + Enemy.Resources - Enemy.ResourceCapacity * 0.25
		Enemy.Resources = Enemy.ResourceCapacity * 0.25
	end
end

WorldLoaded = function()
	Human = Player.GetPlayer("Yuruki Industries")
	Enemy = Player.GetPlayer("Synapol Corporation")

	InitObjectives(Human)

	KillColonists = AddPrimaryObjective(Human, "eliminate-all-colonist")
	Trigger.OnAllKilledOrCaptured(Utils.Concat(Colonists, ColonistsBuildings),
		function() Human.MarkCompletedObjective(KillColonists) end)

	Bridgehead = AddPrimaryObjective(Human, "build-all-structures")
	Trigger.OnKilled(Base, function() Human.MarkFailedObjective(Bridgehead) end)

	Trigger.OnAllRemovedFromWorld(ColonistsBuildings, function()
		Utils.Do(Enemy.GetGroundAttackers(), IdleHunt)
	end)

	Camera.Position = Base.CenterPosition
end
