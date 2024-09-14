using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allusion.WPFCore.Events
{
    public class NewBoardEvent
    {
        public string Name { get; set; } 
        public NewBoardEvent(string name)
        {
            Name = name;
        }
    }
}
