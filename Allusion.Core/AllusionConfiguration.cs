using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allusion.Core
{
    /// <summary>
    /// Main Configuration for the app...duh
    /// </summary>
    public static class AllusionConfiguration
    {
        public static string ProjectFolder { get; set; }

        public static string DefualtFolder { get; set; }
            = Path.Combine(Environment.SpecialFolder.MyDocuments.ToString(), "Allusion");

    }
}
