CurrentWave = 1
Breaches = 10

Rifle = { "rifleman", "rifleman", "rifleman" }
Rocket = { "rocketeer", "rocketeer" }
Tank = { "mbt" }

Waves =
{
	{ delay = DateTime.Seconds(20), units = { Rifle, Rifle, Rifle, Rifle } },
	{ delay = DateTime.Seconds(20), units = { Rocket, Rocket, Rocket, Rifle, Rifle, Rifle, Rifle, Rifle } },
	{ delay = DateTime.Seconds(22), units = { Rocket, Rocket, Rocket, Rocket, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle } },
	{ delay = DateTime.Seconds(24), units = { Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle } },
	{ delay = DateTime.Seconds(26), units = { Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket } },
	{ delay = DateTime.Seconds(26), units = { Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Rifle, Rifle, Rifle, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rifle, Rifle, Tank, Rifle, Rifle, Rifle, Rifle, Rifle , Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Tank } },
	{ delay = DateTime.Seconds(28), units = { Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket } },
	{ delay = DateTime.Seconds(32), units = { Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Tank, Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle , Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket } },
	{ delay = DateTime.Seconds(32), units = { Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank } },
	{ delay = DateTime.Seconds(32), units = { Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rocket, Rocket, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Tank, Tank, Rocket, Rocket, Rocket, Tank, Tank,Rocket, Rocket , Tank, Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Rifle, Tank, Tank,Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Rifle, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Tank, Tank, Tank, Tank } },
	{ delay = DateTime.Seconds(32), units = { Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank , Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank , Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank } },
}

LastWave = false

ExitTriggerArea = { ExitWaypoint1.Location, ExitWaypoint2.Location }

EntryWaypoints =
{
	{ EntryWaypoint1.Location, ExitWaypoint1.Location },
	{ EntryWaypoint2.Location, ExitWaypoint2.Location }
}

WorldLoaded = function()
	Camera.Position = CenterCamera.CenterPosition

	HumanPlayer = Player.GetPlayer("Multi0")
	EnemyPlayer = Player.GetPlayer("Creeps")

	InitObjectives(HumanPlayer)
	TowerDefenseObjective = AddPrimaryObjective(HumanPlayer, "not-too-many-enemies-through-trench")
	UpdateGameStateText()

	Trigger.OnEnteredFootprint(ExitTriggerArea, function(actor)
		actor.Destroy()
		Breaches = Breaches - 1
		if Breaches < 1 then
			HumanPlayer.MarkFailedObjective(TowerDefenseObjective)
		end
		if Breaches < 0 then
			Breaches = 0
		end
		UpdateGameStateText()
	end)

	SendNextWave()
end

CachedWave = -1
CachedBreaches = -1
function UpdateGameStateText()
	if CachedWave == CurrentWave and CachedBreaches == Breaches then
		return
	end
	CachedWave = CurrentWave
	CachedBreaches = Breaches

	local waveInfo = UserInterface.GetFluentMessage("current-wave", { ["wave"] = CurrentWave, ["waves"] = #Waves })
	local tolerableBreaches = UserInterface.GetFluentMessage("tolerable-breaches", { [ "breaches"] = Breaches })
	UserInterface.SetMissionText("\n\n\n" .. waveInfo .. "\n\n" .. tolerableBreaches)
end

SendNextWave = function()
	local wave = Waves[CurrentWave]
	Trigger.AfterDelay(wave.delay, function()
		Utils.Do(wave.units, function(units)
			Attackers = Reinforcements.Reinforce(EnemyPlayer, units, Utils.Random(EntryWaypoints))
			Unstuck(Attackers)
		end)
		UpdateGameStateText()
		if CurrentWave < #Waves then
			CurrentWave = CurrentWave + 1
			SendNextWave()
		else
			LastWave = true
		end
	end)
end

Unstuck = function(actors)
	Utils.Do(actors, function(actor)
		if not actor.IsDead then
			Trigger.OnIdle(actor, function()
				actor.MoveIntoWorld(EntryWaypoint1.Location)
				actor.Move(ExitWaypoint1.Location)
			end)
		end
	end)
end

Won = false
Tick = function()
	if LastWave and not HumanPlayer.IsObjectiveCompleted(TowerDefenseObjective) then
		Trigger.AfterDelay(DateTime.Seconds(8), function()
			if not Won and #EnemyPlayer.GetGroundAttackers() == 0 then
				Media.DisplayMessage(UserInterface.GetFluentMessage("no-more-enemies"))
				HumanPlayer.MarkCompletedObjective(TowerDefenseObjective)
				Won = true
			end
		end)
	end
end
