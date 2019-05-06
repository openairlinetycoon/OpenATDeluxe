using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.CompilerServices;

namespace OpenATD.SDL
{
    /// <summary>
    /// Wrapper for C++ GFXLib. Contains all GFX Files from a .gli file
    /// </summary>
    public class GFXLib
    {
        public long pointer = -1;

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern GFXLib Create(string file);

        public static Dictionary<string, GFXLib> libs;
        public string fileName;

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string[] _GetAllImageNames(long p);
		internal string[] GetAllImageNames() {
			return _GetAllImageNames(pointer);
		}

		private GFXLib() {
            //if(pointer == IntPtr.Zero)
            //    throw new MethodAccessException("Do not try to create this Class by yourself! Create it using the Method \"Create\"!");

            Console.WriteLine(pointer);
        }
    }

    public class Test
    {
        public string fileName;

        public Test()
        {
            fileName = "Hey";
        }
    }
}
