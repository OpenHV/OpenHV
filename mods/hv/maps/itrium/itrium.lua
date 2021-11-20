--[[
   Copyright 2020 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

BaseBuildings = { "base", "generator", "miner2", "module" }

Colonists = { Scout1, Scout2, Scout3, Scout4, Scout5, Scout6, Scout7, Scout8, Scout9, Ballon, Miner1, Miner2, Generator1, Generator2, Generator3, Generator4, Storage}

Civilians = { TechMiner1, TechMiner2 }

Tick = function()
	if (player.PowerProvided <= 20 or player.PowerState ~= "Normal") and DateTime.GameTime % DateTime.Seconds(10) == 0 then
		HasPower = false
		Media.DisplayMessage("Build a power plant to generate electricity.", "Reminder")
	else
		HasPower = true
	end

	if not HasMiner and not Mining and HasPower and DateTime.GameTime % DateTime.Seconds(20) == 0 then
		local miners = Utils.Where(Map.ActorsInWorld, function(actor) return (actor.Type == "storage") and actor.Owner == player end)
		if #miners == 0 then
			Media.DisplayMessage("Build a storage to collect resources.", "Reminder")
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
			Media.DisplayMessage("Build a module to train pods.", "Reminder")
		end
	end

	if DateTime.GameTime % DateTime.Seconds(1) == 0 and not player.IsObjectiveCompleted(bridgehead) and CheckForBase(player, BaseBuildings) then
		player.MarkCompletedObjective(bridgehead)
	end

	if enemy.Resources >= enemy.ResourceCapacity * 0.75 then
		enemy.Cash = enemy.Cash + enemy.Resources - enemy.ResourceCapacity * 0.25
		enemy.Resources = enemy.ResourceCapacity * 0.25
	end
end

WorldLoaded = function()
	player = Player.GetPlayer("Yuruki Industries")
	enemy = Player.GetPlayer("Synapol Corporation")

	InitObjectives(player)

	killColonists = player.AddPrimaryObjective("Eliminate all colonists in the area.")
	Trigger.OnAllKilledOrCaptured(Colonists, function() player.MarkCompletedObjective(killColonists) end)

	bridgehead = player.AddPrimaryObjective("Build all available structures.")
	Trigger.OnKilled(Base, function() player.MarkFailedObjective(bridgehead) end)

	Camera.Position = Base.CenterPosition
end
