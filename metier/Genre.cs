

namespace Mediatek86.metier
{
    /// <summary>
    /// Classe métier représentant la classe Categorie
    /// Classe fille de : Categorie
    /// </summary>
    public class Genre : Categorie
    {
        /// <summary>
        /// Constructueur
        /// Appelle le constructeur de la classe mère
        /// </summary>
        /// <param name="id"></param>
        /// <param name="libelle"></param>
        public Genre(string id, string libelle) : base(id, libelle)
        {
        }

    }
}
