using FinancialStrategyCalculator.Data;
using FinancialStrategyCalculator.Services;
using FinancialStrategyCalculator.Shared;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace FinancialStrategyCalculator.Pages
{
    public partial class Investment
    {
        #region Fields
        bool isBusy = false;

        private decimal previousVersion = 0;

        private string errorMessage = "";

        private List<ContributionChanges> contributionChangesList;
        private ContributionChanges insertItem = null;
        private ContributionChanges editingItem = null;
        private RadzenDataGrid<ContributionChanges> contributionChangesGrid;
        public IEnumerable<DropdownValue> strategicContributionTypes;

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

        private decimal LastPayoutValue
        {
            get
            {
                return FinancialDataService.FinancialResultsModel?.FirstOrDefault((a) => a.Age == FinancialDataService.FinancialStrategyModel.AgeToEndInvestment)?.AnnualCompoundedInvestmentAve ?? 0;
            }
            set
            {
                //Nothing to Set
            }
        }

        private int LastPayoutAge
        {
            get
            {
                return FinancialDataService.FinancialStrategyModel?.AgeToEndInvestment ?? 0;
            }
            set { } //Nothing to Set
        }


        private decimal HighestMonthlyInvestmentContrtibution
        {
            get
            {
                return FinancialDataService.FinancialResultsModel?.MaxBy((m) => m.MonthlyInvestment)?.MonthlyInvestment ?? 0;
            }
            set
            {
                //Nothing to Set
            }
        }

        private bool advancedInvestmentVisibility = false;
        private bool AdvancedInvestmentVisibility
        {
            get
            {
                return advancedInvestmentVisibility;
            }
            set
            {
                advancedInvestmentVisibility = value;
            }
        }



        #endregion

        #region Methods
        protected override void OnInitialized()
        {
            var contributions = FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr.Split(';')?.Select(decimal.Parse).ToList();
            var ages = FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr.Split(';')?.Select(int.Parse).ToList();

            if (contributions.Count == ages.Count)
            {
                contributionChangesList = contributions.Zip(ages, (c, a) => new ContributionChanges
                {
                    AnnualChangesOnMonthlyContributions = c,
                    AgeToStopChange = a
                }).ToList();

                // Now, contributionChangesList contains the combined values.
            }

            errorMessage = Utility.ErroMessages;

            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;

            //Fix Continue Investment for FinancialDataService.FinancialResultsModel & monthlyInsurance
            Utility.CalculateAndMatchAgeToStopInvestment(DialogService);

            if (contributionChangesList != null && contributionChangesList.Count > 0)
            {
                decimal totalchanges = 0;
                foreach (var contribution in contributionChangesList)
                {
                    totalchanges += contribution.AnnualChangesOnMonthlyContributions;
                }
                if (totalchanges > 0)
                {
                    AdvancedInvestmentVisibility = true;

                }
            }
            if (FinancialDataService.FinancialStrategyModel.UseCPIForMonthlyInvestment)
            {
                AdvancedInvestmentVisibility = true;
            }

            StateHasChanged();

            base.OnInitialized();

        }

        private void Reset()
        {
            insertItem = null;
            editingItem = null;
        }
        private async Task AIComputedAsync()
        {
            switch (FinancialDataService.FinancialStrategyModel.IncomeInsuranceInvestmentRatios)
            {
                case 1:
                    await RetirementPensionPayoutRatio();
                    break;
                case 2:
                    await AnnualIncomeRatio();
                    break;
                case 3:
                    await InsuranceFaceAmountRatio();
                    break;
                default:
                    break;
            }

            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;
        }

        private async Task AdjustPayout()
        {
            FinancialDataService.FinancialStrategyModel.ProjectedAnnualRetirementIncome = 0;
            do
            {
                FinancialDataService.FinancialStrategyModel.ProjectedAnnualRetirementIncome += 500;
                await GenerateGraphAsync(false);

            } while (LastPayoutValue >= 0);

            FinancialDataService.FinancialStrategyModel.ProjectedAnnualRetirementIncome -= 500;
            await GenerateGraphAsync(true);

        }
        private async Task InsertAsync()
        {
            ContributionChanges result = await DialogService.OpenAsync<InvestmentDialog>("Investment Details", null, new DialogOptions() { Width = "600px", Height = "400px", ShowClose = false });
            if (result != null)
            {
                if (result.ClearExistingEntries)
                {
                    contributionChangesList.Clear();
                }

                if (result.OneTimeIncrease)
                {
                    insertItem = new ContributionChanges() { AgeToStopChange = result.AgeToStopChange, AnnualChangesOnMonthlyContributions = 0 };
                    await contributionChangesGrid.InsertRow(insertItem);
                    await UpdateAsync(insertItem);

                    insertItem = new ContributionChanges() { AgeToStopChange = result.AgeToStopChange + 1, AnnualChangesOnMonthlyContributions = result.AnnualChangesOnMonthlyContributions };
                    await contributionChangesGrid.InsertRow(insertItem);
                    await UpdateAsync(insertItem);

                    Utility.EditMode = false;
                    Utility.SubmitDisabledState = false;
                }
                else
                {
                    // Handle the result if needed. For example, you can update a property or call a method.
                    insertItem = new ContributionChanges() { AgeToStopChange = result.AgeToStopChange, AnnualChangesOnMonthlyContributions = result.AnnualChangesOnMonthlyContributions };
                    await contributionChangesGrid.InsertRow(insertItem);

                    if (result.AutoUpdate)
                    {
                        await UpdateAsync(insertItem);
                    }
                    Utility.EditMode = true;
                    Utility.SubmitDisabledState = true;
                }
            }
        }

        private async Task EditAsync(ContributionChanges item)
        {
            editingItem = item;
            await contributionChangesGrid.EditRow(editingItem);

            Utility.EditMode = true;
            Utility.SubmitDisabledState = true;
        }

        private async Task UpdateAsync(ContributionChanges item)
        {
            if (insertItem != null && editingItem == null)
            {
                contributionChangesList.Add(item);
            }
            contributionChangesList = contributionChangesList.OrderBy(item => item.AgeToStopChange).ToList();

            FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr = string.Join(";", contributionChangesList.Select(i => i.AnnualChangesOnMonthlyContributions.ToString("N2")));
            FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr = string.Join(";", contributionChangesList.Select(i => i.AgeToStopChange));

            errorMessage = Utility.SubmitNext(FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr, FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr, errorMessage, previousVersion);

            Reset();
            await contributionChangesGrid.UpdateRow(item);

            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;
        }

        private async Task CancelAsync(ContributionChanges item)
        {
            try
            {
                if (editingItem == null)
                {
                    editingItem = item;
                }
                contributionChangesGrid.CancelEditRow(editingItem);

            }
            catch (Exception)
            {

            }
            await contributionChangesGrid.Reload();

            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;
        }

        private async Task DeleteAsync(ContributionChanges item)
        {
            Reset();
            if (contributionChangesList.Count > 1)
            {
                contributionChangesList.Remove(item);
                await contributionChangesGrid.Reload();
            }

            contributionChangesList = contributionChangesList.OrderBy(item => item.AgeToStopChange).ToList();

            FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr = string.Join(";", contributionChangesList.Select(i => i.AnnualChangesOnMonthlyContributions.ToString("N2")));
            FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr = string.Join(";", contributionChangesList.Select(i => i.AgeToStopChange));

            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;
        }

        private void HandleValidSubmit()
        {
            errorMessage = Utility.SubmitNext(FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr, FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr, errorMessage, previousVersion);
            Utility.ErroMessages = errorMessage;

            Navigation.NavigateTo("/Insurance", true);

            StateHasChanged();
        }

        private async Task CalculateRatioBasedOnYearlyAmount(decimal incomebasis)
        {
            // delete records logic
            contributionChangesList.Clear();
            int numberOfPayments = 105 - FinancialDataService.FinancialStrategyModel.AgeToMoveToRRIF;
            double aveRateNumerator = (double)(FinancialDataService.FinancialStrategyModel.AnnualInterestRateHigh + FinancialDataService.FinancialStrategyModel.AnnualInterestRateLow) / 2;
            double AmtToReach = Utility.PresentValueAnnuity((double)incomebasis, (double)FinancialDataService.FinancialStrategyModel.RRIFRate, numberOfPayments);

            double monthlyInv = 25;
            double compGrowth = 5;
            int maxInv = (int)(incomebasis * 0.004m);
            int maxGrowth = (int)(incomebasis);
            for (double inInv = 25; inInv < maxInv; inInv++)
            {
                compGrowth = ComputeYearlyContributionGrowth(incomebasis, aveRateNumerator, AmtToReach, compGrowth, maxGrowth, inInv);
                monthlyInv = Utility.CalculateMonthlyInvestment(inInv, FinancialDataService.FinancialStrategyModel.Age);
                if ((compGrowth * 5) < monthlyInv)
                {
                    break;
                }
            }

            FinancialDataService.FinancialStrategyModel.MonthlyInvestment = (decimal)Math.Round(monthlyInv / 5) * 5 + 5;
            int ageToStopChange = FinancialDataService.FinancialStrategyModel.AgeToStartPayouts < 65 ? FinancialDataService.FinancialStrategyModel.AgeToStartPayouts : FinancialDataService.FinancialStrategyModel.AgeToStartPayouts + 1;
            contributionChangesList.Add(new ContributionChanges() { AgeToStopChange = ageToStopChange, AnnualChangesOnMonthlyContributions = (decimal)(Math.Round(compGrowth / 1) * 1) + 5 });
            await AddContributionsAndSubmit();
        }

        private static double ComputeYearlyContributionGrowth(decimal incomebasis, double aveRateNumerator, double AmtToReach, double compGrowth, int maxGrowth, double inInv)
        {
            for (int growth = 1; growth <= maxGrowth; growth++)
            {
                double futureValue = Utility.CalculateFutureValueWithGrowth(
                    inInv,
                    (double)incomebasis,
                    aveRateNumerator / 100,
                    growth,
                    (FinancialDataService.FinancialStrategyModel.AgeToStartPayouts - FinancialDataService.FinancialStrategyModel.Age));

                compGrowth = Utility.CalculateGrowth(growth, FinancialDataService.FinancialStrategyModel.Age);

                if (futureValue >= AmtToReach)
                {
                    break;
                }
            }

            return Utility.CalculateGrowth(compGrowth, FinancialDataService.FinancialStrategyModel.Age);
        }

        private async Task RetirementPensionPayoutRatio()
        {
            FinancialDataService.CalculationOngoing = true;

            var result = await DialogService.Confirm("Are you sure?", "“Warning: This will erase all Grid records related to ‘Annual Increases / Decreases on Investment Monthly Contributions’ and ‘Ages to Change / Stop Investment Increase / Decrease’. Proceed with caution!”!", new ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "No" });
            if (result.HasValue && result.Value)
            {
                await CalculateRatioBasedOnYearlyAmount(FinancialDataService.FinancialStrategyModel.ProjectedAnnualRetirementIncome);

                FinancialDataService.CalculationOngoing = false;

            }
        }

        private async Task AnnualIncomeRatio()
        {
            FinancialDataService.CalculationOngoing = true;
            var result = await DialogService.Confirm("Are you sure?", "“Warning: This will erase all Grid records related to ‘Annual Increases / Decreases on Investment Monthly Contributions’ and ‘Ages to Change / Stop Investment Increase / Decrease’. Proceed with caution!”!", new ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "No" });
            if (result.HasValue && result.Value)
            {
                //Adjust RetirementPayount (ProjectedAnnualRetirementIncome) and calculate from this amount.
                decimal averageRate = (FinancialDataService.FinancialStrategyModel.AnnualInterestRateHigh + FinancialDataService.FinancialStrategyModel.AnnualInterestRateLow) / 2 / 100;
                FinancialDataService.FinancialStrategyModel.ProjectedAnnualRetirementIncome = Utility.CalculatePresentValue(FinancialDataService.FinancialStrategyModel.AnnualIncome, averageRate, FinancialDataService.FinancialStrategyModel.Age, FinancialDataService.FinancialStrategyModel.AgeToStartPayouts);

                await CalculateRatioBasedOnYearlyAmount(FinancialDataService.FinancialStrategyModel.ProjectedAnnualRetirementIncome);

                FinancialDataService.CalculationOngoing = false;

            }
        }

        private async Task InsuranceFaceAmountRatio()
        {
            var result = await DialogService.Confirm("Are you sure?", "“Warning: This will erase all Grid records related to ‘Annual Increases / Decreases on Investment Monthly Contributions’ and ‘Ages to Change / Stop Investment Increase / Decrease’. Proceed with caution!”!", new ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "No" });
            if (result.HasValue && result.Value)
            {
                // delete records logic
                contributionChangesList.Clear();

                decimal averageRate = (FinancialDataService.FinancialStrategyModel.AnnualInterestRateHigh + FinancialDataService.FinancialStrategyModel.AnnualInterestRateLow) / 2 / 100;

                FinancialDataService.FinancialStrategyModel.ProjectedAnnualRetirementIncome = Utility.CalculatePresentValue(FinancialDataService.FinancialStrategyModel.AnnualIncome, averageRate, FinancialDataService.FinancialStrategyModel.Age, FinancialDataService.FinancialStrategyModel.AgeToStartPayouts);

                decimal initialMonthlyValue = (decimal)Math.Round(Utility.CalculateInitialInvestment((double)FinancialDataService.FinancialStrategyModel.FaceAmount, (double)averageRate, 1, FinancialDataService.FinancialStrategyModel.Age, FinancialDataService.FinancialStrategyModel.AgeToStartPayouts)) / 12 / 2;

                FinancialDataService.FinancialStrategyModel.MonthlyInvestment = Math.Ceiling((initialMonthlyValue < 25 ? 25 : initialMonthlyValue * 0.55m) / 5) * 5;

                decimal monthlyChangesInContribution = Math.Round((FinancialDataService.FinancialStrategyModel.MonthlyInvestment / 10 / 5) + 1, 0) * 5;

                contributionChangesList.Add(new ContributionChanges() { AgeToStopChange = FinancialDataService.FinancialStrategyModel.AgeToStartPayouts, AnnualChangesOnMonthlyContributions = monthlyChangesInContribution });
                await AddContributionsAndSubmit();

            }
        }

        private async Task AddContributionsAndSubmit()
        {
            await contributionChangesGrid.Reload();

            contributionChangesList = contributionChangesList.OrderBy(item => item.AgeToStopChange).ToList();

            FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr = string.Join(";", contributionChangesList.Select(i => i.AnnualChangesOnMonthlyContributions.ToString("N2")));
            FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr = string.Join(";", contributionChangesList.Select(i => i.AgeToStopChange));

            errorMessage = Utility.SubmitNext(FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr, FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr, errorMessage, previousVersion);
            Utility.ErroMessages = errorMessage;

            StateHasChanged();

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

        private void AnnualIncomeChange(decimal value)
        {
            Utility.ComputeTFSABankOnAnnualIncomeChange();
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
            Navigation.NavigateTo("/GeneralInfo", false);
        }

        private void NextScreen()
        {
            Navigation.NavigateTo("/Insurance", false);
        }

        private void ShowTooltip(ElementReference elementReference, string tooltipName, TooltipOptions options = null)
        {
            switch (tooltipName)
            {
                case "adjustpayouttononzero":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string adjustpayouttononzeroTooltip = $"<div>" +
                        "Adjust the Payout to result into the Highest Non-Negative Amount<br>" +
                        "that will be paid for the lifetime of the individual.<br>" +
                        "</div>";
                    tooltipService.Open(elementReference, adjustpayouttononzeroTooltip, options);
                    break;
                case "startpayout":
                    options = new TooltipOptions() { Delay = 500, Duration = 5000 };
                    string startpayoutTooltip = $"<div>" +
                        "The Highest Age Pair Adjustment determines the Age to <u>Stop</u> Contributions." +
                        "</div>";
                    tooltipService.Open(elementReference, startpayoutTooltip, options);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
