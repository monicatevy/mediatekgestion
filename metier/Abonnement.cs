using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//// <summary> Classe métier </summary>
namespace Mediatek86.metier
{
    /// <summary>
    /// Classe métier représentant la table Abonnement
    /// Classe fille de la classe Commande
    /// </summary>
    public class Abonnement : Commande
    {
        private readonly DateTime dateFinAbonnement;
        private readonly string idRevue;

        /// <summary>
        /// Constructueur : valorise les propriétés de la classe
        /// Appelle le constructeur de la classe mère
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dateCommande"></param>
        /// <param name="montant"></param>
        /// <param name="dateFinAbonnement"></param>
        /// <param name="idRevue"></param>
        public Abonnement(string id, DateTime dateCommande, double montant, DateTime dateFinAbonnement, string idRevue) : base(id, dateCommande, montant)
        {
            this.dateFinAbonnement = dateFinAbonnement;
            this.idRevue = idRevue;
        }

        /// <summary>
        /// Getter sur la date de fin de l'abonnement
        /// </summary>
        public DateTime DateFinAbonnement { get => dateFinAbonnement; }
        /// <summary>
        /// Getter sur l'identifiant de la revue concernée par l'abonnement
        /// </summary>
        public string IdRevue { get => idRevue; }
    }
}
