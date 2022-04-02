Feature: AjoutCommandeLivre

@ajoutcommandelivre
Scenario: Recherche de livre puis ajout d'une commande
	Given saisir le numero du livre '00014'
	And click sur le bouton Rechercher
	And click sur le bouton Ajouter
	And saisir le numéro de commande 'MOKO'
	And saisir nombre exemplaires à '2'
	And saisir le montant à '26,07'
	When click sur le bouton Valider
	Then le détail affiche les informations de la commande 'MOKO'