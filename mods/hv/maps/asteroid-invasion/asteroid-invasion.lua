--[[
   Copyright 2024 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

Warning = UserInterface.GetFluentMessage("warning")
ColonyReinforcementsSent = false
MilitaryProductionStarted = false
CooldownBeforeDetection = 180
ColonyReinforcementUnitsPods = { "rifleman", "rifleman", "rifleman", "rifleman", "shocker", "shocker", "shocker", "mortar", "mortar", "sniper", "sniper", "technician", "technician" }
ColonyReinforcementUnitsTanks = { "aatank2", "aatank2", "mbt", "mbt", "mbt", "artillery", "artillery", "repairtank", "repairtank", "bike", "bike", "bike", "bike", "bike", "bike", "bike", "bike" }
ColonyReinforcementUnitsPodsBig = { "rifleman", "rifleman", "rifleman", "rifleman", "shocker", "shocker", "shocker", "mortar", "mortar", "sniper", "sniper", "technician", "technician", "rifleman", "rifleman", "rifleman", "rifleman", "shocker", "shocker", "shocker", "mortar", "mortar", "sniper", "sniper", "technician", "technician", "rifleman", "rifleman", "rifleman", "rifleman", "shocker", "shocker", "shocker", "mortar", "mortar", "sniper", "sniper", "technician", "technician" }
ColonyReinforcementUnitsTanksBig = { "aatank2", "aatank2", "mbt", "mbt", "mbt", "artillery", "artillery", "repairtank", "repairtank", "bike", "bike", "bike", "bike", "bike", "bike", "bike", "bike", "aatank2", "aatank2", "mbt", "mbt", "mbt", "artillery", "artillery", "repairtank", "repairtank", "bike", "bike", "bike", "bike", "bike", "bike", "bike", "bike", "aatank2", "aatank2", "mbt", "mbt", "mbt", "artillery", "artillery", "repairtank", "repairtank", "bike", "bike", "bike", "bike", "bike", "bike", "bike", "bike" }
ColonyDestinationPathFactory = { FactoryExit.Location, ColonyDestination.Location }
ColonyDestinationPathModule = { ModuleExit.Location, ColonyDestination.Location }
HumanDestinationPathFactory = { FactoryExit.Location, HumanBaseDestination.Location }
HumanDestinationPathModule = { ModuleExit.Location, HumanBaseDestination.Location }

Tick = function()
	local colonybasements = Enemy.GetActorsByTypes({ "prop5", "prop8", "prop7", "flagpost", "watchtower", "comlink" })
	local militarybasements = Enemy.GetActorsByTypes({ "generator", "aaturret", "bunker", "turret", "module", "factory", "radar", "uplink", "field", "techcenter", "trader", "starport", "storage" })
	local enemybarracks = Enemy.GetActorsByTypes({ "factory", "module" })

	if Human.IsObjectiveCompleted(DestroyColonyObjective) and not Human.IsObjectiveCompleted(DestroyYurukiMilitary) and not MilitaryProductionStarted and #enemybarracks > 0 then -- if the player has anihilated the colony basements, enable yuruki production to create three waves. Also enables the AI to use the railgun
		MilitaryProductionStarted = true
		Trigger.AfterDelay(DateTime.Seconds(10), function()
			if #enemybarracks > 0 then
				Media.DisplayMessage(UserInterface.GetFluentMessage("enemy-production-started"), Warning)
				MilitaryReinforcementsPods1 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPodsBig, HumanDestinationPathModule)
				MilitaryReinforcementsTanks1 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsTanksBig, HumanDestinationPathFactory)
				Trigger.AfterDelay(DateTime.Seconds(8), function()
					Media.DisplayMessage(UserInterface.GetFluentMessage("synapol-reinforcements-incoming"), Warning)
				end)
			end
		end)
		Trigger.AfterDelay(DateTime.Seconds(38), function()
			if #enemybarracks > 0 then
				MilitaryReinforcementsPods2 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPodsBig, HumanDestinationPathModule)
				MilitaryReinforcementsPods3 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPodsBig, HumanDestinationPathModule)
				MilitaryReinforcementsTanks2 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsTanksBig, HumanDestinationPathFactory)
				Trigger.AfterDelay(DateTime.Seconds(8), function()
					Media.DisplayMessage(UserInterface.GetFluentMessage("synapol-reinforcements-incoming"), Warning)
				end)
			end
		end)
		Trigger.AfterDelay(DateTime.Seconds(48), function()
			if #enemybarracks > 0 then
				MilitaryReinforcementsPods4 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPodsBig, HumanDestinationPathModule)
				MilitaryReinforcementsPods5 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPodsBig, HumanDestinationPathModule)
				MilitaryReinforcementsPods6 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPodsBig, HumanDestinationPathModule)
				MilitaryReinforcementsTanks3 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsTanksBig, HumanDestinationPathFactory)
				MilitaryReinforcementsTanks4 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsTanksBig, HumanDestinationPathFactory)
				Trigger.AfterDelay(DateTime.Seconds(8), function()
					Media.DisplayMessage(UserInterface.GetFluentMessage("synapol-reinforcements-incoming"), Warning)
				end)
			end
		end)
		Enemy.GrantCondition("railgun-enabled")
	end

	if DateTime.GameTime > DateTime.Seconds(180) and not ColonyReinforcementsSent and #enemybarracks > 0 then  -- send reinforcements units at the colony basement at 2"30' mins
		ColonyReinforcementsSent = true
		Media.DisplayMessage(UserInterface.GetFluentMessage("detected"), Warning)
		Trigger.AfterDelay(DateTime.Seconds(8), function()
			ColonyReinforcementsPods1 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPods, ColonyDestinationPathModule, 25, function(unit)
				Trigger.AfterDelay(DateTime.Minutes(1) + DateTime.Seconds(26), function()
					if not unit.IsDead then
						unit.AttackMove(HumanBaseDestination.Location, 5)
					end
				end)
			end)
			ColonyReinforcementsTanks1 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsTanks, ColonyDestinationPathFactory, 25, function(unit)
				Trigger.AfterDelay(DateTime.Minutes(1) + DateTime.Seconds(26), function()
					if not unit.IsDead then
						unit.AttackMove(HumanBaseDestination.Location, 5)
					end
				end)
			end)
		end)
		Trigger.AfterDelay(DateTime.Seconds(38), function()
			ColonyReinforcementsPods2 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsPodsBig, ColonyDestinationPathModule, 25, function(unit)
				Trigger.AfterDelay(DateTime.Seconds(48), function()
					if not unit.IsDead then
						unit.AttackMove(HumanBaseDestination.Location, 5)
					end
				end)
			end)
			ColonyReinforcementsTanks2 = Reinforcements.Reinforce(Enemy, ColonyReinforcementUnitsTanksBig, ColonyDestinationPathFactory, 25 ,function(unit)
				Trigger.AfterDelay(DateTime.Seconds(48), function()
					if not unit.IsDead then
						unit.AttackMove(HumanBaseDestination.Location, 5)
					end
				end)
			end)
			Media.DisplayMessage(UserInterface.GetFluentMessage("reinforcements-near-colony"), Warning)
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

	if Human.HasNoRequiredUnits() then
		Human.MarkFailedObjective(DestroyColonyObjective)
		Human.MarkFailedObjective(DestroyYurukiMilitary)
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
	if ColonyReinforcementsSent then
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
