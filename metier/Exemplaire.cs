using System;

namespace Mediatek86.metier
{
    /// <summary>
    /// Classe métier représentant la table Exemplaire
    /// </summary>
    public class Exemplaire
    {
        /// <summary>
        /// Constructeur : valorise les propriétés de la classe
        /// </summary>
        /// <param name="numero"></param>
        /// <param name="dateAchat"></param>
        /// <param name="photo"></param>
        /// <param name="idEtat"></param>
        /// <param name="idDocument"></param>
        public Exemplaire(int numero, DateTime dateAchat, string photo,string idEtat, string idDocument)
        {
            this.Numero = numero;
            this.DateAchat = dateAchat;
            this.Photo = photo;
            this.IdEtat = idEtat;
            this.IdDocument = idDocument;
        }

        /// <summary>
        /// Getter sur le numéro de l'exemplaire
        /// </summary>
        public int Numero { get; set; }
        /// <summary>
        /// Getter sur la photo de l'exemplaire
        /// </summary>
        public string Photo { get; set; }
        /// <summary>
        /// Getter sur la date d'achat de l'exemplaire
        /// </summary>
        public DateTime DateAchat { get; set; }
        /// <summary>
        /// Getter sur l'identifiant de l'état
        /// </summary>
        public string IdEtat { get; set; }
        /// <summary>
        /// Getter sur l'identifiant du document
        /// </summary>
        public string IdDocument { get; set; }
    }
}
