## rules.yaml
briefing-harbor-defense = Commander! The great Synapol harbor of the Crater Lake, who's responsible for the major Synapol vehicle production of the planet, will soon be attacked by a Yuruki naval covet. Use the power railway and all resources at your disposition to build the necessary defenses to resist to the attack. Try to save at least 65% of the buildings to accomplish your mission.

actor-entry-name = Enemy Spawn Waypoint
actor-exit-name = Enemy Exit Waypoint

actor-clusterturret =
   .description = Base defense.
      Requires power to operate.
      Strong vs Tanks and Pods
   .name = Cluster Turret

actor-amplifier =
   .description = Weapon range amplifier.
      Requires power to operate.
   .name = Amplifier


## harbor-defense.lua
save-65-percent-buildings = Save 65 % of the buildings.
buildings-lost-65-percent = More than 65 % of buildings lost
harbors-destroyed = Harbors destroyed!
extractors-destroyed = Extractors destroyed!
midbuilds-destroyed = Radar and comlink destroyed!
