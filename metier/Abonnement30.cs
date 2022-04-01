using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediatek86.metier
{
    public class Abonnement30
    {
        private readonly DateTime dateFinAbonnement;
        private readonly string idRevue;
        private readonly string titreRevue;

        public Abonnement30(DateTime dateFinAbonnement, string idRevue, string titreRevue)
        {
            this.dateFinAbonnement = dateFinAbonnement;
            this.idRevue = idRevue;
            this.titreRevue = titreRevue;
        }

        public DateTime DateFinAbonnement => dateFinAbonnement;
        public string IdRevue => idRevue;
        public string TitreRevue => titreRevue;
    }
}
