
--[[
   Copyright 2021-2023 The OpenHV Developers (see AUTHORS)
   This file is part of OpenHV, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]


Ticks = 0
Speed = 5

Tick = function()
	Ticks = Ticks + 1

	if Ticks > 1 or not Map.IsPausedShellmap then
		local t = (Ticks + 45) % (360 * Speed) * (math.pi / 180) / Speed;
		Camera.Position = ViewportOrigin + WVec.New(19200 * math.sin(t), 28800 * math.cos(t), 0)
	end
end

WorldLoaded = function()
	ViewportOrigin = Camera.Position
end
