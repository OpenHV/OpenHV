CurrentWave = 1
Breaches = 10

Machine = { "mgpod", "mgpod", "mgpod" }
Rocket = { "rocketpod", "rocketpod" }
Tank = { "mbt" }

Waves =
{
	{ delay = DateTime.Seconds(20), units = { Machine, Machine, Machine, Machine } },
	{ delay = DateTime.Seconds(20), units = { Rocket, Rocket, Rocket, Machine, Machine, Machine, Machine, Machine } },
	{ delay = DateTime.Seconds(22), units = { Rocket, Rocket, Rocket, Rocket, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine } },
	{ delay = DateTime.Seconds(24), units = { Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine } },
	{ delay = DateTime.Seconds(26), units = { Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Machine, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine , Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Rocket,Rocket,Rocket, Rocket, Rocket, Rocket } },
	{ delay = DateTime.Seconds(26), units = { Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Machine, Machine, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Machine,Machine,Tank, Machine,Machine, Machine, Machine, Machine , Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Rocket,Rocket,Rocket, Rocket, Rocket, Rocket, Tank } },
	{ delay = DateTime.Seconds(28), units = { Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Machine, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine , Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Rocket,Rocket,Rocket, Rocket, Rocket, Rocket } },
	{ delay = DateTime.Seconds(32), units = { Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Machine, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine , Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Rocket,Rocket,Rocket, Rocket, Rocket, Rocket , Tank, Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Machine, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine , Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Rocket,Rocket,Rocket, Rocket, Rocket, Rocket } },
	{ delay = DateTime.Seconds(32), units = { Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank } },
	{ delay = DateTime.Seconds(32), units = { Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Machine, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine , Rocket, Rocket, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Rocket,Tank, Tank, Rocket,Rocket, Rocket, Tank, Tank,Rocket, Rocket , Tank, Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Tank, Tank,Machine, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine , Tank, Tank,Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Rocket,Rocket,Tank, Tank,Rocket, Rocket, Rocket, Rocket, Tank, Tank, Tank, Tank } },
	{ delay = DateTime.Seconds(32), units = { Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank , Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank , Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank , Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank } },
}

LastWave = false

ExitTriggerArea = { ExitWaypoint1.Location, ExitWaypoint2.Location }

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

	local waveInfo = UserInterface.Translate("current-wave", { ["wave"] = CurrentWave, ["waves"] = #Waves })
	local tolerableBreaches = UserInterface.Translate("tolerable-breaches", { [ "breaches"] = Breaches })
	UserInterface.SetMissionText("\n\n\n" .. waveInfo .. "\n\n" .. tolerableBreaches)
end

SendNextWave = function()
	local wave = Waves[CurrentWave]
	Trigger.AfterDelay(wave.delay, function()
		Utils.Do(wave.units, function(units)
			Attackers = Reinforcements.Reinforce(EnemyPlayer, units, { EntryWaypoint1.Location, ExitWaypoint1.Location })
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

Won = false
Tick = function()
	if LastWave and not HumanPlayer.IsObjectiveCompleted(TowerDefenseObjective) then
		Trigger.AfterDelay(DateTime.Seconds(8), function()
			if not Won and #EnemyPlayer.GetGroundAttackers() == 0 then
				Media.DisplayMessage(UserInterface.Translate("no-more-enemies"))
				HumanPlayer.MarkCompletedObjective(TowerDefenseObjective)
				Won = true
			end
		end)
	end
end
