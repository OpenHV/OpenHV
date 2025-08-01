CurrentWave = 1
LastWave = false
Won = false

Generic = { "lightboat", "torpedoboat" }
Lightning = { "lightningboat" }
Submarine = { "carrier" }

Waves =
{
	{ delay = DateTime.Seconds(20), units = { Generic, Generic, Generic, Generic } },
	{ delay = DateTime.Seconds(32), units = { Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic, Generic } },
	{ delay = DateTime.Seconds(34), units = { Lightning, Lightning, Lightning, Lightning, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic } },
	{ delay = DateTime.Seconds(36), units = { Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic } },
	{ delay = DateTime.Seconds(38), units = { Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Lightning, Lightning, Lightning, Lightning } },
	{ delay = DateTime.Seconds(38), units = { Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Generic,Generic,Submarine, Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Lightning, Lightning, Lightning, Lightning, Submarine } },
	{ delay = DateTime.Seconds(40), units = { Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Lightning, Lightning, Lightning, Lightning } },
	{ delay = DateTime.Seconds(34), units = { Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Lightning, Lightning, Lightning, Lightning , Submarine, Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Lightning, Lightning, Lightning, Lightning } },
	{ delay = DateTime.Seconds(36), units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
	{ delay = DateTime.Seconds(36), units = { Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Submarine, Submarine, Lightning,Lightning, Lightning, Submarine, Submarine,Lightning, Lightning , Submarine, Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Submarine, Submarine,Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Submarine, Submarine, Submarine, Submarine } },
	{ delay = DateTime.Seconds(36), units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
	{ delay = DateTime.Seconds(36), units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
	{ delay = DateTime.Seconds(36), units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
	{ delay = DateTime.Seconds(48), units = { Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Submarine, Submarine, Lightning,Lightning, Lightning, Submarine, Submarine,Lightning, Lightning , Submarine, Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Submarine, Submarine,Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Submarine, Submarine, Submarine, Submarine } },
	{ delay = DateTime.Seconds(36), units = { Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Submarine, Submarine, Lightning,Lightning, Lightning, Submarine, Submarine,Lightning, Lightning , Submarine, Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Submarine, Submarine,Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Submarine, Submarine, Submarine, Submarine } },
	{ delay = DateTime.Seconds(36), units = { Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Submarine, Submarine, Lightning,Lightning, Lightning, Submarine, Submarine,Lightning, Lightning , Submarine, Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Submarine, Submarine,Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Submarine, Submarine, Submarine, Submarine } },
	{ delay = DateTime.Seconds(36), units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
	{ delay = DateTime.Seconds(36), units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
	{ delay = DateTime.Seconds(36), units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
	{ delay = DateTime.Seconds(36), units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
}

WorldLoaded = function()
	Camera.Position = CenterCamera.CenterPosition

	HumanPlayer = Player.GetPlayer("Multi0")
	EnemyPlayer = Player.GetPlayer("Creeps")

	InitObjectives(HumanPlayer)
	TowerDefenseObjective = AddPrimaryObjective(HumanPlayer, "save-65-percent-buildings")
	UpdateGameStateText()

	SendNextWave()
end

CachedWave = -1
function UpdateGameStateText()
	if CachedWave == CurrentWave then
		return
	end
	CachedWave = CurrentWave

	local waveInfo = UserInterface.GetFluentMessage("current-wave", { ["wave"] = CurrentWave, ["waves"] = #Waves })
	UserInterface.SetMissionText("\n\n\n" .. waveInfo)
end

SendNextWave = function()
	local wave = Waves[CurrentWave]
	local harbors = HumanPlayer.GetActorsByType("harbor2")
	local extractors = HumanPlayer.GetActorsByType("extractor")
	local midbuilds = HumanPlayer.GetActorsByTypes({ "radar2", "comlink" })
	local exitLocation = DestinationWaypoint1.Location

	if #harbors == 0 then
		exitLocation = DestinationWaypoint2.Location
	end
	if #extractors == 0 then
		exitLocation = DestinationWaypoint3.Location
	end
	if #midbuilds == 0 then
		exitLocation = DestinationWaypoint4.Location
	end

	local path = { SpawningWaypoint.Location, exitLocation }

	Trigger.AfterDelay(wave.delay, function()
		Utils.Do(wave.units, function(units)
			Attackers = Reinforcements.Reinforce(EnemyPlayer, units, path)
			Unstuck(Attackers, exitLocation)
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

Unstuck = function(actors, exitLocation)
	Utils.Do(actors, function(actor)
		if not actor.IsDead then
			Trigger.OnIdle(actor, function()
				actor.Move(exitLocation)
			end)
		end
	end)
end

Tick = function()
	local buildingProportions = HumanPlayer.GetActorsByTypes({ "harbor2", "extractor", "radar2", "comlink", "module2", "factory2" })
	local percentage = #buildingProportions * 100 / 22
	if percentage < .65  and not HumanPlayer.IsObjectiveFailed(TowerDefenseObjective) then
		Media.DisplayMessage(UserInterface.GetFluentMessage("buildings-lost-65-percent"))
		HumanPlayer.MarkFailedObjective(TowerDefenseObjective)
	end

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
