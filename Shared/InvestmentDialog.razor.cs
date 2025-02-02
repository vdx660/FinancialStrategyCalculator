using FinancialStrategyCalculator.Data;
using FinancialStrategyCalculator.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace FinancialStrategyCalculator.Shared
{
    public partial class InvestmentDialog
    {
        [Parameter]
        public EventCallback OnSave { get; set; }

        public ContributionChanges InvestmentDetails { get; set; } = new ContributionChanges();

        protected override void OnInitialized()
        {
            base.OnInitialized();
            InvestmentDetails.AutoUpdate = true;
            InvestmentDetails.OneTimeIncrease = true;
            InvestmentDetails.AgeToStopChange = FinancialDataService.FinancialStrategyModel.Age + 1;
        }

        private void OKSubmit()
        {
            DialogService.Close(InvestmentDetails); // passing data back
        }

        private void CancelSubmit()
        {
            DialogService.Close();
        }

        private void OneTimeIncreaseChange(bool value)
        {

        }

        private void ShowTooltip(ElementReference elementReference, string tooltipName, TooltipOptions options = null)
        {
            switch (tooltipName)
            {
                case "adjustcontributions":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string adjustcontributionsTooltip = $"<div>" +
                        "This will Increase or Decrease (+/-) Monthly Contributions,<br>" +
                        "Every year Until Specified Age.<br>" +
                        "</div>";
                    tooltipService.Open(elementReference, adjustcontributionsTooltip, options);
                    break;
                case "adjustuntilage":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string adjustuntilageTooltip = $"<div>" +
                        "Adjustment (+/-) will happen every year Until Specified Age." +
                        "</div>";
                    tooltipService.Open(elementReference, adjustuntilageTooltip, options);
                    break;
                case "onetimeadj":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string onetimeadjTooltip = $"<div>" +
                        "This will make a one time adjustment (+/-) if checked, otherwise<br>" +
                        "Adjustment (+/-) will happen every year Until Specified Age.<br>" +
                        "</div>";
                    tooltipService.Open(elementReference, onetimeadjTooltip, options);
                    break;
                case "autoupdate":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string autoupdateTooltip = $"<div>" +
                        "Confirmation will not be asked when clicking on the OK button." +
                        "</div>";
                    tooltipService.Open(elementReference, autoupdateTooltip, options);
                    break;
                case "clearentries":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string clearentriesTooltip = $"<div>" +
                        "Existing Entries will be cleared." +
                        "</div>";
                    tooltipService.Open(elementReference, clearentriesTooltip, options);
                    break;
                default:
                    break;
            }
        }

    }
}
