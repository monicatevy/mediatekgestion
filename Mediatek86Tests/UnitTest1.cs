using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mediatek86.controleur;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediatek86.controleur.Tests
{
    [TestClass()]
    public class ControleTests
    {
        private readonly Controle controleur = new Controle();
        private readonly DateTime beforeDate = new DateTime(2022, 1, 1);
        private readonly DateTime btwDate = new DateTime(2022, 6, 1);
        private readonly DateTime afterDate = new DateTime(2022, 12, 1);

        [TestMethod()]
        public void ParutionDansAbonnementTest()
        {
            // Date parution valide
            bool resultat1 = controleur.ParutionDansAbonnement(beforeDate, afterDate, btwDate);
            Assert.AreEqual(true, resultat1, "Succès test : dateparution COMPRISE entre date abonnement et fin abonnement => TRUE");

            // Date parution égale aux bornes
            bool resultat2 = controleur.ParutionDansAbonnement(beforeDate, afterDate, beforeDate);
            Assert.AreEqual(false, resultat2, "Succès test : dateparution EGALE à date abonnement => FALSE");

            bool resultat3 = controleur.ParutionDansAbonnement(beforeDate, afterDate, afterDate);
            Assert.AreEqual(false, resultat3, "Succès test : dateparution EGALE à date fin abonnement => FALSE");

            // Date parution en dehors des bornes
            bool resultat4 = controleur.ParutionDansAbonnement(btwDate, afterDate, beforeDate);
            Assert.AreEqual(false, resultat4, "Succès test : dateparution AVANT date fin abonnement => FALSE");

            bool resultat5 = controleur.ParutionDansAbonnement(beforeDate, btwDate, afterDate);
            Assert.AreEqual(false, resultat5, "Succès test : dateparution APRES date fin abonnement => FALSE");
        }
    }
}