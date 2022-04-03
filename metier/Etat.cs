
//// <summary> Classe métier </summary>
namespace Mediatek86.metier
{
    /// <summary>
    /// Classe métier représentant la table Etat
    /// </summary>
    public class Etat
    {
        /// <summary>
        /// Constructeur : valorise les propriétés de la classe
        /// </summary>
        /// <param name="id"></param>
        /// <param name="libelle"></param>
        public Etat(string id, string libelle)
        {
            this.Id = id;
            this.Libelle = libelle;
        }

        /// <summary>
        /// Getter sur l'identifiant de l'état
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Getter sur le libellé de l'état
        /// </summary>
        public string Libelle { get; set; }
    }
}
