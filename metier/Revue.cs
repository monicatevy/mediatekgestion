//// <summary> Classe métier </summary>
namespace Mediatek86.metier
{
    /// <summary>
    /// Classe métier représentant la table Revue
    /// </summary>
    public class Revue : Document
    {
        /// <summary>
        /// Constructeur : valorise les propriétés de la classe
        /// Appelle le constructeur de la classe mère
        /// </summary>
        /// <param name="id"></param>
        /// <param name="titre"></param>
        /// <param name="image"></param>
        /// <param name="idGenre"></param>
        /// <param name="genre"></param>
        /// <param name="idPublic"></param>
        /// <param name="lePublic"></param>
        /// <param name="idRayon"></param>
        /// <param name="rayon"></param>
        /// <param name="empruntable"></param>
        /// <param name="periodicite"></param>
        /// <param name="delaiMiseADispo"></param>
        public Revue(string id, string titre, string image, string idGenre, string genre,
            string idPublic, string lePublic, string idRayon, string rayon, 
            bool empruntable, string periodicite, int delaiMiseADispo)
             : base(id, titre, image, idGenre, genre, idPublic, lePublic, idRayon, rayon)
        {
            Periodicite = periodicite;
            Empruntable = empruntable;
            DelaiMiseADispo = delaiMiseADispo;
        }

        /// <summary>
        /// Getter sur la périodicité de la revue
        /// </summary>
        public string Periodicite { get; set; }
        /// <summary>
        /// Getter sur le booléen qui indique si la revue est empruntable
        /// </summary>
        public bool Empruntable { get; set; }
        /// <summary>
        /// Getter sur le délai de mise à disposition de la revue
        /// </summary>
        public int DelaiMiseADispo { get; set; }
    }
}
