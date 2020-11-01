--[[
   Copyright 2021, 2022 The OpeHV Developers (see CREDITS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

LightTrees = function ()
	Forest.Hit(CPos.New(18, 15), 30000)
	Forest.Hit(CPos.New(19, 15), 30000)
	Forest.Hit(CPos.New(20, 13), 30000)
	Forest.Hit(CPos.New(18, 18), 30000)
	Forest.Hit(CPos.New(19, 18), 30000)

	Forest.Hit(CPos.New(48, 11), 30000)
	Forest.Hit(CPos.New(48, 12), 30000)
	Forest.Hit(CPos.New(50, 14), 30000)
	Forest.Hit(CPos.New(52, 12), 30000)
	Forest.Hit(CPos.New(54, 11), 30000)
	Forest.Hit(CPos.New(54, 15), 30000)

	Forest.Hit(CPos.New(54, 35), 30000)
	Forest.Hit(CPos.New(56, 34), 30000)

	Forest.Hit(CPos.New(56, 56), 30000)
	Forest.Hit(CPos.New(55, 55), 30000)
	Forest.Hit(CPos.New(53, 52), 30000)
	Forest.Hit(CPos.New(52, 51), 30000)
	Forest.Hit(CPos.New(43, 49), 30000)
	Forest.Hit(CPos.New(38, 52), 30000)
	Forest.Hit(CPos.New(40, 55), 30000)

	Forest.Hit(CPos.New(18, 38), 30000)
	Forest.Hit(CPos.New(13, 40), 30000)
	Forest.Hit(CPos.New(16, 41), 30000)

	Forest.Hit(CPos.New(12, 44), 30000)
	Forest.Hit(CPos.New(13, 44), 30000)
	Forest.Hit(CPos.New(14, 44), 30000)
	Forest.Hit(CPos.New(12, 45), 30000)
	Forest.Hit(CPos.New(13, 45), 30000)
	Forest.Hit(CPos.New(14, 45), 30000)
	Forest.Hit(CPos.New(12, 46), 30000)
	Forest.Hit(CPos.New(13, 46), 30000)
	Forest.Hit(CPos.New(14, 46), 30000)

	Forest.Hit(CPos.New(45, 53), 30000)
	Forest.Hit(CPos.New(46, 53), 30000)
	Forest.Hit(CPos.New(47, 53), 30000)

	Forest.Hit(CPos.New(26, 54), 30000)
	Forest.Hit(CPos.New(26, 55), 30000)
	Forest.Hit(CPos.New(27, 54), 30000)
	Forest.Hit(CPos.New(27, 55), 30000)

	Forest.Hit(CPos.New(30, 20), 30000)
	Forest.Hit(CPos.New(30, 21), 30000)
	Forest.Hit(CPos.New(31, 20), 30000)
	Forest.Hit(CPos.New(31, 21), 30000)
end


Tick = function()
	if DateTime.GameTime > 5 then
		UpdateForestStatus()
	end
end

WorldLoaded = function()
	LightTrees()

	Human = Player.GetPlayer("FireBrigade")
	InitObjectives(Human)
	ForestObjective = Human.AddPrimaryObjective("Save the forests.")
	FivePercentObjective = Human.AddSecondaryObjective("Save 95 % of the forest.")
	TenPercentObjective = Human.AddSecondaryObjective("Save 90 % of the forest.")
	FifteenPercentObjective = Human.AddSecondaryObjective("Save 85 % of the forest.")
	TwentyPercentObjective = Human.AddSecondaryObjective("Save 80 % of the forest.")
	TwentyFivePercentObjective = Human.AddSecondaryObjective("Save 75 % of the forest.")
	ThirtyPercentObjective = Human.AddSecondaryObjective("Save 70 % of the forest.")
end

UpdateForestStatus = function()
	local currentColor = HSLColor.White
	local burnedPercentage = (1 - Forest.TreesLeft / Forest.TotalTrees) * 100

	if burnedPercentage > 30 then
		currentColor = HSLColor.DarkRed
		Human.MarkFailedObjective(ThirtyPercentObjective)
	elseif burnedPercentage > 25 then
		currentColor = HSLColor.Red
		Human.MarkFailedObjective(TwentyFivePercentObjective)
	elseif burnedPercentage > 20 then
		currentColor = HSLColor.OrangeRed
		Human.MarkFailedObjective(TwentyPercentObjective)
	elseif burnedPercentage > 15 then
		currentColor = HSLColor.DarkOrange
		Human.MarkFailedObjective(FifteenPercentObjective)
	elseif burnedPercentage > 10 then
		currentColor = HSLColor.Orange
		Human.MarkFailedObjective(TenPercentObjective)
	elseif burnedPercentage > 5 then
		currentColor = HSLColor.Yellow
		Human.MarkFailedObjective(FivePercentObjective)
	end

	UserInterface.SetMissionText(math.floor(burnedPercentage) .. "% of the forests destroyed.", currentColor)

	if Forest.TreesLeft < 1 then
		Human.MarkFailedObjective(ForestObjective)
	end

	if Forest.TreesBurning == 0 then
		Human.MarkCompletedObjective(ForestObjective)
		if burnedPercentage < 30 then
			Human.MarkCompletedObjective(ThirtyPercentObjective)
		end
		if burnedPercentage < 25 then
			Human.MarkCompletedObjective(TwentyFivePercentObjective)
		end
		if burnedPercentage < 20 then
			Human.MarkCompletedObjective(TwentyPercentObjective)
		end
		if burnedPercentage < 15 then
			Human.MarkCompletedObjective(FifteenPercentObjective)
		end
		if burnedPercentage < 10 then
			Human.MarkCompletedObjective(TenPercentObjective)
		end
		if burnedPercentage < 5 then
			Human.MarkCompletedObjective(FivePercentObjective)
		end
	end
end
