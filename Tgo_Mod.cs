using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using TerrariaApi.Server;
using Terraria;
using MySql.Data.MySqlClient;

/// <summary>
/// NOTE: Should sanitize player names in case someone gets the idea to put an SQL injection in their player name.
/// Though i don't think Terraria base game allows for special characters. I need to look into whether sanitization is necessary.
/// </summary>
namespace Tgo_Mod
{
    public enum TGOCType //TGO command types
    {
        mute,
        kick,
        ban
    }
    [ApiVersion(2, 1)]
    public class Tgo_Mod : TerrariaPlugin
    {
        public override string Author => "Tsohg";
        public override string Name => "TGO_Mod";
        public override string Description => "Used in conjunction with TGO_Req for TGO button commands related to server moderation.";
        public override Version Version => new Version(1, 0);

        //MySql connection guide used: https://www.codeproject.com/Articles/43438/Connect-C-to-MySQL
        private MySqlConnection mysql;
        private string server;
        private string db;
        private string uid;
        private string pass;

        /// <summary>
        /// ToDo: Ship collected data to a database.
        /// </summary>
        /// <param name="game"></param>
        public Tgo_Mod(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            try
            {
                server = "sql-us-northeast.nodecraft.com";
                db = "np2_92b7e33b12fcdb170c";
                uid = "np2_060d35d64805";
                pass = "5a2645945cf7c6de9cdef46c";
                mysql = new MySqlConnection(
                    "SERVER=" + server + ";" +
                    "DATABASE=" + db + ";" +
                    "UID=" + uid + ";" +
                    "PASSWORD=" + pass + ";");
                Commands.ChatCommands.Add(new Command("tshock.rest.mute", TgoMute, "TgoMute", "tmute"));
                Commands.ChatCommands.Add(new Command("tshock.rest.kick", TgoKick, "TgoKick", "tkick"));
                Commands.ChatCommands.Add(new Command("tshock.rest.ban", TgoBan, "TgoBan", "tban"));
            }
            catch (Exception e)
            {
                TShock.Log.ConsoleError("TGOMOD Error on DB Connect: " + e.Message);
            }

            //DEBUG SECTION
            //Tgo_Mod_Data data = new Tgo_Mod_Data("127.0.0.1", "Nathan", "127.0.0.2", "Emre", TGOCType.ban, DateTime.Now);
            //InsertTgoModData(data);
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                mysql.Close();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// It is a toggle. Muting same player twice unmutes.
        /// </summary>
        /// <param name="playerName"></param>
        public void TgoMute(CommandArgs args)
        {
            TSPlayer plr = GetTSPlayerByName(args.Parameters[1]);
            if (plr == null)
                return;
            plr.mute = !plr.mute;
            TShock.Log.ConsoleError("" + plr.Name + " :: " + args.Parameters[2]);
            //Player has been muted/unmuted so we collect data...
            Tgo_Mod_Data data = new Tgo_Mod_Data(args.Player.IP, args.Player.Name, plr.IP, plr.Name, TGOCType.mute, DateTime.Now);
            InsertTgoModData(data);
        }

        public void TgoKick(CommandArgs args)
        {
            TSPlayer plr = GetTSPlayerByName(args.Parameters[1]);
            if (plr == null)
            {
                TShock.Log.ConsoleError("Null TSPlayer in Tgo_Mod");
                return;
            }
            TShock.Utils.Kick(plr, args.Parameters[2]);
            TShock.Log.ConsoleError("" + plr.Name + " :: " + args.Parameters[2]);
            //Player has been kicked so we collect our data...
            Tgo_Mod_Data data = new Tgo_Mod_Data(args.Player.IP, args.Player.Name, plr.IP, plr.Name, TGOCType.kick, DateTime.Now);
            InsertTgoModData(data);
        }

        public void TgoBan(CommandArgs args)
        {
            TSPlayer plr = GetTSPlayerByName(args.Parameters[1]);
            if (plr == null)
            {
                TShock.Log.ConsoleError("Null TSPlayer in Tgo_Mod");
                return;
            }
            TShock.Utils.Kick(plr, args.Parameters[2]);
            TShock.Log.ConsoleError("" + plr.Name + " :: " + args.Parameters[2]);
            //Player has been banned so we collect our data...
            Tgo_Mod_Data data = new Tgo_Mod_Data(args.Player.IP, args.Player.Name, plr.IP, plr.Name, TGOCType.ban, DateTime.Now);
            InsertTgoModData(data);
        }

        private TSPlayer GetTSPlayerByName(string name)
        {
            foreach (TSPlayer plr in TShock.Players)
                if (plr.Name == name)
                    return plr;
            return null;
        }

        #region References MySQL Guide
        private void ExecuteNoResultQuery(string query)
        {
            try
            {
                mysql.Open();
                MySqlCommand cmd = new MySqlCommand(query, mysql);
                cmd.ExecuteNonQuery();
                mysql.Close();
            }   
            catch (Exception e)
            {
                TShock.Log.ConsoleError("TGOMOD Error on ExecuteQuery: " + e.Message);
            }
        }

        private List<string>[] ExecuteSelectQuery(string query, string[] colNames)
        {
            List<string>[] resultSet = new List<string>[colNames.Length];
            for (int i = 0; i < colNames.Length; i++)
                resultSet[i] = new List<string>();
            mysql.Open();
            MySqlCommand cmd = new MySqlCommand(query, mysql);
            MySqlDataReader reader = cmd.ExecuteReader();
            int record = 0;
            while(reader.Read())
            {
                for(int i = 0; i < colNames.Length; i++)
                {
                    resultSet[record].Add(reader[colNames[i]] + "");
                }
                record++;
            }
            reader.Close();
            mysql.Close();
            return resultSet;
        }
        #endregion

        private void InsertTgoModData(Tgo_Mod_Data data)
        {
            try
            {
                //first, find out if the user is already in the database. if so, we ignore the first insert, else we insert a new tgouser
                string query = "SELECT NAME FROM TGO_USERS WHERE NAME = '" + data.uName + "'";
                string[] colNames = { "NAME" };
                List<string>[] results = ExecuteSelectQuery(query, colNames);

                if(results[0].Count <= 0) //insert new user then insert the rest of the data.
                {
                    query = "INSERT INTO TGO_USERS (NAME) VALUES('" + data.uName + "')";
                    ExecuteNoResultQuery(query);
                }

                //get primary key after the insert.
                query = "SELECT UID, NAME FROM TGO_USERS WHERE NAME = '" + data.uName + "'";
                colNames = new string[] { "UID", "NAME"};
                results = ExecuteSelectQuery(query, colNames); // results[0] => List looks like: UID, NAME, etc. should be exactly 1 result.

                int pk = int.Parse(results[0][0]); //should be UID
                int mid = 1 + (int)data.type; //should be the correct MID.

                //insert rest of TGO data.
                query = "INSERT INTO TGO_MODLOGS (UID, TARGET_NAME, MID) VALUES (" + pk + ", '" + data.tName + "', " + mid + ")"; //will need edited later based on constraints.
                ExecuteNoResultQuery(query);
            }
            catch (Exception e)
            {
                TShock.Log.ConsoleError("TGOMOD Error on Insert: " + e.Message);
            }
        }
    }

    //internal class Tgo_Mod_Data
    //{
    //    private string iP1;
    //    private string name1;
    //    private string iP2;
    //    private string name2;
    //    private TGOCType mute;
    //    private DateTime now;

    //    public Tgo_Mod_Data(string iP1, string name1, string iP2, string name2, TGOCType mute, DateTime now)
    //    {
    //        this.iP1 = iP1;
    //        this.name1 = name1;
    //        this.iP2 = iP2;
    //        this.name2 = name2;
    //        this.mute = mute;
    //        this.now = now;
    //    }
    //}
}
