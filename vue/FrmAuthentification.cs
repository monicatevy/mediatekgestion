using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mediatek86.controleur;
using Mediatek86.metier;

namespace Mediatek86.vue
{
    /// <summary>
    /// Vue pour l'authentification de l'utilisateur
    /// </summary>
    public partial class FrmAuthentification : Form
    {
        private readonly Controle controle;

        /// <summary>
        /// Booléen vérifie si l'authentification a réussi
        /// </summary>
        public bool OnSuccessAuth { get; private set; }

        /// <summary>
        /// Constructeur : initialise le composant et valorise la propriété contrôleur avec le contrôleur reçu en paramètre
        /// </summary>
        /// <param name="controle"></param>
        public FrmAuthentification(Controle controle)
        {
            InitializeComponent();
            this.controle = controle;
        }

        /// <summary>
        /// Evénement click sur le bouton Connexion, vérification des identifiants de connexion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnexion_Click(object sender, EventArgs e)
        {
            string login = txbLogin.Text.Trim();
            string pwd = txbPwd.Text.Trim();
            Service userService = controle.Authentification(login, pwd);

            // Récupération du service si l'authentification est réussie
            if (userService != null)
            {
                if (userService.Libelle == "culture")
                {
                    MessageBox.Show("Accès réservé aux services administratifs et au service de prêt.", "Information");
                    VideChampsConnexion();
                }
                else
                {
                    OnSuccessAuth = true;
                    Close();
                }
            }
            else
            {
                MessageBox.Show("Identifiants de connexion incorrects.", "Erreur");
                VideChampsConnexion();
            }
        }

        /// <summary>
        /// Evénement sur la touche entrer déclenche la connexion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbPwd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnConnexion_Click(sender, e);
            }
        }

        /// <summary>
        /// Vide les champs de connexion
        /// </summary>
        private void VideChampsConnexion()
        {
            txbLogin.Text = "";
            txbPwd.Text = "";
            txbLogin.Focus();
        }
    }
}
