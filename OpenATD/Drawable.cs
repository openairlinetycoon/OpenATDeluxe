using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenATD.SDL {
	public class Drawable {
		public static List<Drawable> drawers = new List<Drawable>();

		string file;

        //public int x, y;
        public Vector position;

        public Drawable()
        {
            position = new Vector(0, 0);
        }

		public void Setup(string file, int x = 0, int y = 0)
		{
			this.file = file;

            position.x = x;
            position.y = x;

			Console.WriteLine("Setup! " + x + "," + y + " file" + file);
			drawers.Add(this);
		}

        //TODO Add destructor
	}
}
