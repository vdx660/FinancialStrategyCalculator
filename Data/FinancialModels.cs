using Microsoft.AspNetCore.Components;

namespace FinancialStrategyCalculator.Data
{
    public class DropdownValue
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
    public class FinancialStrategyModel
    {
        public int Year { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public DateTime BirthDate { get; set; }
        public int Age { get; set; }
        public int Sex { get; set; }
        public decimal MonthlyInsurancePremium { get; set; }
        public decimal FaceAmount { get; set; }
        public int TermLength { get; set; }
        public bool ContinueInvestment { get; set; }
        public decimal MonthlyInvestment { get; set; }
        public bool UseCPIForMonthlyInvestment { get; set; }
        public decimal InitialInvestment { get; set; }
        public bool InvestmentPayInsurance { get; set; }
        public bool RefundPremiums { get; set; }
        public bool HasARTYRTFileLoaded { get; set; }
        public bool HasDRTFileLoaded { get; set; }
        public bool HasTFSARRSPFHSAFileLoaded { get; set; }
        public int InsuranceType { get; set; }
        public int InsurancePlan { get; set; }

        public decimal AnnualIncome { get; set; }
        public decimal AnnualIncomePercentageIncrease { get; set; }
        public decimal AmountToBankInTFSA { get; set; }
        public int StrategicContributionType { get; set; }
        public int IncomeInsuranceInvestmentRatios { get; set; }

        public decimal TFSARemainingContributionRoom { get; set; }
        public decimal TFSAAnnualMaximumum { get; set; }
        public decimal RRSPRemainingContributionRoomPercentage { get; set; }
        public decimal RRSPAnnualRate { get; set; }
        public decimal FHSARemainingContributionRoom { get; set; }
        public decimal FHSAAnnualMaximum { get; set; }

        public decimal TFSAStartingContributionRoom { get; set; }
        public decimal RRSPStartingContributionRoom { get; set; }
        public decimal FHSAStartingContributionRoom { get; set; }

        public decimal CPPDeductionPercent { get; set; }
        public decimal CPPMaxDeductionAmount { get; set; }
        public decimal AverageAmountForNewBeneficiaries { get; set; }
        public decimal MaximumAmountForBeneficiaries { get; set; }
        public decimal AverageAnnualCPIChange { get; set; }
        public decimal ReplaceWorkEarningsRate { get; set; }
        public bool HasCPPFileLoaded { get; set; }
        public bool HasCPPBenefitsSiteLoaded { get; set; }
        public bool IncludeCPPContributionsBenefits { get; set; }

        public decimal CPPIDpercentageDecrease { get; set; }
        public decimal CPPIDpercentageIncrease { get; set; }
        public int CPPIDminimumAge { get; set; }
        public int CPPIDmaximumAge { get; set; }
        public decimal CPPIDmaximumReduction { get; set; }
        public decimal CPPIDmaximumIncrease { get; set; }
        public int CPPIDAgeToReceiveCPP { get; set; }
        public bool CPPIDIncludeEstimatedOAS { get; set; }
        public bool HasCPPIncDecFormulaSiteLoaded { get; set; }
        public bool HasOASBenefitsSiteLoaded { get; set; }

        public decimal TotalAnnualDefinedBenefitPensionPlan { get; set; }
        public bool IncludeAnnualDefinedBenefitPensionPlan { get; set; }

        public decimal OtherAnnualPassiveIncome { get; set; }
        public decimal AnnualCostOfLivingExpenses { get; set; }
        public bool IncludeOtherIncomeExpenses { get; set; }

        public int NumberOfTimesToBank { get; set; }
        public decimal WithdrawHBPFHSA { get; set; }
        public decimal TaxReturnToTFSA { get; set; }
        public int AgeToWithdrawHBPFHSA { get; set; }

        public int AgeToEndInvestment { get; set; }


        public string AnnualIncreaseOnMonthlyContributionsStr { get; set; }

        public List<decimal> AnnualIncreaseOnMonthlyContributions
        {
            get
            {
                return AnnualIncreaseOnMonthlyContributionsStr.Split(';').Select(decimal.Parse).ToList();
            }
        }
        public string AgeToStopIncreaseStr { get; set; }

        public List<int> AgeToStopIncrease
        {
            get
            {
                return AgeToStopIncreaseStr.Split(';').Select(int.Parse).ToList();
            }
        }
        public decimal AnnualInterestRateLow { get; set; }
        public decimal AnnualInterestRateHigh { get; set; }
        public int AgeToMoveToRRIF { get; set; }
        public decimal RRIFRate { get; set; }
        public decimal ProjectedAnnualRetirementIncome { get; set; }

        //public int SwitchToTSFA { get; set; }
        public int AgeToStartPayouts { get; set; }
    }
    public class FinancialResultsModel
    {
        public int Year { get; set; }
        public string YearStyle { get; set; }

        public int Age { get; set; }
        public string AgeStyle { get; set; }

        public decimal MonthlyInsurancePremium { get; set; }
        public string MonthlyInsurancePremiumStyle { get; set; }

        public decimal MonthlyInvestment { get; set; }
        public string MonthlyInvestmentStyle { get; set; }

        public string InitialInvestment { get; set; }
        public string InitialInvestmentStyle { get; set; }

        public decimal YearlyInsurancePremium { get; set; }
        public string YearlyInsurancePremiumStyle { get; set; }
        public decimal FaceAmount { get; set; }
        public string FaceAmountStyle { get; set; }

        public decimal YearlyInvestmentContribution { get; set; }
        public string YearlyInvestmentContributionStyle { get; set; }

        public decimal TotalContribution { get; set; }
        public string TotalContributionStyle { get; set; }

        public decimal RunningTotalInsurancePaid { get; set; }
        public string RunningTotalInsurancePaidStyle { get; set; }

        public decimal RunningTotalInvestmentContributed { get; set; }
        public string RunningTotalInvestmentContributedStyle { get; set; }

        public decimal AnnualCompoundedInvestmentNoInterestTFSA { get; set; }
        public string AnnualCompoundedInvestmentNoInterestTFSAStyle { get; set; }

        public decimal AnnualCompoundedInvestmentLowTFSA { get; set; }
        public string AnnualCompoundedInvestmentLowTFSAStyle { get; set; }

        public decimal AnnualCompoundedInvestmentAveTFSA { get; set; }
        public string AnnualCompoundedInvestmentAveTFSAStyle { get; set; }

        public decimal AnnualCompoundedInvestmentHighTFSA { get; set; }
        public string AnnualCompoundedInvestmentHighTFSAStyle { get; set; }

        public decimal AnnualCompoundedInvestmentNoInterestRRSP { get; set; }
        public string AnnualCompoundedInvestmentNoInterestRRSPStyle { get; set; }

        public decimal AnnualCompoundedInvestmentLowRRSP { get; set; }
        public string AnnualCompoundedInvestmentLowRRSPStyle { get; set; }

        public decimal AnnualCompoundedInvestmentAveRRSP { get; set; }
        public string AnnualCompoundedInvestmentAveRRSPStyle { get; set; }

        public decimal AnnualCompoundedInvestmentHighRRSP { get; set; }
        public string AnnualCompoundedInvestmentHighRRSPStyle { get; set; }

        public decimal AnnualCompoundedInvestmentNoInterestFHSA { get; set; }
        public string AnnualCompoundedInvestmentNoInterestFHSAStyle { get; set; }

        public decimal AnnualCompoundedInvestmentLowFHSA { get; set; }
        public string AnnualCompoundedInvestmentLowFHSAStyle { get; set; }

        public decimal AnnualCompoundedInvestmentAveFHSA { get; set; }
        public string AnnualCompoundedInvestmentAveFHSAStyle { get; set; }

        public decimal AnnualCompoundedInvestmentHighFHSA { get; set; }
        public string AnnualCompoundedInvestmentHighFHSAStyle { get; set; }

        public decimal AnnualCompoundedInvestmentNoInterest { get; set; }
        public string AnnualCompoundedInvestmentNoInterestStyle { get; set; }

        public decimal AnnualCompoundedInvestmentLow { get; set; }
        public string AnnualCompoundedInvestmentLowStyle { get; set; }

        public decimal AnnualCompoundedInvestmentAve { get; set; }
        public string AnnualCompoundedInvestmentAveStyle { get; set; }

        public decimal AnnualCompoundedInvestmentHigh { get; set; }
        public string AnnualCompoundedInvestmentHighStyle { get; set; }

        public decimal TFSARemainingContributionRoom { get; set; }
        public string TFSARemainingContributionRoomStyle { get; set; }
        public decimal RRSPRemainingContributionRoom { get; set; }
        public string RRSPRemainingContributionRoomStyle { get; set; }
        public decimal FHSARemainingContributionRoom { get; set; }
        public string FHSARemainingContributionRoomStyle { get; set; }

        public decimal AnnualIncome { get; set; }
        public string AnnualIncomeStyle { get; set; }
        public MarkupString AnnualIncomePayoutText { get; set; }
        public decimal AnnualIncomePayout { get; set; }

        public decimal TotalAnnualDefinedBenefitPensionPlan { get; set; }
        public string TotalAnnualDefinedBenefitPensionPlanStyle { get; set; }
        public decimal OtherAnnualPassiveIncome { get; set; }
        public string OtherAnnualPassiveIncomeStyle { get; set; }

        public decimal CPPOASDeductionAmount { get; set; }
        public decimal CPPOASBeneficiaryPayoutAmount { get; set; }

        public decimal CostofLivingExpenses { get; set; }
        public int RRIFAge { get; set; }
        public int PayoutAge { get; set; }

        public string Color { get; set; }
        public string ColorStrat { get; set; }
        public string ColorBackRow { get; set; }
        public string IIFontSize { get; set; }
        public string RRSPDumpStyle { get; set; }
        public string TFSADumpStyle { get; set; }

        public decimal ChartInvestmentNoInt
        {
            get
            {
                return RunningTotalInvestmentContributed;

            }
        }
        public decimal ChartInvestmentLow
        {
            get
            {
                return AnnualCompoundedInvestmentLow - RunningTotalInvestmentContributed;
            }
        }

        public decimal ChartInvestmentAve
        {
            get
            {
                return AnnualCompoundedInvestmentAve - AnnualCompoundedInvestmentLow;
            }
        }

        public decimal ChartInvestmentHigh
        {
            get
            {
                return AnnualCompoundedInvestmentHigh - AnnualCompoundedInvestmentAve;
            }
        }

    }
    public class ContributionChanges
    {
        public decimal AnnualChangesOnMonthlyContributions { get; set; }
        public int AgeToStopChange { get; set; }
        public bool OneTimeIncrease { get; set; }
        public bool ClearExistingEntries { get; set; }
        public bool AutoUpdate { get; set; }
    }
    public class ContributionRooms
    {
        public decimal RRSPRemainingContributionRoom { get; set; }
        public decimal TFSARemainingContributionRoom { get; set; }
        public decimal FHSARemainingContributionRoom { get; set; }
    }
    public class CPPAnnualContributionLimit
    {
        public int Year { get; set; }
        public decimal MaximumAnnualPensionableEarnings { get; set; }
        public decimal BasicExemptionAmount { get; set; }
        public decimal MaximumContributoryEarnings { get; set; }
        public decimal ContributionRate { get; set; }
        public decimal MaximumAnnualEmployeeAndEmployerContribution { get; set; }
        public decimal MaximumAnnualSelfEmployedContribution { get; set; }
    }
    public class CPPAnnualBenefits
    {
        public string TypeOfPensionOrBenefit { get; set; }
        public decimal AverageAmountForNewBeneficiaries { get; set; }
        public decimal MaximumAmountForBeneficiaries { get; set; }
    }
    public class BankAndDumpChanges
    {
        public decimal BankAndDumpAmount { get; set; }
        public int AgeToBankAndDump { get; set; }
        public bool ClearExistingEntries { get; set; }
        public bool AutoUpdate { get; set; }

    }

}

