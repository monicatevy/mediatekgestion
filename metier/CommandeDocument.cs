using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediatek86.metier
{
    /// <summary>
    /// Classe représentant la table CommandeDocument
    /// Classe fille de : Commande
    /// </summary>
    public class CommandeDocument : Commande
    {
        private readonly int nbExemplaires;
        private readonly int idSuivi;
        private readonly string libelleSuivi;
        private readonly string idLivreDvd;

        /// <summary>
        /// Constructueur : valorise les propriétés de la classe
        /// Appelle le constructeur de la classe mère
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dateCommande"></param>
        /// <param name="montant"></param>
        /// <param name="nbExemplaires"></param>
        /// <param name="idLivreDvd"></param>
        /// <param name="idSuivi"></param>
        /// <param name="libelleSuivi"></param>
        public CommandeDocument(string id, DateTime dateCommande, double montant, int nbExemplaires, string idLivreDvd, int idSuivi, string libelleSuivi) : base(id, dateCommande, montant)
        {
            this.nbExemplaires = nbExemplaires;
            this.idSuivi = idSuivi;
            this.libelleSuivi = libelleSuivi;
            this.idLivreDvd = idLivreDvd;
        }

        /// <summary>
        /// Getter sur le nombre d'exemplaires commandés
        /// </summary>
        public int NbExemplaires { get => nbExemplaires; }
        /// <summary>
        /// Getter sur l'identifiant de l'état de suivi de la commande
        /// </summary>
        public int IdSuivi { get => idSuivi; }
        /// <summary>
        /// Getter sur le libellé de l'état de suivi de la commande
        /// </summary>
        public string LibelleSuivi { get => libelleSuivi; }
        /// <summary>
        /// Getter sur l'identifiant du document commandé (Livre/DVD)
        /// </summary>
        public string IdLivreDvd { get => idLivreDvd; }
    }
}
