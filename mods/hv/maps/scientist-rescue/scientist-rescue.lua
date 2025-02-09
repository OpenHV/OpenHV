--[[
   Copyright 2024 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

LabGuardsTeam = { LabGuard1, LabGuard2, LabGuard3, LabGuard4, LabGuard5, LabGuard6 }
ExtractionPath = { DropshipSpawnPoint.Location, RescuePoint.Location }
InsertionPath = { EntryPoint.Location, EntryPoint.Location }
PassengerReinforcements = { "rifleman", "rifleman", "shocker", "shocker", "shocker", "rifleman", "rifleman", "technician" }
PatrolTroop = { "rifleman", "rocketeer" }
PatrolPoints = { PatrolPoint1.Location, PatrolPoint2.Location, PatrolPoint3.Location }
OpeningAttack = { Ship1, Ship2, Ship3, Ship4 }


Warning = UserInterface.GetFluentMessage("warning")
Radar = UserInterface.GetFluentMessage("radar")

SummonPatrolTroops = function()
	TroopFirst = Reinforcements.Reinforce(Enemy, PatrolTroop, PatrolPoints)
	TroopSecond = Reinforcements.Reinforce(Enemy, PatrolTroop, PatrolPoints)
	TroopThird = Reinforcements.Reinforce(Enemy, PatrolTroop, PatrolPoints)

	Utils.Do(TroopFirst, function(a)
		if not a.IsDead then
			a.Patrol(PatrolPoints, true, 180)
		end
	end)
	Utils.Do(TroopSecond, function(a)
		if not a.IsDead then
			a.Patrol(PatrolPoints, true, 180)
		end
	end)
	Utils.Do(TroopThird, function(a)
		if not a.IsDead then
			a.Patrol(PatrolPoints, true, 180)
		end
	end)
end

SendInsertionNavalTransport = function()
	local passengers = Reinforcements.ReinforceWithTransport(Human, "ferry", PassengerReinforcements, InsertionPath)[2]
	Trigger.OnAllKilled(passengers, function()
		if not Human.IsObjectiveCompleted(KillGuardsObjective) then
			RescueFailed()
		end
	end)
end

RunInitialActivities = function()
	SummonPatrolTroops()
	SendInsertionNavalTransport()

	Utils.Do(OpeningAttack, IdleHunt)

end

LabGuardsKilled = function()
	CreateScientist()

	Human.MarkCompletedObjective(KillGuardsObjective)

	Trigger.AfterDelay(DateTime.Seconds(2), SendExtractionDropship)

end

SendExtractionDropship = function()
	Dropship = Reinforcements.ReinforceWithTransport(Human, "dropship", nil, ExtractionPath)[1]
	Beacon.New(Human, RescuePoint.CenterPosition)
	Media.DisplayMessage(UserInterface.GetFluentMessage("dropzone-revealed"), Radar)
	if not Scientist.IsDead then
		Trigger.OnRemovedFromWorld(Scientist, EvacuateDropship)
	end
	Trigger.OnKilled(Dropship, RescueFailed)
end

EvacuateDropship = function()
	if Dropship.IsDead then
		return
	end
	if Dropship.HasPassengers then
		Dropship.Move(DropshipSpawnPoint.Location)
		Dropship.Destroy()
		DropshipGone()
	end
end

RescueFailed = function()
	Human.MarkFailedObjective(RescueScientistObjective)
	Human.MarkFailedObjective(FindScientistObjective)
	Human.MarkFailedObjective(KillGuardsObjective)
end

CreateScientist = function()
	Human.MarkCompletedObjective(FindScientistObjective)
	Media.DisplayMessage(UserInterface.GetFluentMessage("scientist-escaped"), Warning)
	Scientist = Actor.Create("minipod4.scientist", true, { Location = ScientistSpawnPoint.Location, Owner = Human })
	Trigger.OnKilled(Scientist, RescueFailed)
end

DropshipGone = function()
	Trigger.AfterDelay(DateTime.Seconds(3), function()
		Human.MarkCompletedObjective(RescueScientistObjective)
	end)
end

Tick = function()
	Enemy.Resources = Enemy.Resources - (.01 * Enemy.ResourceCapacity / 25)
end

WorldLoaded = function()
	Human = Player.GetPlayer("Yuruki Industries")
	Enemy = Player.GetPlayer("Synapol Corporation")

	InitObjectives(Human)

	Trigger.OnAllKilled(LabGuardsTeam, LabGuardsKilled)

	KillGuardsObjective = AddPrimaryObjective(Human, "kill-guards")
	FindScientistObjective = AddPrimaryObjective(Human, "find-scientist")
	RescueScientistObjective = AddPrimaryObjective(Human, "rescue-scientist")

	RunInitialActivities()

	Camera.Position = EntryPoint.CenterPosition
end
