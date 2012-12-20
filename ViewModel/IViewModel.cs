using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Charrmander.ViewModel
{
    interface IViewModel
    {
        void MarkFileDirty(object sender, EventArgs e);
    }
}
