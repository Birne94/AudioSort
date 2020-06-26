using System;
using System.Collections.Generic;
using System.Linq;

namespace Audiosort.Plugins
{
    public class StyleBunt : Style
    {
        public StyleBunt(System.Reflection.Assembly a)
            : base(a)
        {
            Name = "Bunt";
            Author = "Daniel Birnstiel, Max Ulbrich";
            Homepage = "";
            Version = "1.0";
            AssemblyName = "StyleDefault";

            XamlDirectory = "StyleBunt";
            XamlPaths = new string[] {
                "StyleDefault.xaml",
                "StyleDefaultImages.xaml",
                "StyleDefaultMenu.xaml",
                "StyleDefaultButtons.xaml",
                "StyleDefaultSlider.xaml",
                "StyleDefaultPlaylist.xaml",
            };
        }
    }
}
