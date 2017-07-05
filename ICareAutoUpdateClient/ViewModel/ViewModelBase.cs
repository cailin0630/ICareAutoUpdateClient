namespace ICareAutoUpdateClient.ViewModel
{
    public class ViewModelBase : GalaSoft.MvvmLight.ViewModelBase
    {
        public void OnPropertyChanged()
        {
            base.RaisePropertyChanged(null);
        }
    }
}