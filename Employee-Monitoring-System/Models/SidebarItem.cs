using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Employee_Monitoring_System.Models
{
    public class SidebarItem
    {
        public string Title { get; set; }
        public string Icon { get; set; } // e.g., FontAwesome or image path
        public string NavigationTarget { get; set; }
        public bool RequiresRoleCheck { get; set; } = false;

        // This property is used by the SidebarViewModel's NavigateCommand
        public ICommand NavigateCommand { get; set; }
    }

}
