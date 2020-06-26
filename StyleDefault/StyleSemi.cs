using System;
using System.Collections.Generic;
using System.Linq;

namespace Audiosort.Plugins
{
    public class StyleSemi : Style
    {
        public StyleSemi(System.Reflection.Assembly a)
            : base(a)
        {
            Name = "Alternativ";
            Author = "Daniel Birnstiel, Max Ulbrich";
            Homepage = "";
            Version = "1.0";
            AssemblyName = "StyleDefault";

            XamlDirectory = "StyleSemi";
            XamlPaths = new string[] {
                "StyleDefault.xaml",
                "StyleDefaultImages.xaml",
                "StyleDefaultMenu.xaml",
                "StyleDefaultContextMenus.xaml",
                "StyleDefaultButtons.xaml",
                "StyleDefaultSlider.xaml",
                "StyleDefaultPlaylist.xaml",
            };
        }
    }
}
