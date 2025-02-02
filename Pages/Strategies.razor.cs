using FinancialStrategyCalculator.Data;
using FinancialStrategyCalculator.Services;
using FinancialStrategyCalculator.Shared;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace FinancialStrategyCalculator.Pages
{
    public partial class Strategies
    {
        #region Fields
        private BankAndDumpChanges insertItem = null;
        private BankAndDumpChanges editingItem = null;
        private RadzenDataGrid<BankAndDumpChanges> bankNdumpChangesGrid;
        private List<BankAndDumpChanges> bankNdumpChangesList;

        private bool advancedContributionsVisibility = false;

        private decimal previousVersion = 0;

        public IEnumerable<DropdownValue> strategicContributionTypes;

        private string errorMessage = "";
        private bool isBusy;

        private string DisabledNumericStyle => !FinancialDataService.FinancialStrategyModel.IncludeCPPContributionsBenefits ? "background: #EDF1F7" : string.Empty;


        #endregion

        #region Properties
        private bool IsCPPSelected
        {
            get
            {
                return FinancialDataService.FinancialStrategyModel.IncludeCPPContributionsBenefits;
            }
            set
            {
                if (!value)
                {
                    FinancialDataService.FinancialStrategyModel.CPPIDIncludeEstimatedOAS = false;
                }
                FinancialDataService.FinancialStrategyModel.IncludeCPPContributionsBenefits = value;
            }
        }

        #endregion

        #region Methods
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            strategicContributionTypes = new[] {
                new DropdownValue { Id = 1, Text = "Early Dump (Prioritize RRSP/FHSA - Recommended)" },
				//new DropdownValue { Id = 2, Text = "Divide Between TFSA vs RRSP/FHSA" },
				//new DropdownValue { Id = 3, Text = "Late Dump (Prioritize TFSA)" }
			};

            bankNdumpChangesList = new List<BankAndDumpChanges>();
            bankNdumpChangesList.Add(new BankAndDumpChanges() { BankAndDumpAmount = FinancialDataService.FinancialStrategyModel.AmountToBankInTFSA, AgeToBankAndDump = FinancialDataService.FinancialStrategyModel.AgeToStartPayouts });

            errorMessage = Utility.ErroMessages;

            await ReadARTYRTFileFromLocalPath();
            await ReadDRTFileFromLocalPath();

            await ReadTFSARRSPFHSAFromLocalPath();

            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;

            StateHasChanged();
        }

        private void HandleValidSubmit()
        {
            errorMessage = Utility.SubmitNext(FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr, FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr, errorMessage, previousVersion);
            Utility.ErroMessages = errorMessage;

            StateHasChanged();

            Navigation.NavigateTo("/PlanResult", true);
        }

        private void EnableOrDisableRefundPremium(bool isChecked)
        {
            if (!isChecked)
            {
                FinancialDataService.FinancialStrategyModel.RefundPremiums = false; // Uncheck refundpremium when investmentpayinsurance is unchecked
            }
        }

        private void RevertChangeARTYRT(bool value)
        {
            //Prevent any user interaction to change this button
            FinancialDataService.FinancialStrategyModel.HasARTYRTFileLoaded = !value;
        }
        private void RevertChangeDRT(bool value)
        {
            //Prevent any user interaction to change this button
            FinancialDataService.FinancialStrategyModel.HasDRTFileLoaded = !value;
        }
        private void RevertChangeTFSARRSPFHSA(bool value)
        {
            //Prevent any user interaction to change this button
            FinancialDataService.FinancialStrategyModel.HasTFSARRSPFHSAFileLoaded = !value;
        }

        private void OnChangeStrategicContributionType(object value)
        {
            FinancialDataService.FinancialStrategyModel.StrategicContributionType = (int)value;
        }
        private void ShowTooltip(ElementReference elementReference, string tooltipName, TooltipOptions options = null)
        {
            switch (tooltipName)
            {
                case "banktotfsa":
                    tooltipService.Open(elementReference, "Ideal is around 15%-100% of Annual Income or 100k whichever is lesser", options);
                    break;
                case "strategiccontribution":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string strategiccontributionTooltip = $"<div>" +
                        "Number of times to (Bank in TFSA) and Dump to RRSP<br>" +
                        "Set to Zero (0) if you want TFSA only and no RRSP<br>" +
                        "</div>";
                    tooltipService.Open(elementReference, strategiccontributionTooltip, options);
                    break;
                case "movetorrif":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string movetorrifTooltip = $"<div>" +
                        "Ideal Age to move your RRSP to RRIF would be 71.<br>" +
                        " WARNING: If you move over 71, Gains from your RRSP will be taxed.<br>" +
                        "</div>";
                    tooltipService.Open(elementReference, movetorrifTooltip, options);
                    break;
                case "insuranceplan":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string insuranceplanTooltip = $"<div>" +
                        "This App offer 3 out of the 4 options that Primerica has.<br>" +
                        "Amounts here do not include any riders or additonal options<br>" +
                        "which may increase the Actual Premium.<br>" +
                        "</div>";
                    tooltipService.Open(elementReference, insuranceplanTooltip, options);
                    break;
                case "smartratios1":
                    options = new TooltipOptions() { Delay = 500, Duration = 10000 };
                    string smartratiosTooltip1 = $"<div>" +
                        "<b>Calculate based on Retirement Pension Payout for Investment Ratio</b> does not consider Income/Salary.<br>" +
                        "&nbsp;&nbsp;It looks at the Projected Annual Retirement Payout and tries to provide an appropriate amount.<br>" +
                        "&nbsp;&nbsp;<u>Use this for clients who are willing to pay the amount not matter their present income.</u><br>" +
                        "</div>";
                    tooltipService.Open(elementReference, smartratiosTooltip1, options);
                    break;
                case "smartratios2":
                    options = new TooltipOptions() { Delay = 500, Duration = 10000 };
                    string smartratiosTooltip2 = $"<div>" +
                        "<b>Calculate based on Annual Income for Investment Ratio (Default)</b> uses Annual Income/Salary including<br>" +
                        "&nbsp;&nbsp;anticipated increases. It then reduces the Pension Payout appropriately.<br>" +
                        "&nbsp;&nbsp;<u>Use this for clients who are expecting to get a Pension Payout similar to their Salary/Income.</u><br>" +
                        "</div>";
                    tooltipService.Open(elementReference, smartratiosTooltip2, options);
                    break;
                case "smartratios3":
                    options = new TooltipOptions() { Delay = 500, Duration = 10000 };
                    string smartratiosTooltip3 = $"<div>" +
                        "<b>Calculate based on Insurance for Investment Ratio</b> uses Insurance Face Amount to compute Payout.<br>" +
                        "&nbsp;&nbsp;It does not consider Salary/Income or Projected Annual Retirement Payout.<br>" +
                        "&nbsp;&nbsp;<u>Use this for clients who are expecting to get a Pension Payout similar to their Insurance Face Amount.</u><br>" +
                        "</div>";
                    tooltipService.Open(elementReference, smartratiosTooltip3, options);
                    break;
                case "incomecpi":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string incomecpiTooltip = $"<div>" +
                        "The default of 1.5% is based on Canada's Consumer Price Index (CPI) Historical Average" +
                        "</div>";
                    tooltipService.Open(elementReference, incomecpiTooltip, options);
                    break;
                case "startpayout":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string startpayoutTooltip = $"<div>" +
                        "This is the Age to Start Payouts. The Age to Stop Contributions will be<br>" +
                        "the Highest Investment Age Pair Adjustment on the Investment Tab" +
                        "</div>";
                    tooltipService.Open(elementReference, startpayoutTooltip, options);
                    break;
                default:
                    break;
            }
        }
        private void BackButton()
        {
            Navigation.NavigateTo("/Insurance", true);
        }

        private async Task ReadARTYRTFileFromLocalPath()
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

        private void AnnualIncomeChange(decimal value)
        {
            Utility.ComputeTFSABankOnAnnualIncomeChange();
        }

        private void Reset()
        {
            insertItem = null;
            editingItem = null;
        }
        private async Task InsertAsync()
        {
            BankAndDumpChanges result = await DialogService.OpenAsync<BankAndDumpDialog>("Bank & Dump Details", null, new DialogOptions() { Width = "500px", Height = "400px", ShowClose = false });
            if (result != null)
            {

                if (result.ClearExistingEntries)
                {
                    bankNdumpChangesList.Clear();
                }

                // Handle the result if needed. For example, you can update a property or call a method.
                insertItem = new BankAndDumpChanges() { AgeToBankAndDump = result.AgeToBankAndDump, BankAndDumpAmount = result.BankAndDumpAmount };
                await bankNdumpChangesGrid.InsertRow(insertItem);

                if (result.AutoUpdate)
                {
                    await UpdateAsync(insertItem);
                }
                Utility.EditMode = true;
                Utility.SubmitDisabledState = true;
            }
        }

        private async Task EditAsync(BankAndDumpChanges item)
        {
            editingItem = item;
            await bankNdumpChangesGrid.EditRow(editingItem);

            Utility.EditMode = true;
            Utility.SubmitDisabledState = true;
        }

        private async Task UpdateAsync(BankAndDumpChanges item)
        {
            if (insertItem != null && editingItem == null)
            {
                bankNdumpChangesList.Add(item);
            }
            bankNdumpChangesList = bankNdumpChangesList.OrderBy(item => item.AgeToBankAndDump).ToList();

            //FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr = string.Join(";", bankNdumpChangesList.Select(i => i.BankAndDumpAmount.ToString("N2")));
            //FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr = string.Join(";", bankNdumpChangesList.Select(i => i.AgeToBankAndDump));

            //errorMessage = Utility.SubmitNext(FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr, FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr, errorMessage, previousVersion);

            Reset();
            await bankNdumpChangesGrid.UpdateRow(item);

            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;
        }

        private async Task CancelAsync(BankAndDumpChanges item)
        {
            try
            {
                if (editingItem == null)
                {
                    editingItem = item;
                }
                bankNdumpChangesGrid.CancelEditRow(editingItem);

            }
            catch (Exception)
            {

            }
            await bankNdumpChangesGrid.Reload();

            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;
        }

        private async Task DeleteAsync(BankAndDumpChanges item)
        {
            Reset();
            if (bankNdumpChangesList.Count > 1)
            {
                bankNdumpChangesList.Remove(item);
                await bankNdumpChangesGrid.Reload();
            }

            bankNdumpChangesList = bankNdumpChangesList.OrderBy(item => item.AgeToBankAndDump).ToList();

            //FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr = string.Join(";", bankNdumpChangesList.Select(i => i.AnnualChangesOnMonthlyContributions.ToString("N2")));
            //FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr = string.Join(";", bankNdumpChangesList.Select(i => i.AgeToStopChange));

            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;
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
            Navigation.NavigateTo("/Insurance", false);
        }

        private void NextScreen()
        {
            Navigation.NavigateTo("/PlanResult", false);
        }

        #endregion
    }
}
