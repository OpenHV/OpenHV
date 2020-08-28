--[[
   Copyright 2020 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

BaseBuildings = { "base", "generator", "miner2", "storage", "module" }

Colonists = { Scout1, Scout2, Ballon, Miner1, Miner2, Storage }

Tick = function()
	if (player.PowerProvided <= 20 or player.PowerState ~= "Normal") and DateTime.GameTime % DateTime.Seconds(10) == 0 then
		HasPower = false
		Media.DisplayMessage("Build a generator for power.", "Reminder")
	else
		HasPower = true
	end

	if not HasMiner and not Mining and HasPower and DateTime.GameTime % DateTime.Seconds(20) == 0 then
		local miners = Utils.Where(Map.ActorsInWorld, function(actor) return actor.Type == "miner" and actor.Owner == player end)
		if #miners == 0 then
			Media.DisplayMessage("Build a miner to collect resources.", "Reminder")
			HasMiner = false
		else
			HasMiner = true
		end
	end

	if HasPower and HasMiner and DateTime.GameTime % DateTime.Seconds(30) == 0 then
		local deployedMiners = Utils.Where(Map.ActorsInWorld, function(actor) return actor.Type == "miner2" and actor.Owner == player end)
		if #deployedMiners == 0 then
			Media.DisplayMessage("Deploy the miner on the mountain top with mineral deposits.", "Reminder")
			Mining = false
		else
			Mining = true
		end
	end

	if HasPower and Mining and DateTime.GameTime % DateTime.Seconds(40) == 0 then
		if player.Resources > player.ResourceCapacity * 0.8 then
			Media.DisplayMessage("Build a silo to store additional resources.", "Reminder")
		end
	end

	if HasPower and Mining and DateTime.GameTime % DateTime.Seconds(20) == 0 then
		local modules = Utils.Where(Map.ActorsInWorld, function(actor) return actor.Type == "module" and actor.Owner == player end)
		if #modules == 0 then
			Media.DisplayMessage("Build a module to train a scouting party.", "Reminder")
		end
	end

	if DateTime.GameTime % DateTime.Seconds(1) == 0 and not player.IsObjectiveCompleted(bridgehead) and CheckForBase(player, BaseBuildings) then
		player.MarkCompletedObjective(bridgehead)
	end

	if creeps.Resources >= creeps.ResourceCapacity * 0.75 then
		creeps.Cash = creeps.Cash + creeps.Resources - creeps.ResourceCapacity * 0.25
		creeps.Resources = creeps.ResourceCapacity * 0.25
	end
end

WorldLoaded = function()
	player = Player.GetPlayer("The Company")
	creeps = Player.GetPlayer("Creeps")

	InitObjectives(player)

	killColonists = player.AddPrimaryObjective("Eliminate all colonists in the area.")
	Trigger.OnAllKilled(Colonists, function() player.MarkCompletedObjective(killColonists) end)

	bridgehead = player.AddPrimaryObjective("Establish a bridgehead.")
	Trigger.OnKilled(Base, function() player.MarkFailedObjective(bridgehead) end)

	Camera.Position = Base.CenterPosition
end