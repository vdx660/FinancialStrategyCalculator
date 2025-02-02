namespace FinancialStrategyCalculator.Data
{
    public class ARTYRTInsuranceData
    {
        public int Id { get; set; }
        public string AgeRange { get; set; }
        public decimal PreferredPlus150kto249k { get; set; }
        public decimal PreferredPlus250kto499k { get; set; }
        public decimal PreferredPlus500k { get; set; }
        public decimal SelectNTU149k { get; set; }
        public decimal SelectNTU150kto249k { get; set; }
        public decimal SelectNTU250kto499k { get; set; }
        public decimal SelectNTU500kto749k { get; set; }
        public decimal SelectNTU750k { get; set; }
        public decimal TakenUpTU149k { get; set; }
        public decimal TakenUpTU150kto249k { get; set; }
        public decimal TakenUpTU250kto499k { get; set; }
        public decimal TakenUpTU500kto749k { get; set; }
        public decimal TakenUpTU750k { get; set; }
    }

    public class DRTInsuranceData
    {
        public int Id { get; set; }
        public string AgeRange { get; set; }
        public decimal SelectNTU149k { get; set; }
        public decimal SelectNTU150kto249k { get; set; }
        public decimal SelectNTU250kto499k { get; set; }
        public decimal SelectNTU500kto749k { get; set; }
        public decimal SelectNTU750k { get; set; }
    }
    public class TFSARRSPFHSAData
    {
        public int Id { get; set; }
        public decimal TFSA { get; set; }
        public decimal RRSP { get; set; }
        public decimal FHSA { get; set; }
    }

    public class CPPContribution
    {
        public int Year { get; set; }
        public decimal MaximumAnnualPensionableEarnings { get; set; }
        public decimal BasicExemptionAmount { get; set; }
        public decimal MaximumContributoryEarnings { get; set; }
        public decimal ContributionRate { get; set; }
        public decimal MaximumAnnualEmployeeAndEmployerContribution { get; set; }
        public decimal MaximumAnnualSelfEmployedContribution { get; set; }
    }


    public class OASCalculation
    {
        public string YourSituation { get; set; }
        public decimal AnnualNetIncome { get; set; }
        public decimal MaximumMonthlyPayment { get; set; }
    }

    public class CPPPayoutRule
    {
        // Declare the fields as properties
        public decimal PercentageDecrease { get; set; }
        public decimal PercentageIncrease { get; set; }
        public int MinimumAge { get; set; }
        public int MaximumAge { get; set; }
        public decimal MaximumReduction { get; set; }
        public decimal MaximumIncrease { get; set; }

    }
}
