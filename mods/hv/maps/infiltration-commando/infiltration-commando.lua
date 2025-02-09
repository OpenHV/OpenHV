--[[
   Copyright 2024 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

-- Define paths, and squad units
AirlifterPath = { AirlifterSpawn.Location, AirlifterExit.Location }
AirlifterPathAlternative = { AirlifterSpawn2.Location, AirlifterExit2.Location }
NavalReinforcementPath = { NavalEntry.Location, NavalDestination.Location }
AirPatrolPath = { AirPoint1.Location, AirPoint2.Location, AirPoint3.Location, AirPoint4.Location, AirPoint5.Location, AirPoint6.Location, AirPoint7.Location }
NavalPatrolPath = { NavalPoint1.Location, NavalPoint2.Location, NavalPoint3.Location, NavalPoint4.Location, NavalPoint5.Location, NavalPoint6.Location, NavalPoint7.Location }
NavalPatrol = { "lightboat", "lightboat", "torpedoboat", "lightningboat" }
GroundPatrol = { "rifleman", "rifleman", "shocker", "mbt", "artillery", "apc", "bike" }
AirPatrol = { "gunship", "jet" }
NavalSmallPatrol = { "lightboat.patrol", "lightboat.patrol", "lightboat.patrol" }

-- Booleans to keep track of stuff
SentReminder = false

-- Coefficients for more balance
AirstrikeDelayCoefficient = 1

-- Define message headers
Satellite = UserInterface.GetFluentMessage("satellite-reconaissance")
Warning = UserInterface.GetFluentMessage("warning")
Reminder = UserInterface.GetFluentMessage("reminder")

SendPatroopers = function()
	-- Create 2 airlifter units, at a difference of 1sec, and send them to paradrop their cargo onto the player's base.
	-- Their respective cargo are: 1(miner); 2(main battle tankx2, radar tank and artillery)
	Airlifter1 = Reinforcements.ReinforceWithTransport(Human, "airlifter2", { "miner" }, AirlifterPath)[1]
	Airlifter1.Paradrop(SynapolReinforcementPoint.Location)
	Trigger.AfterDelay(DateTime.Seconds(2), function()
	end)

	Trigger.AfterDelay(DateTime.Seconds(1), function()
		Airlifter2 = Reinforcements.ReinforceWithTransport(Human, "airlifter2", { "mbt2", "mbt2", "radartank", "artillery" } , AirlifterPathAlternative)[1]
		Airlifter2.Paradrop(SynapolReinforcementPoint.Location)
	end)

	-- If the difficulty mode is set to easy, send a naval squad reinforcement
	if Difficulty == "easy" then
		NavalReinforcements = Reinforcements.Reinforce(Human, { "patrolboat", "patrolboat", "submarine", "submarine" }, NavalReinforcementPath)
	end
end

SendAirstrikeWaves = function()
	-- Send airstrike waves at different locations, and different delays.
	-- They start at 02mins20secs and are all spaced by 01mins35secs (without counting the coefficient applied)
	-- There are 18 waves, with 2 targeting the player's base
	Trigger.AfterDelay((DateTime.Minutes(2) + DateTime.Seconds(20)) * AirstrikeDelayCoefficient, function()
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(AirstrikePoint1.CenterPosition)
			Media.DisplayMessage(UserInterface.GetFluentMessage("airstrike-waves-started"), Warning)  -- Warn the player when the airstrike waves start
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(3) + DateTime.Seconds(55)) * AirstrikeDelayCoefficient, function()
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(AirstrikePoint1.CenterPosition)
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(5) + DateTime.Seconds(30)) * AirstrikeDelayCoefficient, function()
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(AirstrikePoint1.CenterPosition)
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(7) + DateTime.Seconds(5)) * AirstrikeDelayCoefficient, function()
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(AirstrikePoint2.CenterPosition)
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(8) + DateTime.Seconds(40)) * AirstrikeDelayCoefficient, function()
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(AirstrikePoint2.CenterPosition)
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(10) + DateTime.Seconds(15)) * AirstrikeDelayCoefficient, function()
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(AirstrikePoint2.CenterPosition)
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(11) + DateTime.Seconds(50)) * AirstrikeDelayCoefficient, function()
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(AirstrikePoint3.CenterPosition)
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(13) + DateTime.Seconds(25)) * AirstrikeDelayCoefficient, function()
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(AirstrikePoint3.CenterPosition)
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(16)) * AirstrikeDelayCoefficient, function()  -- Special wave onto the player's base
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(SynapolReinforcementPoint.CenterPosition)
			Media.DisplayMessage(UserInterface.GetFluentMessage("airstrike-waves-player"), Warning)  -- Warn the player
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(17) + DateTime.Seconds(35)) * AirstrikeDelayCoefficient, function()  -- Special wave onto the player's base
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(SynapolReinforcementPoint.CenterPosition)
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(19) + DateTime.Seconds(10)) * AirstrikeDelayCoefficient, function()
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(AirstrikePoint3.CenterPosition)
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(20) + DateTime.Seconds(45)) * AirstrikeDelayCoefficient, function()
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(AirstrikePoint1.CenterPosition)
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(22) + DateTime.Seconds(20)) * AirstrikeDelayCoefficient, function()
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(AirstrikePoint2.CenterPosition)
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(23) + DateTime.Seconds(55)) * AirstrikeDelayCoefficient, function()
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(AirstrikePoint3.CenterPosition)
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(25) + DateTime.Seconds(30)) * AirstrikeDelayCoefficient, function()
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(AirstrikePoint3.CenterPosition)
		end
	end)
	Trigger.AfterDelay((DateTime.Minutes(27) + DateTime.Seconds(5)) * AirstrikeDelayCoefficient, function()
		if not Human.IsObjectiveCompleted(InfiltrateRadarObjective) then
			Actor1024.TargetAirstrike(AirstrikePoint2.CenterPosition)
		end
	end)
end

RevealPowerPlants = function()
	-- Reveal different enemy outpost locations, throughout the mission.
	-- Starts a 01min00secs, and repeat itself every 00min40secs
	Trigger.AfterDelay(DateTime.Minutes(1), function()
		Beacon.New(Human, PowerPlants1.CenterPosition)
		RadarSweep = Actor.Create("radarsweep", true, { Location = PowerPlants1.Location, Owner = Human })
		Trigger.AfterDelay(750, function()
			RadarSweep.Destroy()
		end)
		Media.DisplayMessage(UserInterface.GetFluentMessage("power-plants-positions-revealed"), Satellite)
	end)
	Trigger.AfterDelay(DateTime.Minutes(1) + DateTime.Seconds(40), function()
		Beacon.New(Human, Actor1024.CenterPosition)
		RadarSweep = Actor.Create("radarsweep", true, { Location = Actor1024.Location, Owner = Human })
		Trigger.AfterDelay(750, function()
			RadarSweep.Destroy()
		end)
		Media.DisplayMessage(UserInterface.GetFluentMessage("com-quarters-revealed"), Satellite)
		Trigger.AfterDelay(300, function()  -- wait for 5 secs to send a reminder
			Media.DisplayMessage(UserInterface.GetFluentMessage("capture-radar-reminder"), Reminder)
		end)
	end)

	Trigger.AfterDelay(DateTime.Minutes(2) + DateTime.Seconds(20), function()
		Beacon.New(Human, PowerPlants3.CenterPosition)
		RadarSweep = Actor.Create("radarsweep", true, { Location = PowerPlants3.Location, Owner = Human })
		Trigger.AfterDelay(750, function()
			RadarSweep.Destroy()
		end)
		Media.DisplayMessage(UserInterface.GetFluentMessage("power-plants-positions-revealed"), Satellite)
	end)
	Trigger.AfterDelay(DateTime.Minutes(3), function()
		Beacon.New(Human, PowerPlants4.CenterPosition)
		RadarSweep = Actor.Create("radarsweep", true, { Location = PowerPlants4.Location, Owner = Human })
		Trigger.AfterDelay(750, function()
			RadarSweep.Destroy()
		end)
		Media.DisplayMessage(UserInterface.GetFluentMessage("power-plants-positions-revealed"), Satellite)
	end)
	Trigger.AfterDelay(DateTime.Minutes(3) + DateTime.Seconds(40), function()
		Beacon.New(Human, PowerPlants5.CenterPosition)
		RadarSweep = Actor.Create("radarsweep", true, { Location = PowerPlants5.Location, Owner = Human })
		Trigger.AfterDelay(750, function()
			RadarSweep.Destroy()
		end)
		Media.DisplayMessage(UserInterface.GetFluentMessage("power-plants-positions-revealed"), Satellite)
	end)
	Trigger.AfterDelay(DateTime.Minutes(5), function()
		Beacon.New(Human, PowerPlants6.CenterPosition)
		RadarSweep = Actor.Create("radarsweep", true, { Location = PowerPlants6.Location, Owner = Human })
		Trigger.AfterDelay(750, function()
			RadarSweep.Destroy()
		end)
		Media.DisplayMessage(UserInterface.GetFluentMessage("power-plants-positions-revealed"), Satellite)
	end)
	Trigger.AfterDelay(DateTime.Minutes(5) + DateTime.Seconds(40), function()
		Beacon.New(Human, PowerPlants6.CenterPosition)
		RadarSweep = Actor.Create("radarsweep", true, { Location = PowerPlants7.Location, Owner = Human })
		Trigger.AfterDelay(750, function()
			RadarSweep.Destroy()
		end)
		Media.DisplayMessage(UserInterface.GetFluentMessage("power-plants-positions-revealed"), Satellite)
	end)
	Trigger.AfterDelay(DateTime.Minutes(6) + DateTime.Seconds(20), function()
		Beacon.New(Human, PowerPlants6.CenterPosition)
		RadarSweep = Actor.Create("radarsweep", true, { Location = PowerPlants8.Location, Owner = Human })
		Trigger.AfterDelay(750, function()
			RadarSweep.Destroy()
		end)
		Media.DisplayMessage(UserInterface.GetFluentMessage("power-plants-positions-revealed"), Satellite)
	end)
	Trigger.AfterDelay(DateTime.Minutes(7), function()
		Beacon.New(Human, PowerPlants6.CenterPosition)
		RadarSweep = Actor.Create("radarsweep", true, { Location = PowerPlants9.Location, Owner = Human })
		Trigger.AfterDelay(750, function()
			RadarSweep.Destroy()
		end)
		Media.DisplayMessage(UserInterface.GetFluentMessage("power-plants-positions-revealed"), Satellite)
	end)

end

Tick = function()
	-- Get needed Human actors for afterward actions
	local radar = Human.GetActorsByType("radar2")
	local radar3 = Human.GetActorsByType("radar")
	local howitzer = Human.GetActorsByType("howitzer")
	local harbor = Human.GetActorsByType("harbor2")

	-- Get needed Enemy actors for afterward actions
	local radar2 = Enemy.GetActorsByType("radar")
	local enemyharbors = Enemy.GetActorsByType("harbor")
	local enemytechcenter = Enemy.GetActorsByType("techcenter")
	local enemyfield = Enemy.GetActorsByType("field")
	local enemyuplink = Enemy.GetActorsByType("uplink")
	local enemyheadquarters = #enemytechcenter + #enemyfield + #enemyuplink  -- Get the number of alive Yuruki headquarters buildings

	local powerplants = Enemy.GetActorsByType("generator")  -- Get the percentage of yuruki power plants still alive (the starting amount is 87)
	local powerplantspercentage = #powerplants * 100 / 87

	-- Update the AirstrikeDelayCoefficient, depending on the percentage of yuruki power plants alive
	-- PERCENTAGE --> COEFFICIENT
	-- < 95%  --> 1.1
	-- < 90%  --> 1.15
	-- < 80%  --> 1.25
	-- < 70%  --> 1.3
	-- < 60%  --> 1.4
	-- < 50%  --> 1.55
	-- < 40%  --> 2
	if powerplantspercentage < 95 then
		AirstrikeDelayCoefficient = 1.1
	end
	if powerplantspercentage < 90 then
		AirstrikeDelayCoefficient = 1.15
	end
	if powerplantspercentage < 80 then
		AirstrikeDelayCoefficient = 1.25
	end
	if powerplantspercentage < 70 then
		AirstrikeDelayCoefficient = 1.3
	end
	if powerplantspercentage < 60 then
		AirstrikeDelayCoefficient = 1.4
	end
	if powerplantspercentage < 50 then
		AirstrikeDelayCoefficient = 1.55
	end
	if powerplantspercentage < 40 then
		AirstrikeDelayCoefficient = 2
	end

	if #radar2 == 0 and #radar3 == 1 and not Human.IsObjectiveCompleted(SaveColonyObjective) then  -- If the enemy has no more radar, and the player has one, complete "infiltrate" and "save colony" objectives
		Human.MarkCompletedObjective(InfiltrateRadarObjective)
		Human.MarkCompletedObjective(SaveColonyObjective)
	end

	-- Check for player secondary objectives
	if #radar > 0 and not Human.IsObjectiveCompleted(BuildRadarObjective) then
		Human.MarkCompletedObjective(BuildRadarObjective)
	end
	if #howitzer > 0 and not Human.IsObjectiveCompleted(BuildHowitzerObjective) then
		Human.MarkCompletedObjective(BuildHowitzerObjective)
	end
	if #harbor > 0 and not Human.IsObjectiveCompleted(BuildHarborObjective) then
		Human.MarkCompletedObjective(BuildHarborObjective)
	end

	-- If the player has built an harbor, make the enemy produce naval squads every 05mins30secs
	-- OR if the player has saved the colony
	if Human.IsObjectiveCompleted(BuildHarborObjective) and DateTime.GameTime % DateTime.Minutes(5.5) or Human.IsObjectiveCompleted(SaveColonyObjective) then
		Enemy.Build(NavalPatrol)
	end

	-- Make the enemy produce ground squads every 01mins20secs
	if DateTime.GameTime % (DateTime.Minutes(1) + DateTime.Seconds(20)) then
		Enemy.Build(GroundPatrol)
	end

	-- Make the enemy produce air squads every 02mins, until 4 are produced and then send them patrolling
	-- Also makes them produce again if they're dead
	if DateTime.GameTime % DateTime.Minutes(2) and #Enemy.GetActorsByType("gunship") < 12 and #Enemy.GetActorsByType("jet") < 8 then
		Enemy.Build(AirPatrol, function(actors)
			Utils.Do(actors, function(a)
				a.Patrol(AirPatrolPath, true, 0)
			end)
		end)
	end

	-- Make the enemy produce naval squads every 02mins, until 3 are produced and then send them patrolling
	-- Also makes them produce again if they're dead
	if DateTime.GameTime % DateTime.Minutes(2) and #Enemy.GetActorsByType("lightboat.patrol") < 9 and #Enemy.GetActorsByType("lightningboat.patrol") < 3 then
		Enemy.Build(NavalSmallPatrol, function(actors)
			Utils.Do(actors, function(a)
				a.Patrol(NavalPatrolPath, true, 0)
			end)
		end)
	end

	-- If the enemy has no more harbors, complete the "neutralize yuruki harbor" objective
	if #enemyharbors == 0 and not Human.IsObjectiveCompleted(DestroyHarborFacilities) then
		Human.MarkCompletedObjective(DestroyHarborFacilities)
	end

	-- If the enemy has no more headquarters basements, succeed the "break in" objective, as well as the "save colony" and the "infiltrate radar" objectives
	if enemyheadquarters == 0 and not Human.IsObjectiveCompleted(SaveColonyObjective) then
		Human.MarkCompletedObjective(AlternativeBreakIn)
		Human.MarkCompletedObjective(SaveColonyObjective)
		Human.MarkCompletedObjective(InfiltrateRadarObjective)
	end

	UpdateGameStateText()  -- Update colony's buildings destruction percentage
end

function UpdateGameStateText()
	-- Determine how many colony buildings there are
	local colonybuildings1 = Human.GetActorsByType("prop5")
	local colonybuildings2 = Human.GetActorsByType("prop6")
	local colonybuildings3 = Human.GetActorsByType("prop7")
	local colonybuildings4 = Human.GetActorsByType("prop8")

	local buildings = #colonybuildings1 + #colonybuildings2 + #colonybuildings3 + #colonybuildings4
	local percentage = math.ceil(buildings / 12 * 100)  -- Find out the percentage of colony buildings still alive

	if Difficulty == "easy" then
		DifficultyPercentage = 55
	end
	if Difficulty == "normal" then
		DifficultyPercentage = 70
	end
	if Difficulty == "hard" then
		DifficultyPercentage = 80
	end

	if percentage < DifficultyPercentage then  -- Fails if it is lower than the difficulty's percentage
		Human.MarkFailedObjective(SaveColonyObjective)
	end

	-- send a reminder if the percentage is lower to the difficulty's percentage times 110%
	if percentage < DifficultyPercentage * 1.1 and not SentReminder then
		SentReminder = true
		Media.DisplayMessage(UserInterface.GetFluentMessage("reminder-low-percentage"), Warning)
	end

	UserInterface.SetMissionText("\n\n\n" .. percentage .. UserInterface.GetFluentMessage("percent-still-alive"))
end

WorldLoaded = function()
	Human = Player.GetPlayer("Synapol Corporation")
	Enemy = Player.GetPlayer("Yuruki Industries")

	InitObjectives(Human)

	if Difficulty == "easy" then
		SaveColonyObjective = AddPrimaryObjective(Human, "save-colony-easy")
	end
	if Difficulty == "normal" then
		SaveColonyObjective = AddPrimaryObjective(Human, "save-colony-normal")
	end
	if Difficulty == "hard" then
		SaveColonyObjective = AddPrimaryObjective(Human, "save-colony-hard")
	end
	InfiltrateRadarObjective = AddPrimaryObjective(Human, "infiltrate-radar")
	DestroyHarborFacilities = AddPrimaryObjective(Human, "annihilate-harbor-facilities")
	BuildRadarObjective = AddSecondaryObjective(Human, "build-radar")
	BuildHowitzerObjective = AddSecondaryObjective(Human, "build-howitzer")
	BuildHarborObjective = AddSecondaryObjective(Human, "build-harbor")
	AlternativeBreakIn = AddSecondaryObjective(Human, "alternative-break-in")

	-- Update patrol troops units to match the difficulty selected
	if Difficulty == "easy" then
		NavalPatrol = { "lightboat", "torpedoboat" }
		GroundPatrol = { "rifleman", "rifleman", "shocker", "mbt", "bike" }
		AirPatrol = { "gunship" }
		NavalSmallPatrol = { "lightboat.patrol" }
	end
	if Difficulty == "hard" then
		NavalPatrol = { "lightboat", "lightboat", "lightboat", "torpedoboat", "torpedoboat", "lightningboat" }
		GroundPatrol = { "rifleman", "rifleman", "rifleman", "shocker", "shocker", "sniper", "sniper", "mbt", "mbt", "artillery", "stealthtank", "apc", "bike", "bike" }
		AirPatrol = { "gunship", "gunship", "gunship", "jet", "jet" }
		NavalSmallPatrol = { "lightboat.patrol", "lightboat.patrol", "lightboat.patrol", "lightningboat.patrol" }
	end

	SendPatroopers()  -- Send Synapol paratroopers to base
	SendAirstrikeWaves()  -- Initiate the Yuruki airstrike waves
	RevealPowerPlants()  -- Initiate the satellite reveals waves

	Camera.Position = Outpost.CenterPosition
	UpdateGameStateText()
end
