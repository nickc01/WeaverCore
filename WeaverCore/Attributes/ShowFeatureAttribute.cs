using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ShowFeatureAttribute : Attribute
    {



        /*// See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string positionalString;

        // This is a positional argument
        public ShowFeatureAttribute(string positionalString)
        {
            this.positionalString = positionalString;
        }

        public string PositionalString
        {
            get { return positionalString; }
        }

        // This is a named argument
        public int NamedInt { get; set; }*/
    }
}
