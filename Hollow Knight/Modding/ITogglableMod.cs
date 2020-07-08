using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Modding
{
    public interface ITogglableMod : IMod
    {
        void Unload();
    }
}
