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
        GFXLib a;

        public CallbackManager()
        {
            c = new Cursor(SDLWrapper.AddDrawableLib(SDLWrapper.GliPath + "glbasis.gli", "CURSOR"));
        }

		public void InputCallback(string keyName){
			Console.WriteLine("Input recieved!# Input "+ keyName);
            //Console.WriteLine(Gimme() + "+ data");
            if (keyName == "Up")
                SDLWrapper.AddDrawableLib(SDLWrapper.GliPath + "glbasis.gli", "CURSORL");
            if (keyName == "Down")
				Drawable.drawers[new Random().Next(0, Drawable.drawers.Count)].position.x +=2;
			if (keyName == "Left")
				Console.WriteLine("FPS: " + SDLWrapper.GetFPS());
			if (keyName == "Right") {
				a = GFXLib.Create(SDLWrapper.GliPath + "glbasis.gli");

				Console.WriteLine(a.pointer);
			}
            if (keyName == "D")
                Console.WriteLine(a.GetAllImageNames()[0]);
		}

        public void OnEvent() {
            c.Update();
        }
	}
}
