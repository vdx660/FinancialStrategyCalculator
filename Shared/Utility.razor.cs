using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using FinancialStrategyCalculator.Data;
using FinancialStrategyCalculator.Services;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FinancialStrategyCalculator.Shared
{
    public partial class Utility
    {
        private static IEnumerable<int> yearByYearDetails = new int[] { 1, 2, 3, 4 };

        public const string constArrowUp = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-arrow-bar-up\" viewBox=\"0 0 16 16\">\r\n  <path fill-rule=\"evenodd\" d=\"M8 10a.5.5 0 0 0 .5-.5V3.707l2.146 2.147a.5.5 0 0 0 .708-.708l-3-3a.5.5 0 0 0-.708 0l-3 3a.5.5 0 1 0 .708.708L7.5 3.707V9.5a.5.5 0 0 0 .5.5zm-7 2.5a.5.5 0 0 1 .5-.5h13a.5.5 0 0 1 0 1h-13a.5.5 0 0 1-.5-.5z\"/>\r\n</svg>";
        public const string constArrowDown = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-arrow-bar-down\" viewBox=\"0 0 16 16\">\r\n  <path fill-rule=\"evenodd\" d=\"M1 3.5a.5.5 0 0 1 .5-.5h13a.5.5 0 0 1 0 1h-13a.5.5 0 0 1-.5-.5zM8 6a.5.5 0 0 1 .5.5v5.793l2.146-2.147a.5.5 0 0 1 .708.708l-3 3a.5.5 0 0 1-.708 0l-3-3a.5.5 0 0 1 .708-.708L7.5 12.293V6.5A.5.5 0 0 1 8 6z\"/>\r\n</svg>";

        public static readonly string fileNameART = "RemarkableFinancialStrategiesART.csv";
        public static readonly string fileNameDRT = "RemarkableFinancialStrategiesDRT.csv";
        public static readonly string fileTFSARRSPFHSAName = "RemarkableTFSARRSPFHSA.csv";
        public static readonly string CPPAnnualContributions = "CPPAnnualContributions.csv";
        public static readonly string CPPAnnualBenefit = "CPPAnnualBenefit.csv";
        public static readonly string CPPIncreaseDecrease = "CPPIncDec.csv";
        public static readonly string OASAnnualBenefit = "OASAnnualBenefit.csv";

        public static readonly string CPPContributionSite = "https://www.canada.ca/en/revenue-agency/services/tax/businesses/topics/payroll/payroll-deductions-contributions/canada-pension-plan-cpp/cpp-contribution-rates-maximums-exemptions.html";
        public static readonly string CPPBenefitsSite = "https://www.canada.ca/en/services/benefits/publicpensions/cpp/payment-amounts.html";
        public static readonly string CPPIncreaseDecreaseSite = "https://www.canada.ca/en/services/benefits/publicpensions/cpp/cpp-benefit/when-start.html";
        public static readonly string OASCalculationBenefits = "https://www.canada.ca/en/services/benefits/publicpensions/cpp/old-age-security/payments.html";


        #region Properties

        public static string ErroMessages { get; set; }
        public static bool SubmitDisabledState { get; set; }
        public static bool EditMode { get; set; }
        public static int PayoutType { get; set; }
        public static bool PreventPopups { get; internal set; }
        public static bool RenderPopoutIn { get; internal set; }
        public static string PopOutInText { get; internal set; }

        public static IEnumerable<int> YearByYearDetails
        {
            get
            {
                return yearByYearDetails;
            }
            set
            {
                if (value.Count() > 0)
                {
                    yearByYearDetails = value;
                }
            }
        }

        public static int CPPPayoutDetails { get; set; }

        public static MarkupString DisplayIncome
        {
            get
            {
                string includeMarkup = "Income / ";
                if (FinancialDataService.FinancialStrategyModel.IncludeCPPContributionsBenefits)
                {
                    if (FinancialDataService.FinancialStrategyModel.ProjectedAnnualRetirementIncome > 0)
                    {
                        includeMarkup += "<span style=\"color: blue;\">Payout + CPP/OAS</span>";
                    }
                    else
                    {
                        includeMarkup += "<span style=\"color: blue;\">CPP/OAS</span>";
                    }
                }

                if (FinancialDataService.FinancialStrategyModel.IncludeAnnualDefinedBenefitPensionPlan)
                {
                    includeMarkup += "<span style=\"color: blue;\"> + DBPP</span>";
                }

                if (!FinancialDataService.FinancialStrategyModel.IncludeCPPContributionsBenefits && !FinancialDataService.FinancialStrategyModel.IncludeAnnualDefinedBenefitPensionPlan)
                {
                    includeMarkup += "<span style=\"color: blue;\">Payout Only</span>";
                }
                return new MarkupString(includeMarkup);
            }
        }

        public static decimal ScreenWidth { get; internal set; }
        public static decimal ScreenHeight { get; internal set; }
        public static int CPPIncDecExisting { get; internal set; }
        public static int CPPBenefitsExisting { get; internal set; }
        public static int OASBenefitExisting { get; internal set; }
        public static int CPPContributionExisting { get; internal set; }

        #endregion

        #region Private Methods
        private static string ReturnLocalDevicePath(string file)
        {
            string localFilePath;
            if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.macOS)
            {
                localFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, file);
            }
            else if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                localFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, file);
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI) // or use DevicePlatform.Windows if it's the identifier in your version
            {
                localFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, file);
            }
            else
            {
                throw new NotImplementedException("Platform not supported");
            }

            return localFilePath;

        }

        // Function to extract percentages from a text using regular expressions
        private static decimal ExtractPercentage(string text)
        {
            // Regular expression pattern to match percentages like "0.6%" or "8.4%"
            string pattern = @"\d+(\.\d+)?%";
            Match match = Regex.Match(text, pattern);

            if (match.Success)
            {
                string percentageText = match.Value.Replace("%", "");
                if (decimal.TryParse(percentageText, out decimal percentage))
                {
                    return percentage;
                }
            }

            return 0;
        }

        // Function to extract minimum age from a text using regular expressions
        private static int ExtractMinimumAge(string text)
        {
            // Regular expression pattern to match minimum age like "if you start at age 60"
            string pattern = @"\bif you start at age (\d+)\b";
            Match match = Regex.Match(text, pattern);

            if (match.Success)
            {
                int age = int.Parse(match.Groups[1].Value);
                return age;
            }

            return 0;
        }

        // Function to extract maximum age from a text using regular expressions
        private static int ExtractMaximumAge(string text)
        {
            // Regular expression pattern to match maximum age like "if you start at age 70"
            string pattern = @"\bif you start at age (\d+)\b";
            Match match = Regex.Match(text, pattern);

            if (match.Success)
            {
                int age = int.Parse(match.Groups[1].Value);
                return age;
            }

            return 0;
        }

        // Function to extract maximum reduction from a text using regular expressions
        private static decimal ExtractMaximumReduction(string text)
        {
            // Regular expression pattern to match maximum reduction percentages
            string pattern = @"maximum reduction of (\d+(\.\d+)?)%";
            Match match = Regex.Match(text, pattern);

            if (match.Success)
            {
                decimal maxReduction = decimal.Parse(match.Groups[1].Value);
                return maxReduction;
            }

            return 0;
        }


        // Function to extract maximum increase from a text using regular expressions
        private static decimal ExtractMaximumIncrease(string text)
        {
            // Regular expression pattern to match maximum increase percentages
            string pattern = @"maximum increase of (\d+(\.\d+)?)%";
            Match match = Regex.Match(text, pattern);

            if (match.Success)
            {
                decimal maxIncrease = decimal.Parse(match.Groups[1].Value);
                return maxIncrease;
            }

            return 0;
        }


        #endregion

        #region Public Methods

        public static string SubmitNext(string annualIncreaseOnMonthlyContributionsStr, string ageToStopIncreaseStr, string errorMessage, decimal previousVersion)
        {
            List<decimal> increases = new List<decimal>();
            List<int> ages = new List<int>();
            try
            {
                increases = annualIncreaseOnMonthlyContributionsStr.Split(';').Select(decimal.Parse).ToList();
                ages = ageToStopIncreaseStr.Split(';').Select(int.Parse).ToList();

                if (increases.Count != ages.Count)
                {
                    errorMessage = "INVESTMENT TAB ISSUE: The number of annual increases does not match the number of ages to change/stop the increase/decrease. Please correct this and try again.";
                    FinancialDataService.FinancialResultsModel = null;
                    return errorMessage;
                }
                else if (ages.FirstOrDefault((a) => a <= FinancialDataService.FinancialStrategyModel.Age) > 0)
                {
                    errorMessage = string.Format("INVESTMENT TAB ISSUE: The ages under age to change/stop the increase/decrease should be greater than the client's age of {0}. Please correct this and try again.", FinancialDataService.FinancialStrategyModel.Age);
                    FinancialDataService.FinancialResultsModel = null;
                    return errorMessage;
                }
                else
                {
                    errorMessage = string.Empty;
                }

                //Trigger One way Values before sending to Submit
                FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr = annualIncreaseOnMonthlyContributionsStr;
                FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr = ageToStopIncreaseStr;


                previousVersion = FinancialDataService.FinancialStrategyModel.MonthlyInvestment;

                FinancialDataService.COMPUTEFinancialResultsModel(FinancialDataService.FinancialStrategyModel);

                FinancialDataService.FinancialStrategyModel = FinancialDataService.FinancialStrategyModel;

                FinancialDataService.FinancialStrategyModel.MonthlyInvestment = previousVersion;

                return errorMessage;

            }
            catch (Exception ex)
            {
                errorMessage = String.Format("Only Numbers are accepted. Please correct this and try again. Tech Error {0}", ex.Message);
                FinancialDataService.FinancialResultsModel = null;

                return errorMessage;
            }
        }

        /// <summary>Save Profile: Converts to CSV.</summary>
        /// <param name="model">The model.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public static KeyValuePair<string, string> ConvertToCsv(FinancialStrategyModel model)
        {
            string errorMessage = string.Empty;
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.AppendLine($"Year,{model.Year}");
                sb.AppendLine($"FirstName,{model.FirstName}");
                sb.AppendLine($"LastName,{model.LastName}");
                sb.AppendLine($"Address,{model.Address}");
                sb.AppendLine($"BirthDate,{model.BirthDate}");
                sb.AppendLine($"Age,{model.Age}");
                sb.AppendLine($"Sex,{model.Sex}");
                sb.AppendLine($"MonthlyInsurancePremium,{model.MonthlyInsurancePremium}");
                sb.AppendLine($"FaceAmount,{model.FaceAmount}");
                sb.AppendLine($"TermLength,{model.TermLength}");
                sb.AppendLine($"ContinueInvestment,{model.ContinueInvestment}");
                sb.AppendLine($"MonthlyInvestment,{model.MonthlyInvestment}");
                sb.AppendLine($"InitialInvestment,{model.InitialInvestment}");
                sb.AppendLine($"InvestmentPayInsurance,{model.InvestmentPayInsurance}");
                sb.AppendLine($"IncomeInsuranceInvestmentRatios,{model.IncomeInsuranceInvestmentRatios}");
                sb.AppendLine($"RefundPremiums,{model.RefundPremiums}");
                sb.AppendLine($"HasARTYRTFileLoaded,{model.HasARTYRTFileLoaded}");
                sb.AppendLine($"HasDRTFileLoaded,{model.HasDRTFileLoaded}");
                sb.AppendLine($"HasTFSARRSPFHSAFileLoaded,{model.HasTFSARRSPFHSAFileLoaded}");
                sb.AppendLine($"InsuranceType,{model.InsuranceType}");
                sb.AppendLine($"InsurancePlan,{model.InsurancePlan}");
                sb.AppendLine($"AnnualIncome,{model.AnnualIncome}");
                sb.AppendLine($"AnnualIncomePercentageIncrease,{model.AnnualIncomePercentageIncrease}");
                sb.AppendLine($"AmountToBankInTFSA,{model.AmountToBankInTFSA}");
                sb.AppendLine($"StrategicContributionType,{model.StrategicContributionType}");
                sb.AppendLine($"TFSARemainingContributionRoom,{model.TFSARemainingContributionRoom}");
                sb.AppendLine($"TFSAAnnualMaximumum,{model.TFSAAnnualMaximumum}");
                sb.AppendLine($"RRSPRemainingContributionRoomPercentage,{model.RRSPRemainingContributionRoomPercentage}");
                sb.AppendLine($"RRSPAnnualRate,{model.RRSPAnnualRate}");
                sb.AppendLine($"FHSARemainingContributionRoom,{model.FHSARemainingContributionRoom}");
                sb.AppendLine($"FHSAAnnualMaximum,{model.FHSAAnnualMaximum}");
                sb.AppendLine($"TFSAStartingContributionRoom,{model.TFSAStartingContributionRoom}");
                sb.AppendLine($"RRSPStartingContributionRoom,{model.RRSPStartingContributionRoom}");
                sb.AppendLine($"FHSAStartingContributionRoom,{model.FHSAStartingContributionRoom}");
                sb.AppendLine($"NumberOfTimesToBank,{model.NumberOfTimesToBank}");
                sb.AppendLine($"WithdrawHBPFHSA,{model.WithdrawHBPFHSA}");
                sb.AppendLine($"TaxReturnToTFSA,{model.TaxReturnToTFSA}");
                sb.AppendLine($"AgeToWithdrawHBPFHSA,{model.AgeToWithdrawHBPFHSA}");
                sb.AppendLine($"AnnualIncreaseOnMonthlyContributionsStr,{model.AnnualIncreaseOnMonthlyContributionsStr}");
                sb.AppendLine($"AgeToStopIncreaseStr,{model.AgeToStopIncreaseStr}");
                sb.AppendLine($"AnnualInterestRateLow,{model.AnnualInterestRateLow}");
                sb.AppendLine($"AnnualInterestRateHigh,{model.AnnualInterestRateHigh}");
                sb.AppendLine($"AgeToMoveToRRIF,{model.AgeToMoveToRRIF}");
                sb.AppendLine($"RRIFRate,{model.RRIFRate}");
                //sb.AppendLine($"SwitchToTSFA,{model.SwitchToTSFA}");
                sb.AppendLine($"AgeToStopInvestment,{model.AgeToStartPayouts}");
                sb.AppendLine($"ProjectedAnnualRetirementIncome,{model.ProjectedAnnualRetirementIncome}");
                sb.AppendLine($"IncludeCPPContributionsBenefits,{model.IncludeCPPContributionsBenefits}");
                sb.AppendLine($"AverageAnnualCPIChange, {model.AverageAnnualCPIChange}");
                sb.AppendLine($"TotalAnnualDefinedBenefitPensionPlan, {model.TotalAnnualDefinedBenefitPensionPlan}");
                sb.AppendLine($"ReplaceWorkEarningsRate, {model.ReplaceWorkEarningsRate}");
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            return new KeyValuePair<string, string>(sb.ToString(), errorMessage);
        }

        /// <summary>Load Profile: Converts the key value CSV to model.</summary>
        /// <param name="content">The content.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public static FinancialStrategyModel ConvertKeyValueCsvToModel(string content)
        {
            var lines = content.Split('\n');
            var model = new FinancialStrategyModel();

            foreach (var line in lines)
            {
                var columns = line.Split(',');

                if (columns.Length == 2)
                {
                    var key = columns[0].Trim();
                    var value = columns[1].Trim();

                    switch (key)
                    {
                        case "Year":
                            model.Year = int.Parse(value);
                            break;
                        case "FirstName":
                            model.FirstName = value;
                            break;
                        case "LastName":
                            model.LastName = value;
                            break;
                        case "Address":
                            model.Address = value;
                            break;
                        case "BirthDate":
                            model.BirthDate = DateTime.Parse(value);
                            break;
                        case "Age":
                            model.Age = int.Parse(value);
                            break;
                        case "Sex":
                            model.Sex = int.Parse(value);
                            break;
                        case "MonthlyInsurancePremium":
                            model.MonthlyInsurancePremium = decimal.Parse(value);
                            break;
                        case "FaceAmount":
                            model.FaceAmount = decimal.Parse(value);
                            break;
                        case "TermLength":
                            model.TermLength = int.Parse(value);
                            break;
                        case "ContinueInvestment":
                            model.ContinueInvestment = bool.Parse(value);
                            break;
                        case "MonthlyInvestment":
                            model.MonthlyInvestment = decimal.Parse(value);
                            break;
                        case "InitialInvestment":
                            model.InitialInvestment = decimal.Parse(value);
                            break;
                        case "InvestmentPayInsurance":
                            model.InvestmentPayInsurance = bool.Parse(value);
                            break;
                        case "IncomeInsuranceInvestmentRatios":
                            model.IncomeInsuranceInvestmentRatios = int.Parse(value);
                            break;
                        case "RefundPremiums":
                            model.RefundPremiums = bool.Parse(value);
                            break;
                        case "AnnualIncome":
                            model.AnnualIncome = decimal.Parse(value);
                            break;
                        case "TFSARemainingContributionRoom":
                            model.TFSARemainingContributionRoom = decimal.Parse(value);
                            break;
                        case "TFSAAnnualMaximumum":
                            model.TFSAAnnualMaximumum = decimal.Parse(value);
                            break;
                        case "RRSPRemainingContributionRoomPercentage":
                            model.RRSPRemainingContributionRoomPercentage = decimal.Parse(value);
                            break;
                        case "RRSPAnnualRate":
                            model.RRSPAnnualRate = decimal.Parse(value);
                            break;
                        case "FHSARemainingContributionRoom":
                            model.FHSARemainingContributionRoom = decimal.Parse(value);
                            break;
                        case "FHSAAnnualMaximum":
                            model.FHSAAnnualMaximum = decimal.Parse(value);
                            break;
                        case "TFSAStartingContributionRoom":
                            model.TFSAStartingContributionRoom = decimal.Parse(value);
                            break;
                        case "RRSPStartingContributionRoom":
                            model.RRSPStartingContributionRoom = decimal.Parse(value);
                            break;
                        case "FHSAStartingContributionRoom":
                            model.FHSAStartingContributionRoom = decimal.Parse(value);
                            break;
                        case "AnnualIncreaseOnMonthlyContributionsStr":
                            model.AnnualIncreaseOnMonthlyContributionsStr = value;
                            break;
                        case "AgeToStopIncreaseStr":
                            model.AgeToStopIncreaseStr = value;
                            break;
                        case "AnnualInterestRateLow":
                            model.AnnualInterestRateLow = decimal.Parse(value);
                            break;
                        case "AnnualInterestRateHigh":
                            model.AnnualInterestRateHigh = decimal.Parse(value);
                            break;
                        case "AgeToMoveToRRIF":
                            model.AgeToMoveToRRIF = int.Parse(value);
                            break;
                        case "RRIFRate":
                            model.RRIFRate = decimal.Parse(value);
                            break;
                        //case "SwitchToTSFA":
                        //	model.SwitchToTSFA = int.Parse(value);
                        //	break;
                        case "AgeToStopInvestment":
                            model.AgeToStartPayouts = int.Parse(value);
                            break;
                        case "HasARTYRTFileLoaded":
                            model.HasARTYRTFileLoaded = bool.Parse(value);
                            break;
                        case "HasDRTFileLoaded":
                            model.HasDRTFileLoaded = bool.Parse(value);
                            break;
                        case "HasTFSARRSPFHSAFileLoaded":
                            model.HasTFSARRSPFHSAFileLoaded = bool.Parse(value);
                            break;
                        case "InsuranceType":
                            model.InsuranceType = int.Parse(value);
                            break;
                        case "InsurancePlan":
                            model.InsurancePlan = int.Parse(value);
                            break;
                        case "AnnualIncomePercentageIncrease":
                            model.AnnualIncomePercentageIncrease = decimal.Parse(value);
                            break;
                        case "AmountToBankInTFSA":
                            model.AmountToBankInTFSA = decimal.Parse(value);
                            break;
                        case "WithdrawHBPFHSA":
                            model.WithdrawHBPFHSA = decimal.Parse(value);
                            break;
                        case "TaxReturnToTFSA":
                            model.TaxReturnToTFSA = decimal.Parse(value);
                            break;
                        case "StrategicContributionType":
                            model.StrategicContributionType = int.Parse(value);
                            break;
                        case "NumberOfTimesToBank":
                            model.NumberOfTimesToBank = int.Parse(value);
                            break;
                        case "AgeToWithdrawHBPFHSA":
                            model.AgeToWithdrawHBPFHSA = int.Parse(value);
                            break;
                        case "ProjectedAnnualRetirementIncome":
                            model.ProjectedAnnualRetirementIncome = decimal.Parse(value);
                            break;
                        case "IncludeCPPContributionsBenefits":
                            model.IncludeCPPContributionsBenefits = bool.Parse(value);
                            break;
                        case "AverageAnnualCPIChange":
                            model.AverageAnnualCPIChange = decimal.Parse(value);
                            break;
                        case "TotalAnnualDefinedBenefitPensionPlan":
                            model.TotalAnnualDefinedBenefitPensionPlan = decimal.Parse(value);
                            break;
                        case "ReplaceWorkEarningsRate":
                            model.ReplaceWorkEarningsRate = decimal.Parse(value);
                            break;
                    }
                }
            }
            return model;
        }

        public static string ReturnDeviceLocalPathForARTYRT()
        {
            return ReturnLocalDevicePath(fileNameART);

        }

        public static string ReturnDeviceLocalPathForDRT()
        {
            return ReturnLocalDevicePath(fileNameDRT);

        }

        public static string ReturnDeviceLocalPathForTFSARRSPFHSA()
        {
            return ReturnLocalDevicePath(fileTFSARRSPFHSAName);
        }

        public static string ReturnDeviceLocalPathForCPPContributions()
        {
            return ReturnLocalDevicePath(CPPAnnualContributions);
        }
        public static string ReturnDeviceLocalPathForCPPRetirement()
        {
            return ReturnLocalDevicePath(CPPAnnualBenefit);
        }

        public static string ReturnDeviceLocalPathForCPPIncreaseDecrease()
        {
            return ReturnLocalDevicePath(CPPIncreaseDecrease);
        }

        public static string ReturnDeviceLocalPathForOASBenefits()
        {
            return ReturnLocalDevicePath(OASAnnualBenefit);
        }

        public static List<ARTYRTInsuranceData> ReadAndParseARTInsuranceData(List<string> csvLines)
        {
            List<ARTYRTInsuranceData> dataARTInsuranceDataList = new List<ARTYRTInsuranceData>();

            // Skip the header line
            for (int i = 3; i < csvLines.Count; i++)
            {
                var values = csvLines[i].Split(',');

                var dataARTInsuranceData = new ARTYRTInsuranceData
                {
                    Id = i,
                    AgeRange = values[0],
                    PreferredPlus150kto249k = decimal.Parse(values[1]),
                    PreferredPlus250kto499k = decimal.Parse(values[2]),
                    PreferredPlus500k = decimal.Parse(values[3]),

                    SelectNTU149k = decimal.Parse(values[4]),
                    SelectNTU150kto249k = decimal.Parse(values[5]),
                    SelectNTU250kto499k = decimal.Parse(values[6]),
                    SelectNTU500kto749k = decimal.Parse(values[7]),
                    SelectNTU750k = decimal.Parse(values[8]),
                    TakenUpTU149k = decimal.Parse(values[9]),
                    TakenUpTU150kto249k = decimal.Parse(values[10]),
                    TakenUpTU250kto499k = decimal.Parse(values[11]),
                    TakenUpTU500kto749k = decimal.Parse(values[12]),
                    TakenUpTU750k = decimal.Parse(values[13])
                };

                dataARTInsuranceDataList.Add(dataARTInsuranceData);
            }
            return dataARTInsuranceDataList;
        }
        public static List<DRTInsuranceData> ReadAndParseDRTInsuranceData(List<string> csvLines)
        {
            List<DRTInsuranceData> dataDRTInsuranceDataList = new List<DRTInsuranceData>();

            // Skip the header line
            for (int i = 1; i < csvLines.Count; i++)
            {
                var values = csvLines[i].Split(',');

                var dataDRTInsuranceData = new DRTInsuranceData
                {
                    Id = i,
                    AgeRange = values[0],
                    SelectNTU149k = decimal.Parse(values[1]),
                    SelectNTU150kto249k = decimal.Parse(values[2]),
                    SelectNTU250kto499k = decimal.Parse(values[3]),
                    SelectNTU500kto749k = decimal.Parse(values[4]),
                    SelectNTU750k = decimal.Parse(values[5]),
                };

                dataDRTInsuranceDataList.Add(dataDRTInsuranceData);
            }
            return dataDRTInsuranceDataList;
        }

        public static List<TFSARRSPFHSAData> ReadAndParseTFSARRSPFHSAData(List<string> csvLines)
        {
            List<TFSARRSPFHSAData> dataTFSARRSPFHSA = new List<TFSARRSPFHSAData>();

            // Skip the header line
            for (int i = 1; i < csvLines.Count; i++)
            {
                var values = csvLines[i].Split(',');

                var dataItemTFSARRSPFHSA = new TFSARRSPFHSAData
                {
                    Id = i,
                    TFSA = decimal.Parse(values[0]),
                    RRSP = decimal.Parse(values[1]),
                    FHSA = decimal.Parse(values[2]),
                };

                dataTFSARRSPFHSA.Add(dataItemTFSARRSPFHSA);
            }
            return dataTFSARRSPFHSA;
        }

        public static async Task SaveAsAsync(string fileName, byte[] data)
        {
            await Task.Run(() => File.WriteAllBytes(fileName, data));
        }

        public static async Task LoadProfileCommon(string errorMessage, decimal previousVersion)
        {
            try
            {
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } }, // UTType values
					{ DevicePlatform.Android, new[] { "text/csv", "text/comma-separated-values", "application/csv" } }, // MIME type
					{ DevicePlatform.WinUI, new[] { ".csv" } }, // file extension
					{ DevicePlatform.Tizen, new[] { "*/*" } },
                    { DevicePlatform.macOS, new[] { "public.comma-separated-values-text" } }, // UTType values
				});
                PickOptions options = new()
                {
                    PickerTitle = "Please select a CSV file",
                    FileTypes = customFileType
                };

                await PermissionService.RequestStoragePermission();

                var filePickerResult = await FilePicker.Default.PickAsync(options);
                if (filePickerResult != null)
                {
                    using var stream = await filePickerResult.OpenReadAsync();
                    using var reader = new StreamReader(stream);
                    var content = await reader.ReadToEndAsync();
                    // Assuming the file has lines with data separated by spaces
                    FinancialDataService.FinancialStrategyModel = ConvertKeyValueCsvToModel(content);

                    //Submit for Computation
                    ErroMessages = SubmitNext(FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr, FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr, errorMessage, previousVersion);

                    await Toast.Make($"The profile was loaded successfully!").Show();
                }

                FinancialDataService.FinancialStrategyModel.AverageAnnualCPIChange = 1.5m;
                FinancialDataService.FinancialStrategyModel.ReplaceWorkEarningsRate = 33;
                FinancialDataService.FinancialStrategyModel.AnnualCostOfLivingExpenses = 31332;

            }
            catch (Exception ex)
            {
                await Toast.Make($"The profile was not loaded! due to {ex.Message}").Show();
            }
        }

        /// <summary>
        /// Saves the profile common.
        /// </summary>
        public static async Task SaveProfileCommon()
        {
            KeyValuePair<string, string> csvContentAndError = Utility.ConvertToCsv(FinancialDataService.FinancialStrategyModel);
            if (csvContentAndError.Value == string.Empty)
            {
                CancellationToken cancellationToken = new CancellationToken();
                await PermissionService.RequestStoragePermission();
                using var stream = new MemoryStream(Encoding.Default.GetBytes(csvContentAndError.Key));
                var fileSaverResult = await FileSaver.SaveAsync("Profile.csv", stream, cancellationToken);
                if (fileSaverResult.IsSuccessful)
                {
                    await Toast.Make($"The file was saved successfully to location: {fileSaverResult.FilePath}").Show();
                }
                else
                {
                    await Toast.Make($"The file was not saved successfully with error: {fileSaverResult.Exception.Message}").Show();
                }
            }
            else
            {
                FinancialDataService.FinancialResultsModel = null;
            }
        }


        public static decimal CalculatePresentValue(decimal annuity, decimal interestRate, int startAge, int endAge)
        {
            int numberOfYears = endAge - startAge + 1;
            interestRate = interestRate * (200 - numberOfYears) / 8;
            decimal futureValueFactor = (decimal)Math.Pow((double)(1 + interestRate), numberOfYears);
            decimal presentValue = annuity * ((futureValueFactor - 1) / interestRate) * (1 + interestRate) / futureValueFactor;

            return presentValue;
        }

        public static double PresentValueAnnuity(double payment, double interestRateNumerator, int numberOfPayments)
        {
            double interestRate = interestRateNumerator / 100;
            double presentValue = payment / interestRate * (1 - Math.Pow(1 / (1 + interestRate), numberOfPayments));
            return presentValue;
        }

        private static double CalculateFutureValue(double initialInvestment, double interestRate, double growthRate, int years)
        {
            double futureValue = 0;
            double currentInvestment = initialInvestment;

            for (int i = 0; i < years; i++)
            {
                futureValue += currentInvestment * Math.Pow(1 + interestRate, i);
                currentInvestment += growthRate;
            }

            return futureValue;
        }

        public static double CalculateFutureValueWithGrowth(double initialInvestment, double annuity, double yearlyRateOfReturn, double yearlyGrowthCad, int periodInYears)
        {
            double r = yearlyRateOfReturn / 12; //In % e.g 0.10 for 10%
            double g = yearlyGrowthCad / (annuity / 500) / 12;
            double n = periodInYears * 12;
            double FV = (initialInvestment * (Math.Pow(1 + r, n) - Math.Pow(1 + g, n)) / (r - g)) / (1 + r);
            return FV;
        }

        public static double CalculateInitialInvestment(double targetValue, double interestRate, double growthRate, int startAge, int endAge)
        {
            int numberOfYears = endAge - startAge;

            // Calculate initial investment without considering growth
            double noGrowthInvestment = (targetValue / (numberOfYears * Math.Pow(1 + interestRate, numberOfYears)));

            // Adjust this value by the growth rate as the initial guess
            double guess = noGrowthInvestment * (1 + interestRate);

            //double denominator = (8 * interestRate) + 6.25;
            //while (CalculateFutureValue(guess, interestRate, growthRate, numberOfYears) < (targetValue / denominator))
            while (CalculateFutureValue(guess, interestRate, growthRate, numberOfYears) < (targetValue))
            {
                guess *= (1 + growthRate);
            }

            return guess;
        }

        public static double CalculateInitialInvestment(double targetValue, double interestRate, int startAge, int endAge)
        {
            int numberOfYears = endAge - startAge;

            // Calculate initial investment without considering growth
            double noGrowthInvestment = (targetValue / (numberOfYears * Math.Pow(1 + interestRate, numberOfYears)));
            return noGrowthInvestment;
        }

        public static double CalculateGrowth(double growth, int age)
        {

            //The below table represents the starting ages until RRIF Age. This determines the monthly increase
            var growthMultipliers = new Dictionary<(int, int), double>
            {
                [(18, 19)] = 1,
                [(20, 21)] = 1,
                [(22, 23)] = 1.05,
                [(24, 25)] = 1.1,
                [(26, 29)] = 1.05,
                [(30, 33)] = 1.05,
                [(34, 37)] = 1.1,
                [(38, 41)] = 1.2,
                [(42, 45)] = 1.25,
                [(46, 49)] = 1.25,
                [(50, 52)] = 1.4,
                [(53, 55)] = 1.35,
                [(56, 57)] = 1.35,
                [(58, 59)] = 1.55,
                [(60, 61)] = 1.55,
                [(62, 63)] = 1.65,
                [(64, 65)] = 2.25,
                [(66, 69)] = 4,
            };

            foreach (var kvp in growthMultipliers)
            {
                if (age >= kvp.Key.Item1 && age <= kvp.Key.Item2)
                {
                    growth *= kvp.Value;
                    break;
                }
            }

            double compGrowth = growth * (age / 20.0);

            return compGrowth;
        }

        public static double CalculateMonthlyInvestment(double inInv, int age)
        {
            double growth = 0;
            var growthMultipliers = new Dictionary<(int, int), double>
            {
                [(18, 25)] = 0.75,
                [(26, 30)] = 1,
                [(31, 35)] = 1.1,
                [(36, 40)] = 1.12,
                [(41, 45)] = 1.13,
                [(46, 50)] = 2.3,
                [(51, 55)] = 6,
                [(56, 58)] = 11,
                [(59, 60)] = 15,
                [(61, 62)] = 30,
                [(63, 65)] = 60,
                [(66, 70)] = 40,
                [(71, 75)] = 45,
                [(76, 80)] = 50,
            };

            foreach (var kvp in growthMultipliers)
            {
                if (age >= kvp.Key.Item1 && age <= kvp.Key.Item2)
                {
                    growth = kvp.Value;
                    break;
                }
            }
            return inInv * growth;
        }



        public static async void CalculateAndMatchAgeToStopInvestment(DialogService dialogService)
        {
            if (!FinancialDataService.FinancialStrategyModel.ContinueInvestment)
            {
                int ageToStop = FinancialDataService.FinancialStrategyModel.Age + FinancialDataService.FinancialStrategyModel.TermLength - 1;
                FinancialDataService.FinancialStrategyModel.AgeToStartPayouts = ageToStop > 65 ? 65 : ageToStop;
                FinancialDataService.FinancialStrategyModel.AgeToMoveToRRIF = FinancialDataService.FinancialStrategyModel.AgeToStartPayouts;
            }
            else if ((!PreventPopups && (FinancialDataService.FinancialStrategyModel.AgeToStartPayouts != 65 || FinancialDataService.FinancialStrategyModel.AgeToMoveToRRIF != 65)))
            {
                var result = await dialogService.Confirm("Update Age Fields?", "Update Age to Stop Investment to 65 & Age to Move RRSP to RRIF to 65 (Default)?", new ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "No", CloseDialogOnEsc = true });
                if (result.HasValue && result.Value)
                {
                    FinancialDataService.FinancialStrategyModel.AgeToStartPayouts = 65;
                    FinancialDataService.FinancialStrategyModel.AgeToMoveToRRIF = 65;
                }
            }
        }

        public static void ComputeTFSABankOnAnnualIncomeChange()
        {
            var bank = FinancialDataService.FinancialStrategyModel.AnnualIncome * 0.18m;
            FinancialDataService.FinancialStrategyModel.AmountToBankInTFSA = bank < 100000 ? bank : 100000;

        }

        public static async Task<KeyValuePair<string, List<CPPContribution>>> ReadCPPContributionFromFederalWebsiteAsync()
        {
            // Create a list to store CPPContribution objects
            var cppContributions = new List<CPPContribution>();

            // Load the webpage
            var url = CPPContributionSite;
            var web = new HtmlWeb();
            var doc = web.Load(url);

            // Find the table with id "ajax-cpprate-tbl"
            var table = doc.DocumentNode.SelectSingleNode("//table[@id='ajax-cpprate-tbl']");

            if (table != null)
            {

                // Iterate through the rows of the table (skip the header row)
                foreach (var row in table.SelectNodes("tbody/tr"))
                {
                    var columns = row.SelectNodes("td");

                    if (columns != null && columns.Count >= 7)
                    {
                        // Extract values from the columns
                        var year = int.Parse(columns[0].InnerText.Trim());
                        var maxAnnualPensionableEarnings = ParseDecimalFromText(columns[1].InnerText);
                        var basicExemptionAmount = ParseDecimalFromText(columns[2].InnerText);
                        var maxContributoryEarnings = ParseDecimalFromText(columns[3].InnerText);
                        var contributionRate = ParseDecimalFromText(columns[4].InnerText);
                        var maxAnnualEmployeeAndEmployerContribution = ParseDecimalFromText(columns[5].InnerText);
                        var maxAnnualSelfEmployedContribution = ParseDecimalFromText(columns[6].InnerText);

                        // Create a CPPContribution object and add it to the list
                        var contribution = new CPPContribution
                        {
                            Year = year,
                            MaximumAnnualPensionableEarnings = maxAnnualPensionableEarnings,
                            BasicExemptionAmount = basicExemptionAmount,
                            MaximumContributoryEarnings = maxContributoryEarnings,
                            ContributionRate = contributionRate,
                            MaximumAnnualEmployeeAndEmployerContribution = maxAnnualEmployeeAndEmployerContribution,
                            MaximumAnnualSelfEmployedContribution = maxAnnualSelfEmployedContribution
                        };

                        cppContributions.Add(contribution);
                    }
                }

                // Print the extracted data
                //foreach (var contribution in cppContributions)
                //{
                //    Console.WriteLine($"{contribution.Year}: Max Annual Pensionable Earnings = {contribution.MaximumAnnualPensionableEarnings:C}, Basic Exemption Amount = {contribution.BasicExemptionAmount:C}");
                //    // Add more properties as needed
                //}
            }
            else
            {
                Console.WriteLine("Table not found on the webpage.");
            }
            return new KeyValuePair<string, List<CPPContribution>>(table.OuterHtml, cppContributions);
        }


        public static async void ReadCPPContributionFromLocalStorage()
        {
            string localFilePath = Utility.ReturnDeviceLocalPathForCPPContributions();

            // Check if the file exists
            if (File.Exists(localFilePath))
            {
                string[] allLines = await File.ReadAllLinesAsync(localFilePath);

                List<CPPAnnualContributionLimit> cppDataList = new List<CPPAnnualContributionLimit>();

                for (int i = 1; i < allLines.Length; i++)
                {
                    var values = Regex.Split(allLines[i], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                    // Removing $ and quotes from the values
                    for (int j = 0; j < values.Length; j++)
                    {
                        values[j] = values[j].Trim('"').Replace("$", "").Replace(",", "").Trim();
                        values[j] = Regex.Replace(values[j], @"[^\d.]", "");
                    }

                    var data = new CPPAnnualContributionLimit
                    {
                        Year = int.Parse(values[0]),
                        MaximumAnnualPensionableEarnings = decimal.Parse(values[1], NumberStyles.Currency),
                        BasicExemptionAmount = decimal.Parse(values[2], NumberStyles.Currency),
                        MaximumContributoryEarnings = decimal.Parse(values[3], NumberStyles.Currency),
                        ContributionRate = decimal.Parse(values[4]),
                        MaximumAnnualEmployeeAndEmployerContribution = decimal.Parse(values[5], NumberStyles.Currency),
                        MaximumAnnualSelfEmployedContribution = decimal.Parse(values[6], NumberStyles.Currency)
                    };

                    cppDataList.Add(data);
                }

                if (cppDataList.Count > 0)
                {
                    FinancialDataService.FinancialStrategyModel.CPPDeductionPercent = cppDataList[0].ContributionRate * 2;
                    FinancialDataService.FinancialStrategyModel.CPPMaxDeductionAmount = cppDataList[0].MaximumAnnualEmployeeAndEmployerContribution * 2;
                    FinancialDataService.FinancialStrategyModel.HasCPPFileLoaded = true;
                    CPPContributionExisting = 4;
                }
                else
                {
                    FinancialDataService.FinancialStrategyModel.CPPDeductionPercent = 0;
                    FinancialDataService.FinancialStrategyModel.CPPMaxDeductionAmount = 0;
                    FinancialDataService.FinancialStrategyModel.HasCPPFileLoaded = false;
                }

            }
        }

        public static async Task<KeyValuePair<string, List<string[]>>> ReadCPPBenefitPayoutFromFederalWebsiteAsync()
        {
            // Create a HttpClient to send requests to the website
            var client = new HttpClient();

            // Get the HTML content of the website as a string
            var html = await client.GetStringAsync(CPPBenefitsSite);

            // Use HtmlAgilityPack to parse the HTML content
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Find the table element by its id attribute
            var table = doc.DocumentNode.SelectSingleNode("//table[@class='table table-bordered']");

            // Create a list to store the table data
            var data = new List<string[]>();

            try
            {
                // Loop through each row of the table
                foreach (var row in table.SelectNodes(".//tr"))
                {
                    // Create an array to store the row data
                    var rowData = new string[3];

                    // Loop through each cell of the row
                    for (int i = 0; i < row.SelectNodes("th|td").Count; i++)
                    {
                        // Get the cell element by its index
                        var cell = row.SelectNodes("th|td")[i];

                        // Get the text content of the cell and trim any whitespace
                        var text = cell.InnerText.Trim();

                        // Store the text in the array
                        rowData[i] = Regex.Replace(text, @"[,$]", ""); ;
                    }

                    // Add the array to the list
                    data.Add(rowData);
                }
                return new KeyValuePair<string, List<string[]>>(table.OuterHtml, data);
            }
            catch (Exception ex)
            {
                ErroMessages = ex.Message;
            }

            return new KeyValuePair<string, List<string[]>>();
        }

        public static async void ReadCPPBenefitsFromLocalStorage()
        {
            string localFilePath = Utility.ReturnDeviceLocalPathForCPPRetirement();

            // Check if the file exists
            if (File.Exists(localFilePath))
            {
                string[] allLines = await File.ReadAllLinesAsync(localFilePath);

                List<CPPAnnualBenefits> cppDataList = new List<CPPAnnualBenefits>();

                for (int i = 1; i < allLines.Length; i++)
                {
                    //var values = Regex.Split(allLines[i], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                    var values = Regex.Split(allLines[i], ",");
                    // Removing $ and quotes from the values
                    for (int j = 1; j < values.Length; j++)
                    {
                        values[j] = values[j].Trim('"').Replace("$", "").Replace(",", "").Trim();
                        values[j] = Regex.Replace(values[j], @"[^\d.]", "");
                    }

                    var data = new CPPAnnualBenefits
                    {
                        TypeOfPensionOrBenefit = values[0],
                        AverageAmountForNewBeneficiaries = values[1] != String.Empty ? decimal.Parse(values[1]) : 0,
                        MaximumAmountForBeneficiaries = values[2] != String.Empty ? decimal.Parse(values[2]) : 0
                    };

                    cppDataList.Add(data);
                }

                if (cppDataList.Count > 0)
                {
                    FinancialDataService.FinancialStrategyModel.AverageAmountForNewBeneficiaries = cppDataList[0].AverageAmountForNewBeneficiaries * 12;
                    FinancialDataService.FinancialStrategyModel.MaximumAmountForBeneficiaries = cppDataList[0].MaximumAmountForBeneficiaries * 12;
                    FinancialDataService.FinancialStrategyModel.HasCPPBenefitsSiteLoaded = true;
                }
                else
                {
                    FinancialDataService.FinancialStrategyModel.AverageAmountForNewBeneficiaries = 0;
                    FinancialDataService.FinancialStrategyModel.MaximumAmountForBeneficiaries = 0;
                    FinancialDataService.FinancialStrategyModel.HasCPPBenefitsSiteLoaded = false;
                }

                Utility.CPPBenefitsExisting = 4;
            }
        }

        public async static Task<KeyValuePair<string, CPPPayoutRule>> GetCPPIncreaseDecreaseComputation()
        {
            CPPPayoutRule rule = new CPPPayoutRule();
            string url = CPPIncreaseDecreaseSite;

            // Create an HttpClient to fetch the webpage content
            HttpClient httpClient = new HttpClient();
            string html = await httpClient.GetStringAsync(url);

            // Load the HTML content
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Use XPath to locate the specific <section>
            string xpath = "//section[h2[contains(text(), 'Should you wait to start collecting CPP')]]";
            HtmlNode sectionNode = doc.DocumentNode.SelectSingleNode(xpath);

            if (sectionNode != null)
            {
                // Extract the rules from the <ul> within the <section>
                var ulNode = sectionNode.SelectSingleNode(".//ul");
                if (ulNode != null)
                {
                    var liNodes = ulNode.SelectNodes(".//li");
                    if (liNodes != null && liNodes.Count >= 2)
                    {
                        // Extract the two rules
                        string rule1Text = liNodes[0].InnerText.Trim();
                        string rule2Text = liNodes[1].InnerText.Trim();

                        // Extract the percentages from the rules using regular expressions
                        decimal percentageDecrease = Utility.ExtractPercentage(rule1Text);
                        decimal percentageIncrease = ExtractPercentage(rule2Text);

                        // Extract the minimum and maximum ages
                        int minimumAge = ExtractMinimumAge(rule1Text);
                        int maximumAge = ExtractMaximumAge(rule2Text);

                        // Extract the maximum reduction and maximum increase
                        decimal maximumReduction = ExtractMaximumReduction(rule1Text);
                        decimal maximumIncrease = ExtractMaximumIncrease(rule2Text);

                        rule.PercentageDecrease = percentageDecrease;
                        rule.PercentageIncrease = percentageIncrease;
                        rule.MaximumReduction = maximumReduction;
                        rule.MaximumIncrease = maximumIncrease;
                        rule.MinimumAge = minimumAge;
                        rule.MaximumAge = maximumAge;

                    }
                }
            }
            return new KeyValuePair<string, CPPPayoutRule>(sectionNode.OuterHtml, rule);
        }

        public static async void ReadCPPIncreaseDecreaseFromLocalStorage()
        {
            string localFilePath = Utility.ReturnDeviceLocalPathForCPPIncreaseDecrease();

            // Check if the file exists
            if (File.Exists(localFilePath))
            {
                string[] allLines = await File.ReadAllLinesAsync(localFilePath);

                if (allLines != null && allLines.Length > 1)
                {
                    var values = allLines[1].Split(',');

                    var rule = new CPPPayoutRule
                    {
                        PercentageDecrease = decimal.Parse(values[0], CultureInfo.InvariantCulture),
                        PercentageIncrease = decimal.Parse(values[1], CultureInfo.InvariantCulture),
                        MinimumAge = int.Parse(values[2]),
                        MaximumAge = int.Parse(values[3]),
                        MaximumReduction = decimal.Parse(values[4], CultureInfo.InvariantCulture),
                        MaximumIncrease = decimal.Parse(values[5], CultureInfo.InvariantCulture)
                    };
                    FinancialDataService.CPPPayoutRule = rule;
                    Utility.CPPIncDecExisting = 4;
                    FinancialDataService.FinancialStrategyModel.HasCPPIncDecFormulaSiteLoaded = true;
                }
            }
        }

        public async static Task<KeyValuePair<string, List<OASCalculation>>> GetOASBenefitPayoutFromFederalWebsiteAsync()
        {
            // Create an HttpClient to fetch the webpage content
            HttpClient httpClient = new HttpClient();
            string html = await httpClient.GetStringAsync(OASCalculationBenefits);

            // Load the HTML content
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Locate the specific <section> by its ID or other unique attribute
            var sectionNode = doc.DocumentNode.SelectSingleNode("//section[h2[@id='h2.2']]");

            if (sectionNode != null)
            {
                // Initialize a list to store the OASCalculation objects
                List<OASCalculation> oasCalculations = new List<OASCalculation>();

                // Iterate through the tables within the section
                foreach (var tableNode in sectionNode.SelectNodes(".//table"))
                {
                    // Extract the caption text as the header for the OASCalculation
                    string header = tableNode.SelectSingleNode(".//caption").InnerText.Trim();

                    // Extract the table data rows
                    var rows = tableNode.SelectNodes(".//tbody/tr");

                    if (rows != null)
                    {
                        foreach (var row in rows)
                        {
                            var cells = row.SelectNodes(".//td");

                            if (cells != null && cells.Count >= 3)
                            {
                                // Create an OASCalculation object and populate its fields
                                var oasCalculation = new OASCalculation
                                {
                                    YourSituation = cells[0].InnerText.Trim(),
                                    AnnualNetIncome = Utility.ParseDecimalFromText(cells[1].InnerText.Trim()),
                                    MaximumMonthlyPayment = Utility.ParseDecimalFromText(cells[2].InnerText.Trim())
                                };

                                oasCalculations.Add(oasCalculation);
                            }
                        }
                    }
                }

                //// Print the extracted data
                //foreach (var calculation in oasCalculations)
                //{
                //	Console.WriteLine("Your Situation: " + calculation.YourSituation);
                //	Console.WriteLine("Annual Net Income: " + calculation.AnnualNetIncome);
                //	Console.WriteLine("Maximum Monthly Payment: " + calculation.MaximumMonthlyPayment);
                //	Console.WriteLine();
                //}

                return new KeyValuePair<string, List<OASCalculation>>(sectionNode.OuterHtml, oasCalculations);
            }
            return new KeyValuePair<string, List<OASCalculation>>(string.Empty, null);
        }

        public static async void ReadOASBenefitsFromLocalStorage()
        {
            string localFilePath = Utility.ReturnDeviceLocalPathForOASBenefits();

            // Check if the file exists
            if (File.Exists(localFilePath))
            {
                string[] allLines = await File.ReadAllLinesAsync(localFilePath);

                List<OASCalculation> oasDataList = new List<OASCalculation>();

                for (int i = 1; i < allLines.Length; i++)
                {
                    //var values = Regex.Split(allLines[i], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                    var values = Regex.Split(allLines[i], ",");
                    // Removing $ and quotes from the values
                    for (int j = 1; j < values.Length; j++)
                    {
                        values[j] = values[j].Trim('"').Replace("$", "").Replace(",", "").Trim();
                        values[j] = Regex.Replace(values[j], @"[^\d.]", "");
                    }

                    var data = new OASCalculation
                    {
                        YourSituation = values[0],
                        AnnualNetIncome = values[1] != String.Empty ? decimal.Parse(values[1]) : 0,
                        MaximumMonthlyPayment = values[2] != String.Empty ? decimal.Parse(values[2]) : 0
                    };

                    oasDataList.Add(data);
                }

                if (oasDataList.Count > 0)
                {
                    FinancialDataService.OASFormulaRules = oasDataList;
                    OASBenefitExisting = 4;
                    FinancialDataService.FinancialStrategyModel.HasOASBenefitsSiteLoaded = true;
                }
            }
        }

        public static string EscapeAndFormatCsvValue(string value)
        {
            // Escape double quotes and format the value if needed
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        public static decimal ParseDecimalFromText(string text)
        {
            // Use regular expressions to extract numeric parts and parse as decimal
            var numericPart = Regex.Match(text, @"[\d\.,]+").Value;
            numericPart = numericPart.Replace(",", ""); // Remove commas for culture-invariant parsing
            return decimal.Parse(numericPart, CultureInfo.InvariantCulture);
        }

        #endregion


    }
}
