CurrentWave = 1
LastWave = false
Won = false

Generic = { "lightboat", "boat" }
Lightning = { "railgunboat" }
Submarine = { "carrier" }

Waves =
{
	{ delay = 500, units = { Generic, Generic, Generic, Generic } },
	{ delay = 800, units = { Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic, Generic } },
	{ delay = 850, units = { Lightning, Lightning, Lightning, Lightning, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic } },
	{ delay = 900, units = { Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic } },
	{ delay = 950, units = { Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Lightning, Lightning, Lightning, Lightning } },
	{ delay = 950, units = { Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Generic,Generic,Submarine, Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Lightning, Lightning, Lightning, Lightning, Submarine } },
	{ delay = 1000, units = { Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Lightning, Lightning, Lightning, Lightning } },
	{ delay = 850, units = { Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Lightning, Lightning, Lightning, Lightning , Submarine, Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Lightning, Lightning, Lightning, Lightning } },
	{ delay = 900, units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
	{ delay = 900, units = { Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Submarine, Submarine, Lightning,Lightning, Lightning, Submarine, Submarine,Lightning, Lightning , Submarine, Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Submarine, Submarine,Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Submarine, Submarine, Submarine, Submarine } },
	{ delay = 900, units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
	{ delay = 900, units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
	{ delay = 900, units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
	{ delay = 1200, units = { Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Submarine, Submarine, Lightning,Lightning, Lightning, Submarine, Submarine,Lightning, Lightning , Submarine, Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Submarine, Submarine,Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Submarine, Submarine, Submarine, Submarine } },
	{ delay = 900, units = { Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Submarine, Submarine, Lightning,Lightning, Lightning, Submarine, Submarine,Lightning, Lightning , Submarine, Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Submarine, Submarine,Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Submarine, Submarine, Submarine, Submarine } },
	{ delay = 900, units = { Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Lightning, Lightning, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Submarine, Submarine, Lightning,Lightning, Lightning, Submarine, Submarine,Lightning, Lightning , Submarine, Submarine, Submarine, Submarine, Submarine, Lightning, Lightning, Lightning, Lightning, Lightning, Generic, Submarine, Submarine,Generic, Generic, Generic,Generic,Generic,Generic, Generic, Generic, Generic , Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning, Lightning,Lightning,Lightning,Submarine, Submarine,Lightning, Lightning, Lightning, Lightning, Submarine, Submarine, Submarine, Submarine } },
	{ delay = 900, units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
	{ delay = 900, units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
	{ delay = 900, units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
	{ delay = 900, units = { Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine , Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine, Submarine } },
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

	local waveInfo = UserInterface.Translate("current-wave", { ["wave"] = CurrentWave, ["waves"] = #Waves })
	UserInterface.SetMissionText("\n\n\n" .. waveInfo)
end

SendNextWave = function()
	local wave = Waves[CurrentWave]
	local harbors = HumanPlayer.GetActorsByType("harbor2")
	local extractors = HumanPlayer.GetActorsByType("extractor")
	local midbuilds = HumanPlayer.GetActorsByTypes({ "radar2", "comlink" })
	local entrylocation = SpawningWaypoint.Location
	local extitlocation = DestinationWaypoint1.Location

	if #harbors == 0 then
		extitlocation = DestinationWaypoint2.Location
	end
	if #extractors == 0 then
		extitlocation = DestinationWaypoint3.Location
	end
	if #midbuilds == 0 then
		extitlocation = DestinationWaypoint4.Location
	end

	Trigger.AfterDelay(wave.delay, function()
		Utils.Do(wave.units, function(units)
			Attackers = Reinforcements.Reinforce(EnemyPlayer, units, { entrylocation , extitlocation })
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

Tick = function()
	local buildingproportions = HumanPlayer.GetActorsByTypes({ "harbor2", "extractor", "radar2", "comlink", "module2", "factory2" })
	local percentage = #buildingproportions * 100 / 22
	if percentage < .65  and not HumanPlayer.IsObjectiveFailed(TowerDefenseObjective) then
		Media.DisplayMessage(UserInterface.Translate("buildings-lost-65-percent"))
		HumanPlayer.MarkFailedObjective(TowerDefenseObjective)
	end

	if LastWave and not HumanPlayer.IsObjectiveCompleted(TowerDefenseObjective) then
		Trigger.AfterDelay(200, function()
			if not Won and #EnemyPlayer.GetGroundAttackers() == 0 then
				Media.DisplayMessage(UserInterface.Translate("no-more-enemies"))
				HumanPlayer.MarkCompletedObjective(TowerDefenseObjective)
				Won = true
			end
		end)
	end
end
