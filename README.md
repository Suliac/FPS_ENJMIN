# Cake Arena (Projet Unity ENJMIN Master1)

Cake Arena est un shooter de survie TPS, **en ligne**. Les joueurs doivent donc rejoindre un serveur créé par l'un d'entre eux.
Très inspiré de Quake, le but est d'être le premier à atteindre 20kills.

##Online
J'ai utilisé le framework réseau (Unet) mis a disposition par Unity.
Les joueurs doivent entrer l'IP de l'host du serveur.
La gestion de la vie, des dégâts, de la mort et des améioration (armes etc...) sont gérés sur le serveur.
Le comportements des PNJ est "lancé" depuis le serveur mais peut éviter des effets de "lag", les clients font une interpretation du déplacement des PNJ qui est ensuite réctifiée si besoin par le serveur.

## Amélioration et Zombies (PNJ)
Des améliorations tombent au hasard sur la carte permettant de débloquer des armes, des munitions ou de récupérer de la vie.
Ces améliorations sont protégées par des "zombies" qui viennent attaquer les joueurs qui s'approchent.
Leur seule attaque est au corps à corps, mais si un zombie touche un joueur il le one-shot.
Les joueurs ne sont pas obligé de tuer les zombies pour récupérer l'amélioration qu'il "garde" et quand les joueurs sont trop loins, les zombies arrêtent de les suivre.
Un zombie attaque le joueur le plus proche : les joueurs peuvent jouer avec ça pour ammener les zombies sur d'autres joueurs.

## Armes
A sa mort, un joueur perd toutes ses armes et repart avec le pistolet de base (munitions illimitées)
Il y à trois armes différentes :
* Le pistolet de base, il inflige peut de dégâts mais possède une quantitée illimité de munitions
* Un pistolet automatique, la fréquence de tir est très élevée
* Un lasergun, sa fréquence de tir est peu élevée et les projectiles sont lents, mais les dégâts sont importants

## Autre
Les joueurs peuvent consulter les scores acutels durant la partie.
Les joueurs possèdent une mini carte qui affichent les coffres sous forme de point rouge
Si un joueur tombe dans l'eau, il meurt et perd 1kill.
