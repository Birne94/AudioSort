using System;
using System.Collections.Generic;
using System.Linq;

namespace Audiosort.Plugins
{
    public class StyleDefault : Style
    {
        public StyleDefault(System.Reflection.Assembly a)
            : base(a)
        {
            Name = "Standard";
            Author = "Daniel Birnstiel, Max Ulbrich";
            Homepage = "";
            Version = "1.0";
            AssemblyName = "StyleDefault";

            XamlDirectory = "StyleDefault";
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
