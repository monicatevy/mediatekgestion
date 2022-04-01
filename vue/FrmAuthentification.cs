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
    public partial class FrmAuthentification : Form
    {
        private readonly Controle controle;

        /// <summary>
        /// Booléen vérifie si l'authentification a réussi
        /// </summary>
        public bool onSuccessAuth { get; private set; }


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
            onSuccessAuth = true;
            Close();
        }

        /// <summary>
        /// Evénement sur la touche entrer déclenche la connexion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txbPwd_KeyDown(object sender, KeyEventArgs e)
        {
            btnConnexion_Click(sender, e);
        }
    }
}
