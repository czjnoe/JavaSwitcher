using JavaSwitcher.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JavaSwitcher.ViewModels
{
    public class NewOrEditJdkViewModel : ViewModelBase
    {
        private string _name = "";
        private string _javaPath = "";

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string JavaPath
        {
            get => _javaPath;
            set => this.RaiseAndSetIfChanged(ref _javaPath, value);
        }

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
