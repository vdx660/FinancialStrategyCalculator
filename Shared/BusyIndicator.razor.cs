namespace FinancialStrategyCalculator.Shared
{
    public class BusyIndicatorService
    {
        public event Action OnChange;
        private bool _isBusy = false;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    NotifyStateChanged();
                }
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }

}
