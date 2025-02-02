using FinancialStrategyCalculator.Data;
using FinancialStrategyCalculator.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace FinancialStrategyCalculator.Shared
{
    public partial class BankAndDumpDialog
    {
        [Parameter]
        public EventCallback OnSave { get; set; }

        public BankAndDumpChanges BankAndDumpDetails { get; set; } = new BankAndDumpChanges();

        protected override void OnInitialized()
        {
            base.OnInitialized();
            BankAndDumpDetails.AutoUpdate = true;
            BankAndDumpDetails.AgeToBankAndDump = FinancialDataService.FinancialStrategyModel.Age + 1;
            BankAndDumpDetails.BankAndDumpAmount = FinancialDataService.FinancialStrategyModel.AmountToBankInTFSA;
        }

        private void OKSubmit()
        {
            DialogService.Close(BankAndDumpDetails); // passing data back
        }

        private void CancelSubmit()
        {
            DialogService.Close();
        }

        private void OneTimeIncreaseChange(bool value)
        {

        }

    }
}
