## World

### StartingUnits
dropdown-starting-units =
    .label = Starting Units
    .description = The units that players start the game with

options-starting-units =
    .base-only = Base only
    .base-miner = Base + Miner
    .base-scout = Base + Scout

### MapBuildRadius
checkbox-build-radius =
    .label = Limit Build Area
    .description = Limits structure placement to areas around base and outposts.

### Faction
faction-yuruki =
   .name = Yuruki
   .description = Yuruki Industries
    A corporation that grew by colonizing new planets.
    Their colonial defense forces helped pave the way for
    an aggressive expansion throughout the solar system.

    Strategy:
        - Focuses on stealth and specialized combat units
        - Transport and aerial scout units are faster but
          less resistant

    Specific Units:
        - Sniper Pod (anti-pod)
        - Shocker Pod (anti-vehicle and anti-air)
        - Bomber Pod (anti-building)
        - Jetpacker2 Pod (anti-vehicle)
        - Assault Tank (anti-vehicle and anti-building)
        - Mobile AA Tank (anti-air)
        - Gatling Bike (anti-pod)
        - Countermeasure Tank (specialist)
        - Stealth Tank (anti-ground)
        - Gun Ship (anti-ground and anti-air)
        - Speeder (anti-air)
        - Athmospheric Bomber (anti-ground)
        - Patrol Boat (anti-ground and anti-air)
        - Submarine (anti-navy)
        - Lightning Boat (anti-ground)
        - Missile Submarine (anti-ground)

    Superunit:
        - Battleship (anti-ground)

    Superweapons:
        - Air Strike
        - Field Generator
        - Orbital Railgun Strike

faction-synapol =
   .name = Synapol
   .description = Synapol Corporation
    A large interplanetary company that manufactures
    everything from common household appliances to armaments.
    Their security department became a large paramilitary force.

    Strategy:
        - Focuses on brute force and flexible combat units
        - Transport and aerial scout units are slower but
          more resistant

    Specific Units:
        - Mortar Pod (anti-ground)
        - Rocketeer Pod (anti-vehicle and anti-air)
        - Flamer Pod (anti-ground)
        - Jetpacker Pod (anti-pod)
        - Missile Tank (anti-ground and anti-air)
        - Artillery Tank (anti-ground)
        - Ramp Buggy (anti-pod)
        - Hacker Tank (specialist)
        - Railgun Tank (anti-ground)
        - Assault Helicopter (anti-ground and anti-air)
        - Turtle (anti-air)
        - Banshee (anti-ground)
        - Light Boat (anti-ground and anti-air)
        - Torpedo Boat (anti-navy)
        - Laser Boat (anti-ground)
        - Drone Ship (anti-ground)

    Superunit:
        - Mothership (anti-ground and anti-air)

    Superweapons:
        - Drop Pods
        - Grand Howitzer
        - Thermonuclear Bomb

faction-random =
   .name = Any
   .description = Random Corporation
    A random corporation will be chosen when the game starts.

## CubeSpawner
checkbox-crates =
    .label = Cubes
    .description = Collect cubes with units to receive random bonuses

## ResourceRenderer
resource-generic = Mineable Resource Deposit
resource-gold = Precious Metal Deposit
resource-iron = Base Metal Deposit

## Player
bot-rogue-ai =
   .name = Rogue AI

## Notifications
notification-game-loaded = Game loaded.
notification-game-saved = Game saved.

notification-unit-lost = Unit lost.
notification-building-lost = Building lost.
notification-building-captured = Building captured.
notification-low-power = Low power.
notification-new-construction-options = New construction options.
notification-cant-place-building = Building cannot be placed.
notification-insufficient-funding = Insufficient funding.
notification-order-placed = Order placed.

notification-building-ready = Building ready.
notification-building = Building.
notification-on-hold = On hold.
notification-cancelled = Cancelled.
notification-unit-ready = Unit ready.
notification-training = Training.
notification-no-space = Not enough space.

notification-outpost-established = Outpost established.
notification-miner-ready = Miner ready.

notification-map-revealed = Map revealed.
notification-unit-cloaked = Unit cloaked.

notification-enemy-units-detected = Enemy units detected.

notifications-repairing = Repairing.
notification-unit-repaired = Unit repaired.

notification-select-target = Select target.

notification-drop-pod-charging = Drop Pod charging.
notification-drop-pod-ready = Drop Pod ready.
notifications-drop-pod-incoming = Drop Pod incoming.
notification-reinforcementa-arrived = Reinforcements have arrived.

notification-howitzer-detected = Howitzer detected.
notification-howitzer-charging = Howitzer charging.
notification-howitzer-ready = Howitzer ready.

notification-force-field-detected = Force Field detected.
notification-force-field-charging = Force Field charging.
notification-force-field-ready = Force Field ready.
notification-force-field-engaged = Force Field engaged.

notification-nuke-detected = Nuke detected.
notification-nuke-charging = Nuke charging.
notification-nuke-ready = Nuke ready.

notification-railgun-detected = Orbital Railgun detected.
notification-railgun-charging = Orbital Railgun charging.
notification-railgun-ready = Orbital Railgun ready.

notification-insufficient-power = Insufficient power.
notification-televator-charging = Televator charging.
notification-televator-ready = Televator ready.

## Aircraft
actor-gunship =
   .description = Attack Ship armed with
    a large chain gun.
      Strong vs Pods, Buildings and Aircraft
      Weak vs Tanks
   .name = Gun Ship
   .encyclopedia = The gun ship is a fast plane, armed with a heavy machine gun. It's a versatile unit, often used to harass the opponents' mining towers and outposts.

     {actor-gunship.description}

     Availability: Yuruki

actor-jet =
   .description = Super Fast Interceptor
      Strong vs Aircraft
      Can't target ground units.
   .name = Speeder
   .encyclopedia = TO BE DONE

     {actor-jet.description}

     Availability: Yuruki

actor-jet2 =
   .description = Attack Bomber
      Strong vs Buildings
      Weak vs Tanks and Pods
      Can't target Aircraft
   .name = Bomber
   .encyclopedia = The bomber is an air-to-ground plane, armed with a plasma cannon that can destroy buildings easily. It is slower but more durable than the attack aircraft.

     {actor-jet2.description}

     Availability: Yuruki

actor-copter =
   .name = Assault Helicopter
   .description = Small Helicopter Gunship
      Strong vs Pods, Buildings and Aircraft
      Weak vs Tanks
   .encyclopedia = The assault helicopter is a small helicopter, armed with a turret heavy machine gun. It's a versatile unit, often used to harass the opponents' mining towers and outposts.

     {actor-copter.description}

     Availability: Synapol

actor-saucer =
   .name = Scout Saucer
   .description = Reconnaissance air unit.
      Unarmed
   .encyclopedia = A disc shaped aircraft with good maneuverability. It's not suited for combat, but for reconnaissance missions. Due to its unusual shape and low radar reflectance, oftentimes civilians will report this sighting to the authorities first.

     {actor-saucer.description}

     Availability: None

actor-observer =
   .name = Observer
   .description = Reconnaissance air unit.
      Unarmed
   .encyclopedia = An orb shaped helicopter with scouting capabilities. It's not suited for combat, but for reconnaissance missions.

     {actor-observer.description}

     Availability: Synapol

actor-banshee =
   .name = Banshee
   .description = Heavy Helicopter Gunship
      Strong vs Buildings
      Weak vs Tanks and Pods
      Can't target Aircraft
   .encyclopedia = The banshee is a more durable and slow helicopter, that launches powerful missiles onto the ground. It is particularly strong against buildings.

     {actor-banshee.description}

     Availability: Synapol

actor-turtle =
   .name = Turtle
   .description = Heavy Flying Bunker
      Strong vs Aircraft
      Can't target ground units.
   .encyclopedia = TO BE DONE

     {actor-turtle.description}

     Availability: Synapol

actor-chopper =
   .name = Transport Helicopter
   .description = Vehicle Transport Helicopter.
      Can load pods
      and lift one vehicle.
   .encyclopedia = The chopper is a heavy transport aircraft that can load a group of pods. It can also lift one vehicle.

     {actor-chopper.description}

     Availability: Synapol

actor-chopper-husk-name = Crashing Transport Helicopter

actor-balloon =
   .name = Scout Balloon
   .description = Reconnaissance air unit.
     Unarmed
   .encyclopedia = A reconnaissance air unit with scouting capabilities. It's not suited for combat, but for reconnaissance missions.

     Unarmed

     Availability: Yuruki

actor-dropship =
   .name = Heavy Transport Dropship
   .description = Vehicle Transport Shuttle.
      Can load pods
      and lift one vehicle.
   .encyclopedia = The dropship is a heavy transport aircraft, than can load up a group of pods. It can also lift one vehicle.

     {actor-dropship.description}

     Availability: Yuruki

actor-dropship-husk-name = Crashing Transport Dropship
actor-drone-name = Drone
actor-landedpod-name = Landed Pod
actor-bomber-name = Athmospheric Bomber
actor-bomber-encyclopedia = The bomber is a fast plane that drops salves of bombs onto the target. They can be launched from the Yuruki radar.

  Strong vs Buildings
  Weak vs Pods and Tanks

  Availability: Non-trainable
actor-cargoship-name = Supply Aircraft
actor-cargoship-encyclopedia = The cargoship is an universal cargo unit that transports units for trading. It's used to deliver units onto the Trade Platform.

  Unarmed

  Availability: Non-trainable
actor-airlifter-name = Transport Aircraft
actor-airlifter-encyclopedia = The airlifter paradrops loaded units onto the ground. It is used in drop zone buildings to deliver mercenary tanks.

  Unarmed

  Availability: Non-trainable

## Animals
actor-beast =
   .name = Critter
   .generic-name = Beast

actor-crow-name = Crows
actor-crow2-name = Crow
actor-gull-name = Seagulls
actor-gull2-name = Seagull
actor-seamonster-name = Sea Monster
actor-worm-generic-name = Worm
actor-flyingmonster-name = Flying monster

## Bonus
actor-goldball =
   .name = Refined Resources
   .generic-name = Gold

## Buildings
actor-base =
   .name = Main Base
   .description = Builds structures, builders and miners.
    Only one can be built.
   .encyclopedia = This base of operations is able to summon new buildings as well as utility vehicles such as miners or builders. New buildings are only allowed within a base radius. You can also spawn buildings into an allied base radius. To extend the build radius you need to train builders which can deploy into outposts. Protect the base building at all costs. If your base ever gets destroyed, select an outpost and request a new main base for a possible comeback.

actor-basefiredeparment =
   .name = Fire Department Base

actor-outpost =
   .name = Outpost
   .encyclopedia = This structure cannot be summoned on the main base like the others. It is created by deploying a builder. Thus it can be used to extend the base radius or even create bridge heads far away from the main base. Another important feature is that it is the only building that can build another base, but only in the event of the initial main base getting destroyed.

actor-generator =
   .description = Generates power for other structures.
   .encyclopedia = Generators power the base. Loss of power effects buildings individually. Without power the radar shows no minimap, production is slowed down, support power charging is paused, base defense system stop operating.

    To restore power you can build more generators or as a short term action disable certain buildings by clicking the zap button and selected buildings to power down. With power lines cut the actions are more drastic: buildings will be unable to produce or contribute to the tech tree also canceling current production queues if they are effected.

    Always plan ahead so you don't accidentally power down your base slowing down your progress. On the other hand it is a vital strategy to destroy enemy power plants to distract the enemy into power micromanagement, losen defenses temporarily and to put enemy on hold.
   .name = Power Plant

actor-radar =
   .description = Provides an overview
    of the battlefield.
      Calls in reinforcements.
      Requires power to operate.
   .encyclopedia = The radar enables a minimap on top right screen of the command interface. It also provides a larger amount of local vision around itself. As a tech building it is opening up the aerial branch of units. It also provides the first tier of support powers which can be an air strike or orbital drop pod reinforcements to be placed anywhere on the battlefield. For the air strike hold the mouse button and move the pointer to set the flight path.
   .name = Radar Dome
   .airstrikepower-name = Air Strike
   .airstrikepower-description = Deploy an aerial bombing run.

actor-radar2 =
   .droppodspower-name = Drop Pods
   .droppodspower-description = Atmospheric assault reinforcements:
    Small team of pods drops onto target location from orbit.

actor-trader =
   .description = Repairs vehicles and aircraft for credits.
    Allows buying units on the free market.
   .encyclopedia = The trade platform provides access to the free market. For a certain amount of credits damaged vehicles can be ordered here and get repaired. Alternatively select the repair icon on the top right of the command interface and click a damaged vehicle which will then automatically head to the nearest trade platform for repairs. As a tech building the trade platform opens up the tank path. It also serves as an additional vehicle production queue by allowing orders from outside. These vary in price and are more expensive than local production.
   .name = Trade Platform

actor-module =
   .description = Produces and repairs very light vehicles.
   .encyclopedia = The module is the assembly for the smallest type of vehicle: the pods. They are very lightly armored and only contain one occupant, but provide some mobility and protection against harsh environments on planets with limited terraforming. Pods can come back in the immediate vicinity of the module to bandage wounds and provide some quick repairs on the vehicle.
   .name = Module

actor-miner2 =
   .name = Mining Tower
   .encyclopedia = The mining tower is errected by creating a miner in the vehicle queue from either main base or factory and ordering it upon a metal bar symbol on the map where gold is more valueable than iron. Once deployed it will start mining and after certain intervals release a tanker which will carry back to a storage building upon then the credits can be spent. When the resources are depleted, the extraction value drops. Right-click on a mining tower to redeploy it into a miner again to move it onto a different spot.

actor-starport =
   .name = Starport
   .encyclopedia = The starport is the base of operations for the air force. Underground facilities construct new aircraft that are self-sufficient for the time of the battle requiring no refueling nor rearming.
   .description = Produces aircraft.

actor-factory =
   .description = Builds tanks.
   .name = Vehicle Factory
   .encyclopedia = The vehicle factory is the source for all armored ground forces. Building multiple ones increases production speed. Right click on a factory to set a primary building whose teleportation pad shall be used for deployment. Righ-click on the ground to set a rally point for new vehicles to gather around.

actor-techcenter =
   .description = Grants access to advanced weaponry.
   .name = Technology Download Center
   .encyclopedia = Company policy is that classified technology can not be transported with unguarded colony ships. It has be wired in from an encrypted interstellar connection and stored in a secure facility. Only then the production facilities get access to the blue prints for machinery designed to resolve conflicts fast and efficient. This facilities requires a lot of power to keep the servers running. Powering it down and access to restricted technology is temporarily lost.

actor-oresmelt =
   .name = Ore Smelter
   .encyclopedia = The ore smelter is an economy boosting building for long lasting conflicts. It refines the harvested resource on site, which yields higher profits than raw materials and creates an independent stream of income. However, heating up the ore requires a lot of energy.
   .description = Ore Smelter.
      Only 10 can be built.
    Processes resources for extra credits.

actor-orepurifier =
   .name = Ore Purifier
   .description = Ore Purifier.
      Only 10 can be built.
    Processes resources for extra credits.
   .encyclopedia = The ore purifier is an economy boosting building for long lasting conflicts. It purifiers the harvested resource on site, which yields higher profits than raw materials and creates an independent stream of income. However, cleansing the ore however requires a lot of energy.

actor-bunker =
   .description = Light base defense.
      Requires power to operate.
      Strong vs Pods
      Weak vs Tanks
   .name = Bunker
   .encyclopedia = The bunker houses a single machine gun nest that is effective against lightly armored pods. While being protected against small arms, rockets or tank shells quickly destroy the bunker including its inhabitants. Right-click on the building to force the personnel to leave. Order another pod type inside to change the weaponry.

actor-turret =
   .description = Base defense.
      Requires power to operate.
      Strong vs Tanks, Ships
      Weak vs Pods
   .name = Turret
   .encyclopedia = The turret is a basic defense against armored vehicles. It is ineffective against pods and can't attack air units. With power turned off the turret can't operate making it vulnerable.

actor-turret2 =
   .description = Base defense.
      Requires power to operate.
      Strong vs Vehicles
      Weak vs Pods
   .name = Turret

actor-aaturret =
   .description = Anti-Air base defense.
      Requires power to operate.
      Strong vs Aircrafts
   .name = AA Turret

actor-aaturret2 =
   .description = Anti-Air base defense.
      Requires power to operate.
      Strong vs Aircrafts
   .name = AA Turret
   .encyclopedia = This ground based anti-air defense system quickly downs enemy aircraft. It can also take down ballistic rockets inside its radius of operation. It requires power to operate and can be individually turned off to protect the power grid.

actor-howitzer =
   .description = Advanced base cannon.
      Requires power to operate.
      Maximum 1 can be built
   .name = Grand Howitzer
   .encyclopedia = A large cannon with a range that spans over the whole battlefield. It can fire a barrage of multiple shells at once. The shells can not be intercepted. It takes a while to reload so only the battlefield command is allowed to operate it manually. It softens up the defenses, making a followup strike force even more effective.
   .attackorderpower-name = Artillery Strike
   .attackorderpower-description = Fires a long range barrage across the map

actor-field =
   .description = Errects an electromagnetic barrier
    around a group of units
    for a short time.
      Requires power to operate.
      Maximum 1 can be built.
   .name = Force Field Generator
   .encyclopedia = Force fields are effective at shielding vehicles from all kind of damage. However the effect only last a short amount of time and the force field generator has to recharge which requires a significant amout of energy.
   .force-field-name = Force Field
   .force-field-description = Protective energy shield:
    Reduces damage taken by 75 %
    to a group of units for 30 seconds.

actor-silo =
   .name = Launch Command
   .encyclopedia = The planet side launch command connects to an orbital bomber who can strike anywhere on the battlefield. While the effect is devastating on the battlefield the effectiveness of the weapon is countered by its long reload times which also require power to operate. A well placed strike followed up by an attack squad can oftentimes change the tide of the battle.
   .description = Orders a nuclear strike.
      Requires power to operate.
      Maximum 1 can be built.
      Special Ability: Atom Bomb
   .nukepower-name = Atom Bomb
   .nukepower-description = Drop an atomic bomb
    at a target location.

actor-uplink =
   .name = Orbital Strike Uplink Command
   .encyclopedia = An orbital railgun uses electromagnetic force to launch high-velocity projectiles. It is a more surgical strike than the nuclear bomb and it cannot be evaded. However it damages only the area surrounding the impact. It requires a long time to reload and consumes energy in that process.
   .description = Orders an orbital railgun strike.
      Requires power to operate.
      Maximum 1 can be built.
      Special Ability: Orbital Railgun
   .railgun-name = Orbital Railgun
   .railgun-description = Initiate a surgical strike.
    Applies instant damage to a small area.

actor-storage =
   .description = Stores excess resources.
   .name = Storage
   .encyclopedia = Storage buildings are the center of mining operations. Upon fabrication it spawns a miner that can be sent to a resource deposit and it also allows the construction of additional miners in the factory. Once a storage and a mining tower exists, tankers will automatically deliver resources from mining operations into the storage building where the compensation takes place. With no storage building to drop off available all mining operations will halt.

actor-televator =
   .description = Teleports units.
   Needs to charge up for transports
   outside the network.
      Requires power to operate.
   .name = Televator
   .encyclopedia = Televators can instantly transport any vehicle onto a connected televator. To select the destination in a televator network with more than two endpoints, select and then right-click on the exit. All nodes require power to operate. Televators can be rotated as two building variants exist. They can also charge up for a teleportation of a group of units at any point of the map.
   .teleportpower-name = Teleportation
   .teleportpower-description = Teleports a group of units across the map.

actor-harbor =
   .description = Builds ships.
   .name = Harbor
   .encyclopedia = The harbor is the production site for the navy. It also operates as a repair bay for damaged vessels. It can only be placed on shallow waters.

actor-barrier =
   .description = Basic wall defense.
   .name = Wall
   .encyclopedia = Barriers are very basic defense buildings that block bullets. They don't have to be destroyed in order to conquer an enemy base.

meta-gate =
   .description = Automated barrier that opens for allied units.
   .name = Gate

actor-hgate-encyclopedia = Gates can be placed onto or next to walls which will then automatically connect with them. Gates open for friendly units to not disrupt mining operations on heavily fortified outposts.

actor-reconstructor =
   .name = Nano Reconstructor
   .captured-desc = Provides self-healing to units when powered.
   .capturable-desc = Capture and provide power to enable self-healing for units.
   .encyclopedia = Both machine and human operator can be healed and restored using nano technology. This advanced form of medicine is only available on some planets which also made it affordable due to universal healthcare plans for the local population. Capture a facilty to grant access to your troops. Lasts only as long as the facility is in possession.

actor-extractor =
   .name = Resource Extractor
   .captured-desc = Provides additional resources.
   .capturable-desc = Capture and provide additional resources.
   .encyclopedia = Civilians in the area might also try to harvest the planet's resources. Capture the facility to keep it running but force all the profits onto the company's account.

actor-comlink =
   .name = Communication Link
   .captured-desc = Provides battlefield intelligence.
   .capturable-desc = Capture to gain intelligence support.
   .encyclopedia = On planets with a satellite network civilian communication arrays can be captured and used to spy on your opponent anywhere on the battlefield for as long as the satellite stays in range. It requires power and takes time to reposition again.
   .radar-name = Active Radar
   .radar-description = Reveals the battlefield and cloaked units
    in the vicinity for a short time.

actor-watchtower =
   .name = Watch Tower
   .captured-desc = Provides additional vision.
   .capturable-desc = Move units near to connect.
   .encyclopedia = A larger building equipped with sensors to surveill the area. It does not require an engineer to capture, but only some forces stationed next to it in order to gain access.

actor-dropzone =
   .name = Drop Zone
   .description = Airborne reinforcement pad
   .encyclopedia = Some planets have mercenary forces available. They have their own tank design and have been paid in advance by the company already so you only need to clear and capture their drop in zone which will fly them in one at a time.

actor-flagpost =
   .name = Flagpost
   .encyclopedia = This flag marks an objective that has to be controlled for a certain amount of time in order to gain strategic victory over your opponent.

## Defaults
meta-vehicle-generic-name = Vehicle

meta-scrap =
   .name = Scrap
   .generic-name = Scrap

meta-plane-generic-name = Plane
meta-planehusk-generic-name = Destroyed Plane
meta-droppod-name = Drop Pod
meta-helicopter-generic-name = Helicopter
meta-building-generic-name = Structure
meta-prop-generic-name = Structure

meta-crate =
   .name = Crates
   .generic-name = Crates

meta-rock-name = Rock
meta-tree-name = Tree
meta-cactus-name = Cactus
meta-ship-generic-name = Ship

meta-cube =
   .name = Cube
   .generic-name = Cube

meta-wall-name = Wall

## Editor
actor-mpspawn-name = Multiplayer spawn point
actor-light-name = Invisible Light Source
actor-camera-name = Area Reveal Camera
actor-fountain-name = Water Source

## Mothership
actor-mother2c-name = Mothership
actor-mother-name = Mothership Base

## Pods
actor-rifleman =
   .description = Fast scout vehicle.
    Armed with machine gun.
      Strong vs Pods
      Weak vs Tanks and Buildings
   .name = Rifleman
   .encyclopedia = The most generic pod. It is a small land vehicle controlled by a human. They are the modern solution for infantry, and offer a fast and cheap production. Pods come in different variants, with different armaments and abilities, but the Rifleman one has a simple machine gun equipped. As pods are small vehicles, they can easily hide themselves in woods and dense forests. As it is universal, both Yuruki and Synapol use it.

     {actor-rifleman.description}

     Availability: Universal

actor-rocketeer =
   .description = Fast support vehicle.
    Shoots surface to air/ground rockets.
      Strong vs Tanks and Aircraft
      Weak vs Pods
   .name = Rocketeer
   .encyclopedia = The rocketeer is equipped with a rocket launcher, and is designed to handle pretty much everything, except pods. It's its versatility that makes it a common pod used in every situation.

     {actor-rocketeer.description}

     Availability: Synapol

actor-mortar =
   .description = Fast support vehicle.
    Armed with portable mortar gun.
      Strong vs Pods and Buildings
      Weak vs Tanks
   .name = Mortar
   .encyclopedia = The mortar pod is a fast support vehicle, armed with a mortar gun, particularly powerful against both pods and buildings. It was designed by the Synapol Corporations to handle small outpost, often defended with pods only.

      {actor-mortar.description}

     Availability: Synapol

actor-sniper =
   .description = Long range sniper vehicle.
    Cloaked when idle.
      Strong vs Pods
      Weak vs Tanks and Buildings
   .name = Sniper
   .encyclopedia = The sniper pod is an advanced unit, capable of generating an invisibility cloak on itself. It's armed with a long range sniper, than can take down pods in one shot, with speed and agility.

     {actor-sniper.description}

     Availability: Yuruki

actor-flamer =
   .description = Short range flame thrower.
      Strong vs Pods and Buildings
      Weak vs Tanks
   .name = Flamer
   .encyclopedia = The flamer pod is armed with a powerful short range flame thrower, designed to annihilate pods and buildings. While it is powerful against buildings, it has a lower health than other pods.

     {actor-flamer.description}

     Availability: Synapol

actor-technician =
   .description = Field engineer.
    Infiltrates and captures enemy structures.
    Repairs pods.
      Unarmed
   .name = Technician
   .encyclopedia = The universal field engineer. Its first ability is to infiltrate and capture enemy structures, changing their allegiance: it is often used to capture enemy mining towers. Its second ability is to repair allied pods, on the battle field.

     {actor-technician.description}

     Availability: Universal

actor-broker =
   .description = Financial analyst.
    Invests into stock market for dividends.
    Capable of remotely stealing money
    from bases and storages.
      Unarmed
   .name = Broker
   .encyclopedia = The broker pod is an advanced pod that can invest in the stock market for a constant stream of revenue. It can also tap into an opponent's base or storage and initiate forged wire transfers to remotely steal that enemy's cash.

     {actor-broker.description}

     Availability: Universal

actor-jetpacker =
   .description = Elite airborne vehicle.
    Armed with heavy missiles.
      Strong vs Pods
      Weak vs Vehicles, Navy and Buildings
      Can't attack Aircraft
   .name = Jetpacker
   .encyclopedia = The ultimate Synapol pod: the jetpacker. It was designed to weaken Yuruki pod masses. The fact that it's an airborne pod makes it less vulnerable to other pods.

     {actor-jetpacker.description}

     Availability: Synapol

actor-jetpacker2 =
   .description = Elite airborne vehicle.
    Armed with scatter gun.
      Strong vs Vehicles, Navy
      Weak vs Pods and Buildings
      Can't attack Aircraft
   .name = Jetpacker2
   .encyclopedia = The ultimate Yuruki pod: the jetpacker. It was designed for quick maneuverability and hit-and-run tactics to weaken Synapol's vehicle and navy forces. The fact that it's an airborne pod makes it less vulnerable to other pods.

     {actor-jetpacker2.description}

     Availability: Yuruki

actor-blaster =
   .description = Remote controlled mine.
    Explodes when reaches enemy.
   .name = Blaster
   .encyclopedia = The only pod that isn't controlled by a human: this pod is controlled from the base, and sent to self destruct at enemy positions. It is mainly used to raid against enemy structures, inflicting important damages. It is thought less durable than other pods.

     {actor-blaster.description}

     Availability: Yuruki

actor-shocker =
   .description = Fast support vehicle.
    Shoots surface to air/ground electricity.
      Strong vs Tanks and Aircraft
      Weak vs Pods
   .name = Shocker
   .encyclopedia = The shocker pod is equipped with a small but yet powerful laser zap, and is designed to handle pretty much everything, except pods. It's its versatility that makes it a common pod used in every situation.

     {actor-shocker.description}

     Availability: Yuruki

meta-minipod =
   .name = Civilian
   .generic-name = Civilian
   .encyclopedia = Pods are not only for combat, but also for civilian transport.

     Unarmed

     Availability: Universal/Non-trainable

## Props
actor-lamppost-name = Lamp Post
actor-sparklamp-name = Spark Lamp
actor-streetlamp1-name = Street Lamp
actor-pyramid-name = Pyramid
actor-pyramid2-name = Small Pyramid
actor-obelisk-name = Obelisk
actor-prop1-name = Civilian tower
actor-prop2-name = Civilian tower
actor-prop3-name = Civilian structure
actor-prop4-name = Civilian tower
actor-prop5-name = Civilian structure
actor-prop6-name = Civilian structure
actor-tanktrap-name = Tank Trap
actor-prop9-name = Civilian lightning tower
actor-prop10-name = Civilian plasma tower
actor-prop13-name = Civilian building
actor-prop14-name = Civilian structure
actor-prop16-name = Civilian structure
actor-crane-name = Crane
actor-windturbine-name = Wind turbine
actor-electricpad-name = Electric pad
actor-rail-name = Rail
actor-misc1-name = Misc

## Ships
actor-lightboat =
   .name = Light Boat
   .generic-name = Boat
   .description = Versatile light boat.
      Strong vs Aircrafts, Navy and Pods
      Weak vs Vehicles and Buildings
   .encyclopedia = The light boat is designed for patrol and fast assaults. It's the fastest boat but also the less durable. It has a heavy turreted chaingun install, as well as a flak turret.

     {actor-lightboat.description}

     Availability: Synapol

actor-patrolboat =
   .name = Patrol boat
   .generic-name = Boat
   .description = Versatile light boat.
      Strong vs Aircrafts, Navy and Pods
      Weak vs Vehicles and Buildings
   .encyclopedia = The patrol boat is designed for patrol and fast assaults. It's the fastest boat but also the less durable. It has a heavy turreted chaingun install, as well as a flak turret.

     {actor-patrolboat.description}

     Availability: Yuruki

actor-mercboat =
   .name = Mercenary Boat
   .generic-name = Boat
   .description = A boat with a turret.
      Strong vs Vehicles
      Weak vs Pods
   .encyclopedia = The mercenary boat is a fast boat, armed with a turret. Its turret is powerful against other ships.

     {actor-mercboat.description}

     Availability: Non-trainable

actor-torpedoboat =
   .name = Torpedo Boat
   .generic-name = Boat
   .description = A boat with torpedo launchers.
      Strong vs Water
      Can't attack Ground or Air
   .encyclopedia = The torpedo boat is designed to counter fast ship assaults. It has two torpedo launchers onboard that can take down multiple boats on its own.

     {actor-torpedoboat.description}

     Availability: Synapol

actor-submarine =
   .name = Submarine
   .generic-name = Submarine
   .description = Submarine with powerful torpedoes.
      Strong vs Water
      Can't attack Pods, Buildings, Vehicles or Air
   .encyclopedia = The submarine is a stealth ship, capable of going underwater. It has a torpedo launcher onboard than can take down a large ship in a few strike. It's slower than Yuruki's torpedo boat though.

     {actor-submarine.description}

     Availability: Yuruki

actor-railgunboat =
   .name = Railgun Boat
   .generic-name = Boat
   .description = A heavy boat with a rail gun.
      Strong vs Vehicles, Pods and Buildings
   .encyclopedia = The railgun boat is a heavy ship capable of handling every situation possible. It's armed with a turreted railgun canon, that's particularly powerful against buildings.

     {actor-railgunpod.description}

     Availability: Synapol

actor-lightningboat =
   .name = Lightning Boat
   .generic-name = Boat
   .description = A heavy boat with a lightning gun.
      Strong vs Vehicles, Pods and Buildings
   .encyclopedia = The lightning boat is a heavy ship capable of handling every situation possible. It's armed with a 360 degrees laser zap, that's particularly powerful against buildings.

     {actor-lightningboat.description}

     Availability: Yuruki

actor-boomer =
   .name = Missile Submarine
   .generic-name = Submarine
   .description = A submarine with powerful long range missiles.
      Strong vs Vehicles, Pods and Buildings
   .encyclopedia = This heavy submarine is capable of launching ballistic missiles to a high range. As a submarine, it has the capability of going underwater.

     {actor-boomer.description}

     Availability: Yuruki

actor-slcm-name = Submarine-launched cruise missile

actor-carrier =
   .description = Launches aerial autonomous attack vessels.
      Strong vs Vehicles, Pods and Buildings
   .name = Drone Ship
   .encyclopedia = This heavy ship has a drone launch bay, containing three aerial autonomous drones, that can attack any target at a certain range. When a drone is shot down, it's reproduced. The drones have to reload themselves in the launch bay.

     {actor-carrier.description}

     Availability: Synapol

actor-ferry =
   .name = Light Naval transporter
   .generic-name = Ferry
   .description = General-purpose naval transport.
    Can carry pods.
      Unarmed
   .encyclopedia = The ferry is a heavy naval transporter based on liquid leviation technology that only works on water. It is quite slow but very durable. It can land on shores to load or unload its cargo.

     {actor-ferry.description}

     Availability: Yuruki

actor-ferry2 =
   .name = Heavy Naval Transporter
   .generic-name = Ferry
   .description = General-purpose naval transport.
    Can carry pods.
      Unarmed
   .encyclopedia = The ferry is a heavy naval transporter based on liquid leviation technology that only works on water. It is quite slow but very durable. It can land on shores to load or unload its cargo.

     {actor-ferry2.description}

     Availability: Synapol

actor-mineship =
   .name = Naval Minelayer
   .generic-name = Boat
   .description = Lays smart mines
    that explode on enemy units
    while avoiding ally units.
      Can detect enemy mines.
      Unarmed
   .encyclopedia = The naval minelayer is able to lay smart mines that explode on enemy units, while avoiding ally units. The mines can also be triggered manually. Minelayers can also detect enemy mines at a certain range. It has to reload at an harbor.
     Unarmed

     Availability: Universal/Faction design can vary

## Vehicles
actor-mbt =
   .name = Assault Tank
   .description = Main Battle Tank.
      Strong vs Vehicles
      Weak vs Pods
   .encyclopedia = Assault tanks are a great solution to deal against any other tank. Their turreted canon allow them to move at a decent speed, while still shooting down enemies. They also are decent against buildings, in groups.

     {actor-mbt.description}

     Availability: Universal/Faction design can vary

actor-aatank =
   .description = Mobile tank with AA missiles.
      Strong vs Aircraft
      Cannot attack grounds units.
   .name = Mobile AA
   .encyclopedia = Mobile anti-air rocket-launcher tank. While less powerful than stationary anti-air turrets they can pursue the enemy when it gets out of range.

     {actor-aatank.description}

     Availability: Synapol

actor-aatank2 =
   .description = Mobile tank with lightning gun.
      Strong vs Aircraft
      Cannot attack grounds units.
   .encyclopedia = Tank that bursts electrical arcs at air units. While less powerful than its stationary couterpart, it can pursue the enemy when it gets out of range.

     {actor-aatank2.description}

     Availability: Yuruki

actor-apc =
   .name = Light APC
   .description = Can transport pods.
      Has fireports for garrisoned units.
   .encyclopedia = The Armored Personal Carriers (APC) is a durable transport tank. It is used to carry pods, giving them better protection, while still being able to fire through firing ports. It was designed to conduct assault missions, when you don't have enough resources to produce tanks.

     Availability: Yuruki

actor-apc2 =
   .name = Heavy APC
   .description = Can transport pods.
      Has fireports for garrisoned units.
   .encyclopedia = The Armored Personal Carriers (APC) is a durable transport tank. It is used to carry pods, giving them better protection, while still being able to fire through firing ports. It was designed to conduct assault missions, when you don't have enough resources to produce tanks.

     Availability: Synapol

actor-artillery =
   .name = Artillery
   .description = Mobile long range weapon.
      Strong vs Pods and Buildings
      Weak vs Tanks
   .encyclopedia = Armed with a long range artillery canon, this mobile artillery vehicle is extremely powerful against pods and buildings, while still being decently strong against other units. It was designed to power down enemies defenses while other units were assaulting a base.

     {actor-artillery.description}

     Availability: Universal

actor-radartank =
   .description = Can detect cloaked units
    Range extends when deployed.
      Unarmed
   .name = Reconnaissance Tank
   .encyclopedia = The radar tank is an universal utility and support unit, that provides reconnaissance. It can also detect cloaked units in a certain range. Its range can be extended by deploying it, though it won't be able to move when deployed.

     Unarmed

     Availability: Universal

actor-repairtank =
   .name = Mobile Repair Vehicle
   .generic-name = Tank
   .description = Repairs nearby tanks.
      Unarmed
   .encyclopedia = The repair tank is a fast field engineer vehicle, that can repair tanks using a special beam.

     Unarmed

     Availability: Universal

actor-minelayer =
   .description = Lays smart mines
    that explode on enemy units
    while avoiding ally units.  Can detect enemy mines.
      Unarmed
   .name = Minelayer
   .encyclopedia = The minelayer is a fast and agile support unit, that can lay smart mines, that explode on enemy units; while avoiding ally units. They can also be triggered manually. It also has the ability to detect enemy mines at a certain range. It has to reload on trade platforms.

     Unarmed

     Availability: Universal

actor-railguntank =
   .name = Railgun Tank
   .generic-name = Tank
   .description = A powerful tank which shoots laser.
      Strong vs Pods, Vehicles and Buildings
   .encyclopedia = The railgun tank is the ultimate tank, able of handling every type of unit. It's armed with a powerful laser canon, and has a heavy armor.

     {actor-railguntank.description}

     Availability: Synapol

actor-lightningtank =
   .description = Fires electric discharges.
      Strong vs Pods, Vehicles and Buildings
   .name = Lightning Tank
   .generic-name = Tank
   .encyclopedia = The lightning tank is the ultimate tank, able of handling every type of unit. It's armed with a powerful laser zap, and has a heavy armor.

     {actor-lightningtank.description}

     Availability: Yuruki

actor-stealthtank =
   .description = Cloaked missile tank.
      Strong vs Vehicles and Buildings
      Weak vs Pods
   .name = Stealth Tank
   .encyclopedia = The stealth tank is a modern tank, able of deploying a cloak on itself. It is armed with a powerful missile launcher, extremely powerful against buildings. It is yet less versatile than the lightning tank, it is stronger against buildings.

     {actor-stealthtank.description}

     Availability: Yuruki

actor-merctank =
   .name = Mercenary Tank
   .generic-name = Tank
   .description = Main battle tank.
      Strong vs Vehicles
      Weak vs Pods
   .encyclopedia = The mercenary tank is a generic tank, that can be only obtained through the drop zone buildings. Its canon cannot rotate like other battles tanks though.

     {actor-merctank.description}

     Availability: Non-trainable

actor-dualmerctank =
   .name = Dual Mercenary Tank
   .generic-name = Tank
   .description = Double barreled tank.
      Strong vs Vehicles
      Weak vs Pods
   .encyclopedia = Another variant of mercenary tank that can be only obtained through the drop zone buildings. Like the single barrelled variant its canon cannot rotate.

     {actor-dualmerctank.description}

     Availability: Non-trainable

actor-ecmtank =
   .name = Countermeasure Tank
   .generic-name = ECM Tank
   .description = Disables units for a brief moment.
      Jams incoming missiles.
   .encyclopedia = The electronic countermeasure (ECM) tank can overload enemy weaponry systems with a controlled beam that disables all electronic system units for a brief moment, disabling them completely. The effect only lasts a few seconds. Its other ability is to deflect incoming enemy missiles.

     Unarmed

     Availability: Yuruki

actor-dualartillery =
   .name = Dual Artillery
   .description = Double barreled artillery tank.
      Strong vs Pods, Vehicles and Buildings
   .encyclopedia = This vehicle is an upgrade of the artillery tank. It's twice bigger, has more range, and fires two bombshells at once. Its range is extremely higher, and it's damaging against any type of armor. Due to intellectual property disputes, this design is rarely seen on the battlefield.

     {actor-dualartillery.description}

     Availability: Non-trainable

actor-builder =
   .description = Deploys into outpost which
    grants additional build radius
    and allows base reconstruction.
      Unarmed
   .name = Builder
   .encyclopedia = The builder is an important support unit, allowing the deployment of outpost which grants additional build radius, and allow base reconstruction.

     Unarmed

     Availability: Universal/Faction design can vary

actor-collector-name = Collector

actor-miner =
   .description = Builds mining facilities.
      Unarmed
   .name = Miner
   .encyclopedia = The role of this vehicle is to deploy mining facilities, allowing the production of resources, that are then transformed into cash at the storage building.

     Unarmed

     Availability: Universal

actor-missiletank =
   .name = Missile Tank
   .generic-name = Tank
   .description = A tank which shoots missiles.
      Strong vs Vehicles, Aircraft, Navy and Buildings
      Weak vs Pods
   .encyclopedia = The missile tank is slow but powerful, able to fire from a turret. It is armed with a powerful missile launcher, good against buildings, vehicles and air units.

     {actor-missiletank.description}

     Availability: Synapol

actor-hackertank =
   .description = Temporarily changes allegiance of targeted units.
      Disrupts satellite video links
      and obscures the battlefield when deployed.
   .name = Hacker Tank
   .encyclopedia = The hacker tank contains equipment to penetrate firewalls of individual units and interfer with their command and control software so they will accept orders from the enemy and engage in friendly fire. The effective change of allegiance is permanent, until the hacker tank gets destroyed or changes the target. It can only have one controlled unit at a time. Its other ability is to disrupt satellite video links, blocking field detection in the radar.

     Unarmed

     Availability: Synapol

actor-buggy =
   .name = Ramp Buggy
   .generic-name = Buggy
   .description = Fires a machine gun.
      Strong vs Pods
      Weak vs Tanks, Buildings
   .encyclopedia = The ramp buggy is a fast anti-pod vehicle, that has a turret heavy machine gun, capable of destroying a group of pods by itself. It is Synapol's solution to the Yuruki gatling bike: while it is slower, its machine gun is turreted, giving it an advantage.

     {actor-buggy.description}

     Availability: Synapol

actor-bike =
   .name = Gatling Bike
   .generic-name = Bike
   .description = Fires a machine gun.
      Strong vs Pods
      Weak vs Tanks, Buildings
   .encyclopedia = The gatling bike is a fast anti-pod vehicle, that has a powerful heavy machine gun, capable of destroying a group of pods by itself.

     {actor-bike.description}

     Availability: Yuruki

actor-tanker1 =
   .description = Transports resources to the headquarter.
      Unarmed
   .name = Loaded Resource Transporter

actor-tanker2 =
   .description = Collects resources at mining towers.
      Unarmed
   .name = Empty Resource Transporter
   .encyclopedia = The tanker is a resource transporter, that moves from mining towers to storage building to collect and depose resources.

     Unarmed

     Availability: Universal

actor-cvit =
   .description = Moves cash to other players.
      Unarmed
   .name = Money Transport
   .encyclopedia = The money transport moves cash to other players, in order to help them financially.

     Unarmed

     Availability: Universal

actor-mothership =
   .name = Mothership
   .description = Launches aerial autonomous attack vessels.
      Only one can be built.
      Strong vs Everything
   .encyclopedia = When the Yuruki colony on Mars started their battleship design program, the Synapol Corporations had to design themselves a massive air unit to counter the battleship. The mothership was their solution. It's a real floating city: it regroups a crew of 2,500 men, and has many different quarters, some being science labs, other armament testing... It is armed with four powerful plasma turreted canon that can target ground structures, and with a drone bay, containing five of them. When a drone is destroyed, another one is immediately reproduced.

     {actor-mothership.description}

     Availability: Synapol

actor-mothership-husk =
   .name = Mothership Husk

actor-drone2 =
   .name = Mothership Drone

actor-battleship =
   .name = Battleship
   .description = Aerial artillery bombardment.
      Only one can be built.
      Strong vs Ground units
      Can't target Aircraft
   .encyclopedia = The battleship is a massive air unit, regrouping a crew of 1,200 men. Its design was launched by the Yuruki colony on Mars a few 15 years ago, and took 10 years to design, test and create. It is no more a prototype and is now used by the Yuruki paramilitary forces. It is armed with 2 synchronized artillery turrets, that launches aerial bombardment, destroying in a few seconds a whole outpost.

     {actor-battleship.description}

     Availability: Yuruki

actor-battleship-husk =
   .name = Battleship Husk

actor-watertank =
   .name = Firefighting Tank
   .generic-name = Tank

actor-firetruck =
   .name = Firefighting Truck
   .generic-name = Truck

actor-firefighter =
   .name = Firefighter Pod

## Weapons
actor-landmine-name = AI Mine
actor-watermine-name = AI Water Mine

robbed-notification = Money stolen.
rob-notification = Awarded stolen money.
