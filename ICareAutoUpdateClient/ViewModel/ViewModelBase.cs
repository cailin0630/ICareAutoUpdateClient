using GalaSoft.MvvmLight.Command;
using System.Windows.Controls;

namespace ICareAutoUpdateClient.ViewModel
{
    public class ViewModelBase : GalaSoft.MvvmLight.ViewModelBase
    {
        public ViewModelBase()
        {
            new RelayCommand(OnLoad).Execute(null);
            new RelayCommand<ContentControl>(OnLoad).Execute(null);
        }

        protected virtual void OnLoad()
        {
        }

        protected virtual void OnLoad(ContentControl param)
        {
        }

        public void OnPropertyChanged(string propertyName=null)
        {
            base.RaisePropertyChanged(propertyName);
        }
    }
}