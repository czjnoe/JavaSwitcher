using ReactiveUI;

namespace JavaSwitcher.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject
    {
        protected virtual void RaisePropertyChanged(string name)
        {
            IReactiveObjectExtensions.RaisePropertyChanged(this, name);
        }

        protected virtual void RaisePropertyChanging(string name)
        {
            IReactiveObjectExtensions.RaisePropertyChanging(this, name);
        }
    }
}
