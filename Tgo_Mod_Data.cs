using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tgo_Mod
{
    class Tgo_Mod_Data
    {
        string uIP { get; } //IP of command user
        string uName { get; } //Name of the command user
        string tIP { get; } //IP of the Target of the command
        string tName { get; } //Name of the target of the command
        TGOCType type { get; } //Type of command used
        DateTime timeStamp { get; } //Time of command execution

        public Tgo_Mod_Data(string uIP, string uName, string tIP, string tName, TGOCType type, DateTime timeStamp)
        {
            this.uIP = uIP;
            this.uName = uName;
            this.tIP = tIP;
            this.tName = tName;
            this.type = type;
            this.timeStamp = timeStamp;
        }

        /// <summary>
        /// Eliminates white spaces of the ToString() method.
        /// </summary>
        /// <returns></returns>
        public string ToCSV()
        {
            return ToString().Replace(" ", "");
        }

        /// <summary>
        /// Returns in csv format.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "" + uIP + ", " + uName + ", " + tIP + ", " + tName + ", " + type.ToString() + ", " + timeStamp.ToString();
        }
    }
}
