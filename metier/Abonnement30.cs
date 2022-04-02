using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediatek86.metier
{
    /// <summary>
    /// Classe métier représentant un abonnement dont la date d'expiration est moins de 30 jours
    /// </summary>
    public class Abonnement30
    {
        private readonly DateTime dateFinAbonnement;
        private readonly string idRevue;
        private readonly string titreRevue;

        /// <summary>
        /// Constructeur : valorise les propriétés de la classe
        /// </summary>
        /// <param name="dateFinAbonnement"></param>
        /// <param name="idRevue"></param>
        /// <param name="titreRevue"></param>
        public Abonnement30(DateTime dateFinAbonnement, string idRevue, string titreRevue)
        {
            this.dateFinAbonnement = dateFinAbonnement;
            this.idRevue = idRevue;
            this.titreRevue = titreRevue;
        }

        /// <summary>
        /// Getter sur la date de fin de l'abonnement
        /// </summary>
        public DateTime DateFinAbonnement => dateFinAbonnement;
        /// <summary>
        /// Getter sur l'identifiant de la revue concernée par l'abonnement
        /// </summary>
        public string IdRevue => idRevue;
        /// <summary>
        /// Getter sur le titre de la revue concernée par l'abonnement
        /// </summary>
        public string TitreRevue => titreRevue;
    }
}
