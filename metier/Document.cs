
//// <summary> Classe métier </summary>
namespace Mediatek86.metier
{
    /// <summary>
    /// Classe métier représentant la table Document
    /// Classe mère de : Revue, LivresDVD
    /// </summary>
    public class Document
    {
        private readonly string id;
        private readonly string titre;
        private readonly string image;
        private readonly string idGenre;
        private readonly string genre;
        private readonly string idPublic;
        private readonly string lePublic;
        private readonly string idRayon;
        private readonly string rayon;

        /// <summary>
        /// Constructeur : valorise les propriétés de la classe
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
#pragma warning disable S107 // Methods should not have too many parameters
        public Document(string id, string titre, string image, string idGenre, string genre, 
            string idPublic, string lePublic, string idRayon, string rayon)
#pragma warning restore S107 // Methods should not have too many parameters
        {
            this.id = id;
            this.titre = titre;
            this.image = image;
            this.idGenre = idGenre;
            this.genre = genre;
            this.idPublic = idPublic;
            this.lePublic = lePublic;
            this.idRayon = idRayon;
            this.rayon = rayon;
        }

        /// <summary>
        /// Getter sur l'identifiant du document
        /// </summary>
        public string Id { get => id; }
        /// <summary>
        /// Getter sur le titre du document
        /// </summary>
        public string Titre { get => titre; }
        /// <summary>
        /// Getter sur l'image du document
        /// </summary>
        public string Image { get => image; }
        /// <summary>
        /// Getter sur l'identifiant du genre du document
        /// </summary>
        public string IdGenre { get => idGenre; }
        /// <summary>
        /// Getter sur le libellé du genre du document
        /// </summary>
        public string Genre { get => genre; }
        /// <summary>
        /// Getter sur l'identifiant du public du document
        /// </summary>
        public string IdPublic { get => idPublic; }
        /// <summary>
        /// Getter sur le libellé du public du document
        /// </summary>
        public string Public { get => lePublic; }
        /// <summary>
        /// Getter sur l'identifiant du rayon du document
        /// </summary>
        public string IdRayon { get => idRayon; }
        /// <summary>
        /// Getter sur le libellé du rayon du document
        /// </summary>
        public string Rayon { get => rayon; }

    }


}
