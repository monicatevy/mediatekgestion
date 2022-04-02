using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediatek86.metier
{
    /// <summary>
    /// Classe métier représentant la table Public
    /// Classe fille de : Categorie
    /// </summary>
    public class Public : Categorie
    {
        /// <summary>
        /// Constructeur
        /// Appelle le constructeur de la classe mère
        /// </summary>
        /// <param name="id"></param>
        /// <param name="libelle"></param>
        public Public(string id, string libelle):base(id, libelle)
        {
        }

    }
}
