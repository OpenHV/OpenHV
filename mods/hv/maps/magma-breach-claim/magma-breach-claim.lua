--[[
   Copyright 2024-2025 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

ReinforcementUnits = { "rifleman", "rifleman", "rifleman", "rocketeer", "rocketeer", "buggy", "buggy", "bike", "bike", "stealthtank", "artillery", "technician", "repairtank", "hackertank", "missiletank" }

Warning = UserInterface.GetFluentMessage("warning")

Tick = function()
	if Enemy.HasNoRequiredUnits() then
		Human.MarkCompletedObjective(EnemyEliminatedObjective)
	end

    if DateTime.GameTime % DateTime.Seconds(150) == 0 and DateTime.GameTime > DateTime.Seconds(150) and not Human.IsObjectiveCompleted(EnemyEliminatedObjective) then  -- happens every 2.5 mins, starting at 2.5 game minutes, if the player hasn't killed all units
        SynapolReinforcements1 = Reinforcements.Reinforce(Enemy, ReinforcementUnits, { SpawningWaypoint1.Location, DestinationWaypoint1.Location })
        SynapolReinforcements2 = Reinforcements.Reinforce(Enemy, ReinforcementUnits, { SpawningWaypoint2.Location, DestinationWaypoint2.Location })
		Utils.Do({ SynapolReinforcements1, SynapolReinforcements2 }, function(SynapolReinforcements)
			Utils.Do(SynapolReinforcements, function(unit)
				if not unit.IsDead then
					Trigger.OnDamaged(unit, function()
						unit.Stop()
						IdleHunt(unit)
					end)
				end
			end)
		end)
        Media.DisplayMessage(UserInterface.GetFluentMessage("reinforcements-incoming"), Warning)
    end

 	local towers = Human.GetActorsByType("miner2")
	local towerDeposits = UserInterface.GetFluentMessage("towers-deposits", { ["towers"] = #towers, ["deposits"] = Resource.TotalResourceCells })
	UserInterface.SetMissionText("\n\n\n" .. towerDeposits)
    if not Human.IsObjectiveCompleted(ResourcesClaimedObjective) and #towers == Resource.TotalResourceCells then -- if the player has built every mining tower
        Human.MarkCompletedObjective(ResourcesClaimedObjective)
    end
end

WorldLoaded = function()
	Human = Player.GetPlayer("Yuruki Industries")
	Enemy = Player.GetPlayer("Synapol Corporation")

	InitObjectives(Human)

	EnemyEliminatedObjective = AddPrimaryObjective(Human, "claim-land")
	ResourcesClaimedObjective = AddPrimaryObjective(Human, "build-all-towers")

	Camera.Position = Base.CenterPosition
end
