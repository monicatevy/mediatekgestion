﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Mediatek86.metier;
using Mediatek86.controleur;
using System.Drawing;
using System.Linq;
using System.Globalization;

//// <summary> Vues de l'application </summary>
namespace Mediatek86.vue
{
    /// <summary>
    /// Vue principale de l'application
    /// </summary>
    public partial class FrmMediatek : Form
    {

        #region Variables globales

        private readonly Controle controle;
        const string ETATNEUF = "00001";
        const string DOSSIERMEDIARECHERCHE = "c:\\MediatekMedia";

        private readonly BindingSource bdgLivresListe = new BindingSource();
        private readonly BindingSource bdgDvdListe = new BindingSource();
        private readonly BindingSource bdgGenres = new BindingSource();
        private readonly BindingSource bdgPublics = new BindingSource();
        private readonly BindingSource bdgRayons = new BindingSource();
        private readonly BindingSource bdgRevuesListe = new BindingSource();
        private readonly BindingSource bdgExemplairesListe = new BindingSource();
        private readonly BindingSource bdgCommandesLivresListe = new BindingSource();
        private readonly BindingSource bdgCommandesDvdListe = new BindingSource();
        private readonly BindingSource bdgAbonnementRevuesListe = new BindingSource();
        private List<Livre> lesLivres = new List<Livre>();
        private List<Dvd> lesDvd = new List<Dvd>();
        private List<Revue> lesRevues = new List<Revue>();
        private List<Exemplaire> lesExemplaires = new List<Exemplaire>();
        private List<CommandeDocument> lesCommandeDocument = new List<CommandeDocument>();
        private List<Suivi> lesSuivis = new List<Suivi>();
        private List<Abonnement> lesAbonnements = new List<Abonnement>();

        #endregion

        /// <summary>
        /// Constructeur
        /// Restriction des fonctionnalités si l'utilisateur est du service Prêt
        /// </summary>
        /// <param name="controle"></param>
        public FrmMediatek(Controle controle)
        {
            InitializeComponent();
            this.controle = controle;

            if (controle.UserService.Libelle == "prêt")
            {
                tabOngletsApplication.TabPages.Remove(tabCommandeLivres);
                tabOngletsApplication.TabPages.Remove(tabCommandeDVD);
                tabOngletsApplication.TabPages.Remove(tabAbonnementRevues);
                grpReceptionExemplaire.Visible = false;
            }
        }

        /// <summary>
        /// Alerte au démarrage de l'application des abonnements qui expirent dans moins de 30 jours
        /// n'apparaît pas si l'utilisateur est du service Prêt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMediatek_Shown(object sender, EventArgs e)
        {
            if (controle.UserService.Libelle != "prêt") {
                FrmAlerteAbonnements30 alerteAbonnements30 = new FrmAlerteAbonnements30(controle)
                {
                    StartPosition = FormStartPosition.CenterParent
                };
                alerteAbonnements30.ShowDialog();
            }
        }


        #region modules communs

        /// <summary>
        /// Rempli un des 3 combo (genre, public, rayon)
        /// </summary>
        /// <param name="lesCategories"></param>
        /// <param name="bdg"></param>
        /// <param name="cbx"></param>
        public void RemplirComboCategorie(List<Categorie> lesCategories, BindingSource bdg, ComboBox cbx)
        {
            bdg.DataSource = lesCategories;
            cbx.DataSource = bdg;
            if (cbx.Items.Count > 0)
            {
                cbx.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Tri sur une colonne pour les listes CommandeDocument
        /// </summary>
        /// <param name="titreColonne">Titre de la colonne concernée</param>
        /// <returns>Liste triée</returns>
        private List<CommandeDocument> TriCommandeDocumentList(string titreColonne)
        {
            List<CommandeDocument> sortedList = new List<CommandeDocument>();
            switch (titreColonne)
            {
                case "Date":
                    sortedList = lesCommandeDocument.OrderBy(o => o.DateCommande).Reverse().ToList();
                    break;
                case "Montant":
                    sortedList = lesCommandeDocument.OrderBy(o => o.Montant).Reverse().ToList();
                    break;
                case "Exemplaires":
                    sortedList = lesCommandeDocument.OrderBy(o => o.NbExemplaires).Reverse().ToList();
                    break;
                case "Etat":
                    sortedList = lesCommandeDocument.OrderBy(o => o.IdSuivi).ToList();
                    break;
            }
            return sortedList;
        }

        /// <summary>
        /// Affichage d'un MessageBox pour demander la confirmation de suppression d'une commande
        /// </summary>
        /// <returns>True si suppression confirmée</returns>
        private bool ConfirmationSupprCommande()
        {
            return (MessageBox.Show("Etes-vous sûr de vouloir supprimer cette commande ?", "Confirmation de suppression", MessageBoxButtons.YesNo) == DialogResult.Yes);
        }

        /// <summary>
        /// Affichage d'un MessageBox pour demander la confirmation de suppression d'un abonnement
        /// </summary>
        /// <returns>True si suppression confirmée</returns>
        private bool ConfirmationSupprAbonnement()
        {
            return (MessageBox.Show("Etes-vous sûr de vouloir supprimer cet abonnement ?", "Confirmation de suppression", MessageBoxButtons.YesNo) == DialogResult.Yes);
        }

        /// <summary>
        /// Affichage d'un MessageBox pour confirmer que l'utilisateur souhaite annuler la saisie d'une commande
        /// </summary>
        /// <returns>True si annulation de saisie</returns>
        private bool ConfirmationAnnulationCommande()
        {
            return (MessageBox.Show("Etes-vous sûr de vouloir annuler votre saisie ?", "Confirmation d'annulation", MessageBoxButtons.YesNo) == DialogResult.Yes);
        }

        /// <summary>
        /// Affichage d'un MessageBox pour demander la confirmation du changement d'état de suivi d'une commande
        /// </summary>
        /// <param name="libelleSuivi">Nouvel état de suivi</param>
        /// <returns>True si confirmation de changement</returns>
        private bool ConfirmationModifSuiviCommande(string libelleSuivi)
        {
            return (MessageBox.Show("Etes-vous sûr de vouloir changer l'état de cette commande à : " + libelleSuivi + " ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes);
        }

        #endregion


        #region Revues
        //-----------------------------------------------------------
        // ONGLET "Revues"
        //------------------------------------------------------------

        /// <summary>
        /// Ouverture de l'onglet Revues : 
        /// appel des méthodes pour remplir le datagrid des revues et des combos (genre, rayon, public)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabRevues_Enter(object sender, EventArgs e)
        {
            lesRevues = controle.GetAllRevues();
            RemplirComboCategorie(controle.GetAllGenres(), bdgGenres, cbxRevuesGenres);
            RemplirComboCategorie(controle.GetAllPublics(), bdgPublics, cbxRevuesPublics);
            RemplirComboCategorie(controle.GetAllRayons(), bdgRayons, cbxRevuesRayons);
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Remplit le dategrid avec la liste reçue en paramètre
        /// </summary>
        private void RemplirRevuesListe(List<Revue> revues)
        {
            bdgRevuesListe.DataSource = revues;
            dgvRevuesListe.DataSource = bdgRevuesListe;
            dgvRevuesListe.Columns["empruntable"].Visible = false;
            dgvRevuesListe.Columns["idRayon"].Visible = false;
            dgvRevuesListe.Columns["idGenre"].Visible = false;
            dgvRevuesListe.Columns["idPublic"].Visible = false;
            dgvRevuesListe.Columns["image"].Visible = false;
            dgvRevuesListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRevuesListe.Columns["id"].DisplayIndex = 0;
            dgvRevuesListe.Columns["titre"].DisplayIndex = 1;
        }

        /// <summary>
        /// Recherche et affichage de la revue dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txbRevuesNumRecherche.Text.Equals(""))
            {
                txbRevuesTitreRecherche.Text = "";
                cbxRevuesGenres.SelectedIndex = -1;
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
                Revue revue = lesRevues.Find(x => x.Id.Equals(txbRevuesNumRecherche.Text));
                if (revue != null)
                {
                    List<Revue> revues = new List<Revue>
                    {
                        revue
                    };
                    RemplirRevuesListe(revues);
                }
                else
                {
                    MessageBox.Show("numéro introuvable");
                    RemplirRevuesListeComplete();
                }
            }
            else
            {
                RemplirRevuesListeComplete();
            }
        }

        /// <summary>
        /// Recherche et affichage des revues dont le titre matche acec la saisie.
        /// Cette procédure est exécutée à chaque ajout ou suppression de caractère
        /// dans le textBox de saisie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbRevuesTitreRecherche_TextChanged(object sender, EventArgs e)
        {
            if (!txbRevuesTitreRecherche.Text.Equals(""))
            {
                cbxRevuesGenres.SelectedIndex = -1;
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
                txbRevuesNumRecherche.Text = "";
                List<Revue> lesRevuesParTitre;
                lesRevuesParTitre = lesRevues.FindAll(x => x.Titre.ToLower().Contains(txbRevuesTitreRecherche.Text.ToLower()));
                RemplirRevuesListe(lesRevuesParTitre);
            }
            else
            {
                // si la zone de saisie est vide et aucun élément combo sélectionné, réaffichage de la liste complète
                if (cbxRevuesGenres.SelectedIndex < 0 && cbxRevuesPublics.SelectedIndex < 0 && cbxRevuesRayons.SelectedIndex < 0
                    && txbRevuesNumRecherche.Text.Equals(""))
                {
                    RemplirRevuesListeComplete();
                }
            }
        }

        /// <summary>
        /// Affichage des informations de la revue sélectionné
        /// </summary>
        /// <param name="revue"></param>
        private void AfficheRevuesInfos(Revue revue)
        {
            txbRevuesPeriodicite.Text = revue.Periodicite;
            chkRevuesEmpruntable.Checked = revue.Empruntable;
            txbRevuesImage.Text = revue.Image;
            txbRevuesDateMiseADispo.Text = revue.DelaiMiseADispo.ToString();
            txbRevuesNumero.Text = revue.Id;
            txbRevuesGenre.Text = revue.Genre;
            txbRevuesPublic.Text = revue.Public;
            txbRevuesRayon.Text = revue.Rayon;
            txbRevuesTitre.Text = revue.Titre;     
            string image = revue.Image;
            try
            {
                pcbRevuesImage.Image = Image.FromFile(image);
            }
            catch 
            { 
                pcbRevuesImage.Image = null;
            }
        }

        /// <summary>
        /// Vide les zones d'affichage des informations de la reuve
        /// </summary>
        private void VideRevuesInfos()
        {
            txbRevuesPeriodicite.Text = "";
            chkRevuesEmpruntable.Checked = false;
            txbRevuesImage.Text = "";
            txbRevuesDateMiseADispo.Text = "";
            txbRevuesNumero.Text = "";
            txbRevuesGenre.Text = "";
            txbRevuesPublic.Text = "";
            txbRevuesRayon.Text = "";
            txbRevuesTitre.Text = "";
            pcbRevuesImage.Image = null;
        }

        /// <summary>
        /// Filtre sur le genre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxRevuesGenres_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxRevuesGenres.SelectedIndex >= 0)
            {
                txbRevuesTitreRecherche.Text = "";
                txbRevuesNumRecherche.Text = "";
                Genre genre = (Genre)cbxRevuesGenres.SelectedItem;
                List<Revue> revues = lesRevues.FindAll(x => x.Genre.Equals(genre.Libelle));
                RemplirRevuesListe(revues);
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur la catégorie de public
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxRevuesPublics_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxRevuesPublics.SelectedIndex >= 0)
            {
                txbRevuesTitreRecherche.Text = "";
                txbRevuesNumRecherche.Text = "";
                Public lePublic = (Public)cbxRevuesPublics.SelectedItem;
                List<Revue> revues = lesRevues.FindAll(x => x.Public.Equals(lePublic.Libelle));
                RemplirRevuesListe(revues);
                cbxRevuesRayons.SelectedIndex = -1;
                cbxRevuesGenres.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur le rayon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxRevuesRayons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxRevuesRayons.SelectedIndex >= 0)
            {
                txbRevuesTitreRecherche.Text = "";
                txbRevuesNumRecherche.Text = "";
                Rayon rayon = (Rayon)cbxRevuesRayons.SelectedItem;
                List<Revue> revues = lesRevues.FindAll(x => x.Rayon.Equals(rayon.Libelle));
                RemplirRevuesListe(revues);
                cbxRevuesGenres.SelectedIndex = -1;
                cbxRevuesPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Sur la sélection d'une ligne ou cellule dans le grid
        /// affichage des informations de la revue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvRevuesListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRevuesListe.CurrentCell != null)
            {
                try
                {
                    Revue revue = (Revue)bdgRevuesListe.List[bdgRevuesListe.Position];
                    AfficheRevuesInfos(revue);
                }
                catch
                {
                    VideRevuesZones();
                }
            }
            else
            {
                VideRevuesInfos();
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des revues
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAnnulPublics_Click(object sender, EventArgs e)
        {
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des revues
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAnnulRayons_Click(object sender, EventArgs e)
        {
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des revues
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRevuesAnnulGenres_Click(object sender, EventArgs e)
        {
            RemplirRevuesListeComplete();
        }

        /// <summary>
        /// Affichage de la liste complète des revues
        /// et annulation de toutes les recherches et filtres
        /// </summary>
        private void RemplirRevuesListeComplete()
        {
            RemplirRevuesListe(lesRevues);
            VideRevuesZones();
        }

        /// <summary>
        /// vide les zones de recherche et de filtre
        /// </summary>
        private void VideRevuesZones()
        {
            cbxRevuesGenres.SelectedIndex = -1;
            cbxRevuesRayons.SelectedIndex = -1;
            cbxRevuesPublics.SelectedIndex = -1;
            txbRevuesNumRecherche.Text = "";
            txbRevuesTitreRecherche.Text = "";
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvRevuesListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            VideRevuesZones();
            string titreColonne = dgvRevuesListe.Columns[e.ColumnIndex].HeaderText;
            List<Revue> sortedList = new List<Revue>();
            switch (titreColonne)
            {
                case "Id":
                    sortedList = lesRevues.OrderBy(o => o.Id).ToList();
                    break;
                case "Titre":
                    sortedList = lesRevues.OrderBy(o => o.Titre).ToList();
                    break;
                case "Periodicite":
                    sortedList = lesRevues.OrderBy(o => o.Periodicite).ToList();
                    break;
                case "DelaiMiseADispo":
                    sortedList = lesRevues.OrderBy(o => o.DelaiMiseADispo).ToList();
                    break;
                case "Genre":
                    sortedList = lesRevues.OrderBy(o => o.Genre).ToList();
                    break;
                case "Public":
                    sortedList = lesRevues.OrderBy(o => o.Public).ToList();
                    break;
                case "Rayon":
                    sortedList = lesRevues.OrderBy(o => o.Rayon).ToList();
                    break;
            }
            RemplirRevuesListe(sortedList);
        }

        #endregion


        #region Livres

        //-----------------------------------------------------------
        // ONGLET "LIVRES"
        //-----------------------------------------------------------

        /// <summary>
        /// Ouverture de l'onglet Livres : 
        /// appel des méthodes pour remplir le datagrid des livres et des combos (genre, rayon, public)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabLivres_Enter(object sender, EventArgs e)
        {
            lesLivres = controle.GetAllLivres();
            RemplirComboCategorie(controle.GetAllGenres(), bdgGenres, cbxLivresGenres);
            RemplirComboCategorie(controle.GetAllPublics(), bdgPublics, cbxLivresPublics);
            RemplirComboCategorie(controle.GetAllRayons(), bdgRayons, cbxLivresRayons);
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Remplit le dategrid avec la liste reçue en paramètre
        /// </summary>
        private void RemplirLivresListe(List<Livre> livres)
        {
            bdgLivresListe.DataSource = livres;
            dgvLivresListe.DataSource = bdgLivresListe;
            dgvLivresListe.Columns["isbn"].Visible = false;
            dgvLivresListe.Columns["idRayon"].Visible = false;
            dgvLivresListe.Columns["idGenre"].Visible = false;
            dgvLivresListe.Columns["idPublic"].Visible = false;
            dgvLivresListe.Columns["image"].Visible = false;
            dgvLivresListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLivresListe.Columns["id"].DisplayIndex = 0;
            dgvLivresListe.Columns["titre"].DisplayIndex = 1;
        }

        /// <summary>
        /// Recherche et affichage du livre dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLivresNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txbLivresNumRecherche.Text.Equals(""))
            {
                txbLivresTitreRecherche.Text = "";
                cbxLivresGenres.SelectedIndex = -1;
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
                Livre livre = lesLivres.Find(x => x.Id.Equals(txbLivresNumRecherche.Text));
                if (livre != null)
                {
                    List<Livre> livres = new List<Livre>
                    {
                        livre
                    };
                    RemplirLivresListe(livres);
                }
                else
                {
                    MessageBox.Show("numéro introuvable");
                    RemplirLivresListeComplete();
                }
            }
            else
            {
                RemplirLivresListeComplete();
            }
        }

        /// <summary>
        /// Recherche et affichage des livres dont le titre matche acec la saisie.
        /// Cette procédure est exécutée à chaque ajout ou suppression de caractère
        /// dans le textBox de saisie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxbLivresTitreRecherche_TextChanged(object sender, EventArgs e)
        {
            if (!txbLivresTitreRecherche.Text.Equals(""))
            {
                cbxLivresGenres.SelectedIndex = -1;
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
                txbLivresNumRecherche.Text = "";
                List<Livre> lesLivresParTitre;
                lesLivresParTitre = lesLivres.FindAll(x => x.Titre.ToLower().Contains(txbLivresTitreRecherche.Text.ToLower()));
                RemplirLivresListe(lesLivresParTitre);
            }
            else
            {
                // si la zone de saisie est vide et aucun élément combo sélectionné, réaffichage de la liste complète
                if (cbxLivresGenres.SelectedIndex < 0 && cbxLivresPublics.SelectedIndex < 0 && cbxLivresRayons.SelectedIndex < 0 
                    && txbLivresNumRecherche.Text.Equals(""))
                {
                    RemplirLivresListeComplete();
                }
            }
        }

        /// <summary>
        /// Affichage des informations du livre sélectionné
        /// </summary>
        /// <param name="livre"></param>
        private void AfficheLivresInfos(Livre livre)
        {
            txbLivresAuteur.Text = livre.Auteur;
            txbLivresCollection.Text = livre.Collection;
            txbLivresImage.Text = livre.Image;
            txbLivresIsbn.Text = livre.Isbn;
            txbLivresNumero.Text = livre.Id;
            txbLivresGenre.Text = livre.Genre;
            txbLivresPublic.Text = livre.Public;
            txbLivresRayon.Text = livre.Rayon;
            txbLivresTitre.Text = livre.Titre;      
            string image = livre.Image;
            try
            {
                pcbLivresImage.Image = Image.FromFile(image);
            }
            catch 
            {
                pcbLivresImage.Image = null;
            }
        }

        /// <summary>
        /// Vide les zones d'affichage des informations du livre
        /// </summary>
        private void VideLivresInfos()
        {
            txbLivresAuteur.Text = "";
            txbLivresCollection.Text = "";
            txbLivresImage.Text = "";
            txbLivresIsbn.Text = "";
            txbLivresNumero.Text = "";
            txbLivresGenre.Text = "";
            txbLivresPublic.Text = "";
            txbLivresRayon.Text = "";
            txbLivresTitre.Text = "";
            pcbLivresImage.Image = null;
        }

        /// <summary>
        /// Filtre sur le genre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbxLivresGenres_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxLivresGenres.SelectedIndex >= 0)
            {
                txbLivresTitreRecherche.Text = "";
                txbLivresNumRecherche.Text = "";
                Genre genre = (Genre)cbxLivresGenres.SelectedItem;
                List<Livre> livres = lesLivres.FindAll(x => x.Genre.Equals(genre.Libelle));
                RemplirLivresListe(livres);
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur la catégorie de public
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbxLivresPublics_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxLivresPublics.SelectedIndex >= 0)
            {
                txbLivresTitreRecherche.Text = "";
                txbLivresNumRecherche.Text = "";
                Public lePublic = (Public)cbxLivresPublics.SelectedItem;
                List<Livre> livres = lesLivres.FindAll(x => x.Public.Equals(lePublic.Libelle));
                RemplirLivresListe(livres);
                cbxLivresRayons.SelectedIndex = -1;
                cbxLivresGenres.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur le rayon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbxLivresRayons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxLivresRayons.SelectedIndex >= 0)
            {
                txbLivresTitreRecherche.Text = "";
                txbLivresNumRecherche.Text = "";
                Rayon rayon = (Rayon)cbxLivresRayons.SelectedItem;
                List<Livre> livres = lesLivres.FindAll(x => x.Rayon.Equals(rayon.Libelle));
                RemplirLivresListe(livres);
                cbxLivresGenres.SelectedIndex = -1;
                cbxLivresPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Sur la sélection d'une ligne ou cellule dans le grid
        /// affichage des informations du livre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgvLivresListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvLivresListe.CurrentCell != null)
            {
                try
                {
                    Livre livre = (Livre)bdgLivresListe.List[bdgLivresListe.Position];
                    AfficheLivresInfos(livre);
                }
                catch
                {
                    VideLivresZones();
                }
            }
            else
            {
                VideLivresInfos();
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des livres
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLivresAnnulPublics_Click(object sender, EventArgs e)
        {
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des livres
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLivresAnnulRayons_Click(object sender, EventArgs e)
        {
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des livres
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLivresAnnulGenres_Click(object sender, EventArgs e)
        {
            RemplirLivresListeComplete();
        }

        /// <summary>
        /// Affichage de la liste complète des livres
        /// et annulation de toutes les recherches et filtres
        /// </summary>
        private void RemplirLivresListeComplete()
        {
            RemplirLivresListe(lesLivres);
            VideLivresZones();
        }

        /// <summary>
        /// vide les zones de recherche et de filtre
        /// </summary>
        private void VideLivresZones()
        {
            cbxLivresGenres.SelectedIndex = -1;
            cbxLivresRayons.SelectedIndex = -1;
            cbxLivresPublics.SelectedIndex = -1;
            txbLivresNumRecherche.Text = "";
            txbLivresTitreRecherche.Text = "";
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgvLivresListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            VideLivresZones();
            string titreColonne = dgvLivresListe.Columns[e.ColumnIndex].HeaderText;
            List<Livre> sortedList = new List<Livre>();
            switch (titreColonne)
            {
                case "Id":
                    sortedList = lesLivres.OrderBy(o => o.Id).ToList();
                    break;
                case "Titre":
                    sortedList = lesLivres.OrderBy(o => o.Titre).ToList();
                    break;
                case "Collection":
                    sortedList = lesLivres.OrderBy(o => o.Collection).ToList();
                    break;
                case "Auteur":
                    sortedList = lesLivres.OrderBy(o => o.Auteur).ToList();
                    break;
                case "Genre":
                    sortedList = lesLivres.OrderBy(o => o.Genre).ToList();
                    break;
                case "Public":
                    sortedList = lesLivres.OrderBy(o => o.Public).ToList();
                    break;
                case "Rayon":
                    sortedList = lesLivres.OrderBy(o => o.Rayon).ToList();
                    break;
            }
            RemplirLivresListe(sortedList);
        }

        #endregion


        #region Dvd
        //-----------------------------------------------------------
        // ONGLET "DVD"
        //-----------------------------------------------------------

        /// <summary>
        /// Ouverture de l'onglet Dvds : 
        /// appel des méthodes pour remplir le datagrid des dvd et des combos (genre, rayon, public)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabDvd_Enter(object sender, EventArgs e)
        {
            lesDvd = controle.GetAllDvd();
            RemplirComboCategorie(controle.GetAllGenres(), bdgGenres, cbxDvdGenres);
            RemplirComboCategorie(controle.GetAllPublics(), bdgPublics, cbxDvdPublics);
            RemplirComboCategorie(controle.GetAllRayons(), bdgRayons, cbxDvdRayons);
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Remplit le dategrid avec la liste reçue en paramètre
        /// </summary>
        private void RemplirDvdListe(List<Dvd> Dvds)
        {
            bdgDvdListe.DataSource = Dvds;
            dgvDvdListe.DataSource = bdgDvdListe;
            dgvDvdListe.Columns["idRayon"].Visible = false;
            dgvDvdListe.Columns["idGenre"].Visible = false;
            dgvDvdListe.Columns["idPublic"].Visible = false;
            dgvDvdListe.Columns["image"].Visible = false;
            dgvDvdListe.Columns["synopsis"].Visible = false;
            dgvDvdListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDvdListe.Columns["id"].DisplayIndex = 0;
            dgvDvdListe.Columns["titre"].DisplayIndex = 1;
        }

        /// <summary>
        /// Recherche et affichage du Dvd dont on a saisi le numéro.
        /// Si non trouvé, affichage d'un MessageBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdNumRecherche_Click(object sender, EventArgs e)
        {
            if (!txbDvdNumRecherche.Text.Equals(""))
            {
                txbDvdTitreRecherche.Text = "";
                cbxDvdGenres.SelectedIndex = -1;
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
                Dvd dvd = lesDvd.Find(x => x.Id.Equals(txbDvdNumRecherche.Text));
                if (dvd != null)
                {
                    List<Dvd> Dvd = new List<Dvd>
                    {
                        dvd
                    };
                    RemplirDvdListe(Dvd);
                }
                else
                {
                    MessageBox.Show("numéro introuvable");
                    RemplirDvdListeComplete();
                }
            }
            else
            {
                RemplirDvdListeComplete();
            }
        }

        /// <summary>
        /// Recherche et affichage des Dvd dont le titre matche acec la saisie.
        /// Cette procédure est exécutée à chaque ajout ou suppression de caractère
        /// dans le textBox de saisie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbDvdTitreRecherche_TextChanged(object sender, EventArgs e)
        {
            if (!txbDvdTitreRecherche.Text.Equals(""))
            {
                cbxDvdGenres.SelectedIndex = -1;
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
                txbDvdNumRecherche.Text = "";
                List<Dvd> lesDvdParTitre;
                lesDvdParTitre = lesDvd.FindAll(x => x.Titre.ToLower().Contains(txbDvdTitreRecherche.Text.ToLower()));
                RemplirDvdListe(lesDvdParTitre);
            }
            else
            {
                // si la zone de saisie est vide et aucun élément combo sélectionné, réaffichage de la liste complète
                if (cbxDvdGenres.SelectedIndex < 0 && cbxDvdPublics.SelectedIndex < 0 && cbxDvdRayons.SelectedIndex < 0
                    && txbDvdNumRecherche.Text.Equals(""))
                {
                    RemplirDvdListeComplete();
                }
            }
        }

        /// <summary>
        /// Affichage des informations du dvd sélectionné
        /// </summary>
        /// <param name="dvd"></param>
        private void AfficheDvdInfos(Dvd dvd)
        {
            txbDvdRealisateur.Text = dvd.Realisateur;
            txbDvdSynopsis.Text = dvd.Synopsis;
            txbDvdImage.Text = dvd.Image;
            txbDvdDuree.Text = dvd.Duree.ToString() ;
            txbDvdNumero.Text = dvd.Id;
            txbDvdGenre.Text = dvd.Genre;
            txbDvdPublic.Text = dvd.Public;
            txbDvdRayon.Text = dvd.Rayon;
            txbDvdTitre.Text = dvd.Titre;
            string image = dvd.Image;
            try
            {
                pcbDvdImage.Image = Image.FromFile(image);
            }
            catch 
            {
                pcbDvdImage.Image = null;
            }
        }

        /// <summary>
        /// Vide les zones d'affichage des informations du dvd
        /// </summary>
        private void VideDvdInfos()
        {
            txbDvdRealisateur.Text = "";
            txbDvdSynopsis.Text = "";
            txbDvdImage.Text = "";
            txbDvdDuree.Text = "";
            txbDvdNumero.Text = "";
            txbDvdGenre.Text = "";
            txbDvdPublic.Text = "";
            txbDvdRayon.Text = "";
            txbDvdTitre.Text = "";
            pcbDvdImage.Image = null;
        }

        /// <summary>
        /// Filtre sur le genre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxDvdGenres_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxDvdGenres.SelectedIndex >= 0)
            {
                txbDvdTitreRecherche.Text = "";
                txbDvdNumRecherche.Text = "";
                Genre genre = (Genre)cbxDvdGenres.SelectedItem;
                List<Dvd> Dvd = lesDvd.FindAll(x => x.Genre.Equals(genre.Libelle));
                RemplirDvdListe(Dvd);
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur la catégorie de public
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxDvdPublics_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxDvdPublics.SelectedIndex >= 0)
            {
                txbDvdTitreRecherche.Text = "";
                txbDvdNumRecherche.Text = "";
                Public lePublic = (Public)cbxDvdPublics.SelectedItem;
                List<Dvd> Dvd = lesDvd.FindAll(x => x.Public.Equals(lePublic.Libelle));
                RemplirDvdListe(Dvd);
                cbxDvdRayons.SelectedIndex = -1;
                cbxDvdGenres.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Filtre sur le rayon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxDvdRayons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbxDvdRayons.SelectedIndex >= 0)
            {
                txbDvdTitreRecherche.Text = "";
                txbDvdNumRecherche.Text = "";
                Rayon rayon = (Rayon)cbxDvdRayons.SelectedItem;
                List<Dvd> Dvd = lesDvd.FindAll(x => x.Rayon.Equals(rayon.Libelle));
                RemplirDvdListe(Dvd);
                cbxDvdGenres.SelectedIndex = -1;
                cbxDvdPublics.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Sur la sélection d'une ligne ou cellule dans le grid
        /// affichage des informations du dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvDvdListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDvdListe.CurrentCell != null)
            {
                try
                {
                    Dvd dvd = (Dvd)bdgDvdListe.List[bdgDvdListe.Position];
                    AfficheDvdInfos(dvd);
                }
                catch
                {
                    VideDvdZones();
                }
            }
            else
            {
                VideDvdInfos();
            }
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des Dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdAnnulPublics_Click(object sender, EventArgs e)
        {
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des Dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdAnnulRayons_Click(object sender, EventArgs e)
        {
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Sur le clic du bouton d'annulation, affichage de la liste complète des Dvd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDvdAnnulGenres_Click(object sender, EventArgs e)
        {
            RemplirDvdListeComplete();
        }

        /// <summary>
        /// Affichage de la liste complète des Dvd
        /// et annulation de toutes les recherches et filtres
        /// </summary>
        private void RemplirDvdListeComplete()
        {
            RemplirDvdListe(lesDvd);
            VideDvdZones();
        }

        /// <summary>
        /// vide les zones de recherche et de filtre
        /// </summary>
        private void VideDvdZones()
        {
            cbxDvdGenres.SelectedIndex = -1;
            cbxDvdRayons.SelectedIndex = -1;
            cbxDvdPublics.SelectedIndex = -1;
            txbDvdNumRecherche.Text = "";
            txbDvdTitreRecherche.Text = "";
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvDvdListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            VideDvdZones();
            string titreColonne = dgvDvdListe.Columns[e.ColumnIndex].HeaderText;
            List<Dvd> sortedList = new List<Dvd>();
            switch (titreColonne)
            {
                case "Id":
                    sortedList = lesDvd.OrderBy(o => o.Id).ToList();
                    break;
                case "Titre":
                    sortedList = lesDvd.OrderBy(o => o.Titre).ToList();
                    break;
                case "Duree":
                    sortedList = lesDvd.OrderBy(o => o.Duree).ToList();
                    break;
                case "Realisateur":
                    sortedList = lesDvd.OrderBy(o => o.Realisateur).ToList();
                    break;
                case "Genre":
                    sortedList = lesDvd.OrderBy(o => o.Genre).ToList();
                    break;
                case "Public":
                    sortedList = lesDvd.OrderBy(o => o.Public).ToList();
                    break;
                case "Rayon":
                    sortedList = lesDvd.OrderBy(o => o.Rayon).ToList();
                    break;
            }
            RemplirDvdListe(sortedList);
        }

        #endregion


        #region Réception Exemplaire de presse
        //-----------------------------------------------------------
        // ONGLET "RECEPTION DE REVUES"
        //-----------------------------------------------------------

        /// <summary>
        /// Ouverture de l'onglet : blocage en saisie des champs de saisie des infos de l'exemplaire
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabReceptionRevue_Enter(object sender, EventArgs e)
        {
            lesRevues = controle.GetAllRevues();
            accesReceptionExemplaireGroupBox(false);
        }

        /// <summary>
        /// Remplit le dategrid avec la liste reçue en paramètre
        /// </summary>
        private void RemplirReceptionExemplairesListe(List<Exemplaire> exemplaires)
        {
            bdgExemplairesListe.DataSource = exemplaires;
            dgvReceptionExemplairesListe.DataSource = bdgExemplairesListe;
            dgvReceptionExemplairesListe.Columns["idEtat"].Visible = false;
            dgvReceptionExemplairesListe.Columns["idDocument"].Visible = false;
            dgvReceptionExemplairesListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReceptionExemplairesListe.Columns["numero"].DisplayIndex = 0;
            dgvReceptionExemplairesListe.Columns["dateAchat"].DisplayIndex = 1;
        }

        /// <summary>
        /// Recherche d'un numéro de revue et affiche ses informations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionRechercher_Click(object sender, EventArgs e)
        {
            if (!txbReceptionRevueNumero.Text.Equals(""))
            {
                Revue revue = lesRevues.Find(x => x.Id.Equals(txbReceptionRevueNumero.Text));
                if (revue != null)
                {
                    AfficheReceptionRevueInfos(revue);
                }
                else
                {
                    MessageBox.Show("numéro introuvable");
                    VideReceptionRevueInfos();
                }
            }
            else
            {
                VideReceptionRevueInfos();
            }
        }

        /// <summary>
        /// Si le numéro de revue est modifié, la zone de l'exemplaire est vidée et inactive
        /// les informations de la revue son aussi effacées
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbReceptionRevueNumero_TextChanged(object sender, EventArgs e)
        {
            accesReceptionExemplaireGroupBox(false);
            VideReceptionRevueInfos();
        }

        /// <summary>
        /// Affichage des informations de la revue sélectionnée et les exemplaires
        /// </summary>
        /// <param name="revue"></param>
        private void AfficheReceptionRevueInfos(Revue revue)
        {
            // informations sur la revue
            txbReceptionRevuePeriodicite.Text = revue.Periodicite;
            chkReceptionRevueEmpruntable.Checked = revue.Empruntable;
            txbReceptionRevueImage.Text = revue.Image;
            txbReceptionRevueDelaiMiseADispo.Text = revue.DelaiMiseADispo.ToString();
            txbReceptionRevueNumero.Text = revue.Id;
            txbReceptionRevueGenre.Text = revue.Genre;
            txbReceptionRevuePublic.Text = revue.Public;
            txbReceptionRevueRayon.Text = revue.Rayon;
            txbReceptionRevueTitre.Text = revue.Titre;         
            string image = revue.Image;
            try
            {
                pcbReceptionRevueImage.Image = Image.FromFile(image);
            }
            catch 
            {
                pcbReceptionRevueImage.Image = null;
            }
            // affiche la liste des exemplaires de la revue
            afficheReceptionExemplairesRevue();
            // accès à la zone d'ajout d'un exemplaire
            accesReceptionExemplaireGroupBox(true);
        }

        private void afficheReceptionExemplairesRevue()
        {
            string idDocuement = txbReceptionRevueNumero.Text;
            lesExemplaires = controle.GetExemplairesRevue(idDocuement);
            RemplirReceptionExemplairesListe(lesExemplaires);
        }

        /// <summary>
        /// Vide les zones d'affchage des informations de la revue
        /// </summary>
        private void VideReceptionRevueInfos()
        {
            txbReceptionRevuePeriodicite.Text = "";
            chkReceptionRevueEmpruntable.Checked = false;
            txbReceptionRevueImage.Text = "";
            txbReceptionRevueDelaiMiseADispo.Text = "";
            txbReceptionRevueGenre.Text = "";
            txbReceptionRevuePublic.Text = "";
            txbReceptionRevueRayon.Text = "";
            txbReceptionRevueTitre.Text = "";
            pcbReceptionRevueImage.Image = null;
            lesExemplaires = new List<Exemplaire>();
            RemplirReceptionExemplairesListe(lesExemplaires);
            accesReceptionExemplaireGroupBox(false);
        }

        /// <summary>
        /// Vide les zones d'affichage des informations de l'exemplaire
        /// </summary>
        private void VideReceptionExemplaireInfos()
        {
            txbReceptionExemplaireImage.Text = "";
            txbReceptionExemplaireNumero.Text = "";
            pcbReceptionExemplaireImage.Image = null;
            dtpReceptionExemplaireDate.Value = DateTime.Now;
        }

        /// <summary>
        /// Permet ou interdit l'accès à la gestion de la réception d'un exemplaire
        /// et vide les objets graphiques
        /// </summary>
        /// <param name="acces"></param>
        private void accesReceptionExemplaireGroupBox(bool acces)
        {
            VideReceptionExemplaireInfos();
            grpReceptionExemplaire.Enabled = acces;
        }

        /// <summary>
        /// Recherche image sur disque (pour l'exemplaire)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionExemplaireImage_Click(object sender, EventArgs e)
        {
            string filePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = DOSSIERMEDIARECHERCHE,
                Filter = "Files|*.jpg;*.bmp;*.jpeg;*.png;*.gif"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
            }
            txbReceptionExemplaireImage.Text = filePath;         
            try
            {
                pcbReceptionExemplaireImage.Image = Image.FromFile(filePath);
            }
            catch 
            {
                pcbReceptionExemplaireImage.Image = null;
            }
        }

        /// <summary>
        /// Enregistrement du nouvel exemplaire
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReceptionExemplaireValider_Click(object sender, EventArgs e)
        {
            if (!txbReceptionExemplaireNumero.Text.Equals(""))
            {
                try
                {
                    int numero = int.Parse(txbReceptionExemplaireNumero.Text);
                    DateTime dateAchat = dtpReceptionExemplaireDate.Value;
                    string photo = txbReceptionExemplaireImage.Text;
                    string idEtat = ETATNEUF;
                    string idDocument = txbReceptionRevueNumero.Text;
                    Exemplaire exemplaire = new Exemplaire(numero, dateAchat, photo, idEtat, idDocument);
                    if (controle.CreerExemplaire(exemplaire))
                    {
                        VideReceptionExemplaireInfos();
                        afficheReceptionExemplairesRevue();
                    }
                    else
                    {
                        MessageBox.Show("numéro de publication déjà existant", "Erreur");
                    }
                }catch
                {
                    MessageBox.Show("le numéro de parution doit être numérique", "Information");
                    txbReceptionExemplaireNumero.Text = "";
                    txbReceptionExemplaireNumero.Focus();
                }
            }
            else
            {
                MessageBox.Show("numéro de parution obligatoire", "Information");
            }
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvExemplairesListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string titreColonne = dgvReceptionExemplairesListe.Columns[e.ColumnIndex].HeaderText;
            List<Exemplaire> sortedList = new List<Exemplaire>();
            switch (titreColonne)
            {
                case "Numero":
                    sortedList = lesExemplaires.OrderBy(o => o.Numero).Reverse().ToList();
                    break;
                case "DateAchat":
                    sortedList = lesExemplaires.OrderBy(o => o.DateAchat).Reverse().ToList();
                    break;
                case "Photo":
                    sortedList = lesExemplaires.OrderBy(o => o.Photo).ToList();
                    break;
            }
            RemplirReceptionExemplairesListe(sortedList);
        }

        /// <summary>
        /// Sélection d'une ligne complète et affichage de l'image sz l'exemplaire
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvReceptionExemplairesListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvReceptionExemplairesListe.CurrentCell != null)
            {
                Exemplaire exemplaire = (Exemplaire)bdgExemplairesListe.List[bdgExemplairesListe.Position];
                string image = exemplaire.Photo;
                try
                {
                    pcbReceptionExemplaireRevueImage.Image = Image.FromFile(image);
                }
                catch
                {
                    pcbReceptionExemplaireRevueImage.Image = null;
                }
            }
            else
            {
                pcbReceptionExemplaireRevueImage.Image = null;
            }
        }

        #endregion

        #region Commande Livres
        //-----------------------------------------------------------
        // ONGLET "COMMANDES LIVRES"
        //-----------------------------------------------------------

        /// <summary>
        /// Ouverture de l'onglet Commande Livres
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabCommandeLivres_Enter(object sender, EventArgs e)
        {
            lesLivres = controle.GetAllLivres();
            lesSuivis = controle.GetAllSuivis();
            AccesGestionCommandeLivres(false);
            AccesDetailsCommandeLivres(false);
            txbCommandeLivresNumero.Text = "";
            VideCommandeLivresInfos();
            VideDetailsCommandeLivres();
            dgvCommandeLivresListe.DataSource = null;
        }

        /// <summary>
        /// Recherche d'un livre à partir du numéro et affiche les informations
        /// </summary>
        private void CommandeLivresRechercher()
        {
            if (!txbCommandeLivresNumero.Text.Equals(""))
            {
                Livre livre = lesLivres.Find(x => x.Id.Equals(txbCommandeLivresNumero.Text.Trim()));
                if (livre != null)
                {
                    AfficheCommandeLivresInfos(livre);
                }
                else
                {
                    MessageBox.Show("Numéro introuvable");
                    txbCommandeLivresNumero.Text = "";
                    txbCommandeLivresNumero.Focus();
                    VideCommandeLivresInfos();
                }
            }
            else
            {
                VideCommandeLivresInfos();
            }
        }

        /// <summary>
        /// Affiche les informations du livre
        /// </summary>
        /// <param name="livre">Le livre sélectionné</param>
        private void AfficheCommandeLivresInfos(Livre livre)
        {
            // affiche les informations
            txbCommandeLivresTitre.Text = livre.Titre;
            txbCommandeLivresAuteur.Text = livre.Auteur;
            txbCommandeLivresCollection.Text = livre.Collection;
            txbCommandeLivresGenre.Text = livre.Genre;
            txbCommandeLivresPublic.Text = livre.Public;
            txbCommandeLivresRayon.Text = livre.Rayon;
            txbCommandeLivresImage.Text = livre.Image;
            txbCommandeLivresISBN.Text = livre.Isbn;
            string image = livre.Image;
            try
            {
                pcbCommandeLivresImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbCommandeLivresImage.Image = null;
            }
            // affiche la liste des commandes
            AfficheCommandeDocumentLivre();

            if(dgvCommandeLivresListe.RowCount != 0)
            {
                // active la zone de gestion des commandes
                AccesGestionCommandeLivres(true);
            }
        }

        /// <summary>
        /// Récupère, affiche les commandes d'un livre
        /// </summary>
        private void AfficheCommandeDocumentLivre()
        {
            string idDocument = txbCommandeLivresNumero.Text.Trim();
            lesCommandeDocument = controle.GetCommandeDocument(idDocument);
            RemplirCommandeLivresListe(lesCommandeDocument);
            AfficheCommandeLivresDetailSelect();
        }

        /// <summary>
        /// Affiche le détail de la commande sélectionnée
        /// </summary>
        private void AfficheCommandeLivresDetailSelect()
        {
            if (dgvCommandeLivresListe.CurrentCell != null)
            {
                CommandeDocument commandeDocument = (CommandeDocument)bdgCommandesLivresListe.List[bdgCommandesLivresListe.Position];
                AfficheCommandeLivresDetails(commandeDocument);
                AccesBtnModificationCommandeLivres(commandeDocument);
            }
            else
            {
                AccesGestionCommandeLivres(false);
                VideDetailsCommandeLivres();
            }
        }

        /// <summary>
        /// Affiche les détails d'une commande de livre
        /// </summary>
        /// <param name="commandeDocument">Commande concernée</param>
        private void AfficheCommandeLivresDetails(CommandeDocument commandeDocument)
        {
            txbCommandeLivresNumeroCommande.Text = commandeDocument.Id;
            dtpCommandeLivresDateCommande.Value = commandeDocument.DateCommande;
            nudCommandeLivresExemplaires.Value = commandeDocument.NbExemplaires;
            txbCommandeLivresMontant.Text = commandeDocument.Montant.ToString("C2", CultureInfo.CreateSpecificCulture("fr-FR"));
        }

        /// <summary>
        /// Remplit le dategrid avec la collection reçue en paramètre
        /// </summary>
        /// <param name="lesCommandeDocument">Collection de CommandeDocument</param>
        private void RemplirCommandeLivresListe(List<CommandeDocument> lesCommandeDocument)
        {
            bdgCommandesLivresListe.DataSource = lesCommandeDocument;
            dgvCommandeLivresListe.DataSource = bdgCommandesLivresListe;
            dgvCommandeLivresListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCommandeLivresListe.Columns["id"].Visible = false;
            dgvCommandeLivresListe.Columns["idSuivi"].Visible = false;
            dgvCommandeLivresListe.Columns["idLivreDvd"].Visible = false;
            dgvCommandeLivresListe.Columns["dateCommande"].DisplayIndex = 0;
            dgvCommandeLivresListe.Columns[5].HeaderCell.Value = "Date";
            dgvCommandeLivresListe.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCommandeLivresListe.Columns["montant"].DisplayIndex = 1;
            dgvCommandeLivresListe.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCommandeLivresListe.Columns[6].DefaultCellStyle.Format = "c2";
            dgvCommandeLivresListe.Columns[6].DefaultCellStyle.FormatProvider = CultureInfo.GetCultureInfo("fr-FR");
            dgvCommandeLivresListe.Columns[0].HeaderCell.Value = "Exemplaires";
            dgvCommandeLivresListe.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCommandeLivresListe.Columns[2].HeaderCell.Value = "Etat";
            dgvCommandeLivresListe.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        /// <summary>
        /// Evénement clic sur le bouton de recherche de livre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeLivresRechercher_Click(object sender, EventArgs e)
        {
            CommandeLivresRechercher();
        }

        /// <summary>
        /// Evénement sur la touche entrer déclenche la recherche
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbCommandeLivresNumero_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnCommandeLivresRechercher_Click(sender, e);
            }
        }

        /// <summary>
        /// Evénement sur la saisie du numéro du livre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbCommandeLivresNumero_TextChanged(object sender, EventArgs e)
        {
            AccesGestionCommandeLivres(false);
            VideCommandeLivresInfos();
        }

        /// <summary>
        /// Evénement sur le changement de ligne, réaffiche les infos du livre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCommandeLivresListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCommandeLivresListe.CurrentCell != null)
            {
                AfficheCommandeLivresDetailSelect();
            }
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCommandeLivresListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string titreColonne = dgvCommandeLivresListe.Columns[e.ColumnIndex].HeaderText;
            RemplirCommandeLivresListe(TriCommandeDocumentList(titreColonne));
        }

        /// <summary>
        /// Evénement clic sur le bouton d'ajout de commande
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeLivresAjouter_Click(object sender, EventArgs e)
        {
            AccesDetailsCommandeLivres(true);
            AccesModificationCommandeLivres(true);
        }

        /// <summary>
        /// Evénement clic sur le bouton valider une commande
        /// Enregistrement d'une commande à condition que tous les champs soient remplis et valides
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeLivresValider_Click(object sender, EventArgs e)
        {
            if (txbCommandeLivresNumeroCommande.Text == "" || txbCommandeLivresMontant.Text == "")
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Information");
                return;
            }

            String id = txbCommandeLivresNumeroCommande.Text;
            DateTime dateCommande = dtpCommandeLivresDateCommande.Value;
            int nbExemplaires = (int)nudCommandeLivresExemplaires.Value;
            string idLivreDvd = txbCommandeLivresNumero.Text.Trim();
            int idSuivi = lesSuivis[0].Id;
            string libelleSuivi = lesSuivis[0].Libelle;
            String montantSaisie = txbCommandeLivresMontant.Text.Replace('.', ',');

            // validation du champ montant
            if (!Double.TryParse(montantSaisie, out double montant))
            {
                MessageBox.Show("Le montant doit être numérique.", "Erreur");
                txbCommandeLivresMontant.Text = "";
                txbCommandeLivresMontant.Focus();
                return;
            }
            CommandeDocument laCommandeDocument = new CommandeDocument(id, dateCommande, montant, nbExemplaires, idLivreDvd, idSuivi, libelleSuivi);
            if (txbCommandeLivresNumeroCommande.TextLength <= 5) {
                if (controle.CreerCommandeDocument(laCommandeDocument))
                {
                    AfficheCommandeDocumentLivre();

                    // sélectionne la commande nouvellement créée
                    int addedRowIndex = -1;
                    DataGridViewRow row = dgvCommandeLivresListe.Rows
                        .Cast<DataGridViewRow>()
                        .First(r => r.Cells["id"].Value.ToString().Equals(id));
                    addedRowIndex = row.Index;
                    dgvCommandeLivresListe.Rows[addedRowIndex].Selected = true;

                    AccesDetailsCommandeLivres(false);
                    AfficheCommandeLivresDetails(laCommandeDocument);
                    AccesGestionCommandeLivres(true);
                }
                else
                {
                    MessageBox.Show("Ce numéro de commande existe déjà.", "Erreur");
                    txbCommandeLivresNumeroCommande.Text = "";
                    txbCommandeLivresNumeroCommande.Focus();
                }
            }
            else
            {
                MessageBox.Show("Le numéro de commande ne doit pas dépasser 5 caractères.", "Erreur");
                txbCommandeLivresNumeroCommande.Text = "";
                txbCommandeLivresNumeroCommande.Focus();
            }
        }

        /// <summary>
        /// Evénement sur le bouton annuler la saisie d'une nouvelle commande
        /// à condition que l'utilisateur le confirme
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeLivresAnnuler_Click(object sender, EventArgs e)
        {
            if(!(txbCommandeLivresNumeroCommande.Text == "" && txbCommandeLivresMontant.Text == ""))
            {
                if (ConfirmationAnnulationCommande())
                {
                    AccesDetailsCommandeLivres(false);
                    AfficheCommandeDocumentLivre();
                    AccesGestionCommandeLivres(true);
                }
            }
            else
            {
                AccesDetailsCommandeLivres(false);
                AfficheCommandeDocumentLivre();
                AccesGestionCommandeLivres(true);
            }
        }

        /// <summary>
        /// Evénement clic sur le bouton supprimer une commande
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeLivresSupprimer_Click(object sender, EventArgs e)
        {
            if (ConfirmationSupprCommande())
            {
                CommandeDocument commandeDocument = (CommandeDocument)bdgCommandesLivresListe.Current;
                if (controle.SupprCommandeDocument(commandeDocument.Id))
                {
                    AfficheCommandeDocumentLivre();
                }
                else
                {
                    MessageBox.Show("Une erreur s'est produite.", "Erreur");
                }
            }
        }

        /// <summary>
        /// Modifie l'état de la commande à : rélancée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeLivresRelancer_Click(object sender, EventArgs e)
        {
            CommandeDocument commandeDocument = (CommandeDocument)bdgCommandesLivresListe.List[bdgCommandesLivresListe.Position];
            Suivi nouveauSuivi = lesSuivis.Find(suivi => suivi.Libelle == "relancée");
            ModifEtatSuiviCommandeDocumentLivre(commandeDocument.Id, nouveauSuivi);
        }

        /// <summary>
        /// Modifie l'état de la commande à : livrée
        /// Notifie la création des exemplaires
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeLivresConfirmerLivraison_Click(object sender, EventArgs e)
        {
            CommandeDocument commandeDocument = (CommandeDocument)bdgCommandesLivresListe.List[bdgCommandesLivresListe.Position];
            Suivi nouveauSuivi = lesSuivis.Find(suivi => suivi.Libelle == "livrée");
            if (ModifEtatSuiviCommandeDocumentLivre(commandeDocument.Id, nouveauSuivi))
            {
                MessageBox.Show("Les exemplaires ont été ajoutés dans la base de données.", "Information");
            }
        }

        /// <summary>
        /// Modifie l'état de la commande à : réglée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeLivresRegler_Click(object sender, EventArgs e)
        {
            CommandeDocument commandeDocument = (CommandeDocument)bdgCommandesLivresListe.List[bdgCommandesLivresListe.Position];
            Suivi nouveauSuivi = lesSuivis.Find(suivi => suivi.Libelle == "réglée");
            ModifEtatSuiviCommandeDocumentLivre(commandeDocument.Id, nouveauSuivi);
        }

        /// <summary>
        /// Demande de modification de l'état de suivi au contrôleur après validation utilisateur
        /// </summary>
        /// <param name="idCommandeDocument">identifiant du document concerné</param>
        /// <param name="nouveauSuivi">nouvel état de suivi</param>
        /// <returns>True si modification a réussi</returns>
        private bool ModifEtatSuiviCommandeDocumentLivre(string idCommandeDocument, Suivi nouveauSuivi)
        {
            if (ConfirmationModifSuiviCommande(nouveauSuivi.Libelle))
            {
                if (controle.ModifSuiviCommandeDocument(idCommandeDocument, nouveauSuivi.Id))
                {
                    AfficheCommandeDocumentLivre();
                    return true;
                }
                else
                {
                    MessageBox.Show("Une erreur s'est produite.", "Erreur");
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Vide les zones d'affichage des informations du livre
        /// </summary>
        private void VideCommandeLivresInfos()
        {
            txbCommandeLivresTitre.Text = "";
            txbCommandeLivresAuteur.Text = "";
            txbCommandeLivresCollection.Text = "";
            txbCommandeLivresGenre.Text = "";
            txbCommandeLivresPublic.Text = "";
            txbCommandeLivresRayon.Text = "";
            txbCommandeLivresImage.Text = "";
            txbCommandeLivresISBN.Text = "";
            pcbCommandeLivresImage.Image = null;
        }

        /// <summary>
        /// Vide les zones d'affichage des détails de commande
        /// </summary>
        private void VideDetailsCommandeLivres()
        {
            txbCommandeLivresNumeroCommande.Text = "";
            dtpCommandeLivresDateCommande.Value = DateTime.Now;
            nudCommandeLivresExemplaires.Value = 1;
            txbCommandeLivresMontant.Text = "";
        }

        /// <summary>
        /// Active/Désactive la zone de gestion des commandes et bouton ajouter
        /// </summary>
        /// <param name="acces">true autorise l'accès</param>
        private void AccesGestionCommandeLivres(bool acces)
        {
            grpGestionCommandeLivres.Enabled = acces;
            btnCommandeLivresAjouter.Enabled = acces;
        }

        /// <summary>
        /// Active/Désactive la zone détails d'une commande et les boutons (valider, annuler, ajouter)
        /// </summary>
        /// <param name="acces">True active les boutons Valider et Annuler, désactive le bouton Ajouter, dévérouille les champs</param>
        private void AccesDetailsCommandeLivres(bool acces)
        {
            VideDetailsCommandeLivres();
            grpCommandeLivres.Enabled = acces;
            txbCommandeLivresNumeroCommande.Enabled = acces;
            txbCommandeLivresNumeroCommande.Focus();
            dtpCommandeLivresDateCommande.Enabled = acces;
            nudCommandeLivresExemplaires.Enabled = acces;
            txbCommandeLivresMontant.Enabled = acces;
            btnCommandeLivresValider.Enabled = acces;
            btnCommandeLivresAnnuler.Enabled = acces;
            btnCommandeLivresAjouter.Enabled = !acces;
        }

        /// <summary>
        /// Active/Désactive les boutons de gestion de commande (sauf ajout)
        /// </summary>
        private void AccesModificationCommandeLivres(bool acces)
        {
            btnCommandeLivresRelancer.Enabled = acces;
            btnCommandeLivresConfirmerLivraison.Enabled = acces;
            btnCommandeLivresRegler.Enabled = acces;
            btnCommandeLivresSupprimer.Enabled = acces;
        }

        /// <summary>
        /// Active/Désactive les boutons de gestion de commande en fonction de l'état de suivi
        /// </summary>
        /// <param name="commandeDocument">CommandeDocument concernée</param>
        private void AccesBtnModificationCommandeLivres(CommandeDocument commandeDocument)
        {
            string etatSuivi = commandeDocument.LibelleSuivi;
            switch (etatSuivi)
            {
                case "en cours":
                case "relancée":
                    btnCommandeLivresRelancer.Enabled = true;
                    btnCommandeLivresConfirmerLivraison.Enabled = true;
                    btnCommandeLivresRegler.Enabled = false;
                    btnCommandeLivresSupprimer.Enabled = true;
                    break;
                case "livrée":
                    btnCommandeLivresRelancer.Enabled = false;
                    btnCommandeLivresConfirmerLivraison.Enabled = false;
                    btnCommandeLivresRegler.Enabled = true;
                    btnCommandeLivresSupprimer.Enabled = false;
                    break;
                case "réglée":
                    AccesModificationCommandeLivres(false);
                    break;
            }
        }

        #endregion

        #region Commande DVD
        //-----------------------------------------------------------
        // ONGLET "COMMANDES DVD"
        //-----------------------------------------------------------

        /// <summary>
        /// Ouverture de l'onglet Commande DVD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabCommandeDVD_Enter(object sender, EventArgs e)
        {
            lesDvd = controle.GetAllDvd();
            lesSuivis = controle.GetAllSuivis();
            AccesGestionCommandeDvd(false);
            AccesDetailsCommandeDvd(false);
            txbCommandeDvdNumero.Text = "";
            VideCommandeDvdInfos();
            VideDetailsCommandeDvd();
            dgvCommandeDvdListe.DataSource = null;
        }

        /// <summary>
        /// Recherche d'un DVD à partir du numéro et affichage les informations
        /// </summary>
        private void CommandeDvdRechercher()
        {
            if (!txbCommandeDvdNumero.Text.Equals(""))
            {
                Dvd dvd = lesDvd.Find(x => x.Id.Equals(txbCommandeDvdNumero.Text.Trim()));
                if (dvd != null)
                {
                    AfficheCommandeDvdInfos(dvd);
                }
                else
                {
                    MessageBox.Show("Numéro introuvable");
                    txbCommandeDvdNumero.Text = "";
                    txbCommandeDvdNumero.Focus();
                    VideCommandeDvdInfos();
                }
            }
            else
            {
                VideCommandeDvdInfos();
            }
        }

        /// <summary>
        /// Affiche les informations du DVD
        /// </summary>
        /// <param name="dvd">Le DVD sélectionné</param>
        private void AfficheCommandeDvdInfos(Dvd dvd)
        {
            // affiche les informations
            txbCommandeDvdTitre.Text = dvd.Titre;
            txbCommandeDvdRealisateur.Text = dvd.Realisateur;
            txbCommandeDvdSynopsis.Text = dvd.Synopsis;
            txbCommandeDvdGenre.Text = dvd.Genre;
            txbCommandeDvdPublic.Text = dvd.Public;
            txbCommandeDvdRayon.Text = dvd.Rayon;
            txbCommandeDvdImage.Text = dvd.Image;
            txbCommandeDvdDuree.Text = dvd.Duree.ToString();
            string image = dvd.Image;
            try
            {
                pcbCommandeDvdImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbCommandeDvdImage.Image = null;
            }
            // affiche la liste des commandes
            AfficheCommandeDocumentDvd();

            if (dgvCommandeDvdListe.RowCount != 0)
            {
                // active la zone de gestion des commandes
                AccesGestionCommandeDvd(true);
            }
        }

        /// <summary>
        /// Récupère, affiche les commandes d'un DVD
        /// </summary>
        private void AfficheCommandeDocumentDvd()
        {
            string idDocument = txbCommandeDvdNumero.Text.Trim();
            lesCommandeDocument = controle.GetCommandeDocument(idDocument);
            RemplirCommandeDvdListe(lesCommandeDocument);
            AfficheCommandeDvdDetailSelect();
        }

        /// <summary>
        /// Affiche le détail de la commande sélectionnée
        /// </summary>
        private void AfficheCommandeDvdDetailSelect()
        {
            if (dgvCommandeDvdListe.CurrentCell != null)
            {
                CommandeDocument commandeDocument = (CommandeDocument)bdgCommandesDvdListe.List[bdgCommandesDvdListe.Position];
                AfficheCommandeDvdDetails(commandeDocument);
                AccesBtnModificationCommandeDvd(commandeDocument);
            }
            else
            {
                AccesGestionCommandeDvd(false);
                VideDetailsCommandeDvd();
            }
        }

        /// <summary>
        /// Affiche les détails d'une commande de DVD
        /// </summary>
        /// <param name="commandeDocument">Commande concernée</param>
        private void AfficheCommandeDvdDetails(CommandeDocument commandeDocument)
        {
            txbCommandeDvdNumeroCommande.Text = commandeDocument.Id;
            dtpCommandeDvdDateCommande.Value = commandeDocument.DateCommande;
            nudCommandeDvdExemplaires.Value = commandeDocument.NbExemplaires;
            txbCommandeDvdMontant.Text = commandeDocument.Montant.ToString("C2", CultureInfo.CreateSpecificCulture("fr-FR"));
        }

        /// <summary>
        /// Remplit le dategrid avec la collection reçue en paramètre
        /// </summary>
        /// <param name="lesCommandeDocument">Collection de CommandeDocument</param>
        private void RemplirCommandeDvdListe(List<CommandeDocument> lesCommandeDocument)
        {
            bdgCommandesDvdListe.DataSource = lesCommandeDocument;
            dgvCommandeDvdListe.DataSource = bdgCommandesDvdListe;
            dgvCommandeDvdListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCommandeDvdListe.Columns["id"].Visible = false;
            dgvCommandeDvdListe.Columns["idSuivi"].Visible = false;
            dgvCommandeDvdListe.Columns["idLivreDvd"].Visible = false;
            dgvCommandeDvdListe.Columns["dateCommande"].DisplayIndex = 0;
            dgvCommandeDvdListe.Columns[5].HeaderCell.Value = "Date";
            dgvCommandeDvdListe.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCommandeDvdListe.Columns["montant"].DisplayIndex = 1;
            dgvCommandeDvdListe.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCommandeDvdListe.Columns[6].DefaultCellStyle.Format = "c2";
            dgvCommandeDvdListe.Columns[6].DefaultCellStyle.FormatProvider = CultureInfo.GetCultureInfo("fr-FR");
            dgvCommandeDvdListe.Columns[0].HeaderCell.Value = "Exemplaires";
            dgvCommandeDvdListe.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCommandeDvdListe.Columns[2].HeaderCell.Value = "Etat";
            dgvCommandeDvdListe.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        /// <summary>
        /// Evénement clic sur le bouton de recherche de DVD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeDvdRechercher_Click(object sender, EventArgs e)
        {
            CommandeDvdRechercher();
        }

        /// <summary>
        /// Evénement sur la touche entrer déclenche la recherche
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbCommandeDvdNumero_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnCommandeDvdRechercher_Click(sender, e);
            }
        }

        /// <summary>
        /// Evénement sur la saisie du numéro du DVD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbCommandeDvdNumero_TextChanged(object sender, EventArgs e)
        {
            AccesGestionCommandeDvd(false);
            VideCommandeDvdInfos();
        }

        /// <summary>
        /// Evénement sur le changement de ligne, réaffiche les infos du DVD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCommandeDvdListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCommandeDvdListe.CurrentCell != null)
            {
                AfficheCommandeDvdDetailSelect();
            }
        }

        /// <summary>
        /// Tri sur les colonnes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCommandeDvdListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string titreColonne = dgvCommandeDvdListe.Columns[e.ColumnIndex].HeaderText;
            RemplirCommandeDvdListe(TriCommandeDocumentList(titreColonne));
        }

        /// <summary>
        /// Evénement clic sur le bouton d'ajout de commande
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeDvdAjouter_Click(object sender, EventArgs e)
        {
            AccesDetailsCommandeDvd(true);
            AccesModificationCommandeDvd(true);
        }

        /// <summary>
        /// Evénement clic sur le bouton valider une commande
        /// Enregistrement d'une commande à condition que tous les champs soient remplis et valides
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeDvdValider_Click(object sender, EventArgs e)
        {
            if (txbCommandeDvdNumeroCommande.Text == "" || txbCommandeDvdMontant.Text == "")
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Information");
                return;
            }
            String id = txbCommandeDvdNumeroCommande.Text;
            DateTime dateCommande = dtpCommandeDvdDateCommande.Value;
            int nbExemplaires = (int)nudCommandeDvdExemplaires.Value;
            string idLivreDvd = txbCommandeDvdNumero.Text.Trim();
            int idSuivi = lesSuivis[0].Id;
            string libelleSuivi = lesSuivis[0].Libelle;
            String montantSaisie = txbCommandeDvdMontant.Text.Replace('.', ',');
            // validation du champ montant
            if (!Double.TryParse(montantSaisie, out double montant))
            {
                MessageBox.Show("Le montant doit être numérique.", "Erreur");
                txbCommandeDvdMontant.Text = "";
                txbCommandeDvdMontant.Focus();
                return;
            }
            CommandeDocument laCommandeDocument = new CommandeDocument(id, dateCommande, montant, nbExemplaires, idLivreDvd, idSuivi, libelleSuivi);
            if(txbCommandeDvdNumeroCommande.TextLength <= 5)
            {
                if (controle.CreerCommandeDocument(laCommandeDocument))
                {
                    AfficheCommandeDocumentDvd();
                    // sélectionne la commande nouvellement créée
                    int addedRowIndex = -1;
                    DataGridViewRow row = dgvCommandeDvdListe.Rows
                        .Cast<DataGridViewRow>()
                        .First(r => r.Cells["id"].Value.ToString().Equals(id));
                    addedRowIndex = row.Index;
                    dgvCommandeDvdListe.Rows[addedRowIndex].Selected = true;

                    AccesDetailsCommandeDvd(false);
                    AfficheCommandeDvdDetails(laCommandeDocument);
                    AccesGestionCommandeDvd(true);
                }
                else
                {
                    MessageBox.Show("Ce numéro de commande existe déjà.", "Erreur");
                    txbCommandeDvdNumeroCommande.Text = "";
                    txbCommandeDvdNumeroCommande.Focus();
                }
            }
            else
            {
                MessageBox.Show("Le numéro de commande ne doit pas dépasser 5 caractères.", "Erreur");
                txbCommandeDvdNumeroCommande.Text = "";
                txbCommandeDvdNumeroCommande.Focus();
            }
        }

        /// <summary>
        /// Evénement sur le bouton annuler la saisie d'une nouvelle commande
        /// à condition que l'utilisateur le confirme
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeDvdAnnuler_Click(object sender, EventArgs e)
        {
            if (!(txbCommandeDvdNumeroCommande.Text == "" && txbCommandeDvdMontant.Text == ""))
            {
                if (ConfirmationAnnulationCommande())
                {
                    AccesDetailsCommandeDvd(false);
                    AfficheCommandeDocumentDvd();
                    AccesGestionCommandeDvd(true);
                }
            }
            else
            {
                AccesDetailsCommandeDvd(false);
                AfficheCommandeDocumentDvd();
                AccesGestionCommandeDvd(true);
            }
        }

        /// <summary>
        /// Evénement clic sur le bouton supprimer une commande
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeDvdSupprimer_Click(object sender, EventArgs e)
        {
            if (ConfirmationSupprCommande())
            {
                CommandeDocument commandeDocument = (CommandeDocument)bdgCommandesDvdListe.Current;
                if (controle.SupprCommandeDocument(commandeDocument.Id))
                {
                    AfficheCommandeDocumentDvd();
                }
                else
                {
                    MessageBox.Show("Une erreur s'est produite.", "Erreur");
                }
            }
        }

        /// <summary>
        /// Modifie l'état de la commande à : rélancée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeDvdRelancer_Click(object sender, EventArgs e)
        {
            CommandeDocument commandeDocument = (CommandeDocument)bdgCommandesDvdListe.List[bdgCommandesDvdListe.Position];
            Suivi nouveauSuivi = lesSuivis.Find(suivi => suivi.Libelle == "relancée");
            ModifEtatSuiviCommandeDocumentDvd(commandeDocument.Id, nouveauSuivi);
        }
        /// <summary>
        /// Modifie l'état de la commande à : livrée
        /// Notifie la création des exemplaires
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeDvdConfirmerLivraison_Click(object sender, EventArgs e)
        {
            CommandeDocument commandeDocument = (CommandeDocument)bdgCommandesDvdListe.List[bdgCommandesDvdListe.Position];
            Suivi nouveauSuivi = lesSuivis.Find(suivi => suivi.Libelle == "livrée");
            if (ModifEtatSuiviCommandeDocumentDvd(commandeDocument.Id, nouveauSuivi))
            {
                MessageBox.Show("Les exemplaires ont été ajoutés dans la base de données.", "Information");
            }
        }
        /// <summary>
        /// Modifie l'état de la commande à : réglée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCommandeDvdRegler_Click(object sender, EventArgs e)
        {
            CommandeDocument commandeDocument = (CommandeDocument)bdgCommandesDvdListe.List[bdgCommandesDvdListe.Position];
            Suivi nouveauSuivi = lesSuivis.Find(suivi => suivi.Libelle == "réglée");
            ModifEtatSuiviCommandeDocumentDvd(commandeDocument.Id, nouveauSuivi);
        }
        /// <summary>
        /// Demande de modification de l'état de suivi au contrôleur après validation utilisateur
        /// </summary>
        /// <param name="idCommandeDocument">identifiant du document concerné</param>
        /// <param name="nouveauSuivi">nouvel état de suivi</param>
        /// <returns>True si modification a réussi</returns>
        private bool ModifEtatSuiviCommandeDocumentDvd(string idCommandeDocument, Suivi nouveauSuivi)
        {
            if (ConfirmationModifSuiviCommande(nouveauSuivi.Libelle))
            {
                if (controle.ModifSuiviCommandeDocument(idCommandeDocument, nouveauSuivi.Id))
                {
                    AfficheCommandeDocumentDvd();
                    return true;
                }
                else
                {
                    MessageBox.Show("Une erreur s'est produite.", "Erreur");
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Vide les zones d'affichage des informations du DVD
        /// </summary>
        private void VideCommandeDvdInfos()
        {
            txbCommandeDvdTitre.Text = "";
            txbCommandeDvdRealisateur.Text = "";
            txbCommandeDvdSynopsis.Text = "";
            txbCommandeDvdGenre.Text = "";
            txbCommandeDvdPublic.Text = "";
            txbCommandeDvdRayon.Text = "";
            txbCommandeDvdImage.Text = "";
            txbCommandeDvdDuree.Text = "";
            pcbCommandeDvdImage.Image = null;
        }

        /// <summary>
        /// Vide les zones d'affichage des détails de commande
        /// </summary>
        private void VideDetailsCommandeDvd()
        {
            txbCommandeDvdNumeroCommande.Text = "";
            dtpCommandeDvdDateCommande.Value = DateTime.Now;
            nudCommandeDvdExemplaires.Value = 1;
            txbCommandeDvdMontant.Text = "";
        }

        /// <summary>
        /// Active/Désactive la zone de gestion des commandes et bouton ajouter
        /// </summary>
        /// <param name="acces">true autorise l'accès</param>
        private void AccesGestionCommandeDvd(bool acces)
        {
            grpGestionCommandeDvd.Enabled = acces;
            btnCommandeDvdAjouter.Enabled = acces;
        }

        /// <summary>
        /// Active/Désactive la zone détails d'une commande et les boutons (valider, annuler, ajouter)
        /// </summary>
        /// <param name="acces">True active les boutons Valider et Annuler, désactive le bouton Ajouter, dévérouille les champs</param>
        private void AccesDetailsCommandeDvd(bool acces)
        {
            VideDetailsCommandeDvd();
            grpCommandeDvd.Enabled = acces;
            txbCommandeDvdNumeroCommande.Enabled = acces;
            txbCommandeDvdNumeroCommande.Focus();
            dtpCommandeDvdDateCommande.Enabled = acces;
            nudCommandeDvdExemplaires.Enabled = acces;
            txbCommandeDvdMontant.Enabled = acces;
            btnCommandeDvdValider.Enabled = acces;
            btnCommandeDvdAnnuler.Enabled = acces;
            btnCommandeDvdAjouter.Enabled = !acces;
        }

        /// <summary>
        /// Active/Désactive les boutons de gestion de commande (sauf ajout)
        /// </summary>
        private void AccesModificationCommandeDvd(bool acces)
        {
            btnCommandeDvdRelancer.Enabled = acces;
            btnCommandeDvdConfirmerLivraison.Enabled = acces;
            btnCommandeDvdRegler.Enabled = acces;
            btnCommandeDvdSupprimer.Enabled = acces;
        }

        /// <summary>
        /// Active/Désactive les boutons de gestion de commande en fonction de l'état de suivi
        /// </summary>
        /// <param name="commandeDocument">CommandeDocument concernée</param>
        private void AccesBtnModificationCommandeDvd(CommandeDocument commandeDocument)
        {
            string etatSuivi = commandeDocument.LibelleSuivi;
            switch (etatSuivi)
            {
                case "en cours":
                case "relancée":
                    btnCommandeDvdRelancer.Enabled = true;
                    btnCommandeDvdConfirmerLivraison.Enabled = true;
                    btnCommandeDvdRegler.Enabled = false;
                    btnCommandeDvdSupprimer.Enabled = true;
                    break;
                case "livrée":
                    btnCommandeDvdRelancer.Enabled = false;
                    btnCommandeDvdConfirmerLivraison.Enabled = false;
                    btnCommandeDvdRegler.Enabled = true;
                    btnCommandeDvdSupprimer.Enabled = false;
                    break;
                case "réglée":
                    AccesModificationCommandeDvd(false);
                    break;
            }
        }



        #endregion

        #region
        //-----------------------------------------------------------
        // ONGLET "ABONNEMENTS REVUES"
        //-----------------------------------------------------------

        /// <summary>
        /// Ouverture de l'onglet Commande Abonnement Revues
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabAbonnementRevues_Enter(object sender, EventArgs e)
        {
            lesRevues = controle.GetAllRevues();
            AccesGestionAbonnementRevues(false);
            AccesDetailsAbonnementRevues(false);
            txbAbonnementRevuesNumero.Text = "";
            VideAbonnementRevuesInfos();
            VideDetailsAbonnementRevues();
            dgvAbonnementRevuesListe.DataSource = null;
        }

        /// <summary>
        /// Recherche d'une revue à partir du numéro et affiche les informations
        /// </summary>
        private void AbonnementRevuesRechercher()
        {
            if (!txbAbonnementRevuesNumero.Text.Equals(""))
            {
                Revue revue = lesRevues.Find(x => x.Id.Equals(txbAbonnementRevuesNumero.Text.Trim()));
                if (revue != null)
                {
                    AfficheAbonnementRevuesInfos(revue);
                }
                else
                {
                    MessageBox.Show("Numéro introuvable");
                    txbAbonnementRevuesNumero.Text = "";
                    txbAbonnementRevuesNumero.Focus();
                    VideAbonnementRevuesInfos();
                }
            }
            else
            {
                VideAbonnementRevuesInfos();
            }
        }

        /// <summary>
        /// Affiche les informations de la revue
        /// </summary>
        /// <param name="revue">Revue sélectionné</param>
        private void AfficheAbonnementRevuesInfos(Revue revue)
        {
            // affiche les informations
            txbAbonnementRevuesTitre.Text = revue.Titre;
            txbAbonnementRevuesPeriodicite.Text = revue.Periodicite;
            txbAbonnementRevuesDelai.Text = revue.DelaiMiseADispo.ToString();
            txbAbonnementRevuesGenre.Text = revue.Genre;
            txbAbonnementRevuesPublic.Text = revue.Public;
            txbAbonnementRevuesRayon.Text = revue.Rayon;
            txbAbonnementRevuesImage.Text = revue.Image;
            chkAbonnementRevuesEmpruntable.Checked = revue.Empruntable;
            string image = revue.Image;
            try
            {
                pcbAbonnementRevuesImage.Image = Image.FromFile(image);
            }
            catch
            {
                pcbAbonnementRevuesImage.Image = null;
            }
            // affiche la liste des commandes
            AfficheAbonnementRevues();

            if(dgvAbonnementRevuesListe.RowCount != 0)
            {
                // active la zone de gestion des commandes
                AccesGestionAbonnementRevues(true);
            }
        }

        /// <summary>
        /// Récupère, affiche les abonnements d'une revue
        /// </summary>
        private void AfficheAbonnementRevues()
        {
            string idDocument = txbAbonnementRevuesNumero.Text.Trim();
            lesAbonnements = controle.GetAbonnement(idDocument);
            RemplirAbonnementRevuesListe(lesAbonnements);
            AfficheAbonnementRevuesDetailSelect();
        }

        /// <summary>
        /// Affiche le détail de l'abonnement sélectionné
        /// </summary>
        private void AfficheAbonnementRevuesDetailSelect()
        {
            if (dgvAbonnementRevuesListe.CurrentCell != null)
            {
                Abonnement abonnement = (Abonnement)bdgAbonnementRevuesListe.List[bdgAbonnementRevuesListe.Position];
                AfficheAbonnementRevuesDetails(abonnement);
            }
            else
            {
                AccesGestionAbonnementRevues(false);
                VideDetailsAbonnementRevues();
            }
        }

        /// <summary>
        /// Affiche les détails d'un abonnement d'une revue
        /// </summary>
        /// <param name="abonnement">Abonnement concerné</param>
        private void AfficheAbonnementRevuesDetails(Abonnement abonnement)
        {
            txbAbonnementRevuesNumeroCommande.Text = abonnement.Id;
            dtpAbonnementRevuesDateCommande.Value = abonnement.DateCommande;
            dtpAbonnementRevuesFinAbonnement.Value = abonnement.DateFinAbonnement;
            txbAbonnementRevuesMontant.Text = abonnement.Montant.ToString("C2", CultureInfo.CreateSpecificCulture("fr-FR"));
        }

        /// <summary>
        /// Remplit le dategrid avec la collection reçue en paramètre
        /// </summary>
        /// <param name="lesAbonnements">Collection de lesAbonnements</param>
        private void RemplirAbonnementRevuesListe(List<Abonnement> lesAbonnements)
        {
            bdgAbonnementRevuesListe.DataSource = lesAbonnements;
            dgvAbonnementRevuesListe.DataSource = bdgAbonnementRevuesListe;
            dgvAbonnementRevuesListe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAbonnementRevuesListe.Columns["id"].Visible = false;
            dgvAbonnementRevuesListe.Columns["idRevue"].Visible = false;
            dgvAbonnementRevuesListe.Columns["dateCommande"].DisplayIndex = 0;
            dgvAbonnementRevuesListe.Columns[3].HeaderCell.Value = "Date commande";
            dgvAbonnementRevuesListe.Columns["montant"].DisplayIndex = 1;
            dgvAbonnementRevuesListe.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvAbonnementRevuesListe.Columns[4].DefaultCellStyle.Format = "c2";
            dgvAbonnementRevuesListe.Columns[4].DefaultCellStyle.FormatProvider = CultureInfo.GetCultureInfo("fr-FR");
            dgvAbonnementRevuesListe.Columns[0].HeaderCell.Value = "Date fin abonnement";
        }

        /// <summary>
        /// Evénement clic sur le bouton de recherche de revue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAbonnementRevuesRechercher_Click(object sender, EventArgs e)
        {
            AbonnementRevuesRechercher();
        }

        /// <summary>
        /// Evénement sur la touche entrer déclenche la recherche
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbAbonnementRevuesNumero_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnAbonnementRevuesRechercher_Click(sender, e);
            }
        }

        /// <summary>
        /// Evénement sur la saisie du numéro de la revue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbAbonnementRevuesNumero_TextChanged(object sender, EventArgs e)
        {
            AccesGestionAbonnementRevues(false);
            VideAbonnementRevuesInfos();
        }

        /// <summary>
        /// Evénement sur le changement de ligne, réaffiche les infos de la revue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvAbonnementRevuesListe_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvAbonnementRevuesListe.CurrentCell != null)
            {
                AfficheAbonnementRevuesDetailSelect();
            }
        }

        /// <summary>
        /// Tri sur une colonne
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvAbonnementRevuesListe_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string titrecolonne = dgvAbonnementRevuesListe.Columns[e.ColumnIndex].HeaderText;
            List<Abonnement> sortedList = new List<Abonnement>();
            switch (titrecolonne)
            {
                case "Date commande":
                    sortedList = lesAbonnements.OrderBy(o => o.DateCommande).Reverse().ToList();
                    break;
                case "Montant":
                    sortedList = lesAbonnements.OrderBy(o => o.Montant).Reverse().ToList();
                    break;
                case "Date fin abonnement":
                    sortedList = lesAbonnements.OrderBy(o => o.DateFinAbonnement).Reverse().ToList();
                    break;
            }
            RemplirAbonnementRevuesListe(sortedList);
        }

        /// <summary>
        /// Evénement clic sur le bouton d'ajout d'un abonnement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAbonnementRevuesAjouter_Click(object sender, EventArgs e)
        {
            AccesDetailsAbonnementRevues(true);
            dtpAbonnementRevuesFinAbonnement.Value = DateTime.Now.AddYears(1);
            AccesGestionAbonnementRevues(true);
        }

        /// <summary>
        /// Evénement clic sur le bouton valider un abonnement
        /// Enregistrement d'un abonnement à condition que tous les champs soient remplis et valides
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAbonnementRevuesValider_Click(object sender, EventArgs e)
        {
            if (txbAbonnementRevuesNumeroCommande.Text == "" || txbAbonnementRevuesMontant.Text == "")
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Information");
                return;
            }

            if (DateTime.Compare(dtpAbonnementRevuesDateCommande.Value, dtpAbonnementRevuesFinAbonnement.Value) >= 0)
            {
                MessageBox.Show("La date de fin d'abonnement n'est pas valide.", "Information");
                dtpAbonnementRevuesFinAbonnement.Value = DateTime.Now.AddYears(1);
                dtpAbonnementRevuesFinAbonnement.Focus();
                return;
            }

            String id = txbAbonnementRevuesNumeroCommande.Text;
            DateTime dateCommande = dtpAbonnementRevuesDateCommande.Value;
            DateTime finAbonnement = dtpAbonnementRevuesFinAbonnement.Value;
            string idRevue = txbAbonnementRevuesNumero.Text.Trim();
            String montantSaisie = txbAbonnementRevuesMontant.Text.Replace('.', ',');

            // validation du champ montant
            if (!Double.TryParse(montantSaisie, out double montant))
            {
                MessageBox.Show("Le montant doit être numérique.", "Erreur");
                txbAbonnementRevuesMontant.Text = "";
                txbAbonnementRevuesMontant.Focus();
                return;
            }
            Abonnement abonnement = new Abonnement(id, dateCommande, montant, finAbonnement, idRevue);
            if(txbAbonnementRevuesNumeroCommande.TextLength <= 5)
            {
                if (controle.CreerAbonnement(abonnement))
                {
                    AfficheAbonnementRevues();

                    // sélectionne la commande nouvellement créée
                    int addedRowIndex = -1;
                    DataGridViewRow row = dgvAbonnementRevuesListe.Rows
                        .Cast<DataGridViewRow>()
                        .First(r => r.Cells["id"].Value.ToString().Equals(id));
                    addedRowIndex = row.Index;
                    dgvAbonnementRevuesListe.Rows[addedRowIndex].Selected = true;

                    AccesDetailsAbonnementRevues(false);
                    AfficheAbonnementRevuesDetails(abonnement);
                    AccesGestionAbonnementRevues(true);
                }
                else
                {
                    MessageBox.Show("Ce numéro d'abonnement existe déjà.", "Erreur");
                    txbAbonnementRevuesNumeroCommande.Text = "";
                    txbAbonnementRevuesNumeroCommande.Focus();
                }
            }
            else
            {
                MessageBox.Show("Le numéro d'abonnement ne doit pas dépasser 5 caractères.", "Erreur");
                txbAbonnementRevuesNumeroCommande.Text = "";
                txbAbonnementRevuesNumeroCommande.Focus();
            }
        }

        /// <summary>
        /// Evénement clic sur le bouton supprimer un abonnement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAbonnementRevuesSupprimer_Click(object sender, EventArgs e)
        {
            Abonnement abonnement = (Abonnement)bdgAbonnementRevuesListe.Current;

            // check l'existence d'exemplaires
            if (controle.CheckSupprAbonnement(abonnement))
            {
                // demande confirmation à l'utilisateur
                if (ConfirmationSupprAbonnement())
                {
                    // tente de supprimer
                    if (controle.SupprAbonnement(abonnement.Id))
                    {
                        AfficheAbonnementRevues();
                    }
                    else
                    {
                        MessageBox.Show("Une erreur s'est produite.", "Erreur");
                    }
                }
            }
            else
            {
                MessageBox.Show("Impossible de supprimer cet abonnement car il est lié à des exemplaires.", "Information");
            }
        }

        /// <summary>
        /// Evénement sur le bouton annuler la saisie d'un nouvel abonnement
        /// à condition que l'utilisateur le confirme
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAbonnementRevuesAnnuler_Click(object sender, EventArgs e)
        {
            if (!(txbAbonnementRevuesNumeroCommande.Text == "" && txbAbonnementRevuesMontant.Text == ""))
            {
                if (ConfirmationAnnulationCommande())
                {
                    AccesDetailsAbonnementRevues(false);
                    AfficheAbonnementRevues();
                    AccesGestionAbonnementRevues(true);
                }
            }
            else
            {
                AccesDetailsAbonnementRevues(false);
                AfficheAbonnementRevues();
                AccesGestionAbonnementRevues(true);
            }
        }

        /// <summary>
        /// Vide les zones d'affichage des informations de la revue
        /// </summary>
        private void VideAbonnementRevuesInfos()
        {
            txbAbonnementRevuesTitre.Text = "";
            txbAbonnementRevuesPeriodicite.Text = "";
            txbAbonnementRevuesDelai.Text = "";
            txbAbonnementRevuesGenre.Text = "";
            txbAbonnementRevuesPublic.Text = "";
            txbAbonnementRevuesRayon.Text = "";
            txbAbonnementRevuesImage.Text = "";
            chkAbonnementRevuesEmpruntable.Checked = false;
            pcbAbonnementRevuesImage.Image = null;
        }

        /// <summary>
        /// Vide les zones d'affichage des détails d'un abonnement
        /// </summary>
        private void VideDetailsAbonnementRevues()
        {
            txbAbonnementRevuesNumeroCommande.Text = "";
            dtpAbonnementRevuesDateCommande.Value = DateTime.Now;
            dtpAbonnementRevuesFinAbonnement.Value = DateTime.Now;
            txbAbonnementRevuesMontant.Text = "";
        }

        /// <summary>
        /// Active/Désactive la zone de gestion des abonnements
        /// </summary>
        /// <param name="acces">true autorise l'accès</param>
        private void AccesGestionAbonnementRevues(bool acces)
        {
            grpGestionAbonnementRevues.Enabled = acces;
            btnAbonnementRevuesAjouter.Enabled = acces;
            btnAbonnementRevuesSupprimer.Enabled = acces;
        }

        /// <summary>
        /// Active/Désactive la zone détails d'un abonnement et les boutons (valider, annuler, ajouter)
        /// </summary>
        /// <param name="acces">True active les boutons Valider et Annuler, désactive le bouton Ajouter, dévérouille les champs</param>
        private void AccesDetailsAbonnementRevues(bool acces)
        {
            VideDetailsAbonnementRevues();
            grpAbonnementRevues.Enabled = acces;
            txbAbonnementRevuesNumeroCommande.Enabled = acces;
            txbAbonnementRevuesNumeroCommande.Focus();
            dtpAbonnementRevuesDateCommande.Enabled = acces;
            dtpAbonnementRevuesFinAbonnement.Enabled = acces;
            txbAbonnementRevuesMontant.Enabled = acces;
            btnAbonnementRevuesValider.Enabled = acces;
            btnAbonnementRevuesAnnuler.Enabled = acces;
            btnAbonnementRevuesAjouter.Enabled = !acces;
        }

        #endregion
    }
}
