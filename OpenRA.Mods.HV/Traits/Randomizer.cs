#region Copyright & License Information
/*
 * Copyright 2019-2021 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Wastes a few random numbers.")]
	public class RandomizerInfo : TraitInfo<Randomizer> { }

	public class Randomizer
	{
		public Randomizer()
		{
			// The shellmap is static which causes RNG problems in the UI.
			for (var i = 0; i < DateTime.Now.Second; i++)
				Game.CosmeticRandom.Next();
		}
	}
}
