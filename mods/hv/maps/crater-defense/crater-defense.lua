Wave = 1
Breaches = 10

Pods = { "scout1", "scout2", "scout2", "scout1", "scout1" }
Tank = { "tank3" }

Waves =
{
	{ delay = 500, units = { Pods } },
	{ delay = 500, units = { Pods, Pods } },
	{ delay = 400, units = { Pods, Pods, Pods } },
	{ delay = 400, units = { Pods, Tank, Pods, Pods } },
	{ delay = 300, units = { Pods, Tank, Pods, Pods, Tank } },
	{ delay = 300, units = { Pods, Tank, Pods, Tank, Pods, Tank } },
}

LastWave = false

ExitTriggerArea = { ExitWaypoint1.Location, ExitWaypoint2.Location }

function CenterCamera()
	Camera.Position = WPos.New(Map.BottomRight.X / 2, Map.BottomRight.Y / 2, 0)
end

WorldLoaded = function()
	CenterCamera()

	HumanPlayer = Player.GetPlayer("Multi0")
	EnemyPlayer = Player.GetPlayer("Creeps")

	TowerDefenseObjective = HumanPlayer.AddPrimaryObjective("Do not allow too many enemies to exit the trench.")
	UpdateGameStateText()

	Trigger.OnEnteredFootprint(ExitTriggerArea, function(actor)
		actor.Destroy()
		Breaches = Breaches - 1
		if Breaches < 1 then
			HumanPlayer.MarkFailedObjective(TowerDefenseObjective)
		end
		UpdateGameStateText()
	end)

	SendNextWave()
end

function UpdateGameStateText()
	UserInterface.SetMissionText("\n\n\nWave: " .. Wave .. "/" .. #Waves .. "\n\nBreaches: " .. Breaches)
end

SendNextWave = function()
	local wave = Waves[Wave]
	Trigger.AfterDelay(wave.delay, function()
		Utils.Do(wave.units, function(units)
			Attackers = Reinforcements.Reinforce(EnemyPlayer, units, { EntryWaypoint1.Location, ExitWaypoint1.Location })
		end)
		UpdateGameStateText()
		if Wave < #Waves then
			SendNextWave()
			Wave = Wave + 1
		else
			LastWave = true
		end
	end)
end

Tick = function()
	if LastWave and not HumanPlayer.IsObjectiveCompleted(TowerDefenseObjective) then
		if #EnemyPlayer.GetGroundAttackers() == 0 then
			Media.DisplayMessage("No more enemies incoming.")
			HumanPlayer.MarkCompletedObjective(TowerDefenseObjective)
		end
	end
end
