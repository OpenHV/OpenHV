--[[
   Copyright 2024 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

-- Squad units and paths
Paratroopers = { "mgpod", "mgpod", "bike", "bike", "repairtank", "radartank" }
ProductionPath = ProductionDestination.Location
ParadropPath = { ParadropSpawn.Location, ParadropDestination.Location }
NavalReinforcementPath = { NavalEntry.Location, NavalDestination.Location}

-- Booleans to keep track of stuff
ProductionStarted = false
NavalReinforcementsSent = false

-- Define message headers
Satellite = UserInterface.Translate("satellite-reconaissance")
Reminder = UserInterface.Translate("reminder")
Warning = UserInterface.Translate("warning")

CountDown = 240

SendPatroopers = function()
	-- Create a camera sweep for a few sec before the airlifter paradrops
	RadarSweep2 = Actor.Create("radarsweep", true, { Location = ParadropPoint.Location, Owner = Human })
	Trigger.AfterDelay(120, function()
		RadarSweep2.Destroy()
	end)
	-- Create an airlifter unit, and send them to paradrop their cargo onto the paradrop point.
	-- Once they paradropped their cargo, remove them from the world.
	-- Their cargo vary depending on the difficulty mode chosen
	Airlifter = Reinforcements.ReinforceWithTransport(Human, "airlifter2", Paratroopers, ParadropPath)[1]
	Airlifter.Paradrop(ParadropPoint.Location)
	Trigger.AfterDelay(60, function()
		Airlifter.Destroy()
	end)
end

SendNavalReinforcement = function()
	-- Send ground reinforcements through naval transports
	Reinforcements.ReinforceWithTransport(Human, "navaltransport", Paratroopers, NavalReinforcementPath)
	-- Send a reminder
	Media.DisplayMessage(UserInterface.Translate("annihilate-outpost-reminder"), Reminder)
end

StartProduction = function()
	Media.DisplayMessage(UserInterface.Translate("submarine-production-started"), Warning)
	Enemy.Build({ "boomer", "boomer", "boomer", "boomer" }, function(actors)
		Utils.Do(actors, function(a)
			a.AttackMove(ProductionDestination.Location)
		end)
	end)
end

Tick = function()
	-- Get the enemy's base buildings to find out if they've be destroyed
	local enemyharbor = Enemy.GetActorsByType("harbor2")
	local enemyradar = Enemy.GetActorsByType("radar2")
	local enemyoutpost = Enemy.GetActorsByType("outpost2")
	local enemyrefinery = Enemy.GetActorsByType("storage")
	local enemyoresmelts = Enemy.GetActorsByType("oresmelt")

	local enemybuildings = #enemyharbor + #enemyradar + #enemyoutpost + #enemyrefinery + #enemyoresmelts

	-- Complete the objective "annihilate outpost", if it's been achieved
	if enemybuildings == 0 then
		Human.MarkCompletedObjective(AnnihilateOutpost)
		Human.MarkCompletedObjective(NoEscape)
	end

	-- Update the countdown
	if DateTime.GameTime % DateTime.Seconds(1) == 0 then
		CountDown = CountDown - 1
	end

	-- If the countdown has come to 0, start the Synapol submarine production
	if CountDown < 0 and not ProductionStarted then
		ProductionStarted = true
		StartProduction()
	end

	-- Check if the two defensed power grid generators have been destroyed
	-- If they have, trigger a power outage to let the reinforcements pass
	if Actor63.IsDead and Actor62.IsDead and not NavalReinforcementsSent then
		NavalReinforcementsSent = true
		Human.MarkCompletedObjective(PowerDownGrid)
		Enemy.TriggerPowerOutage(300)
		SendNavalReinforcement()
	end

	UpdateGameStateText()  -- Update Synapol harbor production countdown

	-- Failure conditions
	Trigger.AfterDelay(120, function()
		if Human.HasNoRequiredUnits() then
			Human.MarkFailedObjective(AnnihilateOutpost)
		end
	end)
end

function UpdateGameStateText()
	local enemyharbor = Enemy.GetActorsByType("harbor2")
	local time = math.ceil(CountDown / 60)
	local minute = "mins "
	if time == 1 then
		minute = "min "
	end
	if time > 0 then
		UserInterface.SetMissionText("\n\n\n" .. time .. minute .. UserInterface.Translate("submarine-detection"))
	end
	if ProductionStarted or #enemyharbor == 0 then
		UserInterface.SetMissionText("")
	end
end

WorldLoaded = function()
	Human = Player.GetPlayer("Yuruki Industries")
	Enemy = Player.GetPlayer("Synapol Corporation")

	InitObjectives(Human)

	PowerDownGrid = AddPrimaryObjective(Human, "power-down-grid")
	AnnihilateOutpost = AddPrimaryObjective(Human, "annihilate-outpost")
	NoEscape = AddPrimaryObjective(Human, "no-submarine-escape")

	-- Update squad units, depending on the difficulty mode chosen
	if Difficulty == "easy" then
		Paratroopers = { "mgpod", "mgpod", "mgpod", "bike", "bike", "bike", "repairtank", "radartank" }
	end
	-- Update squad units, depending on the difficulty mode chosen
	if Difficulty == "hard" then
		Paratroopers = { "mgpod", "mgpod", "mgpod", "bike", "bike" }
	end

	-- Update countdown, depending on the difficulty mode chosen
	if Difficulty == "easy" then
		CountDown = 270
	end
	if Difficulty == "hard" then
		CountDown = 200
	end

	SendPatroopers()  -- Paradrop Yuruki squad

	Camera.Position = ParadropPoint.CenterPosition
	UpdateGameStateText()

	-- Localize the power grid to destroy
	Trigger.AfterDelay(120, function()
		Beacon.New(Human, PowerGrid.CenterPosition, 120)
		RadarSweep = Actor.Create("radarsweep", true, { Location = PowerGrid.Location, Owner = Human })
		Trigger.AfterDelay(120, function()
			RadarSweep.Destroy()
		end)
		Media.DisplayMessage(UserInterface.Translate("power-grid-revealed"), Satellite)
	end)

	-- Detect if submarines escaped
	Trigger.OnEnteredFootprint({ ProductionDestination.Location }, function(actor)
		actor.Destroy()
		Human.MarkFailedObjective(NoEscape)
	end)
end
