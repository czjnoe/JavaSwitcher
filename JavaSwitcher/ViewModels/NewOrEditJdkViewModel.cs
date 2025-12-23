using JavaSwitcher.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PropertyChanged.SourceGenerator;

namespace JavaSwitcher.ViewModels
{
    public partial class NewOrEditJdkViewModel : ViewModelBase
    {
        [Notify]
        private string _name;

        [Notify]
        private string _javaPath;

        public NewOrEditJdkViewModel()
        {

        }

        public Jdk ToJdk()
        {
            return new Jdk
            {
                Name = Name,
                JavaPath = JavaPath
            };
        }

        public void FromJdk(Jdk jdk)
        {
            Name = jdk.Name;
            JavaPath = jdk.JavaPath;
        }
    }
}
