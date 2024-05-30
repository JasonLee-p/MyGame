using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
    internal class MyLogger
    {
        public static MainWin mainWin;


        // 连接到MainWin的statusLabel
        public static void Log(object msg)
        {
            if (mainWin == null) { return; }
            mainWin.updateStatus(msg.ToString());
        }
    }
}
