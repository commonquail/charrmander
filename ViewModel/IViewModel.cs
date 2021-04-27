using System;

namespace Charrmander.ViewModel
{
    interface IViewModel : IDisposable
    {
        void MarkFileDirty(object sender, EventArgs e);
    }
}
