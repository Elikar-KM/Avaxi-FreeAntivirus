using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Avaxi
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ReadConfigs();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }

        private static void ReadConfigs()
        {
            try
            {
                tuneAutomaticUpdates = (bool)Properties.Settings.Default["tuneAutomaticUpdates"];
                tuneDesktopCleanUpWizard = (bool)Properties.Settings.Default["tuneDesktopCleanUpWizard"];
                tuneMenuShowDelay = (bool)Properties.Settings.Default["tuneMenuShowDelay"];
                tuneAppearanceInPerformance = (bool)Properties.Settings.Default["tuneAppearanceInPerformance"];
                tuneQuickAccessHistory = (bool)Properties.Settings.Default["tuneQuickAccessHistory"];
                tuneStartMenuAds = (bool)Properties.Settings.Default["tuneStartMenuAds"];
                tuneSensorService = (bool)Properties.Settings.Default["tuneSensorService"];
                tuneFeatureUpdates = (bool)Properties.Settings.Default["tuneFeatureUpdates"];

                optClearMemory = (bool)Properties.Settings.Default["optClearMemory"];
                optClearCache = (bool)Properties.Settings.Default["optClearCache"];

                RealTimeProtection = (bool)Properties.Settings.Default["RealTimeProtection"];
                AutoUSBScanner = (bool)Properties.Settings.Default["AutoUSBScanner"];

                flagPhishing = (bool)Properties.Settings.Default["flagPhishing"];
                flagCryptojacking = (bool)Properties.Settings.Default["flagCryptojacking"];
                flagRansomware = (bool)Properties.Settings.Default["flagRansomware"];
                flagAffiliateOffers = (bool)Properties.Settings.Default["flagAffiliateOffers"];
            }
            catch(Exception e)
            {
                tuneAutomaticUpdates = true;
                tuneDesktopCleanUpWizard = true;
                tuneMenuShowDelay = true;
                tuneAppearanceInPerformance = true;
                tuneQuickAccessHistory = true;
                tuneStartMenuAds = true;
                tuneSensorService = true;
                tuneFeatureUpdates = true;

                optClearMemory = true;
                optClearCache = true;

                RealTimeProtection = true;
                AutoUSBScanner = true;

                flagPhishing = true;
                flagCryptojacking = true;
                flagRansomware = true;
                flagAffiliateOffers = true;
            }
        }

        public static void SaveConfigs()
        {
            Properties.Settings.Default["tuneAutomaticUpdates"] = tuneAutomaticUpdates;
            Properties.Settings.Default["tuneDesktopCleanUpWizard"] = tuneDesktopCleanUpWizard;
            Properties.Settings.Default["tuneMenuShowDelay"] = tuneMenuShowDelay;
            Properties.Settings.Default["tuneAppearanceInPerformance"] = tuneAppearanceInPerformance;
            Properties.Settings.Default["tuneQuickAccessHistory"] = tuneQuickAccessHistory;
            Properties.Settings.Default["tuneStartMenuAds"] = tuneStartMenuAds;
            Properties.Settings.Default["tuneSensorService"] = tuneSensorService;
            Properties.Settings.Default["tuneFeatureUpdates"] = tuneFeatureUpdates;
            Properties.Settings.Default["optClearMemory"] = optClearMemory;
            Properties.Settings.Default["optClearCache"] = optClearCache;
            Properties.Settings.Default["RealTimeProtection"] = RealTimeProtection;
            Properties.Settings.Default["AutoUSBScanner"] = AutoUSBScanner;
            Properties.Settings.Default["flagPhishing"] = flagPhishing;
            Properties.Settings.Default["flagCryptojacking"] = flagCryptojacking;
            Properties.Settings.Default["flagRansomware"] = flagRansomware;
            Properties.Settings.Default["flagAffiliateOffers"] = flagAffiliateOffers;
            Properties.Settings.Default.Save();
        }

        internal static bool UNSAFE_MODE = false;

        public static bool tuneAutomaticUpdates;
        public static bool tuneDesktopCleanUpWizard;
        public static bool tuneMenuShowDelay;
        public static bool tuneAppearanceInPerformance;
        public static bool tuneQuickAccessHistory;
        public static bool tuneStartMenuAds;
        public static bool tuneSensorService;
        public static bool tuneFeatureUpdates;
        public static bool optClearMemory;
        public static bool optClearCache;
        public static bool RealTimeProtection;
        public static bool AutoUSBScanner;
        public static bool flagPhishing;
        public static bool flagCryptojacking;
        public static bool flagRansomware;
        public static bool flagAffiliateOffers;
    }

}
