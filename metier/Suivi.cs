using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//// <summary> Classe métier </summary>
namespace Mediatek86.metier
{
    /// <summary>
    /// Classe métier représentant la table Suivi
    /// </summary>
    public class Suivi
    {
        private readonly int id;
        private readonly string libelle;

        /// <summary>
        /// Constructeur : valorise les propriétés de la classe
        /// </summary>
        /// <param name="id"></param>
        /// <param name="libelle"></param>
        public Suivi(int id, string libelle)
        {
            this.id = id;
            this.libelle = libelle;
        }

        /// <summary>
        /// Getter sur l'identifiant du suivi
        /// </summary>
        public int Id { get => id; }
        /// <summary>
        /// Getter sur le libellé du suivi
        /// </summary>
        public string Libelle { get => libelle; }
    }
}
