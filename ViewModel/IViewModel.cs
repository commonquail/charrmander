using System;

namespace Charrmander.ViewModel
{
    internal interface IViewModel : IDisposable
    {
        void MarkFileDirty(object? sender, EventArgs e);
    }
}
