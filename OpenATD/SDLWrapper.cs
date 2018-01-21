using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenATD.SDL
{
	class SDLWrapper {
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern Drawable AddDrawable(string file);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern float GetFPS();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetMouseX();
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int GetMouseY();

        public static void GetMousePos() {

        }

        /* This makes a kinda functional headerfile from a "dumpbin.exe /LINENUMBERS ***.lib | findstr /c:"public:"" with that regex: @"\(public\:(.*\ ([^<>\n]*(<(.*)>)?)\:\:.*)\)"
foreach (Match m in reg) {
    string k = m.Groups[2].Value;
    string v = "DllExport " + m.Groups[1].Value + ";";

    v = v.Replace(k+"::", "");
    bool template = k.Contains("<");
    string kT = "", vT = "";


    string alphabet = "abcdefghijklmnopqrstuvwxyz";
    if (template)
    {
        kT += "<typename " + alphabet[0];
        vT += "<" + alphabet[0];
    }

    for(int i = 1; i < m.Groups[4].Value.Count(s => s == ',')+1; i++) {
        kT += ", typename " + alphabet[i];
        vT += ", " + alphabet[i];
    }

    if (template)
    {
        kT += ">";
        vT += ">";
    }


    if (template)
    {
        vT = v.Replace(m.Groups[3].Value, vT);
        kT = k.Replace(m.Groups[3].Value, kT);
    }
    else {
        vT = v;
        kT = k;
    }

    if (o.ContainsKey(kT)) {
        //if(!o[kT].Contains(vT))
          o[kT] += "\n" + vT;
    }
    else {
        o.Add(kT, vT);
    }

}*/
    }
}
