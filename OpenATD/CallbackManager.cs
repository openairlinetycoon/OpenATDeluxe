using OpenATD.SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenATD {
	public class CallbackManager {
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static string Gimme();

        Cursor c;

        public CallbackManager()
        {
            c = new Cursor(SDLWrapper.AddDrawable("CURSOR.bmp"));
        }

		public void InputCallback(string keyName){
			Console.WriteLine("Input recieved!# Input "+ keyName);
			//Console.WriteLine(Gimme() + "+ data");
			if(keyName == "Up")
				SDLWrapper.AddDrawable("test.bmp");
			if (keyName == "Down")
				Drawable.drawers[new Random().Next(0, Drawable.drawers.Count)].x +=2;
			if (keyName == "Left")
				Console.WriteLine("FPS: " + SDLWrapper.GetFPS());
		}

        public void OnEvent() {
            c.Update();
        }
	}
}
