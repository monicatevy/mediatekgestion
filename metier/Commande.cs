using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//// <summary> Classe métier </summary>
namespace Mediatek86.metier
{
    /// <summary>
    /// Classe métier représentant la table Commande
    /// Classe mère de : Abonnement, CommandeDocument
    /// </summary>
    public class Commande
    {
        private readonly string id;
        private readonly DateTime dateCommande;
        private readonly Double montant;

        /// <summary>
        /// Constructeur : valorise les propriétés de la classe
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dateCommande"></param>
        /// <param name="montant"></param>
        public Commande(string id, DateTime dateCommande, double montant)
        {
            this.id = id;
            this.dateCommande = dateCommande;
            this.montant = montant;
        }

        /// <summary>
        /// Getter sur l'identifiant de la commande
        /// </summary>
        public string Id { get => id; }
        /// <summary>
        /// Getter sur la date de la commande
        /// </summary>
        public DateTime DateCommande { get => dateCommande; }
        /// <summary>
        /// Getter sur le montant de la commande
        /// </summary>
        public Double Montant { get => montant; }
    }
}
