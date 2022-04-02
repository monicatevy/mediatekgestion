using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediatek86.metier
{
    /// <summary>
    /// Classe métier représentant la table Service
    /// </summary>
    public class Service
    {
        private readonly int id;
        private readonly string libelle;

        /// <summary>
        /// Constructeur : valorise les propriétés de la classe
        /// </summary>
        /// <param name="id"></param>
        /// <param name="libelle"></param>
        public Service(int id, string libelle)
        {
            this.id = id;
            this.libelle = libelle;
        }

        /// <summary>
        /// Getter sur l'identifiant du service
        /// </summary>
        public int Id => id;
        /// <summary>
        /// Getter sur le libellé du service
        /// </summary>
        public string Libelle => libelle;
    }
}
