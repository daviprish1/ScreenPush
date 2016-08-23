using HooksLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace ScreenPush.SupportClasses
{
    [Serializable]
    public class AppSettings
    {
        private string customBrushName;
        private Key activateKey;
        private KeyModifier activateMod;
        private KeyModifier activateSecondaryMod;

        public KeyModifier ActivateSecondaryMod
        {
            get { return activateSecondaryMod; }
            set { activateSecondaryMod = value; }
        }

        public string CustomBrushName
        {
            get
            {
                return customBrushName;
            }
            set
            {
                customBrushName = value;
            }
        }

        public Key ActivateKey
        {
            get
            {
                return activateKey;
            }
            set
            {
                activateKey = value;
            }
        }

        public KeyModifier ActivateMod
        {
            get
            {
                return activateMod;
            }
            set
            {
                activateMod = value;
            }
        }

        public AppSettings(string brushName, Key key, KeyModifier keymod)
        {
            this.CustomBrushName = brushName;
            this.ActivateKey = key;
            this.ActivateMod = keymod;
        }

        public AppSettings()
            :this("Indigo", Key.S, KeyModifier.Shift)
        { }
    }
}
