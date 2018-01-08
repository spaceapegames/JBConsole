using UnityEngine;

namespace com.spaceape.jbconsole
{
    public class JBConsoleConfig : ScriptableObject
    {
        public Font font;
        public GameObject consoleUI;

        public static JBConsoleConfig Load()
        {
            return Resources.Load<JBConsoleConfig>("JBConsoleConfig");
        }

        public static Font GetDefaultFont()
        {
            var config = Load();
            Font font = null;
            if (config)
            {
                font = config.font;
            }
            return font;
        }

        public static GameObject GetExternalUIPrefab()
        {
            var config = Load();
            GameObject consoleUI = null;
            if (config)
            {
                consoleUI = config.consoleUI;
            }
            return consoleUI; 
        }
    }
}