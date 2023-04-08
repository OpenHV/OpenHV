Wave = 1
Breaches = 10

Machine = { "scout1", "scout1", "scout1" }
Rocket = { "scout2", "scout2" }
Tank = { "tank3" }

Waves =
{
	{ delay = 500, units = { Machine, Machine, Machine, Machine } },
	{ delay = 500, units = { Rocket, Rocket, Rocket, Machine, Machine, Machine, Machine, Machine } },
	{ delay = 550, units = { Rocket, Rocket, Rocket, Rocket, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine } },
	{ delay = 600, units = { Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine } },
	{ delay = 650, units = { Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Machine, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine , Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Rocket,Rocket,Rocket, Rocket, Rocket, Rocket } },
	{ delay = 650, units = { Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Machine, Machine, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Machine,Machine,Tank, Machine,Machine, Machine, Machine, Machine , Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Rocket,Rocket,Rocket, Rocket, Rocket, Rocket, Tank } },
	{ delay = 700, units = { Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Machine, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine , Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Rocket,Rocket,Rocket, Rocket, Rocket, Rocket } },
	{ delay = 800, units = { Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Machine, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine , Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Rocket,Rocket,Rocket, Rocket, Rocket, Rocket , Tank, Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Machine, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine , Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Rocket,Rocket,Rocket, Rocket, Rocket, Rocket } },
	{ delay = 800, units = { Tank,Tank,Tank,Tank,Tank,Tank,Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank } },
	{ delay = 800, units = { Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Machine, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine , Rocket, Rocket, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Rocket,Tank, Tank, Rocket,Rocket, Rocket, Tank, Tank,Rocket, Rocket , Tank, Tank, Tank, Tank, Tank, Rocket, Rocket, Rocket, Rocket, Rocket, Machine, Tank, Tank,Machine, Machine, Machine,Machine,Machine,Machine, Machine, Machine, Machine , Tank, Tank,Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket, Rocket,Rocket,Rocket,Tank, Tank,Rocket, Rocket, Rocket, Rocket, Tank, Tank, Tank, Tank } },
	{ delay = 800, units = { Tank,Tank,Tank,Tank,Tank,Tank,Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank , Tank,Tank,Tank,Tank,Tank,Tank,Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank , Tank,Tank,Tank,Tank,Tank,Tank,Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank , Tank,Tank,Tank,Tank,Tank,Tank,Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank, Tank } },
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
			Wave = Wave + 1
			SendNextWave()
		else
			LastWave = true
		end
	end)
end

Tick = function()
	if LastWave and not HumanPlayer.IsObjectiveCompleted(TowerDefenseObjective) then
		Trigger.AfterDelay(200, function()
		    if #EnemyPlayer.GetGroundAttackers() == 0 then
		    	Media.DisplayMessage("No more enemies incoming.")
		    	HumanPlayer.MarkCompletedObjective(TowerDefenseObjective)
		    end
                end)
	end
end
