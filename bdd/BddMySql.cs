﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Serilog;

//// <summary> Connexion à et interactions avec la base de données </summary>
namespace Mediatek86.bdd
{
    /// <summary>
    /// Classe qui gère la connexion avec la base de données
    /// </summary>
    public class BddMySql
    {
        /// <summary>
        /// Unique instance de la classe
        /// </summary>
        private static BddMySql instance = null;
        
        /// <summary>
        /// objet de connexion à la BDD à partir d'une chaîne de connexion
        /// </summary>
        private readonly MySqlConnection connection;

        /// <summary>
        /// objet contenant le résultat d'une requête "select" (curseur)
        /// </summary>
        private MySqlDataReader reader;

        /// <summary>
        /// Constructeur privé pour créer la connexion à la BDD et l'ouvrir
        /// </summary>
        /// <param name="stringConnect">chaine de connexion</param>
        private BddMySql(string stringConnect)
        {
            try
            {
                connection = new MySqlConnection(stringConnect);
                connection.Open();
            }
            catch (MySqlException e)
            {
                Log.Error("BddMySql.BddMySql catch **** stringConnect = " + stringConnect + " **** MySqlException = " + e.Message);
                ErreurGraveBddNonAccessible();
            }
        }

        /// <summary>
        /// Crée une instance unique de la classe
        /// </summary>
        /// <param name="stringConnect">chaine de connexion</param>
        /// <returns>instance unique de la classe</returns>
        public static BddMySql GetInstance(string stringConnect)
        {
            if (instance is null)
            {
                instance = new BddMySql(stringConnect);
            }
            return instance;
        }

        /// <summary>
        /// Exécute une requête type "select" et valorise le curseur
        /// </summary>
        /// <param name="stringQuery">requête select</param>
        /// <param name="parameters">paramètres à insérer dans la requête</param>
        public void ReqSelect(string stringQuery, Dictionary<string, object> parameters)
        {
            MySqlCommand command;
            
            try
            {
                command = new MySqlCommand(stringQuery, connection);
                if (!(parameters is null))
                {
                    foreach (KeyValuePair<string, object> parameter in parameters)
                    {
                        command.Parameters.Add(new MySqlParameter(parameter.Key, parameter.Value));
                    }
                }
                command.Prepare();
                reader = command.ExecuteReader();
            }
            catch (MySqlException e)
            {
                Log.Error("BddMySql.ReqSelect catch **** stringQuery = " + stringQuery + " **** MySqlException = " + e.Message);
            }
            catch (InvalidOperationException e)
            {
                Log.Error("BddMySql.ReqSelect catch **** stringQuery = " + stringQuery + " **** InvaldOperationException = " + e.Message);
                ErreurGraveBddNonAccessible();
            }
        }

        /// <summary>
        /// Tente de lire la ligne suivante du curseur
        /// </summary>
        /// <returns>false si fin de curseur atteinte</returns>
        public bool Read()
        {
            if (reader is null)
            {
                return false;
            }
            try
            {
                return reader.Read();
            }
            catch (Exception e)
            {
                Log.Error("BddMySql.Read catch **** Exception = " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Retourne le contenu d'un champ dont le nom est passé en paramètre
        /// </summary>
        /// <param name="nameField">nom du champ</param>
        /// <returns>valeur du champ</returns>
        public object Field(string nameField)
        {
            if (reader is null)
            {
                return null;
            }
            try
            {
                return reader[nameField];
            }
            catch (Exception e)
            {
                Log.Error("BddMySql.Field catch **** nameField = " + nameField + " **** MySqlException = " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Exécution de requêtes autre que "select" dans une seule transaction
        /// </summary>
        /// <param name="queries">Liste des requêtes à effectuer</param>
        /// <param name="parameters">Paramètres à insérer dans les requêtes</param>
        public void ReqUpdate(List<string> queries, Dictionary<string, object> parameters)
        {
            MySqlCommand command;
            MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                foreach (string stringQuery in queries)
                {
                    command = new MySqlCommand(stringQuery, connection, transaction);
                    if (!(parameters is null))
                    {
                        foreach (KeyValuePair<string, object> parameter in parameters)
                        {
                            command.Parameters.Add(new MySqlParameter(parameter.Key, parameter.Value));
                        }
                    }
                    command.Prepare();
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (MySqlException e)
            {
                transaction.Rollback();
                Log.Error("BddMySql.ReqUpdate catch **** queries = " + string.Join(" --- ", queries) + " **** MySqlException = " + e.Message);
                throw;
            }
            catch (InvalidOperationException e)
            {
                Log.Error("BddMySql.ReqUpdate catch **** queries = " + string.Join(" --- ", queries) + " **** InvalidOperationException = " + e.Message);
                ErreurGraveBddNonAccessible();
            }
        }

        /// <summary>
        /// Fermeture du curseur
        /// </summary>
        public void Close()
        {
            if (!(reader is null))
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Pas d'accès à la BDD : arrêt de l'application
        /// </summary>
        private void ErreurGraveBddNonAccessible()
        {
            MessageBox.Show("Base de données non accessibles", "Erreur grave");
            Environment.Exit(1);
        }
    }
}
