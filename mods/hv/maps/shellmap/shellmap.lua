
--[[
   Copyright 2021 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]


ticks = 0
speed = 5

Tick = function()
	ticks = ticks + 1

	if ticks > 1 or not Map.IsPausedShellmap then
		local t = (ticks + 45) % (360 * speed) * (math.pi / 180) / speed;
		Camera.Position = viewportOrigin + WVec.New(19200 * math.sin(t), 28800 * math.cos(t), 0)
	end
end

WorldLoaded = function()
	viewportOrigin = Camera.Position
end
