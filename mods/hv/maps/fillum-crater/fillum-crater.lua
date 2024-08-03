--[[
   Copyright 2024 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

GroundReinforcementUnits = { "rifleman", "rifleman", "rifleman", "rocketeer", "rocketeer", "rocketeer", "rocketeer", "rocketeer", "buggy", "buggy", "buggy", "buggy", "buggy", "buggy", "buggy", "hackertank", "artillery", "artillery", "artillery", "technician", "technician", "technician", "repairtank", "hackertank", "missiletank", "missiletank", "missiletank", "dualartillery" }
AirReinforcementUnits = { "copter", "copter", "copter", "banshee", "banshee", "chopper"}
NavalReinforcementUnits = { "boomer", "patrolboat", "patrolboat", "patrolboat", "submarine", "railgunboat", "railgunboat" }
YurukiReinforcements = { "submarine", "carrier", "lightningboat", "lightboat", "torpedoboat" }

Warning = UserInterface.Translate("warning")

Tick = function()
    local towers = Human.GetActorsByType("miner2")
    local communications = Enemy.GetActorsByType("comlink")

    if DateTime.GameTime % DateTime.Seconds(1) == 0 and not Human.IsObjectiveCompleted(BuildStorageObjective) and CheckForBase(Human, { "storage" }) then
		Human.MarkCompletedObjective(BuildStorageObjective)
	end

	if DateTime.GameTime % DateTime.Seconds(1) == 0 and not Human.IsObjectiveCompleted(BuildTechcenterObjective) and CheckForBase(Human, { "techcenter" }) then
		Human.MarkCompletedObjective(BuildTechcenterObjective)
	end

	if DateTime.GameTime % DateTime.Seconds(1) == 0 and not Human.IsObjectiveCompleted(BuildUplinkObjective) and CheckForBase(Human, { "uplink" }) then
		Human.MarkCompletedObjective(BuildUplinkObjective)
	end

	if DateTime.GameTime % DateTime.Seconds(1) == 0 and not Human.IsObjectiveCompleted(BuildModuleObjective) and CheckForBase(Human, { "module" }) then
		Human.MarkCompletedObjective(BuildModuleObjective)
	end

	if DateTime.GameTime % DateTime.Seconds(1) == 0 and not Human.IsObjectiveCompleted(BuildFactoryObjective) and CheckForBase(Human, { "factory" }) then
		Human.MarkCompletedObjective(BuildFactoryObjective)
	end

    if DateTime.GameTime % DateTime.Seconds(180) == 0 and DateTime.GameTime > DateTime.Seconds(210) and not Human.IsObjectiveCompleted(DestroyCommunicationsObjective) then  -- happens every 3 mins, starting at 3.5 game minutes, if the player hasn't destroyed Synapol comlinks
        SynapolReinforcementsGround = Reinforcements.Reinforce(Enemy, GroundReinforcementUnits, { SpawningWaypoint1.Location, DestinationWaypoint1.Location })
        SynapolReinforcementsAir = Reinforcements.Reinforce(Enemy, AirReinforcementUnits, { SpawningWaypoint2.Location, DestinationWaypoint2.Location })
        SynapolReinforcementsNaval = Reinforcements.Reinforce(Enemy, NavalReinforcementUnits, { SpawningWaypoint3.Location, DestinationWaypoint3.Location })
        Media.DisplayMessage(UserInterface.Translate("reinforcements-incoming"), Warning)
    end

    if DateTime.GameTime == DateTime.Seconds(120) or DateTime.GameTime % DateTime.Seconds(180) == 0 and DateTime.GameTime > DateTime.Seconds(120) then-- spawn Yuruki reinforcements at 2 mins, and starting from there, every 3 mins
        YurukiReinforcement = Reinforcements.Reinforce(Human, YurukiReinforcements, { SpawningWaypoint4.Location, DestinationWaypoint4.Location })
    end

    if not Human.IsObjectiveCompleted(ResourcesClaimedObjective) and #towers == 18 then -- if the player has built every mining tower
        Human.MarkCompletedObjective(ResourcesClaimedObjective)
    end

    if not Human.IsObjectiveCompleted(DestroyCommunicationsObjective) and #communications == 0 then -- if the player has destroyed every synpapol comlinks
        Human.MarkCompletedObjective(DestroyCommunicationsObjective)
    end

end

WorldLoaded = function()
	Human = Player.GetPlayer("Yuruki Industries")
	Enemy = Player.GetPlayer("Synapol Corporation")

	InitObjectives(Human)

	BuildStorageObjective = AddPrimaryObjective(Human, "build-storage")
	BuildTechcenterObjective = AddPrimaryObjective(Human, "build-techcenter")
	BuildUplinkObjective = AddPrimaryObjective(Human, "build-uplink")
	BuildModuleObjective = AddPrimaryObjective(Human, "build-module")
	BuildFactoryObjective = AddPrimaryObjective(Human, "build-factory")
	ResourcesClaimedObjective = AddPrimaryObjective(Human, "build-all-towers")
	DestroyCommunicationsObjective = AddPrimaryObjective(Human, "destroy-enemy-communication")

	Camera.Position = Outpost.CenterPosition
end
