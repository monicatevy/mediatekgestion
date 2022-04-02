using System;
using TechTalk.SpecFlow;
using Mediatek86.vue;
using Mediatek86.controleur;
using System.Windows.Forms;
using NUnit.Framework;

namespace SpecFlowMediaTek86.Steps
{
    [Binding]
    public class AjoutCommandeLivreSteps
    {
        private readonly FrmMediatek frmMediatek = new FrmMediatek(new Controle());

        [Given(@"saisir le numero du livre '(.*)'")]
        public void GivenSaisirLeNumeroDuLivre(string numeroLivre)
        {
            TextBox TxbNumeroLivre = (TextBox)frmMediatek.Controls["tabOngletsApplication"].Controls["tabCommandeLivre"].Controls["grpLivresCommande"].Controls["txbCommandeLivresNumero"];
            frmMediatek.Visible = true;
            TxbNumeroLivre.Text = numeroLivre;
        }
        
        [Given(@"click sur le bouton Rechercher")]
        public void GivenClickSurLeBoutonRechercher()
        {
            Button BtnRechercher = (Button)frmMediatek.Controls["tabOngletsApplication"].Controls["tabCommandeLivre"].Controls["grpLivresCommande"].Controls["btnCommandeLivresRechercher"];
            frmMediatek.Visible = true;
            BtnRechercher.PerformClick();
        }
        
        [Given(@"click sur le bouton Ajouter")]
        public void GivenClickSurLeBoutonAjouter()
        {
            Button BtnAjouter = (Button)frmMediatek.Controls["tabOngletsApplication"].Controls["tabCommandeLivre"].Controls["grpLivresCommande"].Controls["btnCommandeLivresAjouter"];
            frmMediatek.Visible = true;
            BtnAjouter.PerformClick();
        }
        
        [Given(@"saisir le numéro de commande '(.*)'")]
        public void GivenSaisirLeNumeroDeCommande(string numeroCommande)
        {
            TextBox TxbNumeroCommande = (TextBox)frmMediatek.Controls["tabOngletsApplication"].Controls["tabCommandeLivre"].Controls["grpLivresCommande"].Controls["txbCommandeLivresNumeroCommande"];
            frmMediatek.Visible = true;
            TxbNumeroCommande.Text = numeroCommande;
        }
        
        [Given(@"saisir nombre exemplaires à '(.*)'")]
        public void GivenSaisirNombreExemplairesA(int nbExemplaires)
        {
            NumericUpDown NudNbExemplaires = (NumericUpDown)frmMediatek.Controls["tabOngletsApplication"].Controls["tabCommandeLivre"].Controls["grpLivresCommande"].Controls["nudCommandeLivresExemplaires"];
            frmMediatek.Visible = true;
            NudNbExemplaires.Value = nbExemplaires;
        }
        
        [Given(@"saisir le montant à '(.*)'")]
        public void GivenSaisirLeMontantA(double montant)
        {
            TextBox TxbMontant = (TextBox)frmMediatek.Controls["tabOngletsApplication"].Controls["tabCommandeLivre"].Controls["grpLivresCommande"].Controls["txbCommandeLivresMontant"];
            frmMediatek.Visible = true;
            TxbMontant.Text = montant.ToString();
        }
        
        [When(@"click sur le bouton Valider")]
        public void WhenClickSurLeBoutonValider()
        {
            Button BtnValider = (Button)frmMediatek.Controls["tabOngletsApplication"].Controls["tabCommandeLivre"].Controls["grpLivresCommande"].Controls["btnCommandeLivresValider"];
            frmMediatek.Visible = true;
            BtnValider.PerformClick();
        }
        
        [Then(@"le détail affiche les informations de la commande '(.*)'")]
        public void ThenLeDetailAfficheLesInformationsDeLaCommande(string numeroCommandeAttentdu)
        {
            TextBox TxbNumeroCommande = (TextBox)frmMediatek.Controls["tabOngletsApplication"].Controls["tabCommandeLivre"].Controls["grpLivresCommande"].Controls["txbCommandeLivresNumeroCommande"];
            string numeroCommandeObtenu = TxbNumeroCommande.Text;
            Assert.AreEqual(numeroCommandeAttentdu, numeroCommandeObtenu);
        }
    }
}
