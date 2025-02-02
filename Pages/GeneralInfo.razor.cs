using FinancialStrategyCalculator.Data;
using FinancialStrategyCalculator.Services;
using FinancialStrategyCalculator.Shared;
using Microsoft.AspNetCore.Components;
using Radzen;


namespace FinancialStrategyCalculator.Pages
{
    public partial class GeneralInfo
    {
        bool isBusy = false;

        private decimal previousVersion = 0;

        private string errorMessage = "";

        public IEnumerable<DropdownValue> strategicContributionTypes;

        protected override void OnInitialized()
        {
            strategicContributionTypes = new[] {
                new DropdownValue { Id = 1, Text = "Early Dump (Prioritize RRSP/FHSA - Recommended)" },
				//new DropdownValue { Id = 2, Text = "Divide Between TFSA vs RRSP/FHSA" },
				//new DropdownValue { Id = 3, Text = "Late Dump (Prioritize TFSA)" }
			};

            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;

            StateHasChanged();
        }

        private async Task ReadARTYRTFileFromLocalPath()
        {
            try
            {
                string localFilePath = Utility.ReturnDeviceLocalPathForARTYRT();
                // Check if the file exists
                if (File.Exists(localFilePath))
                {
                    var allLines = await File.ReadAllLinesAsync(localFilePath);
                    FinancialDataService.InsuranceARTYRTDataSettings = Utility.ReadAndParseARTInsuranceData(allLines.ToList());

                    FinancialDataService.FinancialStrategyModel.HasARTYRTFileLoaded = true;
                    StateHasChanged();
                }

            }
            catch (Exception ex)
            {
                Utility.ErroMessages = ex.Message;
            }
        }

        private async Task ReadDRTFileFromLocalPath()
        {
            try
            {
                string localFilePath = Utility.ReturnDeviceLocalPathForDRT();
                // Check if the file exists
                if (File.Exists(localFilePath))
                {
                    var allLines = await File.ReadAllLinesAsync(localFilePath);
                    FinancialDataService.InsuranceDRTDataSettings = Utility.ReadAndParseDRTInsuranceData(allLines.ToList());

                    FinancialDataService.FinancialStrategyModel.HasDRTFileLoaded = true;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Utility.ErroMessages = ex.Message;
            }
        }

        private async Task ReadTFSARRSPFHSAFromLocalPath()
        {
            string localFilePath = Utility.ReturnDeviceLocalPathForTFSARRSPFHSA();
            // Check if the file exists
            if (File.Exists(localFilePath))
            {
                var allLines = await File.ReadAllLinesAsync(localFilePath);
                FinancialDataService.TFSARRSPFHSADataSettings = Utility.ReadAndParseTFSARRSPFHSAData(allLines.ToList());

                FinancialDataService.FinancialStrategyModel.TFSAAnnualMaximumum = FinancialDataService.TFSARRSPFHSADataSettings[0].TFSA;
                FinancialDataService.FinancialStrategyModel.RRSPAnnualRate = FinancialDataService.TFSARRSPFHSADataSettings[0].RRSP;
                FinancialDataService.FinancialStrategyModel.FHSAAnnualMaximum = FinancialDataService.TFSARRSPFHSADataSettings[0].FHSA;

                FinancialDataService.FinancialStrategyModel.TFSARemainingContributionRoom = FinancialDataService.TFSARRSPFHSADataSettings[0].TFSA;
                FinancialDataService.FinancialStrategyModel.RRSPRemainingContributionRoomPercentage = FinancialDataService.TFSARRSPFHSADataSettings[0].RRSP * 100;
                FinancialDataService.FinancialStrategyModel.FHSARemainingContributionRoom = FinancialDataService.TFSARRSPFHSADataSettings[0].FHSA;
                FinancialDataService.FinancialStrategyModel.HasTFSARRSPFHSAFileLoaded = true;
                StateHasChanged();
            }
        }

        private void EnableOrDisableRefundPremium(bool isChecked)
        {
            if (!isChecked)
            {
                FinancialDataService.FinancialStrategyModel.RefundPremiums = false; // Uncheck refundpremium when investmentpayinsurance is unchecked
            }
        }
        private void ResetStrategies()
        {
            FinancialDataService.FinancialStrategyModel.InvestmentPayInsurance = false;
            FinancialDataService.FinancialStrategyModel.RefundPremiums = false;
            FinancialDataService.FinancialStrategyModel.HasARTYRTFileLoaded = false;

            FinancialDataService.InsuranceARTYRTDataSettings = new List<ARTYRTInsuranceData>();

            FinancialDataService.FinancialResultsModel = null;

            StateHasChanged();
        }

        private void RevertChange(bool value)
        {
            //Prevent any user interaction to change this button
            FinancialDataService.FinancialStrategyModel.HasARTYRTFileLoaded = !value;
            FinancialDataService.FinancialStrategyModel.HasTFSARRSPFHSAFileLoaded = !value;
        }

        private void OnIncDecChange(string value, string name)
        {
            try
            {
                List<decimal> increases = new List<decimal>();
                increases = FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr.Split(';').Select(decimal.Parse).ToList();
                string formattedString = string.Join("; ", increases.Select(i => i.ToString("N2")));
                FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr = formattedString;
                Utility.SubmitDisabledState = false;
            }
            catch (Exception)
            {
                errorMessage = "Only Numbers are accepted. Please correct this and try again.";
                Utility.SubmitDisabledState = true;
                FinancialDataService.FinancialResultsModel = null;
            }
        }

        private async void HandleValidSubmit()
        {
            if (FinancialDataService.FinancialStrategyModel?.FirstName != null && FinancialDataService.FinancialStrategyModel?.LastName != null)
            {
                errorMessage = Utility.SubmitNext(FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr, FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr, errorMessage, previousVersion);
                Utility.ErroMessages = errorMessage;

                Navigation.NavigateTo("/Investment", true);

            }
            else
            {
                await DialogService.Alert("At least First Name & Last Name are required");
            }
            StateHasChanged();
        }

        private void AnnualIncomeChange(decimal value)
        {
            Utility.ComputeTFSABankOnAnnualIncomeChange();
        }

        private void RetirementIncomeChange(decimal value)
        {

        }

        private void OnChangeStrategicContributionType(object value)
        {
            FinancialDataService.FinancialStrategyModel.StrategicContributionType = (int)value;
        }
        private void ShowTooltip(ElementReference elementReference, string tooltipName, TooltipOptions options = null)
        {
            switch (tooltipName)
            {
                case "currentincome":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string currentincomeTooltip = $"<div>" +
                        "Enter your current income to calculate CPP contributions." +
                        "</div>";
                    tooltipService.Open(elementReference, currentincomeTooltip, options);
                    break;
                case "incomecpi":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string incomecpiTooltip = $"<div>" +
                        "The default of 1.5% is based on Canada's Consumer Price Index (CPI) Historical Average." +
                        "</div>";
                    tooltipService.Open(elementReference, incomecpiTooltip, options);
                    break;
                case "annualpayout":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string annualpayoutTooltip = $"<div>" +
                        "This is the Amount you are expecting to receive Monthly/Annually when you retire." +
                        "</div>";
                    tooltipService.Open(elementReference, annualpayoutTooltip, options);
                    break;
                default:
                    break;
            }
        }

        private void OnBirthDateChange(DateTime? value)
        {
            FinancialDataService.FinancialStrategyModel.Age = (int)((DateTime.Now - FinancialDataService.FinancialStrategyModel.BirthDate).TotalDays / 365.25);
        }

        private async Task LoadProfile()
        {
            isBusy = true;
            ResetStrategies();
            await Utility.LoadProfileCommon(errorMessage, previousVersion);
            isBusy = false;

        }

        private async Task SaveProfile()
        {
            await Utility.SaveProfileCommon();
        }

        private void NextScreen()
        {
            Navigation.NavigateTo("/Investment", false);
        }
    }
}
