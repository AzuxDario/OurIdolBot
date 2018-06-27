using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurIdolBot.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandsGroupAttribute : Attribute
    {
        public string Group { get; }

        public CommandsGroupAttribute(string group)
        {
            Group = group;
        }
    }
}
