## rules.yaml
briefing-crater-defense = Commander! The harbors around the Crater Lake are under attack. Use the old railway network to connect your turrets to the energy grid. We are holding the grounds at the exits of the trench, but don't allow too many breaches so our defenses are not overwhelmed.

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


## crater-defense.lua
not-too-many-enemies-through-trench = Do not allow too many enemies to exit the trench.
tolerable-breaches = Tolerable Breaches: { $breaches }

