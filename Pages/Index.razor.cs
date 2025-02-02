using FinancialStrategyCalculator.Data;
using FinancialStrategyCalculator.Services;
using FinancialStrategyCalculator.Shared;
using Microsoft.JSInterop;


namespace FinancialStrategyCalculator.Pages
{
    public partial class Index
    {
        private int currentIndex = 0; // This will keep track of the current image index
        private Timer timer;

        String item = "resources/slide1.jpg";

        List<string> items = new List<string>()
        {
            "resources/slide2.jpg",
            "resources/slide1.jpg",
        };

        protected override async void OnInitialized()
        {
            timer = new Timer(RotateImages, null, 0, 5000);

            if (FinancialDataService.FinancialStrategyModel == null)
            {
                Utility.PreventPopups = true;
                FinancialDataService.FinancialStrategyModel = new FinancialStrategyModel
                {
                    Year = DateTime.Now.Year,
                    Age = 29,
                    Sex = 1,
                    MonthlyInsurancePremium = 50,
                    FaceAmount = 500000,
                    TermLength = 35,
                    ContinueInvestment = true,
                    AgeToStartPayouts = 66,
                    InvestmentPayInsurance = false,
                    InsurancePlan = 3, //1 = YRT; 2 = DRT; 3=10YR DRT
                    InitialInvestment = 500,
                    MonthlyInvestment = 130,
                    AnnualInterestRateLow = 4,
                    AnnualInterestRateHigh = 14,
                    AnnualIncome = 45000,
                    AnnualIncomePercentageIncrease = 1.5m,
                    AmountToBankInTFSA = ((decimal)(40000.00 * 0.15)),
                    WithdrawHBPFHSA = 0,
                    TaxReturnToTFSA = 0,
                    NumberOfTimesToBank = 1,
                    AgeToWithdrawHBPFHSA = 65,
                    StrategicContributionType = 1,
                    IncomeInsuranceInvestmentRatios = 1, //Retirement Payout
                    AgeToMoveToRRIF = 71,
                    ProjectedAnnualRetirementIncome = 24000,
                    RRIFRate = 3.5m,
                    InsuranceType = 2,  //NTU,
                    AverageAnnualCPIChange = 1.5m,
                    TotalAnnualDefinedBenefitPensionPlan = 0, //23,930 for LAPP
                    ReplaceWorkEarningsRate = 33,
                    AnnualCostOfLivingExpenses = 31332,
                    AgeToEndInvestment = 99,
                    CPPIDAgeToReceiveCPP = 65,
                    IncludeCPPContributionsBenefits = true

                };

                FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr = "20.00";
                FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr = "65";

                //FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr = "0.00; 50.00; 0.00; 25.00; 0.00; 10.00; 0.00; 150.00";
                //FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr = "25; 26; 30; 31; 40; 41";

                Utility.YearByYearDetails = new[] {
                    3
                };
            }

            //Fix Continue Investment
            Utility.CalculateAndMatchAgeToStopInvestment(DialogService);

            await ReadARTYRTFileFromLocalPath();
            await ReadDRTFileFromLocalPath();

            await ReadTFSARRSPFHSAFromLocalPath();

            Utility.ReadCPPContributionFromLocalStorage();
            Utility.ReadCPPBenefitsFromLocalStorage();
            Utility.ReadCPPIncreaseDecreaseFromLocalStorage();
            Utility.ReadOASBenefitsFromLocalStorage();

            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;

            Utility.CPPPayoutDetails = 1;   //User Average instead of Max

            base.OnInitialized();
        }

        protected override async void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            Utility.PreventPopups = false;
            Utility.ScreenWidth = await JSRuntime.InvokeAsync<decimal>("getScreenWidth");
            Utility.ScreenHeight = await JSRuntime.InvokeAsync<decimal>("getScreenHeight");

        }

        private void RotateImages(object state)
        {
            currentIndex++; // Move to the next image

            // If we've gone past the end of the list, loop back to the start
            if (currentIndex >= items.Count)
            {
                currentIndex = 0;
            }

            item = items[currentIndex];

            InvokeAsync(StateHasChanged);  // Notify Blazor that the state has changed and the UI needs to be updated
        }

        private async Task ReadARTYRTFileFromLocalPath()
        {
            try
            {
                string localFilePath = Utility.ReturnDeviceLocalPathForARTYRT();
                // Check if the file exists
                if (!File.Exists(localFilePath))
                {
                    using Stream stream = await FileSystem.OpenAppPackageFileAsync(Utility.fileNameART);

                    // Write the stream to a specific path
                    using StreamWriter writer = new StreamWriter(localFilePath);
                    await stream.CopyToAsync(writer.BaseStream);
                }

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
                if (!File.Exists(localFilePath))
                {
                    //Read Asset
                    using Stream stream = await FileSystem.OpenAppPackageFileAsync(Utility.fileNameDRT);

                    // Write the stream to a specific path
                    using StreamWriter writer = new StreamWriter(localFilePath);
                    await stream.CopyToAsync(writer.BaseStream);
                }

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
            if (!File.Exists(localFilePath))
            {
                //Read Asset
                using Stream stream = await FileSystem.OpenAppPackageFileAsync(Utility.fileTFSARRSPFHSAName);

                // Write the stream to a specific path
                using StreamWriter writer = new StreamWriter(localFilePath);
                await stream.CopyToAsync(writer.BaseStream);
            }

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

    }
}
