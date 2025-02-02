using FinancialStrategyCalculator.Data;
using FinancialStrategyCalculator.Services;
using FinancialStrategyCalculator.Shared;

namespace FinancialStrategyCalculator.Pages
{
    public partial class Insurance
    {
        bool isBusy = false;

        private decimal previousVersion = 0;

        public IEnumerable<DropdownValue> strategicContributionTypes;

        private string errorMessage = "";

        protected override async void OnInitialized()
        {

            errorMessage = Utility.ErroMessages;

            await ReadARTYRTFileFromLocalPath();
            await ReadDRTFileFromLocalPath();

            await ReadTFSARRSPFHSAFromLocalPath();

            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;

            StateHasChanged();

            base.OnInitialized();
        }
        private void HandleValidSubmit()
        {
            errorMessage = Utility.SubmitNext(FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr, FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr, errorMessage, previousVersion);
            Utility.ErroMessages = errorMessage;

            Navigation.NavigateTo("/Strategies", true);

            StateHasChanged();
        }

        private void RevertChangeARTYRT(bool value)
        {
            //Prevent any user interaction to change this button
            FinancialDataService.FinancialStrategyModel.HasARTYRTFileLoaded = !value;

            //FinancialDataService.FinancialStrategyModel.HasTFSARRSPFHSAFileLoaded = !value;
        }
        private void RevertChangeDRT(bool value)
        {
            //Prevent any user interaction to change this button
            FinancialDataService.FinancialStrategyModel.HasDRTFileLoaded = !value;
        }

        private void EnableOrDisableRefundPremium(bool isChecked)
        {
            if (!isChecked)
            {
                FinancialDataService.FinancialStrategyModel.RefundPremiums = false; // Uncheck refundpremium when investmentpayinsurance is unchecked
            }
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

        private void HandleContinueInvestmentCheckboxChange(bool isChecked)
        {
            FinancialDataService.FinancialStrategyModel.AgeToStartPayouts = 65;
            Utility.CalculateAndMatchAgeToStopInvestment(DialogService);
        }

        private async Task GenerateGraphAsync(bool firstRender)
        {
            Utility.PreventPopups = true;

            isBusy = true;

            //Fix Continue Investment for FinancialDataService.FinancialResultsModel & monthlyInsurance
            Utility.CalculateAndMatchAgeToStopInvestment(DialogService);

            //Submit for Computation
            Utility.ErroMessages = Utility.SubmitNext(FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr, FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr, errorMessage, previousVersion);

            isBusy = false;
            Utility.PreventPopups = false;

            //This should always be at the end of this method
            if (firstRender)
            {
                await Task.Delay(200);
                StateHasChanged();
            }
        }

        private async Task LoadProfile()
        {
            isBusy = true;
            await Utility.LoadProfileCommon(errorMessage, previousVersion);
            isBusy = false;

        }
        private async Task SaveProfile()
        {
            await Utility.SaveProfileCommon();
        }
        private void BackScreenn()
        {
            Navigation.NavigateTo("/Investment", false);
        }

        private void NextScreen()
        {
            Navigation.NavigateTo("/Strategies", false);
        }



    }
}
