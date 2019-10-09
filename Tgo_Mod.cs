﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using TerrariaApi.Server;
using Terraria;

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
        /// <summary>
        /// ToDo: Ship collected data to a database.
        /// </summary>
        /// <param name="game"></param>
        public Tgo_Mod(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override string Author => "Tsohg";
        public override string Name => "TGO_Mod";
        public override string Description => "Used in conjunction with TGO_Req for TGO button commands related to server moderation.";
        public override Version Version => new Version(1, 0);

        /// <summary>
        /// It is a toggle. Muting same player twice unmutes.
        /// </summary>
        /// <param name="playerName"></param>
        public void TgoMute(string playerName, CommandArgs args)
        {
            TSPlayer tplr = GetTSPlayerByName(playerName);
            tplr.mute = !tplr.mute;

            //Player has been muted/unmuted so we collect data...
            Tgo_Mod_Data data = new Tgo_Mod_Data(args.Player.IP, args.Player.Name, tplr.IP, tplr.Name, TGOCType.mute, DateTime.Now);
        }

        public void TgoKick(string playerName, string reason, CommandArgs args)
        {
            TSPlayer tplr = GetTSPlayerByName(playerName);
            TShock.Utils.Kick(tplr, reason);

            //Player has been kicked so we collect our data...
            Tgo_Mod_Data data = new Tgo_Mod_Data(args.Player.IP, args.Player.Name, tplr.IP, tplr.Name, TGOCType.kick, DateTime.Now);
        }

        public void TgoBan(string playerName, string reason, CommandArgs args)
        {
            TSPlayer tplr = GetTSPlayerByName(playerName);
            TShock.Utils.Ban(tplr, reason);

            //Player has been banned so we collect our data...
            Tgo_Mod_Data data = new Tgo_Mod_Data(args.Player.IP, args.Player.Name, tplr.IP, tplr.Name, TGOCType.ban, DateTime.Now);
        }

        private TSPlayer GetTSPlayerByName(string name)
        {
            foreach (TSPlayer plr in TShock.Players)
                if (plr.Name == name)
                    return plr;
            throw new Exception("Null TSPlayer.");
        }
    }
}
