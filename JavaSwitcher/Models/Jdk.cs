using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reactive.Disposables;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace JavaSwitcher.Models
{
    public class Jdk
    {
        public string Name { get; set; }

        public string JavaPath { get; set; }

        public Jdk() { }

        public Jdk(string name, string path)
        {
            Name = name;
            JavaPath = path;
        }
    }
}
