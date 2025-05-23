﻿using System.Collections.Generic;
using Mediatek86.modele;
using Mediatek86.metier;
using Mediatek86.vue;
using System.Drawing;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;

//// <summary> Contrôleur de l'application </summary>
namespace Mediatek86.controleur
{
    /// <summary>
    /// Contrôleur de l'application
    /// </summary>
    public class Controle
    {
        private readonly List<Livre> lesLivres;
        private readonly List<Dvd> lesDvd;
        private readonly List<Revue> lesRevues;
        private readonly List<Categorie> lesRayons;
        private readonly List<Categorie> lesPublics;
        private readonly List<Categorie> lesGenres;
        private readonly List<Suivi> lesSuivis;
        /// <summary>
        /// Service auquel l'utilisateur appartient
        /// </summary>
        public Service UserService { get; private set; }

        /// <summary>
        /// Ouverture de la fenêtre
        /// </summary>
        public Controle()
        {
            lesLivres = Dao.GetAllLivres();
            lesDvd = Dao.GetAllDvd();
            lesRevues = Dao.GetAllRevues();
            lesGenres = Dao.GetAllGenres();
            lesRayons = Dao.GetAllRayons();
            lesPublics = Dao.GetAllPublics();
            lesSuivis = Dao.GetAllSuivis();
            
            FrmAuthentification authentification = new FrmAuthentification(this);
            Application.Run(authentification);
            if (authentification.OnSuccessAuth)
            {
                FrmMediatek frmMediatek = new FrmMediatek(this);
                Application.Run(frmMediatek);
            }

        }

        /// <summary>
        /// Récupère le service de l'utilisateur
        /// </summary>
        /// <param name="login">Login de l'utilisateur</param>
        /// <param name="pwd">Mot de passe de l'utilisateur</param>
        /// <returns>Service si authentification réussie, sinon retourne null</returns>
        public Service Authentification(string login, string pwd)
        {
            Service service = Dao.Authentification(login, HashMD5(pwd));
            UserService = service;
            return service;
        }

        /// <summary>
        /// Calcul du hash à partir du mot de passe
        /// </summary>
        /// <param name="mdp">Mot de passe</param>
        /// <returns>Mot de passe haché</returns>
        public string HashMD5(string mdp)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(mdp);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Conversion du vecteur vers un string hexadecimal
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// getter sur la liste des genres
        /// </summary>
        /// <returns>Collection d'objets Genre</returns>
        public List<Categorie> GetAllGenres()
        {
            return lesGenres;
        }

        /// <summary>
        /// getter sur la liste des livres
        /// </summary>
        /// <returns>Collection d'objets Livre</returns>
        public List<Livre> GetAllLivres()
        {
            return lesLivres;
        }

        /// <summary>
        /// getter sur la liste des Dvd
        /// </summary>
        /// <returns>Collection d'objets dvd</returns>
        public List<Dvd> GetAllDvd()
        {
            return lesDvd;
        }

        /// <summary>
        /// getter sur la liste des revues
        /// </summary>
        /// <returns>Collection d'objets Revue</returns>
        public List<Revue> GetAllRevues()
        {
            return lesRevues;
        }

        /// <summary>
        /// getter sur les rayons
        /// </summary>
        /// <returns>Collection d'objets Rayon</returns>
        public List<Categorie> GetAllRayons()
        {
            return lesRayons;
        }

        /// <summary>
        /// getter sur les publics
        /// </summary>
        /// <returns>Collection d'objets Public</returns>
        public List<Categorie> GetAllPublics()
        {
            return lesPublics;
        }

        /// <summary>
        /// getter sur les suivis
        /// </summary>
        /// <returns>Collection d'objets Suivi</returns>
        public List<Suivi> GetAllSuivis()
        {
            return lesSuivis;
        }

        /// <summary>
        /// récupère les exemplaires d'une revue
        /// </summary>
        /// <returns>Collection d'objets Exemplaire</returns>
        public List<Exemplaire> GetExemplairesRevue(string idDocuement)
        {
            return Dao.GetExemplairesRevue(idDocuement);
        }

        /// <summary>
        /// Crée un exemplaire d'une revue dans la bdd
        /// </summary>
        /// <param name="exemplaire">L'objet Exemplaire concerné</param>
        /// <returns>True si la création a pu se faire</returns>
        public bool CreerExemplaire(Exemplaire exemplaire)
        {
            return Dao.CreerExemplaire(exemplaire);
        }

        /// <summary>
        /// Récupère les commandes d'un Livre/DVD
        /// </summary>
        /// <param name="idDocument">Identifiant du livre ou DVD concerné</param>
        /// <returns>Collection d'objets de type CommandeDocument</returns>
        public List<CommandeDocument> GetCommandeDocument(string idDocument)
        {
            return Dao.GetCommandeDocument(idDocument);
        }

        /// <summary>
        /// Crée une CommandeDocument dans la bdd
        /// </summary>
        /// <param name="commandeDocument">L'objet CommandeDocument concerné</param>
        /// <returns>Le message de confirmation ou d'erreur</returns>
        public bool CreerCommandeDocument(CommandeDocument commandeDocument)
        {
            return Dao.CreerCommandeDocument(commandeDocument);
        }

        /// <summary>
        /// Supprime une CommandeDocument de la bdd
        /// </summary>
        /// <param name="id">Identifiant de CommandeDocument à supprimer</param>
        /// <returns>True si la suppression a pu se faire</returns>
        public bool SupprCommandeDocument(string id)
        {
            return Dao.SupprCommandeDocument(id);
        }

        /// <summary>
        /// Modifie l'état de suivi d'une CommandeDocument
        /// </summary>
        /// <param name="idCommandeDocument">Identifiant CommandeDocument à modifier</param>
        /// <param name="idSuivi">Identifiant du nouvel état de suivi</param>
        /// <returns>True si la modification a pu se faire</returns>
        public bool ModifSuiviCommandeDocument(string idCommandeDocument, int idSuivi)
        {
            return Dao.ModifSuiviCommandeDocument(idCommandeDocument, idSuivi);
        }

        /// <summary>
        /// Récupère les abonnements d'une revue
        /// </summary>
        /// <returns>Collection d'objets Abonnement</returns>
        public List<Abonnement> GetAbonnement(string idDocument)
        {
            return Dao.GetAbonnement(idDocument);
        }

        /// <summary>
        /// Crée un abonnement de revue dans la bdd
        /// </summary>
        /// <param name="abonnement">Objet Abonnement concerné</param>
        /// <returns>True si la création a pu se faire</returns>
        public bool CreerAbonnement(Abonnement abonnement)
        {
            return Dao.CreerAbonnement(abonnement);
        }

        /// <summary>
        /// Teste si la suppression d'un abonnement est possible
        /// Si l'abonnement n'est lié à aucun exemplaire
        /// </summary>
        /// <param name="abonnement">Abonnement concerné</param>
        /// <returns>True si la suppression est possible</returns>
        public bool CheckSupprAbonnement(Abonnement abonnement)
        {
            List<Exemplaire> lesExemplaires = GetExemplairesRevue(abonnement.IdRevue);
            bool suppression = false;

            foreach (Exemplaire exemplaire in lesExemplaires.Where(ex => ParutionDansAbonnement(abonnement.DateCommande, abonnement.DateFinAbonnement, ex.DateAchat)))
            {
                suppression = true;
            }
            return !suppression;
        }

        /// <summary>
        /// Teste si dateParution est compris entre dateCommande et dateFinAbonnement
        /// </summary>
        /// <param name="dateCommande">Date de commande d'un abonnement</param>
        /// <param name="dateFinAbonnement">Date de fin d'un abonnement</param>
        /// <param name="dateParution">Date de parution d'un exemplaire</param>
        /// <returns>True si la date est comprise</returns>
        public bool ParutionDansAbonnement(DateTime dateCommande, DateTime dateFinAbonnement, DateTime dateParution)
        {
            return (DateTime.Compare(dateCommande, dateParution) < 0 && DateTime.Compare(dateParution, dateFinAbonnement) < 0);
        }

        /// <summary>
        /// Demande la suppression d'un abonnement de la bdd
        /// </summary>
        /// <param name="idAbonnement">L'identifiant de l'abonnement à supprimer</param>
        /// <returns>True si l'opération a réussi, sinon false</returns>
        public bool SupprAbonnement(string idAbonnement)
        {
            return Dao.SupprAbonnement(idAbonnement);
        }

        /// <summary>
        /// Récupère les abonnements qui expirent dans moins de 30 jours
        /// </summary>
        /// <returns>Collection d'objets de type Abonnement</returns>
        public List<Abonnement30> GetAbonnement30()
        {
            return Dao.GetAbonnement30();
        }

    }

}

