using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using FinancialStrategyCalculator.Data;
using FinancialStrategyCalculator.Services;
using FinancialStrategyCalculator.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Plotly.Blazor;
using Plotly.Blazor.LayoutLib;
using Plotly.Blazor.Traces;
using Plotly.Blazor.Traces.ScatterGlLib;
using Radzen.Blazor;
using SixLabors.ImageSharp;

namespace FinancialStrategyCalculator.Pages
{
    public partial class PlanResult
    {
        #region Fields
        const string constForwardButton = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-caret-right-fill\" viewBox=\"0 0 16 16\">  <path d=\"m12.14 8.753-5.482 4.796c-.646.566-1.658.106-1.658-.753V3.204a1 1 0 0 1 1.659-.753l5.48 4.796a1 1 0 0 1 0 1.506z\"/></svg>";
        const string constBackwardButton = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-caret-left-fill\" viewBox=\"0 0 16 16\">  <path d=\"m3.86 8.753 5.482 4.796c.646.566 1.658.106 1.658-.753V3.204a1 1 0 0 0-1.659-.753l-5.48 4.796a1 1 0 0 0 0 1.506z\"/></svg>";

        private readonly Dictionary<string, bool> legendInvForVisibility = new Dictionary<string, bool>
        {
            { "Total Principal Amount", true },
            { "Compounded Maturity Value (Low)", true },
            { "Compounded Maturity Value (Ave)", true },
            { "Compounded Maturity Value (High)", true }
        };

        private PlotlyChart insInvChart;
        private Config insInvConfig;
        private Plotly.Blazor.Layout insInvLayout;
        private IList<ITrace> insInvData;

        private PlotlyChart insFAveInvAveChart;
        private Config insFAveConfig;
        private Plotly.Blazor.Layout insFAveLayout;
        private IList<ITrace> insFAveData;

        private PlotlyChart invForChart;
        private Config invForConfig;
        private Plotly.Blazor.Layout invForLayout;
        private IList<ITrace> invForData;

        private PlotlyChart incCPPChart;
        private Config incCPPConfig;
        private Plotly.Blazor.Layout incCPPLayout;
        private IList<ITrace> incCPPData;

        private RadzenTabs rzTabs;
        private RadzenChart rzchart;
        private int rzTabIndex;
        private bool sidebar2Expanded = false;

        bool showDataLabels = false;
        bool isBusy = false;
        long maxNumberInvFor = 20;
        long intervalInvFor = 1;
        int InvForNotVisible = 0;

        long maxNumberInsInv = 20;
        long intervalInsInv = 1;

        private bool renderState = false;
        private RadzenButton toggleButton;
        private MarkupString toggleButtonText = new MarkupString(constForwardButton);
        private bool renderInsState = false;
        private RadzenButton yearlyInsToggleButton;
        private MarkupString yearlyInsToggleButtonText = new MarkupString(constForwardButton);
        private bool renderInvState = false;
        private RadzenButton yearlyInvToggleButton;
        private MarkupString yearlyInvToggleButtonText = new MarkupString(constForwardButton);

        private bool printPreviewState = false;

        private decimal previousVersion = 0;

        private IEnumerable<DropdownValue> strategicContributionTypes;

        private string errorMessage = "";

        PlanDetailsView planDetailsView;

        #endregion

        #region Properties
        private static List<FinancialResultsModel> FinancialFinancialResultsModel { get; set; }
        private static List<FinancialResultsModel> MonthlyInsurance { get; set; }
        private static List<FinancialResultsModel> MonthlyInvestment { get; set; }

        private static string ProvidePaymentInYears
        {
            get
            {
                string resultPaymentYears = string.Empty;
                if (Utility.YearByYearDetails.FirstOrDefault((i) => i == 1, 0) == 1)
                {
                    resultPaymentYears += string.Format(" Age {0} (No Interest); ", @FinancialDataService.FinancialResultsModel?.LastOrDefault((a) => a.AnnualCompoundedInvestmentNoInterest > 0)?.Age);
                }
                if (Utility.YearByYearDetails.FirstOrDefault((i) => i == 2, 0) == 2)
                {
                    resultPaymentYears += string.Format(" Age {0} (Low Interest); ", @FinancialDataService.FinancialResultsModel?.LastOrDefault((a) => a.AnnualCompoundedInvestmentLow > 0)?.Age);
                }
                if (Utility.YearByYearDetails.FirstOrDefault((i) => i == 3, 0) == 3)
                {
                    resultPaymentYears += string.Format(" Age {0} (Ave Interest); ", @FinancialDataService.FinancialResultsModel?.LastOrDefault((a) => a.AnnualCompoundedInvestmentAve > 0)?.Age);
                }
                if (Utility.YearByYearDetails.FirstOrDefault((i) => i == 4, 0) == 4)
                {
                    resultPaymentYears += string.Format(" Age {0} (High Interest); ", @FinancialDataService.FinancialResultsModel?.LastOrDefault((a) => a.AnnualCompoundedInvestmentHigh > 0)?.Age);
                }
                return resultPaymentYears;
            }
        }

        private static MarkupString CPPAnnualPayoutCalculation
        {
            get
            {
                if (Utility.CPPPayoutDetails == 1)
                {
                    return new MarkupString(string.Format("Average <b><u>Monthly</u></b> CPP Payment Amount of <b><u>{0:C2}</u></b>", FinancialDataService.FinancialStrategyModel.AverageAmountForNewBeneficiaries / 12));
                }
                else
                {
                    return new MarkupString(string.Format("Maximum <b><u>Monthly</u></b> CPP Payment Amount of <b><u>{0:C2}</u></b>", FinancialDataService.FinancialStrategyModel.MaximumAmountForBeneficiaries / 12));
                }
            }
        }

        private static MarkupString CPPAtRRIF
        {
            get
            {
                var monthlyPayout = FinancialDataService.FinancialResultsModel?
                    .FirstOrDefault(x => x.Age == FinancialDataService.FinancialStrategyModel.AgeToStartPayouts)?
                    .CPPOASBeneficiaryPayoutAmount / 12 ?? 0;

                var formattedString = string.Format(
                    "<b><u>{0:C2} Monthly</u></b> at Age {1}",
                    monthlyPayout,
                    FinancialDataService.FinancialStrategyModel.AgeToStartPayouts);

                return new MarkupString(formattedString);
            }
        }

        #endregion

        #region Methods

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            errorMessage = Utility.ErroMessages;

            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;

            Utility.RenderPopoutIn = false;
            Utility.PopOutInText = Utility.constArrowUp;

            InitializeRenderState();

            InitializePlotlyCharts();

        }

        protected override void OnAfterRender(bool firstRender)
        {
            try
            {
                InvokeAsync(() => GenerateGraphAsync(firstRender));
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            base.OnAfterRender(firstRender);
        }

        private void InitializePlotlyCharts()
        {
            insInvChart = new PlotlyChart();
            insFAveInvAveChart = new PlotlyChart();
            invForChart = new PlotlyChart();

            insInvConfig = new Config
            {
                Responsive = true
            };

            insFAveConfig = new Config
            {
                Responsive = true
            };

            invForConfig = new Config
            {
                Responsive = true
            };

            incCPPConfig = new Config
            {
                Responsive = true
            };

            #region Insurance vs Investment
            var maxYRange = FinancialDataService.FinancialStrategyModel.MonthlyInvestment * 19;

            insInvLayout = new Plotly.Blazor.Layout
            {
                Title = new Title
                {
                    Text = "Insurance vs Investment Comparison"
                },
                Font = new Plotly.Blazor.LayoutLib.Font
                {
                    Color = Colors.Black
                },
                BarMode = BarModeEnum.Stack,
                YAxis = new List<YAxis>
                {
                    new()
                    {
                        Title = new Plotly.Blazor.LayoutLib.YAxisLib.Title
                        {
                            Text = "Amount ($)"
                        },
                        Range = new List<object>() { 0, maxYRange}
                    }
                },
                XAxis = new List<XAxis>
                {
                    new()
                    {
                        Title = new Plotly.Blazor.LayoutLib.XAxisLib.Title
                        {
                            Text = "Age"
                        }
                    },
                },
                Legend = new List<Legend>() {
                    new Legend
                    {
                        X = 0.0m,
                        Y = -0.45m
                    }
                },
                AutoSize = true,
                Width = Utility.ScreenWidth * 0.4m,
                Height = Utility.ScreenHeight * 0.8m,
            };

            insFAveLayout = new Plotly.Blazor.Layout
            {
                Title = new Title
                {
                    Text = "Face Amount vs Maturity Value"
                },
                Font = new Plotly.Blazor.LayoutLib.Font
                {
                    Color = Colors.Black
                },
                BarMode = BarModeEnum.Stack,
                YAxis = new List<YAxis>
                {
                    new()
                    {
                        Title = new Plotly.Blazor.LayoutLib.YAxisLib.Title
                        {
                            Text = "Amount ($)"
                        }
                    },
                },
                XAxis = new List<XAxis>
                {
                    new()
                    {
                        Title = new Plotly.Blazor.LayoutLib.XAxisLib.Title
                        {
                            Text = "Age"
                        }
                    },
                },
                Legend = new List<Legend>() {
                    new Legend
                    {
                        X = 0.0m,
                        Y = -0.45m
                    }
                },
                AutoSize = true,
                Width = Utility.ScreenWidth * 0.4m,
                Height = Utility.ScreenHeight * 0.8m,
            };

            #endregion

            #region Compounded Returns
            invForLayout = new Plotly.Blazor.Layout
            {
                Title = new Title
                {
                    Text = "Compounding Returns & Retirement Payout"
                },

                Font = new Plotly.Blazor.LayoutLib.Font
                {
                    Color = Colors.Black
                },
                BarMode = BarModeEnum.Overlay,
                YAxis = new List<YAxis>
                {
                    new()
                    {
                        Title = new Plotly.Blazor.LayoutLib.YAxisLib.Title
                        {
                            Text = "Amount ($)"
                        }
                    },
                },
                XAxis = new List<XAxis>
                {
                    new()
                    {
                        Title = new Plotly.Blazor.LayoutLib.XAxisLib.Title
                        {
                            Text = "Age"
                        }
                    },
                },
                Legend = new List<Legend>() {
                    new Legend
                    {
                        X = 0.0m,
                        Y = -0.25m
                    }
                },
                AutoSize = true,
                Width = Utility.ScreenWidth * 0.8m,
                Height = Utility.ScreenHeight * 0.8m,
            };

            #endregion

            #region Income vs CPP
            incCPPLayout = new Plotly.Blazor.Layout
            {
                Title = new Title
                {
                    Text = "Income vs CPP"
                },

                Font = new Plotly.Blazor.LayoutLib.Font
                {
                    Color = Colors.Black
                },
                BarMode = BarModeEnum.Overlay,
                YAxis = new List<YAxis>
                {
                    new()
                    {
                        Title = new Plotly.Blazor.LayoutLib.YAxisLib.Title
                        {
                            Text = "Amount ($)"
                        }
                    },
                },
                XAxis = new List<XAxis>
                {
                    new()
                    {
                        Title = new Plotly.Blazor.LayoutLib.XAxisLib.Title
                        {
                            Text = "Age"
                        }
                    },
                },
                Legend = new List<Legend>() {
                    new Legend
                    {
                        X = 0.0m,
                        Y = -0.5m
                    }
                },
                AutoSize = true,
                Width = Utility.ScreenWidth * 0.8m,
                Height = Utility.ScreenHeight * 0.8m,
            };

            #endregion

            insInvData = new List<ITrace>();
            insFAveData = new List<ITrace>();
            invForData = new List<ITrace>();
            incCPPData = new List<ITrace>();
        }

        private void InitializeRenderState()
        {
            renderState = true;
            toggleButtonText = new MarkupString(constForwardButton);

            renderInsState = true;
            yearlyInsToggleButtonText = new MarkupString(constForwardButton);

            renderInvState = true;
            yearlyInvToggleButtonText = new MarkupString(constForwardButton);

        }

        private async Task PreviewButton()
        {
            if (FinancialDataService.FinancialStrategyModel?.FirstName != null && FinancialDataService.FinancialStrategyModel?.LastName != null)
            {
                isBusy = true;
                await Task.Delay(100);
                string mergedBase64 = string.Empty;

                string fileName = string.Format("FinancialResults-{0}_{1}.jpg", FinancialDataService.FinancialStrategyModel.LastName, FinancialDataService.FinancialStrategyModel.FirstName);

                if (rzTabIndex == 0)
                {
                    StateHasChanged();
                    fileName = string.Format("FinancialResults-planIncomeVsCPP-{0}_{1}.jpg", FinancialDataService.FinancialStrategyModel.LastName, FinancialDataService.FinancialStrategyModel.FirstName);
                    mergedBase64 = await JSRuntime.InvokeAsync<string>("takeScreenshot", "planIncomeVsCPP");
                }
                else if (rzTabIndex == 1)
                {
                    StateHasChanged();
                    fileName = string.Format("FinancialResults-InvVsIns-{0}_{1}.jpg", FinancialDataService.FinancialStrategyModel.LastName, FinancialDataService.FinancialStrategyModel.FirstName);
                    mergedBase64 = await JSRuntime.InvokeAsync<string>("takeScreenshot", "planInsuranceInvestment");

                }
                else if (rzTabIndex == 2)
                {
                    StateHasChanged();
                    fileName = string.Format("FinancialResults-CompoundReturns-{0}_{1}.jpg", FinancialDataService.FinancialStrategyModel.LastName, FinancialDataService.FinancialStrategyModel.FirstName);
                    mergedBase64 = await JSRuntime.InvokeAsync<string>("takeScreenshot", "planCompoundReturns");
                }
                else if (rzTabIndex == 3)
                {
                    StateHasChanged();
                    fileName = string.Format("FinancialResults-Details-{0}_{1}.jpg", FinancialDataService.FinancialStrategyModel.LastName, FinancialDataService.FinancialStrategyModel.FirstName);
                    mergedBase64 = await JSRuntime.InvokeAsync<string>("takeScreenshot", "planFinancialResultsModelRoot");
                }

                CancellationToken cancellationToken = new CancellationToken();
                await PermissionService.RequestStoragePermission();

                using var stream = new MemoryStream(Convert.FromBase64String(mergedBase64.Split(',')[1]));
                var fileSaverResult = await FileSaver.SaveAsync(fileName, stream, cancellationToken);
                if (fileSaverResult.IsSuccessful)
                {
                    //await Toast.Make($"The file was saved successfully to location: {fileSaverResult.FilePath}").Show();
                }
                else
                {
                    // await Toast.Make($"The file was not saved successfully with error: {fileSaverResult.Exception.Message}").Show();
                }


                isBusy = false;
            }
            else
            {
                await DialogService.Alert("First Name & Last Name are required to Export!");
                Navigation.NavigateTo("/GeneralInfo", true);
            }
        }


        private async Task LoadProfile()
        {
            isBusy = true;
            ResetStrategies();

            await Utility.LoadProfileCommon(errorMessage, previousVersion);
            await Task.Delay(200);
            await GenerateGraphAsync(true);

            isBusy = false;
        }

        private async Task SaveProfile()
        {
            await Utility.SaveProfileCommon();
        }

        private async Task GenerateGraphAsync(bool firstRender)
        {
            printPreviewState = true;

            Utility.PreventPopups = true;

            isBusy = true;

            //Fix Continue Investment for FinancialDataService.FinancialResultsModel & monthlyInsurance
            //DIALOG POP-UP
            Utility.CalculateAndMatchAgeToStopInvestment(DialogService);

            Utility.ReadCPPContributionFromLocalStorage();
            Utility.ReadCPPBenefitsFromLocalStorage();

            await Task.Delay(200);
            //Submit for Computation
            Utility.ErroMessages = Utility.SubmitNext(FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr, FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr, errorMessage, previousVersion);

            InitializeRenderState();

            //Create The Graphs
            GraphIncomeVsCPPOtherPension();
            GraphInsuranceVsInvestment();
            GraphCompoundingReturnsVsRetirementPayout();

            await insInvChart.Update();
            await insFAveInvAveChart.Update();
            await invForChart.Update();
            await incCPPChart.Update();

            FinancialFinancialResultsModel = FinancialDataService.FinancialResultsModel;

            planDetailsView.RefreshContent();

            isBusy = false;
            Utility.PreventPopups = false;

            //This should always be at the end of this method
            if (firstRender)
            {
                await Task.Delay(200);
                StateHasChanged();
            }
        }

        /// <summary>
        /// Creates the Graph for Income vs CPP &amp; other pension.
        /// </summary>
        private void GraphIncomeVsCPPOtherPension()
        {
            Bar incomeBar = new Bar
            {
                X = FinancialDataService.FinancialResultsModel.Where((x) => x.Age < x.PayoutAge).Select(x => (object)x.Age).ToList(),
                Y = FinancialDataService.FinancialResultsModel.Where((x) => x.Age < x.PayoutAge).Select(x => (object)x.AnnualIncome).ToList(),
                CustomData = FinancialDataService.FinancialResultsModel.Where((x) => x.Age < x.PayoutAge).Select(x => (object)(x.AnnualIncome / 12)).ToList(),
                HoverTemplate = "Salary / Income (Yearly): %{y:$,.2f}<br>Salary / Income (Monthly): %{customdata:$,.2f}<br>",
                Name = "Salary / Income",
                Opacity = 1.0m,
                LegendRank = 901,
                Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#093CFF" } //Blue
            };
            Bar dbppBar = new Bar
            {
                X = FinancialDataService.FinancialResultsModel.Where((x) => x.Age >= x.PayoutAge).Select(x => (object)x.Age).ToList(),
                Y = FinancialDataService.FinancialResultsModel.Where((x) => x.Age >= x.PayoutAge).Select(x => (object)(x.TotalAnnualDefinedBenefitPensionPlan + x.CPPOASBeneficiaryPayoutAmount)).ToList(),
                CustomData = FinancialDataService.FinancialResultsModel.Where((x) => x.Age >= x.PayoutAge).Select(x => (object)((x.TotalAnnualDefinedBenefitPensionPlan + x.CPPOASBeneficiaryPayoutAmount) / 12)).ToList(),
                HoverTemplate = "Defined Benefit Pension Plans (Yearly): %{y:$,.2f}<br>Defined Benefit Pension Plans (Monthly): %{customdata:$,.2f}<br>",
                Name = "Other Defined Benefit Pension Plans",
                Opacity = 1.0m,
                LegendRank = 904,
                Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#FF0DA2" } //Purple
            };

            Bar invPayoutBar = new Bar
            {
                X = FinancialDataService.FinancialResultsModel.Where((x) => x.AnnualCompoundedInvestmentAve > 0 && x.Age >= x.PayoutAge).Select(x => (object)x.Age).ToList(),
                Y = FinancialDataService.FinancialResultsModel.Where((x) => x.AnnualCompoundedInvestmentAve > 0 && x.Age >= x.PayoutAge).Select(x => (object)(x.AnnualIncomePayout)).ToList(),
                CustomData = FinancialDataService.FinancialResultsModel.Where(x => x.AnnualCompoundedInvestmentAve > 0 && x.Age >= x.PayoutAge).Select(x => (object)(x.AnnualIncomePayout / 12)).ToList(),
                HoverTemplate = "Average Investment Payout After Retirement (Yearly): %{y:$,.2f}<br>Average Investment Payout After Retirement (Monthly): %{customdata:$,.2f}<br />",
                Name = "Average Investment Payout After Retirement",
                Opacity = 1.0m,
                LegendRank = 905,
                Visible = Plotly.Blazor.Traces.BarLib.VisibleEnum.LegendOnly,
                Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#0C9E15" } //Green
            };

            Bar cpp1BenefitBar = new Bar
            {
                X = FinancialDataService.FinancialResultsModel.Where((x) => x.Age < x.PayoutAge).Select(x => (object)x.Age).ToList(),
                Y = FinancialDataService.FinancialResultsModel.Where((x) => x.Age < x.PayoutAge).Select(x => (object)x.CPPOASBeneficiaryPayoutAmount).ToList(),
                CustomData = FinancialDataService.FinancialResultsModel.Where(x => x.Age < x.PayoutAge).Select(x => (object)(x.CPPOASBeneficiaryPayoutAmount / 12)).ToList(),
                HoverTemplate = "CPP Benefit Payout Amount (Yearly): %{y:$,.2f} Before Age %{x}<br>CPP Benefit Payout Amount (Monthly): %{customdata:$,.2f} Before Age %{x}<br />",
                Name = string.Format("{0} CPP Benefit Payout Amount Before {1}", Utility.CPPPayoutDetails == 1 ? "Average" : "Maximum", FinancialDataService.FinancialStrategyModel.ContinueInvestment ? string.Format("Age {0}", FinancialDataService.FinancialStrategyModel.AgeToStartPayouts) : string.Format("Term {0} Years", FinancialDataService.FinancialStrategyModel.TermLength)),
                Opacity = 0.6m,
                ShowLegend = false,
                Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#FF7A00" } // brownish
            };

            Bar cpp2BenefitBar = new Bar
            {
                X = FinancialDataService.FinancialResultsModel.Where((x) => x.Age >= FinancialDataService.FinancialStrategyModel.CPPIDAgeToReceiveCPP).Select(x => (object)x.Age).ToList(),
                Y = FinancialDataService.FinancialResultsModel.Where((x) => x.Age >= FinancialDataService.FinancialStrategyModel.CPPIDAgeToReceiveCPP).Select(x => (object)x.CPPOASBeneficiaryPayoutAmount).ToList(),
                CustomData = FinancialDataService.FinancialResultsModel.Where(x => x.Age >= FinancialDataService.FinancialStrategyModel.CPPIDAgeToReceiveCPP).Select(x => (object)(x.CPPOASBeneficiaryPayoutAmount / 12)).ToList(),
                HoverTemplate = "CPP Benefit Payout Amount (Yearly): %{y:$,.2f} After Age %{x}<br>CPP Benefit Payout Amount (Monthly): %{customdata:$,.2f} After Age %{x}<br />",
                Name = string.Format("{0} CPP Benefit Payout Amount After {1}", Utility.CPPPayoutDetails == 1 ? "Average" : "Maximum", FinancialDataService.FinancialStrategyModel.ContinueInvestment ? string.Format("Age {0}", FinancialDataService.FinancialStrategyModel.AgeToStartPayouts) : string.Format("Term {0} Years", FinancialDataService.FinancialStrategyModel.TermLength)),
                Opacity = 1.0m,
                LegendRank = 902,
                Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#FF7A00" } // brownish
            };

            Bar cppContributionBar = new Bar
            {
                X = FinancialDataService.FinancialResultsModel.Where((x) => x.Age <= x.PayoutAge).Select(x => (object)x.Age).ToList(),
                Y = FinancialDataService.FinancialResultsModel.Where((x) => x.Age <= x.PayoutAge).Select(x => (object)x.CPPOASDeductionAmount).ToList(),
                CustomData = FinancialDataService.FinancialResultsModel.Where(x => x.Age <= x.PayoutAge).Select(x => (object)(x.CPPOASDeductionAmount / 12)).ToList(),
                HoverTemplate = "CPP Contribution (Yearly): %{y:$,.2f} until Age %{x}<br>CPP Contribution (Monthly): %{customdata:$,.2f} until Age %{x}<br />",
                Name = string.Format("CPP Contribution until {0}", FinancialDataService.FinancialStrategyModel.ContinueInvestment ? string.Format("Age {0}", FinancialDataService.FinancialStrategyModel.AgeToStartPayouts) : string.Format("Term {0} Years", FinancialDataService.FinancialStrategyModel.TermLength)),
                Opacity = 0.75m,
                LegendRank = 903,
                Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#FF3D00" } // lighter orange
            };

            Bar cppScatterYearlyInv = new Bar
            {
                X = FinancialDataService.FinancialResultsModel.Where((x) => x.Age < x.PayoutAge).Select(x => (object)x.Age).ToList(),
                Y = FinancialDataService.FinancialResultsModel.Where((x) => x.Age < x.PayoutAge).Select(x => (object)x.YearlyInvestmentContribution).ToList(),
                CustomData = FinancialDataService.FinancialResultsModel.Where(x => x.Age < x.PayoutAge).Select(x => (object)(x.YearlyInvestmentContribution / 12)).ToList(),
                HoverTemplate = "Annual Investment Contribution (Yearly): %{y:$,.2f} at Age %{x}<br>Annual Investment Contribution (Monthly): %{customdata:$,.2f} at Age %{x}<br />",
                Name = "Annual Investment Contribution",
                Visible = Plotly.Blazor.Traces.BarLib.VisibleEnum.LegendOnly,
                Opacity = 0.75m,
                LegendRank = 906,
                Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "5DF01F" } // green
            };

            Scatter expScatter = new Scatter
            {
                X = FinancialDataService.FinancialResultsModel.Select(x => (object)x.Age).ToList(),
                Y = FinancialDataService.FinancialResultsModel.Select(x => (object)x.CostofLivingExpenses).ToList(),
                CustomData = FinancialDataService.FinancialResultsModel.Select(x => (object)(x.CostofLivingExpenses / 12)).ToList(),
                HoverTemplate = "Cost of Living Expenses (Yearly): %{y:$,.2f} at Age %{x}<br>Cost of Living Expenses (Monthly): %{customdata:$,.2f} at Age %{x}<br />",
                Name = string.Format("Annual Cost of Living Expenses (Monthly {0:c})", (FinancialDataService.FinancialStrategyModel.AnnualCostOfLivingExpenses / 12)),
                LegendRank = 906,
                Mode = (Plotly.Blazor.Traces.ScatterLib.ModeFlag?)(ModeFlag.Lines | ModeFlag.Markers),
                Marker = new Plotly.Blazor.Traces.ScatterLib.Marker() { Color = "Yellow" } // yellow
            };

            incCPPData = new List<ITrace>();
            incCPPData.Add(incomeBar);
            incCPPData.Add(invPayoutBar);
            if (FinancialDataService.FinancialStrategyModel.TotalAnnualDefinedBenefitPensionPlan > 0)
            {
                incCPPData.Add(dbppBar);
            }
            incCPPData.Add(cpp1BenefitBar);
            incCPPData.Add(cpp2BenefitBar);
            incCPPData.Add(cppContributionBar);
            incCPPData.Add(cppScatterYearlyInv);
            incCPPData.Add(expScatter);
        }

        /// <summary>
        /// Computes the Graph for compounding returns vs retirement payout.
        /// </summary>
        private void GraphCompoundingReturnsVsRetirementPayout()
        {
            Bar highChar = new Bar
            {
                X = FinancialDataService.FinancialResultsModel.Select(x => (object)x.Age).ToList(),
                Y = FinancialDataService.FinancialResultsModel.Select(x => (object)x.AnnualCompoundedInvestmentHigh).ToList(),
                HoverTemplate = "Maturity Value: %{y:$,.2f} @ Age %{x} <br />",
                Name = string.Format("Maturity Value (High) @ {0}%", FinancialDataService.FinancialStrategyModel.AnnualInterestRateHigh),
                Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#116ED1" }
            };
            Bar aveChar = new Bar
            {
                X = FinancialDataService.FinancialResultsModel.Select(x => (object)x.Age).ToList(),
                Y = FinancialDataService.FinancialResultsModel.Select(x => (object)x.AnnualCompoundedInvestmentAve).ToList(),
                HoverTemplate = "Maturity Value: %{y:$,.2f} @ Age %{x} <br />",
                Name = string.Format("Maturity Value (Ave) @ {0}%", (FinancialDataService.FinancialStrategyModel.AnnualInterestRateHigh + FinancialDataService.FinancialStrategyModel.AnnualInterestRateLow) / 2),
                Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#0C9E15" }
            };
            Bar lowChar = new Bar
            {
                X = FinancialDataService.FinancialResultsModel.Select(x => (object)x.Age).ToList(),
                Y = FinancialDataService.FinancialResultsModel.Select(x => (object)x.AnnualCompoundedInvestmentLow).ToList(),
                HoverTemplate = "Maturity Value: %{y:$,.2f} @ Age %{x} <br />",
                Name = string.Format("Maturity Value (Low) @ {0}%", FinancialDataService.FinancialStrategyModel.AnnualInterestRateLow),
                Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#EDE500" }
            };
            Bar noIntChar = new Bar
            {
                X = FinancialDataService.FinancialResultsModel.Select(x => (object)x.Age).ToList(),
                Y = FinancialDataService.FinancialResultsModel.Select(x => (object)x.RunningTotalInvestmentContributed).ToList(),
                HoverTemplate = "Principal Amount: %{y:$,.2f} @ Age %{x} <br />",
                Name = "Principal Amount",
                Opacity = 0.5m,
                Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#D6710B" }
            };

            Bar faceAmount = new Bar
            {
                X = FinancialDataService.FinancialResultsModel.Select(x => (object)x.Age).ToList(),
                Y = FinancialDataService.FinancialResultsModel.Select(x => (object)x.FaceAmount).ToList(),
                HoverTemplate = "Insurance Face Amount: %{y:$,.2f} @ Age %{x} <br />",
                Opacity = 0.75m,
                Name = "Insurance Face Amount",
                Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#7E4AFF" }
            };

            int topChart = 0;
            invForData = new List<ITrace>();
            if (Utility.YearByYearDetails.FirstOrDefault((i) => i == 4, 0) == 4)
            {
                topChart = 4;
                invForData.Add(highChar);
            }
            if (Utility.YearByYearDetails.FirstOrDefault((i) => i == 3, 0) == 3)
            {
                topChart = topChart < 3 ? 3 : topChart;
                invForData.Add(aveChar);
            }
            if (Utility.YearByYearDetails.FirstOrDefault((i) => i == 2, 0) == 2)
            {
                topChart = topChart < 2 ? 2 : topChart;
                invForData.Add(lowChar);
            }
            if (Utility.YearByYearDetails.FirstOrDefault((i) => i == 1, 0) == 1)
            {
                topChart = topChart < 1 ? 1 : topChart;
                invForData.Add(noIntChar);
            }
            invForData.Add(faceAmount);

            decimal? payoutAmnt = FinancialDataService.FinancialResultsModel
                .Where(d => d.Age == FinancialDataService.FinancialStrategyModel.AgeToStartPayouts)
                .Select(d => topChart == 4 ? d.AnnualCompoundedInvestmentHigh : (topChart == 3 ? d.AnnualCompoundedInvestmentAve : (topChart == 2 ? d.AnnualCompoundedInvestmentLow : d.AnnualCompoundedInvestmentNoInterest)))
                .FirstOrDefault() * 1.1m;
            payoutAmnt ??= 0;
            Bar retPayout = new Bar
            {
                Name = "Payout Start",
                X = new List<object> { FinancialDataService.FinancialStrategyModel.AgeToStartPayouts },
                Y = new List<object> { payoutAmnt },
                ShowLegend = false,
                HoverTemplate = "Payout Start from Age %{x}",
                Opacity = 0.6m,
                Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#E41E7C" }
            };
            invForData.Add(retPayout);
        }

        /// <summary>
        /// Computes the Graph for insurance vs investment.
        /// </summary>
        private void GraphInsuranceVsInvestment()
        {
            maxNumberInvFor = Convert.ToInt64(FinancialDataService.FinancialResultsModel.Max((g) => g.AnnualCompoundedInvestmentHigh) * 1.2m);
            intervalInvFor = Convert.ToInt64(maxNumberInvFor * 0.1);

            long insMax = Convert.ToInt64(FinancialDataService.FinancialResultsModel.Max((g) => g.MonthlyInsurancePremium) * 1.2m);
            long invMax = Convert.ToInt64(FinancialDataService.FinancialResultsModel.Max((g) => g.MonthlyInvestment) * 1.2m);
            maxNumberInsInv = new List<long>() { insMax, invMax }.Max();
            intervalInsInv = Convert.ToInt64(maxNumberInsInv * 0.1);

            MonthlyInsurance = new List<FinancialResultsModel>
            {
                FinancialDataService.FinancialResultsModel.FirstOrDefault((g) => g.Age == FinancialDataService.FinancialStrategyModel.Age),
                FinancialDataService.FinancialResultsModel.FirstOrDefault((g) => g.Age == FinancialDataService.FinancialStrategyModel.AgeToStartPayouts),
            };

            MonthlyInvestment = new List<FinancialResultsModel>
            {
                FinancialDataService.FinancialResultsModel.FirstOrDefault((g) => g.Age == FinancialDataService.FinancialStrategyModel.Age),
                FinancialDataService.FinancialResultsModel.FirstOrDefault((g) => g.Age == FinancialDataService.FinancialStrategyModel.AgeToStartPayouts),
            };

            int ageInsuranceEnds = FinancialDataService.FinancialStrategyModel.Age + FinancialDataService.FinancialStrategyModel.TermLength;

            insInvData = new List<ITrace>
                {
                    new Bar
                    {
                        X = MonthlyInsurance.Select(x => (object)x.Age).ToList(),
                        Y = MonthlyInsurance.Select(x => (object)x.MonthlyInvestment).ToList(),
                        HoverTemplate = "Monthly Investment: %{y:$,.2f} @ Age %{x} <br />",
                        Name = "Monthly Investment",
                        Opacity = 0.85m,
                        Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#0C9E15" } //Gold
					},
                    new Bar
                    {
                        X = MonthlyInsurance.Select(x => (object)x.Age).ToList(),
                        Y = MonthlyInsurance.Select(x => (object)(FinancialDataService.FinancialStrategyModel.InvestmentPayInsurance && x.Age > ageInsuranceEnds ? 0 : x.MonthlyInsurancePremium)).ToList(),
                        HoverTemplate = "Monthly Insurance: %{y:$,.2f} @ Age %{x} <br />",
                        Name = "Monthly Insurance",
                        Opacity = 0.85m,
                        Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#7E4AFF" } // Green
					}
            };

            insFAveData = new List<ITrace>
            {
                    new Bar
                    {
                        X = MonthlyInsurance.Select(x => (object)x.Age).ToList(),
                        Y = MonthlyInsurance.Select(x => (object)x.AnnualCompoundedInvestmentAve).ToList(),
                        HoverTemplate = "Investment Maturity Value (Ave): %{y:$,.2f} @ Age %{x} <br />",
                        Name = string.Format("Investment Maturity Value (Ave) @ {0}%", (FinancialDataService.FinancialStrategyModel.AnnualInterestRateHigh + FinancialDataService.FinancialStrategyModel.AnnualInterestRateLow) /2),
                        Opacity = 0.85m,
                        Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#0C9E15" } //Gold
					},
                    new Bar
                    {
                        X = MonthlyInsurance.Select(x => (object)x.Age).ToList(),
                        Y = MonthlyInsurance.Select(x => (object)x.FaceAmount).ToList(),
                        HoverTemplate = "Insurance Face Amount: %{y:$,.2f} @ Age %{x} <br />",
                        Name = string.Format("Insurance Face Amount @ {0}", FinancialDataService.FinancialStrategyModel.ContinueInvestment ? string.Format("Age {0}", FinancialDataService.FinancialStrategyModel.AgeToStartPayouts) : string.Format("Term {0} Years",FinancialDataService.FinancialStrategyModel.TermLength)),
                        Opacity = 0.85m,
                        Marker = new Plotly.Blazor.Traces.BarLib.Marker() { Color = "#7E4AFF" } // Green
					}
            };

        }

        private void ResetStrategies()
        {
            FinancialDataService.FinancialStrategyModel.InvestmentPayInsurance = false;
            FinancialDataService.FinancialStrategyModel.RefundPremiums = false;
            FinancialDataService.FinancialStrategyModel.HasARTYRTFileLoaded = false;

            FinancialDataService.InsuranceARTYRTDataSettings = new List<ARTYRTInsuranceData>();

            FinancialDataService.FinancialResultsModel = null;
        }

        private void BackScreenn()
        {
            Navigation.NavigateTo("/Strategies", false);
        }

        #endregion
    }
}
