--[[
   Copyright 2021-2023 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

Warning = UserInterface.Translate("warning")
ColonyReinforcementsSent = false
ColonyReinforcementUnitsPods = { "scout1", "scout1", "scout1", "scout1", "electricpod", "electricpod", "electricpod", "mortarpod", "mortarpod", "sniperpod", "sniperpod", "technician", "technician" }
ColonyReinforcementUnitsTanks = { "aatank2", "aatank2", "tank3", "tank3", "tank3", "artil", "artil", "tank16", "tank16", "tank2", "tank2", "tank2", "tank2", "tank1", "tank1", "tank1", "tank1" }

Tick = function()
	local colonybasements = Enemy.GetActorsByTypes({ "prop5", "prop8", "prop7", "flagpost", "watchtower", "comlink" })

	if Human.IsObjectiveCompleted(DestroyColonyObjective) then
		return
	end
    
	if DateTime.GameTime > DateTime.Seconds(180) and not ColonyReinforcementsSent then  -- send reinforcements units at the colony basement at 2"30' mins
		ColonyReinforcementsSent = true
		Media.DisplayMessage(UserInterface.Translate("detected"), Warning)
		Trigger.AfterDelay(200, function()
			ColonyReinforcementsPods = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPods, { ModuleExit.Location, ColonyDestination.Location })
			ColonyReinforcementsTanks = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsTanks, { FactoryExit.Location, ColonyDestination.Location })
			Media.DisplayMessage(UserInterface.Translate("reinforcements-near-colony"), Warning)
		end)
	end

    if not Human.IsObjectiveCompleted(DestroyColonyObjective) and #colonybasements == 0 then -- if the colony has been eliminated
		Human.MarkCompletedObjective(DestroyColonyObjective)
	end

end

WorldLoaded = function()
	Human = Player.GetPlayer("Synapol Corporation")
	Enemy = Player.GetPlayer("Yuruki Industries")

	InitObjectives(Human)

	DestroyColonyObjective = AddPrimaryObjective(Human, "destroy-colony")
	DestroyYurukiMilitary = AddPrimaryObjective(Human, "destroy-yuruki-military")

	Camera.Position = Outpost.CenterPosition
end
