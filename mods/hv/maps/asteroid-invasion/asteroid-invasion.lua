--[[
   Copyright 2024 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

Warning = UserInterface.Translate("warning")
ColonyReinforcementsSent = false
MilitaryProductionStarted = false
CooldownBeforeDetection = 180
ColonyReinforcementUnitsPods = { "scout1", "scout1", "scout1", "scout1", "electricpod", "electricpod", "electricpod", "mortarpod", "mortarpod", "sniperpod", "sniperpod", "technician", "technician", "saucer" }
ColonyReinforcementUnitsTanks = { "aatank2", "aatank2", "tank3", "tank3", "tank3", "artil", "artil", "tank16", "tank16", "tank2", "tank2", "tank2", "tank2", "tank1", "tank1", "tank1", "tank1" }
ColonyReinforcementUnitsPodsBig = { "scout1", "scout1", "scout1", "scout1", "electricpod", "electricpod", "electricpod", "mortarpod", "mortarpod", "sniperpod", "sniperpod", "technician", "technician", "scout1", "scout1", "scout1", "scout1", "electricpod", "electricpod", "electricpod", "mortarpod", "mortarpod", "sniperpod", "sniperpod", "technician", "technician", "scout1", "scout1", "scout1", "scout1", "electricpod", "electricpod", "electricpod", "mortarpod", "mortarpod", "sniperpod", "sniperpod", "technician", "technician", "saucer", "saucer" }
ColonyReinforcementUnitsTanksBig = { "aatank2", "aatank2", "tank3", "tank3", "tank3", "artil", "artil", "tank16", "tank16", "tank2", "tank2", "tank2", "tank2", "tank1", "tank1", "tank1", "tank1", "aatank2", "aatank2", "tank3", "tank3", "tank3", "artil", "artil", "tank16", "tank16", "tank2", "tank2", "tank2", "tank2", "tank1", "tank1", "tank1", "tank1", "aatank2", "aatank2", "tank3", "tank3", "tank3", "artil", "artil", "tank16", "tank16", "tank2", "tank2", "tank2", "tank2", "tank1", "tank1", "tank1", "tank1" }

Tick = function()
	local colonybasements = Enemy.GetActorsByTypes({ "prop5", "prop8", "prop7", "flagpost", "watchtower", "comlink" })
	local militarybasements = Enemy.GetActorsByTypes({ "generator", "aaturret", "bunker", "turret", "module", "factory3", "radar", "uplink", "field", "techcenter", "tradplat", "starport", "storage" })
	local enemybarracks = Enemy.GetActorsByTypes({ "factory3", "module" })

	if Human.IsObjectiveCompleted(DestroyColonyObjective) and not Human.IsObjectiveCompleted(DestroyYurukiMilitary) and not MilitaryProductionStarted and #enemybarracks > 0 then -- if the player has anihilated the colony basements, enable yuruki production to create three waves. Also enables the AI to use the railgun
		MilitaryProductionStarted = true
		Trigger.AfterDelay(250, function()
			Media.DisplayMessage(UserInterface.Translate("enemy-production-started"), Warning)
			MilitaryReinforcementsPods1 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPodsBig, { ModuleExit.Location, HumanBaseDestination.Location })
			MilitaryReinforcementsTanks1 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsTanksBig, { FactoryExit.Location, HumanBaseDestination.Location })
			Trigger.AfterDelay(200, function()
				Media.DisplayMessage(UserInterface.Translate("reinforcements-incoming"), Warning)
			end)
		end)
		Trigger.AfterDelay(950, function()
			MilitaryReinforcementsPods2 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPodsBig, { ModuleExit.Location, HumanBaseDestination.Location })
			MilitaryReinforcementsPods3 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPodsBig, { ModuleExit.Location, HumanBaseDestination.Location })
			MilitaryReinforcementsTanks2 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsTanksBig, { FactoryExit.Location, HumanBaseDestination.Location })
			Trigger.AfterDelay(200, function()
				Media.DisplayMessage(UserInterface.Translate("reinforcements-incoming"), Warning)
			end)
		end)
		Trigger.AfterDelay(1200, function()
			MilitaryReinforcementsPods4 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPodsBig, { ModuleExit.Location, HumanBaseDestination.Location })
			MilitaryReinforcementsPods5 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPodsBig, { ModuleExit.Location, HumanBaseDestination.Location })
			MilitaryReinforcementsPods6 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPodsBig, { ModuleExit.Location, HumanBaseDestination.Location })
			MilitaryReinforcementsTanks3 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsTanksBig, { FactoryExit.Location, HumanBaseDestination.Location })
			MilitaryReinforcementsTanks4 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsTanksBig, { FactoryExit.Location, HumanBaseDestination.Location })
			Trigger.AfterDelay(200, function()
				Media.DisplayMessage(UserInterface.Translate("reinforcements-incoming"), Warning)
			end)
		end)
		Enemy.GrantCondition("railgun-enabled")
	end
	
	if DateTime.GameTime > DateTime.Seconds(180) and not ColonyReinforcementsSent and #enemybarracks > 0 then  -- send reinforcements units at the colony basement at 2"30' mins
		ColonyReinforcementsSent = true
		Media.DisplayMessage(UserInterface.Translate("detected"), Warning)
		Trigger.AfterDelay(200, function()
			ColonyReinforcementsPods1 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPods, { ModuleExit.Location, ColonyDestination.Location })
			ColonyReinforcementsTanks1 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsTanks, { FactoryExit.Location, ColonyDestination.Location })
			Media.DisplayMessage(UserInterface.Translate("reinforcements-near-colony"), Warning)
		end)
		Trigger.AfterDelay(950, function()
			ColonyReinforcementsPods2 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPodsBig, { ModuleExit.Location, ColonyDestination.Location })
			ColonyReinforcementsTanks2 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsTanksBig, { FactoryExit.Location, ColonyDestination.Location })
			Media.DisplayMessage(UserInterface.Translate("reinforcements-near-colony"), Warning)
		end)
	end
	
	if not Human.IsObjectiveCompleted(DestroyColonyObjective) and #colonybasements == 0 then -- if the colony has been eliminated
		Human.MarkCompletedObjective(DestroyColonyObjective)
	end

	if not Human.IsObjectiveCompleted(DestroyYurukiMilitary) and #militarybasements == 0 then -- if the military basement has been eliminated
		Human.MarkCompletedObjective(DestroyYurukiMilitary)
	end

	if DateTime.GameTime % DateTime.Seconds(1) == 0 then
		CooldownBeforeDetection = CooldownBeforeDetection - 1
	end

	UpdateGameStateText()

end

function UpdateGameStateText()
	local time = math.ceil(CooldownBeforeDetection / 60)
	local minute = "mins"
	if time == 1 then
		minute = "min"
	end
	if time > 0 then
		UserInterface.SetMissionText("\n\n\n" .. time .. minute .. " before detection")
	end
	if time < 0 then
		UserInterface.SetMissionText("")
	end
end

WorldLoaded = function()
	Human = Player.GetPlayer("Synapol Corporation")
	Enemy = Player.GetPlayer("Yuruki Industries")

	InitObjectives(Human)

	DestroyColonyObjective = AddPrimaryObjective(Human, "destroy-colony")
	DestroyYurukiMilitary = AddPrimaryObjective(Human, "destroy-yuruki-military")

	Camera.Position = Outpost.CenterPosition
	UpdateGameStateText()
end
