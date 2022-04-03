Feature: AjoutCommandeLivre
	Recherche de livre par son numéro et ajout d'une commande 

@ajoutCommandeLivre
Scenario: Chercher un livre puis lui ajouter une commande
	Given Je saisie la valeur 00014
	And Je clique sur le bouton Rechercher
	And Je clique sur le bouton Ajouter
	And Je saisie le numéro de commande MOKO
	And Je saisie le nombre d'exemplaire à 2
	And Je saisie le montant à 26
	When Je clique sur le bouton Valider
	Then Le détail de la commande doit afficher le numéro de commande MOKO