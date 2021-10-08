using ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordPlus
{
    public class Entry : IMod
    {
        public string Name => "DiscordPlus";

        public string Description => "";

        public string Author => "Mo10";

        public string HomePage => "https://github.com/mo10/DiscordPlus";

        public void DoPatching()
        {
            DiscordPlus.DoPatching();
        }
    }
}
