using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automaton.Utils;
public class Events
{
    public delegate void OnGMAppearanceDelegate();
    public event OnGMAppearanceDelegate OnGMAppearance;
}
