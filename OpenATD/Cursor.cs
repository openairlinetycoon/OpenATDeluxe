using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenATD.SDL
{
    class Cursor
    {
        Drawable image;

        public Cursor(Drawable image) {
            this.image = image;
            Drawable.drawers.Remove(this.image);
        }

        public void Update()
        {
            image.position = SDLWrapper.GetMousePos();
        }
    }
}
