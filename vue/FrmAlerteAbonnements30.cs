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
    public partial class FrmAlerteAbonnements30 : Form
    {
        private readonly BindingSource bdgAbonnements30 = new BindingSource();
        private readonly List<Abonnement30> lesAbonnements30;

        public FrmAlerteAbonnements30(Controle controle)
        {
            InitializeComponent();
            lesAbonnements30 = controle.GetAbonnement30();
            bdgAbonnements30.DataSource = lesAbonnements30;
            dgvAbonnements30Liste.DataSource = bdgAbonnements30;
            dgvAbonnements30Liste.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvAbonnements30Liste.Columns["dateFinAbonnement"].DisplayIndex = 2;
            dgvAbonnements30Liste.Columns[0].HeaderCell.Value = "Date d'expiration";
            dgvAbonnements30Liste.Columns[1].HeaderCell.Value = "Numéro Revue";
            dgvAbonnements30Liste.Columns[2].HeaderCell.Value = "Titre";
        }
    }
}
