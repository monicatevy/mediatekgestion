
namespace Mediatek86.metier
{
    /// <summary>
    /// Classe métier représentant la table Revue
    /// </summary>
    public class Revue : Document
    {
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
