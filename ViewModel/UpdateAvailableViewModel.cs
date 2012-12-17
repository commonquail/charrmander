using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Charrmander.Util;
using System.Windows;
using Charrmander.View;

namespace Charrmander.ViewModel
{
    class UpdateAvailableViewModel : AbstractNotifier
    {
        public UpdateAvailableViewModel()
        {
            Window window = new UpdateAvailableView();
            window.DataContext = this;
            window.Show();
        }
    }
}
