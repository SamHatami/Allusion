using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allusion.Core.Artboard
{
    [Serializable]
    public class BoardPage
    {
        public string Description { get; set; }
        public ImageItem[] ImageItems { get; set; }

        public NoteItem[] NoteItems { get; set; }

    }


}
