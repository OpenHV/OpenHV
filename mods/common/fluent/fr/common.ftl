button-cancel = Annuler
button-retry = Réessayer
button-back = Retour
button-continue = Continuer
button-quit = Quitter
notification-custom-rules = Cette carte contient des règles personnalisées. L'expérience de jeu peut changer.
notification-map-bots-disabled = Les bots ont été désactivés sur cette carte.
notification-two-humans-required = Ce serveur nécessite au moins deux joueurs humains pour démarrer une partie.
notification-unknown-server-command = Commande serveur inconnue : { $command }
notification-admin-start-game = Seul l'hôte peut démarrer la partie.
notification-no-start-until-required-slots-full = Impossible de démarrer la partie tant que les emplacements requis ne sont pas remplis.
notification-no-start-without-players = Impossible de démarrer la partie sans joueurs.
notification-insufficient-enabled-spawn-points = Impossible de démarrer la partie tant que plus de points d'apparition ne sont pas activés.
notification-malformed-command = Commande { $command } mal formée
notification-state-unchanged-ready = Impossible de changer l'état quand marqué comme prêt.
notification-invalid-faction-selected = Faction sélectionnée invalide : { $faction }
notification-state-unchanged-game-started = Impossible de changer l'état quand la partie a commencé. ({ $command })
notification-requires-host = Seul l'hôte peut faire cela.
notification-invalid-bot-slot = Impossible d'ajouter des bots dans un emplacement occupé par un autre client.
notification-invalid-bot-type = Type de bot invalide.
notification-admin-change-map = Seul l'hôte peut changer la carte.
notification-player-disconnected = { $player } s'est déconnecté.
notification-team-player-disconnected = { $player } (Équipe { $team }) s'est déconnecté.
notification-observer-disconnected = { $player } (Spectateur) s'est déconnecté.
notification-unknown-map = La carte n'a pas été trouvée sur le serveur.
notification-searching-map = Recherche de la carte sur le Centre de Ressources...
notification-admin-change-configuration = Seul l'hôte peut changer la configuration.
notification-changed-map = { $player } a changé la carte pour { $map }
notification-you-were-kicked = Vous avez été expulsé du serveur.
notification-admin-kicked = { $admin } a expulsé { $player } du serveur.
notification-kicked = { $player } a été expulsé du serveur.
notification-temp-ban = { $admin } a temporairement banni { $player } du serveur.
notification-admin-transfer-admin = Seuls les admins peuvent transférer les droits admin à un autre joueur.
notification-admin-move-spectators = Seul l'hôte peut déplacer les joueurs vers les spectateurs.
notification-empty-slot = Personne dans cet emplacement.
notification-move-spectators = { $admin } a déplacé { $player } vers les spectateurs.
notification-nick-changed = { $player } est maintenant connu sous le nom de { $name }.
notification-player-dropped = Un joueur a été déconnecté après expiration du délai.
notification-connection-problems = { $player } rencontre des problèmes de connexion.
notification-timeout-dropped = { $player } a été déconnecté après expiration du délai.
notification-timeout-dropped-in =
    { $timeout ->
        [one] { $player } sera déconnecté dans { $timeout } seconde.
       *[other] { $player } sera déconnecté dans { $timeout } secondes.
    }
notification-error-game-started = La partie a déjà commencé.
notification-requires-password = Le serveur nécessite un mot de passe.
notification-incorrect-password = Mot de passe incorrect.
notification-incompatible-mod = Le serveur utilise un mod incompatible.
notification-incompatible-version = Le serveur utilise une version incompatible.
notification-incompatible-protocol = Le serveur utilise un protocole incompatible.
notification-you-were-banned = Vous avez été banni du serveur.
notification-you-were-temp-banned = Vous avez été temporairement banni du serveur.
notification-game-full = La partie est complète.
notification-new-admin = { $player } est maintenant l'admin.
notification-invalid-configuration-command = Commande de configuration invalide.
notification-admin-option = Seul l'hôte peut définir cette option.
notification-error-number-teams = Le nombre d'équipes n'a pas pu être analysé : { $raw }
notification-admin-kick = Seul l'hôte peut expulser des joueurs.
notification-kick-self = L'hôte n'est pas autorisé à s'expulser lui-même.
notification-kick-none = Personne dans cet emplacement.
notification-no-kick-game-started = Seuls les spectateurs et les joueurs vaincus peuvent être expulsés après le début de la partie.
notification-admin-clear-spawn = Seuls les admins peuvent effacer les points d'apparition.
notification-spawn-occupied = Vous ne pouvez pas occuper le même point d'apparition qu'un autre joueur.
notification-spawn-locked = Le point d'apparition est verrouillé pour un autre emplacement de joueur.
notification-admin-lobby-info = Seul l'hôte peut définir les infos du lobby.
notification-invalid-lobby-info = Infos de lobby invalides envoyées.
notification-player-color-terrain = La couleur a été ajustée pour être moins similaire au terrain.
notification-player-color-player = La couleur a été ajustée pour être moins similaire à un autre joueur.
notification-invalid-player-color = Impossible de déterminer une couleur de joueur valide. Une couleur aléatoire a été sélectionnée.
notification-invalid-error-code = Échec de l'analyse du message d'erreur.
notification-master-server-connected = Communication avec le serveur maître établie.
notification-master-server-error = "Échec de la communication avec le serveur maître."
notification-game-offline = La partie n'a pas été annoncée en ligne.
notification-no-port-forward = Le port du serveur n'est pas accessible depuis Internet.
notification-blacklisted-server-name = Le nom du serveur contient un mot interdit.
notification-requires-authentication = Le serveur nécessite que les joueurs aient un compte forum OpenRA.
notification-no-permission-to-join = Vous n'avez pas la permission de rejoindre ce serveur.
notification-slot-closed = Votre emplacement a été fermé par l'hôte.
notification-joined = { $player } a rejoint la partie.
notification-lobby-disconnected = { $player } est parti.
notification-game-has-started = La partie a commencé.
notification-game-saved = Partie sauvegardée.
notification-game-paused = La partie est mise en pause par { $player }
notification-game-unpaused = La partie est reprise par { $player }
notification-game-started = Partie démarrée
notification-chat-temp-disabled =
    { $remaining ->
        [one] Le chat est désactivé. Veuillez réessayer dans { $remaining } seconde.
       *[other] Le chat est désactivé. Veuillez réessayer dans { $remaining } secondes.
    }
notification-unable-to-start-a-vote = Impossible de lancer un vote.
notification-insufficient-votes-to-kick = Votes insuffisants pour expulser le joueur { $kickee }.
notification-kick-already-voted = Vous avez déjà voté.
notification-vote-kick-started = Le joueur { $kicker } a lancé un vote pour expulser le joueur { $kickee }.
notification-vote-kick-in-progress = { $percentage }% des joueurs ont voté pour expulser le joueur { $kickee }.
notification-vote-kick-ended = Le vote pour expulser le joueur { $kickee } a échoué.
label-duplicate-actor-id = ID d'acteur en double
label-actor-id = Entrez un ID d'acteur
label-actor-owner = Propriétaire
label-actor-type = Type : { $actorType }
options-common-selector =
    .search-results = Résultats de recherche
    .all = Tous
    .multiple = Plusieurs
    .none = Aucun
label-unpacked-map = décompressé
dialog-save-map-failed =
    .title = Échec de la sauvegarde de la carte
    .prompt = Voir debug.log pour plus de détails.
    .confirm = OK
dialog-overwrite-map-failed =
    .title = Avertissement
    .prompt =
        En sauvegardant, vous écraserez
        une carte déjà existante.
    .confirm = Sauvegarder
dialog-overwrite-map-outside-edit =
    .title = Avertissement
    .prompt =
        La carte a été modifiée en dehors de l'éditeur.
        En sauvegardant, vous pourriez écraser des progrès
    .confirm = Sauvegarder
notification-save-current-map = Carte actuelle sauvegardée.
menu-game-info =
    .objectives = Objectifs
    .briefing = Briefing
    .options = Options
    .debug = Débogage
    .chat = Tchat
label-mission-in-progress = En cours
label-mission-accomplished = Accompli
label-mission-failed = Échoué
label-mute-player = Rendre ce joueur muet
label-unmute-player = Réactiver le son de ce joueur
button-kick-player = Expulser ce joueur
button-vote-kick-player = Voter pour expulser ce joueur
dialog-kick =
    .title = Expulser { $player } ?
    .prompt = Ce joueur ne pourra pas rejoindre la partie.
    .confirm = Expulser
dialog-vote-kick =
    .title = Voter pour expulser { $player } ?
    .prompt = Ce joueur ne pourra pas rejoindre la partie.
    .prompt-break-bots =
        { $bots ->
            [one] Expulser l'admin de la partie expulsera aussi 1 bot.
           *[other] Expulser l'admin de la partie expulsera aussi { $bots } bots.
        }
    .vote-start = Lancer le vote
    .vote-for = Voter pour
    .vote-against = Voter contre
    .vote-cancel = S'abstenir
notification-vote-kick-disabled = Le vote d'expulsion est désactivé sur ce serveur.
label-paused = En pause
label-max-speed = Vitesse max
label-replay-speed = Vitesse { $percentage }%
label-replay-complete = { $percentage }% terminé
label-chat-disabled = Chat désactivé
label-chat-availability =
    { $seconds ->
        [one] Chat disponible dans { $seconds } seconde...
       *[other] Chat disponible dans { $seconds } secondes...
    }
menu-ingame =
    .leave = Quitter
    .abort = Abandonner la mission
    .restart = Redémarrer
    .surrender = Se rendre
    .load-game = Charger une partie
    .save-game = Sauvegarder la partie
    .music = Musique
    .settings = Paramètres
    .return-to-map = Retourner à la carte
    .resume = Reprendre
    .save-map = Sauvegarder la carte
    .exit-map = Quitter l'éditeur de carte
dialog-leave-mission =
    .title = Quitter la mission
    .prompt = Quitter cette partie et retourner au menu ?
    .confirm = Quitter
    .cancel = Rester
dialog-restart-mission =
    .title = Redémarrer
    .prompt = Êtes-vous sûr de vouloir redémarrer ?
    .confirm = Redémarrer
    .cancel = Rester
dialog-surrender =
    .title = Se rendre
    .prompt = Êtes-vous sûr de vouloir vous rendre ?
    .confirm = Se rendre
    .cancel = Rester
dialog-error-max-player =
    .title = Erreur : Nombre maximum de joueurs dépassé
    .prompt = Il y a trop de joueurs définis ({ $players }/{ $max }).
    .confirm = Retour
dialog-exit-map-editor =
    .title = Quitter l'éditeur de carte
    .prompt-unsaved = Quitter et perdre toutes les modifications non sauvegardées ?
    .prompt-deleted = La carte a peut-être été supprimée en dehors de l'éditeur.
    .confirm-anyway = Quitter quand même
    .confirm = Quitter
dialog-play-map-warning =
    .title = Avertissement
    .prompt =
        La carte a peut-être été supprimée ou contient
        des erreurs empêchant son chargement.
    .cancel = OK
dialog-exit-to-map-editor =
    .title = Quitter la mission
    .prompt = Quitter cette partie et retourner à l'éditeur ?
    .confirm = Retour à l'éditeur
    .cancel = Rester
label-power-usage = Consommation d'énergie : { $usage }/{ $capacity }
label-infinite-power = Infini
label-silo-usage = Utilisation du silo : { $usage }/{ $capacity }
options-shroud-selector =
    .all-players = Tous les joueurs
    .disable-shroud = Désactiver le brouillard
    .other = Autre
options-observer-stats =
    .none = Informations : Aucune
    .basic = Basiques
    .economy = Économie
    .production = Production
    .support-powers = Pouvoirs de soutien
    .combat = Combat
    .army = Armée
    .earnings-graph = Revenus (graphique)
    .army-graph = Armée (graphique)
label-unrevealed-terrain = Terrain non révélé
dialog-kick-client =
    .prompt = Expulser { $player } ?
dialog-kick-spectators =
    .prompt =
        { $count ->
            [one] Êtes-vous sûr de vouloir expulser un spectateur ?
           *[other] Êtes-vous sûr de vouloir expulser { $count } spectateurs ?
        }
options-slot-admin =
    .add-bots = Ajouter
    .remove-bots = Supprimer
    .configure-bots = Configurer les bots
    .teams-count = { $count } Équipes
    .humans-vs-bots = Humains contre Bots
    .free-for-all = Chacun pour soi
    .configure-teams = Configurer les équipes
button-general-chat = Tous
button-team-chat = Équipe
label-not-available = Non disponible
options-lobby-slot =
    .slot = Emplacement
    .open = Ouvert
    .closed = Fermé
    .bots = Bots
    .bots-disabled = Bots désactivés
label-connecting = Connexion...
label-downloading-map = Téléchargement { $size } ko
label-downloading-map-progress = Téléchargement { $size } ko ({ $progress }%)
button-retry-install = Réessayer l'installation
button-retry-search = Réessayer la recherche
label-created-by = Créée par { $author }
label-disabled-spawn = Point d'apparition désactivé
label-available-spawn = Point d'apparition disponible
options-camera =
    .close = Proche
    .medium = Moyenne
    .far = Loin
    .furthest = Très loin
options-display-mode =
    .windowed = Fenêtré
    .legacy-fullscreen = Plein écran (ancien)
    .fullscreen = Plein écran
label-video-display-index = Écran { $number }
options-status-bars =
    .standard = Standard
    .show-on-damage = Afficher si endommagé
    .always-show = Toujours afficher
options-target-lines =
    .automatic = Automatique
    .manual = Manuel
    .disabled = Désactivé
checkbox-frame-limiter = Activer le limiteur d'images ({ $fps } IPS)
label-original-notice = La valeur par défaut est « { $key } »
label-duplicate-notice = Déjà utilisé pour « { $key } » dans le contexte { $context }
options-mouse-scroll-type =
    .disabled = Désactivé
    .standard = Standard
    .inverted = Inversé
    .joystick = Joystick
options-control-scheme =
    .classic = Classique
    .modern = Moderne
dialog-settings-save =
    .title = Redémarrage requis
    .prompt =
        Certaines modifications ne seront appliquées
        qu'après le redémarrage du jeu.
    .cancel = Continuer
dialog-settings-restart =
    .title = Redémarrer maintenant ?
    .prompt =
        Certaines modifications ne seront appliquées
        qu'après le redémarrage du jeu. Redémarrer maintenant ?
    .confirm = Redémarrer maintenant
    .cancel = Redémarrer plus tard
dialog-settings-reset =
    .title = Réinitialiser { $panel }
    .prompt =
        Êtes-vous sûr de vouloir réinitialiser
        tous les paramètres de ce panneau ?
    .confirm = Réinitialiser
    .cancel = Annuler
label-all-packages = Tous les paquets
label-length-in-seconds = { $length } sec
label-connecting-to-endpoint = Connexion à { $endpoint }...
label-could-not-connect-to-target = Impossible de se connecter à { $target }
label-unknown-error = Erreur inconnue
label-password-required = Mot de passe requis
label-connection-failed = Échec de la connexion
notification-mod-switch-failed = Échec du changement de mod.
dialog-rename-save =
    .title = Renommer la sauvegarde
    .prompt = Entrez un nouveau nom de fichier :
    .confirm = Renommer
dialog-delete-save =
    .title = Supprimer la sauvegarde sélectionnée ?
    .prompt = Supprimer '{ $save }'
    .confirm = Supprimer
dialog-delete-all-saves =
    .title = Supprimer toutes les sauvegardes ?
    .prompt =
        { $count ->
            [one] Supprimer { $count } sauvegarde.
           *[other] Supprimer { $count } sauvegardes.
        }
    .confirm = Tout supprimer
notification-save-deletion-failed = Échec de la suppression du fichier '{ $savePath }'. Consultez les journaux pour plus de détails.
dialog-overwrite-save =
    .title = Écraser la sauvegarde ?
    .prompt = Écraser { $file } ?
    .confirm = Écraser
label-loading-news = Chargement des actualités
label-news-retrieval-failed = Échec de la récupération des actualités : { $message }
label-news-parsing-failed = Échec de l'analyse des actualités : { $message }
label-author-datetime = par { $author } le { $datetime }
label-all-maps = Toutes les cartes
label-no-matches = Aucun résultat
label-player-count =
    { $players ->
        [one] { $players } Joueur
       *[other] { $players } Joueurs
    }
label-map-size-huge = (Immense)
label-map-size-large = (Grande)
label-map-size-medium = (Moyenne)
label-map-size-small = (Petite)
label-map-searching-count =
    { $count ->
        [one] Recherche de { $count } carte sur le Centre de Ressources OpenRA...
       *[other] Recherche de { $count } cartes sur le Centre de Ressources OpenRA...
    }
label-map-unavailable-count =
    { $count ->
        [one] { $count } carte n'a pas été trouvée sur le Centre de Ressources OpenRA
       *[other] { $count } cartes n'ont pas été trouvées sur le Centre de Ressources OpenRA
    }
notification-map-deletion-failed = Échec de la suppression de la carte '{ $map }'. Consultez le fichier debug.log pour plus de détails.
dialog-delete-map =
    .title = Supprimer la carte
    .prompt = Supprimer la carte '{ $title }' ?
    .confirm = Supprimer
dialog-delete-all-maps =
    .title = Supprimer les cartes
    .prompt = Supprimer toutes les cartes de cette page ?
    .confirm = Supprimer
options-order-maps =
    .player-count = Joueurs
    .title = Titre
    .date = Date
    .size = Taille
dialog-no-video =
    .title = Vidéo non installée
    .prompt =
        Les vidéos du jeu peuvent être installées depuis le
        menu « Gérer le contenu » dans le sélecteur de mod.
    .cancel = Retour
dialog-cant-play-video =
    .title = Impossible de lire la vidéo
    .prompt = Une erreur s'est produite lors de la lecture de la vidéo.
    .cancel = Retour
label-sound-muted = L'audio a été désactivé dans les paramètres.
label-no-song-playing = Aucune musique en cours
label-audio-muted = Audio désactivé.
label-audio-unmuted = Audio réactivé.
label-loading-player-profile = Chargement du profil joueur...
label-loading-player-profile-failed = Échec du chargement du profil joueur.
label-requires = Nécessite { $prequisites }
label-duration = Durée : { $time }
options-replay-type =
    .singleplayer = Solo
    .multiplayer = Multijoueur
options-winstate =
    .victory = Victoire
    .defeat = Défaite
options-replay-date =
    .today = Aujourd'hui
    .last-week = 7 derniers jours
    .last-fortnight = 14 derniers jours
    .last-month = 30 derniers jours
options-replay-duration =
    .very-short = Moins de 5 min
    .short = Courte (10 min)
    .medium = Moyenne (30 min)
    .long = Longue (60+ min)
dialog-rename-replay =
    .title = Renommer le replay
    .prompt = Entrez un nouveau nom de fichier :
    .confirm = Renommer
dialog-delete-replay =
    .title = Supprimer le replay sélectionné ?
    .prompt = Supprimer le replay { $replay } ?
    .confirm = Supprimer
dialog-delete-all-replays =
    .title = Supprimer tous les replays sélectionnés ?
    .prompt =
        { $count ->
            [one] Supprimer { $count } replay.
           *[other] Supprimer { $count } replays.
        }
    .confirm = Tout supprimer
notification-replay-deletion-failed = Échec de la suppression du fichier replay '{ $file }'. Consultez le fichier debug.log pour plus de détails.
-incompatible-replay-recorded = Il a été enregistré avec
dialog-incompatible-replay =
    .title = Replay incompatible
    .prompt = Les métadonnées du replay n'ont pas pu être lues.
    .confirm = OK
    .prompt-unknown-version = { -incompatible-replay-recorded } une version inconnue.
    .prompt-unknown-mod = { -incompatible-replay-recorded } un mod inconnu.
    .prompt-unavailable-mod = { -incompatible-replay-recorded } un mod indisponible : { $mod }.
    .prompt-incompatible-version =
        { -incompatible-replay-recorded } une version incompatible :
        { $version }.
    .prompt-unavailable-map =
        { -incompatible-replay-recorded } une carte indisponible :
        { $map }.
nothing-selected = Rien de sélectionné.
selected-units-across-screen =
    { $units ->
        [one] Une unité sélectionnée à l'écran.
       *[other] { $units } unités sélectionnées à l'écran.
    }
selected-units-across-map =
    { $units ->
        [one] Une unité sélectionnée sur la carte.
       *[other] { $units } unités sélectionnées sur la carte.
    }
label-internet-server-nat-A = Serveur Internet (UPnP/NAT-PMP
label-internet-server-nat-B-enabled = Activé
label-internet-server-nat-B-not-supported = Non supporté
label-internet-server-nat-B-disabled = Désactivé
label-internet-server-nat-C = ) :
label-local-server = Serveur local :
dialog-server-creation-failed =
    .prompt = Impossible d'écouter sur le port { $port }
    .prompt-port-used = Vérifiez si le port est déjà utilisé.
    .prompt-error = Erreur : « { $message } » ({ $code })
    .title = Échec de la création du serveur
    .cancel = Retour
label-players-online-count =
    { $players ->
        [one] { $players } joueur en ligne
       *[other] { $players } joueurs en ligne
    }
label-search-status-failed = Échec de la requête de la liste des serveurs.
label-search-status-no-games = Aucune partie trouvée. Essayez de modifier les filtres.
label-no-server-selected = Aucun serveur sélectionné
label-map-status-searching = Recherche...
label-map-classification-unknown = Carte inconnue
label-players-count =
    { $players ->
        [0] Aucun joueur
        [one] Un joueur
       *[other] { $players } joueurs
    }
label-bots-count =
    { $bots ->
        [0] Aucun bot
        [one] Un bot
       *[other] { $bots } bots
    }
label-players = Joueurs
label-spectators = Spectateurs
label-spectators-count =
    { $spectators ->
        [0] Aucun spectateur
        [one] Un spectateur
       *[other] { $spectators } spectateurs
    }
label-team-name = Équipe { $team }
label-no-team = Sans équipe
label-playing = En cours
label-waiting = En attente
label-other-players-count =
    { $players ->
        [one] Un autre joueur
       *[other] { $players } autres joueurs
    }
label-in-progress-for =
    { $minutes ->
        [0] En cours depuis moins d'une minute.
        [one] En cours depuis { $minutes } minute.
       *[other] En cours depuis { $minutes } minutes.
    }
label-password-protected = Protégé par mot de passe
label-waiting-for-players = En attente de joueurs
label-server-shutting-down = Arrêt du serveur
label-unknown-server-state = État du serveur inconnu
notification-saved-screenshot = Capture d'écran enregistrée { $filename }
notification-invalid-command = { $name } n'est pas une commande valide.
description-combat-geometry = active/désactive l'affichage de la géométrie de combat.
description-render-geometry = active/désactive l'affichage de la géométrie de rendu.
description-screen-map-overlay = active/désactive l'affichage de la carte écran.
description-depth-buffer = active/désactive l'affichage du tampon de profondeur.
description-actor-tags-overlay = active/désactive l'affichage des tags d'acteurs.
notification-cheats-disabled = Les codes de triche sont désactivés.
notification-invalid-cash-amount = Montant d'argent invalide.
description-toggle-visibility = active/désactive les vérifications de visibilité et la minicarte.
description-give-cash = donne le montant d'argent par défaut ou spécifié.
description-give-cash-all = donne le montant d'argent par défaut ou spécifié à tous les joueurs et IA.
description-instant-building = active/désactive la construction instantanée.
description-build-anywhere = active/désactive la possibilité de construire partout.
description-unlimited-power = active/désactive l'énergie infinie.
description-enable-tech = active/désactive la possibilité de tout construire.
description-fast-charge = active/désactive le chargement quasi instantané des pouvoirs de soutien.
description-dev-cheat-all = active/désactive tous les codes de triche et vous donne de l'argent.
description-dev-crash = fait planter le jeu.
description-levelup-actor = ajoute un nombre spécifié de niveaux aux acteurs sélectionnés.
description-player-experience = ajoute une quantité spécifiée d'expérience aux propriétaires des acteurs sélectionnés.
description-power-outage = provoque une panne de courant de 5 secondes chez les propriétaires des acteurs sélectionnés.
description-kill-selected-actors = tue les acteurs sélectionnés.
description-dispose-selected-actors = supprime les acteurs sélectionnés.
notification-available-commands = Voici les commandes disponibles :
description-no-description = aucune description disponible.
description-help-description = fournit des informations utiles sur diverses commandes
description-pause-description = met en pause ou reprend la partie
description-surrender-description = autodétruit tout et fait perdre la partie
notification-cheat-used = Code de triche utilisé : { $cheat } par { $player }{ $suffix }
description-custom-terrain-debug-overlay = active/désactive l'affichage de débogage du terrain personnalisé.
description-cell-triggers-overlay = active/désactive l'affichage des déclencheurs de script.
description-hpf-debug-overlay = active/désactive l'affichage du pathfinder hiérarchique.
description-path-debug-overlay = active/désactive la visualisation de la recherche de chemin.
description-terrain-geometry-overlay = active/désactive l'affichage de la géométrie du terrain.
options-game-speed =
    .slowest = Très lent
    .slower = Lent
    .normal = Normal
    .fast = Rapide
    .faster = Très rapide
    .fastest = Ultra rapide
options-time-limit =
    .no-limit = Sans limite
    .options =
        { $minutes ->
            [une] { $minutes } minute
           *[autre] { $minutes } minutes
        }
notification-time-limit-expired = Le temps imparti est écoulé.
notification-added-actor = Ajouté { $name } ({ $id })
notification-copied-tiles =
    { $amount ->
        [one] Une tuile copiée
       *[other] { $amount } tuiles copiées
    }
notification-selected-area = Zone sélectionnée { $x },{ $y } ({ $width },{ $height })
notification-selected-actor = Acteur sélectionné { $id }
notification-cleared-selection = Sélection effacée
notification-removed-actor = Supprimé { $name } ({ $id })
notification-removed-resource = Supprimé { $type }
notification-moved-actor = Déplacé { $id } de { $x1 },{ $y1 } vers { $x2 },{ $y2 }
notification-added-resource =
    { $amount ->
        [one] Une cellule de { $type } ajoutée
       *[other] { $amount } cellules de { $type } ajoutées
    }
notification-added-tile = Tuile { $id } ajoutée
notification-filled-tile = Rempli avec la tuile { $id }
notification-added-marker-tiles =
    { $amount ->
        [one] Une tuile marqueur de type { $type } ajoutée
       *[other] { $amount } tuiles marqueurs de type { $type } ajoutées
    }
notification-removed-marker-tiles =
    { $amount ->
        [one] Une tuile marqueur supprimée
       *[other] { $amount } tuiles marqueurs supprimées
    }
notification-cleared-selected-marker-tiles = { $amount } tuiles marqueurs de type { $type } effacées
notification-cleared-all-marker-tiles = { $amount } tuiles marqueurs effacées
notification-opened = Ouvert
mirror-mode =
    .none = Aucun
    .flip = Retourner
    .rotate = Rotation
notification-edited-actor = Modifié { $name } ({ $id })
notification-edited-actor-id = Modifié { $name } ({ $old-id }->{ $new-id })
notification-player-is-victorious = { $player } est victorieux.
notification-player-is-defeated = { $player } est vaincu.
notification-desync-compare-logs =
    Désynchronisation à l'image { $frame }.
    Comparez syncreport.log avec les autres joueurs.
label-client-state-disconnected = Parti
