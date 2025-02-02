using FinancialStrategyCalculator.Data;
using FinancialStrategyCalculator.Shared;
using Microsoft.AspNetCore.Components;

namespace FinancialStrategyCalculator.Services
{
    public class FinancialDataService
    {
        #region Fields
        private static decimal runningTotalInsurancePaid = 0;
        private static decimal tenUnitsAfterTerm = 0;
        private static int tenCountAfterTerm = 0;

        private static decimal runningTotalRRSPRemainingContributionRoom = 0;
        private static decimal runningTotalTFSARemainingContributionRoom = 0;
        private static decimal runningTotalFHSARemainingContributionRoom = 0;

        private static decimal runningTotalRRSPStrategicContribution = 0;
        private static decimal runningTotalTFSAStrategicContribution = 0;
        private static decimal runningTotalFHSAStrategicContribution = 0;

        private static decimal runningTotalInvestmentContributed = 0;

        private static decimal runningTotalInvestmentNoInterest = 0;
        private static decimal runningTotalInvestmentLow = 0;
        private static decimal runningTotalInvestmentAve = 0;
        private static decimal runningTotalInvestmentHigh = 0;

        private static decimal runningTotalInvestmentTFSANoInterest = 0;
        private static decimal runningTotalInvestmentTFSALow = 0;
        private static decimal runningTotalInvestmentTFSAAve = 0;
        private static decimal runningTotalInvestmentTFSAHigh = 0;

        private static decimal runningTotalInvestmentRRSPNoInterest = 0;
        private static decimal runningTotalInvestmentRRSPLow = 0;
        private static decimal runningTotalInvestmentRRSPAve = 0;
        private static decimal runningTotalInvestmentRRSPHigh = 0;

        private static decimal runningTotalInvestmentFHSANoInterest = 0;
        private static decimal runningTotalInvestmentFHSALow = 0;
        private static decimal runningTotalInvestmentFHSAAve = 0;
        private static decimal runningTotalInvestmentFHSAHigh = 0;

        private static decimal runningTotalProjectedAnnualRetirementIncome = 0;
        private static decimal runningTotalCPPAmountForNewBeneficiaries = 0;

        private static decimal annualIncomeWithIncrease = 0;

        private static int timesToBank = 0;

        private static int yearPeriod = 0;
        private static int termLength = 0;

        public static bool CalculationOngoing = false;

        #endregion

        #region Properties
        public static List<FinancialResultsModel> FinancialResultsModel { get; set; }
        public static FinancialStrategyModel FinancialStrategyModel { get; set; }
        public static List<ARTYRTInsuranceData> InsuranceARTYRTDataSettings { get; set; }
        public static List<DRTInsuranceData> InsuranceDRTDataSettings { get; set; }
        public static List<TFSARRSPFHSAData> TFSARRSPFHSADataSettings { get; set; }
        public static CPPPayoutRule CPPPayoutRule { get; set; }
        public static List<OASCalculation> OASFormulaRules { get; set; }


        #endregion

        #region Methods
        /// <summary>The MAIN Method to Compute FinancialResultsModel and stores in Public Static Property FinancialResultsModel</summary>
        /// <param name="model">The model.</param>
        public static void COMPUTEFinancialResultsModel(FinancialStrategyModel model)
        {
            FinancialResultsModel = new List<FinancialResultsModel>();

            runningTotalInsurancePaid = 0;

            runningTotalRRSPRemainingContributionRoom = 0;
            runningTotalTFSARemainingContributionRoom = 0;
            runningTotalFHSARemainingContributionRoom = 0;

            runningTotalRRSPStrategicContribution = 0;
            runningTotalTFSAStrategicContribution = 0;
            runningTotalFHSAStrategicContribution = 0;

            runningTotalInvestmentContributed = model.InitialInvestment;

            runningTotalInvestmentNoInterest = model.InitialInvestment;
            runningTotalInvestmentLow = model.InitialInvestment;
            runningTotalInvestmentAve = model.InitialInvestment;
            runningTotalInvestmentHigh = model.InitialInvestment;

            runningTotalInvestmentTFSANoInterest = model.InitialInvestment;
            runningTotalInvestmentTFSALow = model.InitialInvestment;
            runningTotalInvestmentTFSAAve = model.InitialInvestment;
            runningTotalInvestmentTFSAHigh = model.InitialInvestment;

            runningTotalInvestmentRRSPNoInterest = 0;
            runningTotalInvestmentRRSPLow = 0;
            runningTotalInvestmentRRSPAve = 0;
            runningTotalInvestmentRRSPHigh = 0;

            runningTotalInvestmentFHSANoInterest = 0;
            runningTotalInvestmentFHSALow = 0;
            runningTotalInvestmentFHSAAve = 0;
            runningTotalInvestmentFHSAHigh = 0;

            runningTotalProjectedAnnualRetirementIncome = 0;

            annualIncomeWithIncrease = model.AnnualIncome;

            tenCountAfterTerm = 0;      //Reset 10YR DRT

            timesToBank = 0;

            yearPeriod = model.Year;
            termLength = 0;

            for (int age = model.Age; age <= 99; age++)
            {
                var result = new FinancialResultsModel { Age = age };

                result.IIFontSize = "initial";
                result.Year = yearPeriod;
                yearPeriod++;

                if (termLength < model.TermLength || model.ContinueInvestment)
                {
                    result.MonthlyInsurancePremium = model.MonthlyInsurancePremium;

                    if (age <= model.AgeToStartPayouts)
                    {
                        if (model.UseCPIForMonthlyInvestment && age > model.Age)
                        {
                            model.MonthlyInvestment = model.MonthlyInvestment + (model.MonthlyInvestment * (model.AverageAnnualCPIChange / 100));
                        }
                        result.MonthlyInvestment = model.MonthlyInvestment;
                    }
                    result.RRIFAge = model.AgeToMoveToRRIF;
                    result.PayoutAge = model.AgeToStartPayouts;

                    result.YearlyInvestmentContribution = result.MonthlyInvestment * 12;
                    result.YearlyInsurancePremium = result.MonthlyInsurancePremium * 12;

                    //ART/YRT COMPUTATION - Used when deducting insurance from Investment for Free Insurance
                    decimal yearlyInsuranceDeduct = CalculateFreeInsuranceStrategies(model, ref runningTotalInsurancePaid, termLength, age, result);


                    //This code should be above any running Totals
                    if (model.AnnualIncreaseOnMonthlyContributions.Count > 0 && model.AgeToStopIncrease.Count > 0)
                    {
                        for (int i = 0; i < model.AgeToStopIncrease.Count; i++)
                        {
                            if (age > model.Age && age < model.AgeToStopIncrease[i])
                            {
                                //This calculation Impacts Initial Calculation Above for YearlyInvestmentContribution
                                model.MonthlyInvestment += model.AnnualIncreaseOnMonthlyContributions[i];
                                result.MonthlyInvestment = model.MonthlyInvestment;
                                result.YearlyInvestmentContribution = result.MonthlyInvestment * 12;
                                break;
                            }
                        }
                    }

                    #region Bank and Dump

                    int bankAndDumpFlag = 0;
                    decimal dumpAmount = model.AmountToBankInTFSA;

                    //CALCULATE BANK AND DUMP
                    if (timesToBank < model.NumberOfTimesToBank)
                    {
                        //Use NoInterest for Now but add option to choose later for Low, Ave, High
                        runningTotalTFSAStrategicContribution = runningTotalInvestmentTFSANoInterest;
                        bankAndDumpFlag = SetupBankNDumpContributionOnly(model, result);
                    }

                    if (bankAndDumpFlag == 1)
                    {
                        timesToBank++;
                    }

                    //This impacts Annual Income
                    #region This Impacts Annual Income Computation

                    if (age < model.AgeToStartPayouts)
                    {
                        //Before Retirement
                        result.AnnualIncomePayoutText = new MarkupString("");

                        if (age == model.AgeToWithdrawHBPFHSA)
                        {
                            result.InitialInvestment += string.Format("Withdrawal of HBP/FHSA = {0:N2}; Potential Tax Return = {1:N2}; ", model.WithdrawHBPFHSA, model.TaxReturnToTFSA);
                            result.InitialInvestmentStyle = "color: brown; font-weight: bold; font-size: x-small;";
                        }

                        //This is used by BankAndDumpNoInterest;
                        //CPP Contribution Before Retirement 25%-33%
                        result.CPPOASDeductionAmount = annualIncomeWithIncrease * (model.CPPDeductionPercent / 100) < model.CPPMaxDeductionAmount ? annualIncomeWithIncrease * (model.CPPDeductionPercent / 100) : model.CPPMaxDeductionAmount;
                        result.AnnualIncome = annualIncomeWithIncrease;

                        annualIncomeWithIncrease *= 1 + model.AnnualIncomePercentageIncrease / 100;
                    }
                    else
                    {
                        if (age <= model.AgeToEndInvestment)
                        {
                            //After Retirement
                            runningTotalProjectedAnnualRetirementIncome = model.ProjectedAnnualRetirementIncome;
                            result.AnnualIncome = model.ProjectedAnnualRetirementIncome;
                            result.AnnualIncomeStyle = "color: blue; font-weight: bold; ";
                            result.AnnualIncomePayoutText = new MarkupString("<span style=\"color:blue\" ><b></b></span>");
                            result.AnnualIncomePayout = result.AnnualIncome;

                        }

                        result.TotalAnnualDefinedBenefitPensionPlan = model.TotalAnnualDefinedBenefitPensionPlan * (1 + model.AverageAnnualCPIChange / 100);
                        result.TotalAnnualDefinedBenefitPensionPlanStyle = "color: blue; font-weight: bold; ";
                    }

                    // CALUATE CPP & OAS
                    result = CalculateCPPOASRetirementPayout(model, age, result);

                    result.CostofLivingExpenses = model.AnnualCostOfLivingExpenses;

                    #endregion

                    if (age <= model.AgeToEndInvestment)
                    {
                        //NOTE: runningTotalProjectedAnnualRetirementIncome = This is the amount that gets deducted from the Investment until it becomes zero

                        //CONTRIBUTION & REMAINING CONTRIBUTION ROOM
                        BankAndDumpNoInterest(model, result, yearlyInsuranceDeduct, bankAndDumpFlag, dumpAmount, runningTotalProjectedAnnualRetirementIncome);

                        //LOW COMPOUNDING
                        BankAndDumpLowInterest(model, result, yearlyInsuranceDeduct, bankAndDumpFlag, dumpAmount, runningTotalProjectedAnnualRetirementIncome);

                        //HIGH COMPOUNDING
                        BankAndDumpHighInterest(model, result, yearlyInsuranceDeduct, bankAndDumpFlag, dumpAmount, runningTotalProjectedAnnualRetirementIncome);

                        //AVERAGE COMPOUNDING
                        BankAndDumpAveInterest(model, result, yearlyInsuranceDeduct, bankAndDumpFlag, dumpAmount, runningTotalProjectedAnnualRetirementIncome);
                    }
                    #endregion

                    result.TotalContribution = (yearlyInsuranceDeduct == 0 ? result.MonthlyInsurancePremium : 0) + result.MonthlyInvestment;


                    if (age == 65 || age == 71)
                    {
                        result.ColorBackRow = "Lavender";
                    }
                    else
                    {
                        result.ColorBackRow = "Transparent";
                    }

                    if (age == model.Age)
                    {
                        result.InitialInvestment = model.InitialInvestment.ToString("#,##0.00");
                        result.InitialInvestmentStyle = "color: Black; font-weight: bold; ";
                    }

                    FinancialResultsModel.Add(result);
                }

                termLength++;
            }
        }

        private static FinancialResultsModel CalculateCPPOASRetirementPayout(FinancialStrategyModel model, int age, FinancialResultsModel result)
        {
            //CPP Retirement Payout for Current Year to Retirement Year
            if (age == model.Age)
            {
                decimal cpppayoutAmt = 0;
                if (Utility.CPPPayoutDetails == 1)
                {
                    cpppayoutAmt = model.AverageAmountForNewBeneficiaries;
                }
                else if (Utility.CPPPayoutDetails == 2)
                {
                    cpppayoutAmt = model.MaximumAmountForBeneficiaries;
                }

                if (result.AnnualIncome * (model.ReplaceWorkEarningsRate / 100) < cpppayoutAmt)
                {
                    runningTotalCPPAmountForNewBeneficiaries = result.AnnualIncome * (model.ReplaceWorkEarningsRate / 100);
                }
                else
                {
                    runningTotalCPPAmountForNewBeneficiaries = cpppayoutAmt;
                }
            }
            else
            {
                //Adjust based on CPI in curent year until 65 or 71
                runningTotalCPPAmountForNewBeneficiaries *= 1 + model.AverageAnnualCPIChange / 100; //Ave or Max
            }


            #region Strategy to Use Investment as CPP Replacement to Maximize CPP
            //=======================================================================
            // Strategy to Use Investment as CPP + OAS Replacement to Maximize CPP
            //=======================================================================

            //=======================================================================
            //Calculate OAS
            //-----------------------------------------------------------------------
            decimal oasAdd = 0;
            //Don't check for OAS Age 65 here check below
            if (model.CPPIDIncludeEstimatedOAS && model.IncludeCPPContributionsBenefits)
            {
                if (age < 75)
                {
                    //TODO: Get from Website
                    oasAdd = OASFormulaRules != null ? OASFormulaRules[0].MaximumMonthlyPayment * 12 : 0;

                }
                else
                {
                    //TODO: Get from Website
                    oasAdd = OASFormulaRules != null ? OASFormulaRules[1].MaximumMonthlyPayment * 12 : 0;
                }
            }

            //=======================================================================
            // Calculate CPP. If you put 70 this strategy computed the max of 42% increase
            //-----------------------------------------------------------------------

            //-----------------------------------------------------------------------
            //CPP Replacement
            if (age >= model.AgeToStartPayouts && model.CPPIDAgeToReceiveCPP > model.AgeToStartPayouts)
            {
                if (model.AgeToStartPayouts < (model.CPPIDAgeToReceiveCPP) && age < (model.CPPIDAgeToReceiveCPP))
                {
                    decimal cppReplacementAmount = CalculateCPPIDPayout(model.CPPIDAgeToReceiveCPP, runningTotalCPPAmountForNewBeneficiaries);

                    runningTotalProjectedAnnualRetirementIncome += cppReplacementAmount;

                    //Green Annual Payout
                    result.AnnualIncomePayout += cppReplacementAmount;

                    result.AnnualIncome += cppReplacementAmount;

                    result.InitialInvestment += "+CRepI; ";
                    result.AnnualIncomeStyle = "color: #AA1AF0; font-weight: bold; ";
                }
            }

            //OAS Replacement
            if (age >= model.AgeToStartPayouts && (age < 65 || age < model.CPPIDAgeToReceiveCPP))
            {
                decimal oasReplacementAmount = oasAdd;
                runningTotalProjectedAnnualRetirementIncome += oasReplacementAmount;

                //Green Annual Payout
                result.AnnualIncomePayout += oasReplacementAmount;

                result.AnnualIncome += oasReplacementAmount;

                result.InitialInvestment += "+ORepI; ";
                result.AnnualIncomeStyle = "color: #AA1AF0; font-weight: bold; ";
            }
            //-----------------------------------------------------------------------

            //-----------------------------------------------------------------------
            //Actual CPP/OAS - NOT replacement
            if (age >= model.CPPIDAgeToReceiveCPP && model.IncludeCPPContributionsBenefits && age >= 60)
            {
                decimal cppReplacementAmount = CalculateCPPIDPayout(model.CPPIDAgeToReceiveCPP, runningTotalCPPAmountForNewBeneficiaries);


                if (age >= 65)
                {
                    cppReplacementAmount = cppReplacementAmount + oasAdd;
                    result.InitialInvestment += "+OAS; ";
                }
                result.CPPOASBeneficiaryPayoutAmount += cppReplacementAmount;
                result.AnnualIncomePayout += cppReplacementAmount;


                result.InitialInvestment += "+CPP; ";
                result.AnnualIncome += cppReplacementAmount;
            }
            //-----------------------------------------------------------------------

            #endregion

            if (age >= model.AgeToStartPayouts && age >= 60)
            {
                if (model.IncludeAnnualDefinedBenefitPensionPlan)
                {
                    result.InitialInvestment += "+DBPP; ";
                    result.AnnualIncome += result.TotalAnnualDefinedBenefitPensionPlan;
                }

                if (model.IncludeOtherIncomeExpenses)
                {
                    result.InitialInvestment += "+/-PInc/COLE; ";
                    result.AnnualIncome += result.OtherAnnualPassiveIncome;
                    result.AnnualIncome -= model.AnnualCostOfLivingExpenses;
                }
            }

            return result;
        }

        private static decimal CalculateCPPIDPayout(int ageToStartPayouts, decimal cppAmount)
        {
            //TODO: Get from Website
            decimal percentageDecrease = CPPPayoutRule != null ? CPPPayoutRule.PercentageDecrease * 12 : 0.6m * 12;
            decimal percentageIncrease = CPPPayoutRule != null ? CPPPayoutRule.PercentageIncrease * 12 : 0.7m * 12;
            decimal maximumReduction = CPPPayoutRule != null ? CPPPayoutRule.MaximumReduction : 36;
            decimal maximumIncrease = CPPPayoutRule != null ? CPPPayoutRule.MaximumIncrease : 42;

            decimal netAmount = cppAmount;

            if (ageToStartPayouts < 65)
            {
                int yearsBelow65 = 65 - ageToStartPayouts;
                decimal reduction = cppAmount * ((percentageDecrease / 100) * 12 * yearsBelow65);
                if (reduction > (cppAmount * (maximumReduction / 100)))
                {
                    reduction = cppAmount * (maximumReduction / 100);
                }
                netAmount -= reduction;
            }
            else if (ageToStartPayouts > 65)
            {
                int yearsAbove65 = ageToStartPayouts - 65;
                decimal increase = cppAmount * ((percentageIncrease / 100) * 12 * yearsAbove65);
                if (increase > (cppAmount * (maximumIncrease / 100)))
                {
                    increase = cppAmount * (maximumIncrease / 100);
                }
                netAmount += increase;
            }

            return netAmount;
        }

        /// <summary>This is the Base Method for Calculating Contribution Room</summary>
        /// <param name="currentRecord">The current record.</param>
        /// <param name="bankAndDumpFlag">The bank and dump flag.</param>
        /// <param name="dumpAmount">The dump amount.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        private static ContributionRooms CalculateBaseRemainingContributionRoom(FinancialResultsModel currentRecord, int bankAndDumpFlag, decimal dumpAmount)
        {
            FinancialResultsModel lastResult = FinancialResultsModel.Count > 0 ? FinancialResultsModel.LastOrDefault() : new FinancialResultsModel();

            ContributionRooms remainingRooms = new ContributionRooms();

            if (bankAndDumpFlag == 0)
            {
                remainingRooms.TFSARemainingContributionRoom = CalculateIncDecInContributionsAndInterest(runningTotalTFSARemainingContributionRoom, 0, YearlyAllocationContribution(currentRecord, "TFSA")) - (currentRecord.AnnualCompoundedInvestmentNoInterestTFSA - lastResult.AnnualCompoundedInvestmentNoInterestTFSA);
                if (currentRecord.Age < 71)
                {
                    remainingRooms.RRSPRemainingContributionRoom = CalculateIncDecInContributionsAndInterest(runningTotalRRSPRemainingContributionRoom, 0, YearlyAllocationContribution(currentRecord, "RRSP")) - (currentRecord.AnnualCompoundedInvestmentNoInterestRRSP - lastResult.AnnualCompoundedInvestmentNoInterestRRSP);
                }
                remainingRooms.FHSARemainingContributionRoom = CalculateIncDecInContributionsAndInterest(runningTotalFHSARemainingContributionRoom, 0, YearlyAllocationContribution(currentRecord, "FHSA")) - (currentRecord.AnnualCompoundedInvestmentNoInterestFHSA - lastResult.AnnualCompoundedInvestmentNoInterestFHSA);
            }
            else if (bankAndDumpFlag == 1)
            {
                remainingRooms.TFSARemainingContributionRoom = runningTotalTFSARemainingContributionRoom + dumpAmount + currentRecord.YearlyInvestmentContribution;
                remainingRooms.RRSPRemainingContributionRoom = runningTotalRRSPRemainingContributionRoom - dumpAmount;
            }

            return remainingRooms;
        }

        /// <summary>Calculate Bank and Dump Strategy for Average Interest Rates</summary>
        /// <param name="model">The model.</param>
        /// <param name="currentResultRecord">The current result record.</param>
        /// <param name="yearlyInsuranceDeduct">The yearly insurance deduct.</param>
        /// <param name="bankAndDumpFlag">The bank and dump flag.</param>
        /// <param name="dumpAmount">The dump amount.</param>
        /// <param name="runningTotalProjectedAnnualRetirementIncome">The running total projected annual retirement income.</param>
        private static void BankAndDumpAveInterest(FinancialStrategyModel model, FinancialResultsModel currentResultRecord, decimal yearlyInsuranceDeduct, int bankAndDumpFlag, decimal dumpAmount, decimal runningTotalProjectedAnnualRetirementIncome)
        {
            decimal computedInterest = (model.AnnualInterestRateHigh + model.AnnualInterestRateLow) / 2;
            if (currentResultRecord.Age > model.AgeToMoveToRRIF)
            {
                computedInterest = model.RRIFRate;
            }

            runningTotalInvestmentAve = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentAve, computedInterest, currentResultRecord.YearlyInvestmentContribution);
            if (bankAndDumpFlag == 0)
            {
                runningTotalInvestmentTFSAAve = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentTFSAAve, computedInterest, YearlyAllocationContribution(currentResultRecord, "TFSA"));
                runningTotalInvestmentRRSPAve = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentRRSPAve, computedInterest, YearlyAllocationContribution(currentResultRecord, "RRSP"));
                runningTotalInvestmentFHSAAve = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentFHSAAve, computedInterest, YearlyAllocationContribution(currentResultRecord, "FHSA"));
            }
            else if (bankAndDumpFlag == 1)
            {
                runningTotalInvestmentTFSAAve = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentTFSAAve, computedInterest, YearlyAllocationContribution(currentResultRecord, "TFSA")) - dumpAmount;
                runningTotalInvestmentRRSPAve = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentRRSPAve, computedInterest, YearlyAllocationContribution(currentResultRecord, "RRSP")) + dumpAmount;
                runningTotalInvestmentFHSAAve = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentFHSAAve, computedInterest, YearlyAllocationContribution(currentResultRecord, "FHSA"));

                //Dump Style
                currentResultRecord.AnnualCompoundedInvestmentAveTFSAStyle = "color: blue; font-weight: bold;";
                currentResultRecord.AnnualCompoundedInvestmentAveRRSPStyle = "color: blue; font-weight: bold;";

            }

            decimal totalInvestmentAve = runningTotalInvestmentTFSAAve + runningTotalInvestmentRRSPAve;

            decimal tfsaPercentage = totalInvestmentAve != 0
                                     ? runningTotalInvestmentTFSAAve / totalInvestmentAve
                                     : 1; // Default to 1 for tfsaPercentage

            decimal rrspPercentage = totalInvestmentAve != 0
                                     ? runningTotalInvestmentRRSPAve / totalInvestmentAve
                                     : 1; // Default to 1 for rrspPercentage

            runningTotalInvestmentTFSAAve -= runningTotalProjectedAnnualRetirementIncome * tfsaPercentage + yearlyInsuranceDeduct;  //You pay Insurance Premiums from TFSA not RRSP
            runningTotalInvestmentRRSPAve -= runningTotalProjectedAnnualRetirementIncome * rrspPercentage;
            runningTotalInvestmentFHSAAve -= 0;
            currentResultRecord.AnnualCompoundedInvestmentAveTFSA = runningTotalInvestmentTFSAAve;
            currentResultRecord.AnnualCompoundedInvestmentAveRRSP = runningTotalInvestmentRRSPAve;
            currentResultRecord.AnnualCompoundedInvestmentAveFHSA = runningTotalInvestmentFHSAAve;

            currentResultRecord.AnnualCompoundedInvestmentAve = currentResultRecord.AnnualCompoundedInvestmentAveTFSA + currentResultRecord.AnnualCompoundedInvestmentAveRRSP + currentResultRecord.AnnualCompoundedInvestmentAveFHSA;

            if (currentResultRecord.Age > currentResultRecord.RRIFAge)
            {
                currentResultRecord.AnnualCompoundedInvestmentAveStyle = "color: #7A4E61";
            }

            if (currentResultRecord.AnnualCompoundedInvestmentAve < 0)
            {
                currentResultRecord.AnnualCompoundedInvestmentAveStyle = "color: Red";
                currentResultRecord.InitialInvestment += "Neg Ave Rate; ";
                currentResultRecord.InitialInvestmentStyle = "color: Red; font-weight: bold; font-size: x-small;";
                //currentResultRecord.IIFontSize = "x-small";
            }

            if (currentResultRecord.AnnualCompoundedInvestmentAveTFSA < 0)
            {
                currentResultRecord.AnnualCompoundedInvestmentAveTFSAStyle = "color: Red";
                currentResultRecord.InitialInvestment += "Neg TFSA Ave Rate; ";
                currentResultRecord.InitialInvestmentStyle = "color: Red; font-weight: bold; font-size: x-small;";
                //result.IIFontSize = "x-small";
            }
            if (currentResultRecord.AnnualCompoundedInvestmentAveRRSP < 0)
            {
                currentResultRecord.AnnualCompoundedInvestmentAveRRSPStyle = "color: Red";
                currentResultRecord.InitialInvestment += "Neg RRSP Ave Rate; ";
                currentResultRecord.InitialInvestmentStyle = "color: Red; font-weight: bold; font-size: x-small;";
                //currentResultRecord.IIFontSize = "x-small";
            }
        }

        /// <summary>Calculate Bank and Dump Strategy for High Interest Rates</summary>
        /// <param name="model">The model.</param>
        /// <param name="currentResultRecord">The current result record.</param>
        /// <param name="yearlyInsuranceDeduct">The yearly insurance deduct.</param>
        /// <param name="bankAndDumpFlag">The bank and dump flag.</param>
        /// <param name="dumpAmount">The dump amount.</param>
        /// <param name="runningTotalProjectedAnnualRetirementIncome">The running total projected annual retirement income.</param>
        private static void BankAndDumpHighInterest(FinancialStrategyModel model, FinancialResultsModel currentResultRecord, decimal yearlyInsuranceDeduct, int bankAndDumpFlag, decimal dumpAmount, decimal runningTotalProjectedAnnualRetirementIncome)
        {
            decimal computedInterest = model.AnnualInterestRateHigh;
            if (currentResultRecord.Age > model.AgeToMoveToRRIF)
            {
                computedInterest = model.RRIFRate;
            }

            runningTotalInvestmentHigh = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentHigh, computedInterest, currentResultRecord.YearlyInvestmentContribution);
            if (bankAndDumpFlag == 0)
            {
                runningTotalInvestmentTFSAHigh = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentTFSAHigh, computedInterest, YearlyAllocationContribution(currentResultRecord, "TFSA"));
                runningTotalInvestmentRRSPHigh = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentRRSPHigh, computedInterest, YearlyAllocationContribution(currentResultRecord, "RRSP"));
                runningTotalInvestmentFHSAHigh = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentFHSAHigh, computedInterest, YearlyAllocationContribution(currentResultRecord, "FHSA"));
            }
            else if (bankAndDumpFlag == 1)
            {
                runningTotalInvestmentTFSAHigh = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentTFSAHigh, computedInterest, YearlyAllocationContribution(currentResultRecord, "TFSA")) - dumpAmount;
                runningTotalInvestmentRRSPHigh = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentRRSPHigh, computedInterest, YearlyAllocationContribution(currentResultRecord, "RRSP")) + dumpAmount;
                runningTotalInvestmentFHSAHigh = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentFHSAHigh, computedInterest, YearlyAllocationContribution(currentResultRecord, "FHSA"));

                //Dump Style
                currentResultRecord.AnnualCompoundedInvestmentHighTFSAStyle = "color: blue; font-weight: bold;";
                currentResultRecord.AnnualCompoundedInvestmentHighRRSPStyle = "color: blue; font-weight: bold;";
            }

            decimal totalInvestmentHigh = runningTotalInvestmentTFSAHigh + runningTotalInvestmentRRSPHigh;

            decimal tfsaPercentage = totalInvestmentHigh != 0
                                     ? runningTotalInvestmentTFSAHigh / totalInvestmentHigh
                                     : 1; // Default to 0 for tfsaPercentage

            decimal rrspPercentage = totalInvestmentHigh != 0
                                     ? runningTotalInvestmentRRSPHigh / totalInvestmentHigh
                                     : 1; // Default to 1 for rrspPercentage

            runningTotalInvestmentTFSAHigh -= runningTotalProjectedAnnualRetirementIncome * tfsaPercentage + yearlyInsuranceDeduct;  //You pay Insurance Premiums from TFSA not RRSP
            runningTotalInvestmentRRSPHigh -= runningTotalProjectedAnnualRetirementIncome * rrspPercentage;
            runningTotalInvestmentFHSAHigh -= 0;
            currentResultRecord.AnnualCompoundedInvestmentHighTFSA = runningTotalInvestmentTFSAHigh;
            currentResultRecord.AnnualCompoundedInvestmentHighRRSP = runningTotalInvestmentRRSPHigh;
            currentResultRecord.AnnualCompoundedInvestmentHighFHSA = runningTotalInvestmentFHSAHigh;

            currentResultRecord.AnnualCompoundedInvestmentHigh = currentResultRecord.AnnualCompoundedInvestmentHighTFSA + currentResultRecord.AnnualCompoundedInvestmentHighRRSP + currentResultRecord.AnnualCompoundedInvestmentHighFHSA;

            if (currentResultRecord.Age > currentResultRecord.RRIFAge)
            {
                currentResultRecord.AnnualCompoundedInvestmentHighStyle = "color: #7A4E61";
            }

            if (currentResultRecord.AnnualCompoundedInvestmentHigh < 0)
            {
                currentResultRecord.AnnualCompoundedInvestmentHighStyle = "color: Red";
                currentResultRecord.InitialInvestment += "Neg High Rate; ";
                currentResultRecord.InitialInvestmentStyle = "color: Red; font-weight: bold; font-size: x-small;";
                //result.IIFontSize = "x-small";
            }
            if (currentResultRecord.AnnualCompoundedInvestmentHighTFSA < 0)
            {
                currentResultRecord.AnnualCompoundedInvestmentHighTFSAStyle = "color: Red";
                currentResultRecord.InitialInvestment += "Neg TFSA High Rate; ";
                currentResultRecord.InitialInvestmentStyle = "color: Red; font-weight: bold; font-size: x-small;";
                //currentResultRecord.IIFontSize = "x-small";
            }
            if (currentResultRecord.AnnualCompoundedInvestmentHighRRSP < 0)
            {
                currentResultRecord.AnnualCompoundedInvestmentHighRRSPStyle = "color: Red";
                currentResultRecord.InitialInvestment += "Neg RRSP High Rate; ";
                currentResultRecord.InitialInvestmentStyle = "color: Red; font-weight: bold; font-size: x-small;";
                //currentResultRecord.IIFontSize = "x-small";
            }
        }

        /// <summary>Calculate Bank and Dump Strategy for Low Interest Rates.</summary>
        /// <param name="model">The model.</param>
        /// <param name="currentResultRecord">The current result record.</param>
        /// <param name="yearlyInsuranceDeduct">The yearly insurance deduct.</param>
        /// <param name="bankAndDumpFlag">The bank and dump flag.</param>
        /// <param name="dumpAmount">The dump amount.</param>
        /// <param name="runningTotalProjectedAnnualRetirementIncome">The running total projected annual retirement income.</param>
        private static void BankAndDumpLowInterest(FinancialStrategyModel model, FinancialResultsModel currentResultRecord, decimal yearlyInsuranceDeduct, int bankAndDumpFlag, decimal dumpAmount, decimal runningTotalProjectedAnnualRetirementIncome)
        {
            decimal computedInterest = model.AnnualInterestRateLow;
            if (currentResultRecord.Age > model.AgeToMoveToRRIF)
            {
                computedInterest = model.RRIFRate;
            }

            runningTotalInvestmentLow = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentLow, computedInterest, currentResultRecord.YearlyInvestmentContribution);
            if (bankAndDumpFlag == 0)
            {
                runningTotalInvestmentTFSALow = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentTFSALow, computedInterest, YearlyAllocationContribution(currentResultRecord, "TFSA"));
                runningTotalInvestmentRRSPLow = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentRRSPLow, computedInterest, YearlyAllocationContribution(currentResultRecord, "RRSP"));
                runningTotalInvestmentFHSALow = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentFHSALow, computedInterest, YearlyAllocationContribution(currentResultRecord, "FHSA"));
            }
            else if (bankAndDumpFlag == 1)
            {
                runningTotalInvestmentTFSALow = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentTFSALow, computedInterest, YearlyAllocationContribution(currentResultRecord, "TFSA")) - dumpAmount;
                runningTotalInvestmentRRSPLow = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentRRSPLow, computedInterest, YearlyAllocationContribution(currentResultRecord, "RRSP")) + dumpAmount;
                runningTotalInvestmentFHSALow = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentFHSALow, computedInterest, YearlyAllocationContribution(currentResultRecord, "FHSA"));

                //Dump Style
                currentResultRecord.AnnualCompoundedInvestmentLowTFSAStyle = "color: blue; font-weight: bold;";
                currentResultRecord.AnnualCompoundedInvestmentLowRRSPStyle = "color: blue; font-weight: bold;";

            }

            decimal totalInvestmentLow = runningTotalInvestmentTFSALow + runningTotalInvestmentRRSPLow;

            decimal tfsaPercentage = totalInvestmentLow != 0
                                     ? runningTotalInvestmentTFSALow / totalInvestmentLow
                                     : 1; // Default to 0 for tfsaPercentage

            decimal rrspPercentage = totalInvestmentLow != 0
                                     ? runningTotalInvestmentRRSPLow / totalInvestmentLow
                                     : 1; // Default to 1 for rrspPercentage

            runningTotalInvestmentTFSALow -= runningTotalProjectedAnnualRetirementIncome * tfsaPercentage + yearlyInsuranceDeduct;  //You pay Insurance Premiums from TFSA not RRSP
            runningTotalInvestmentRRSPLow -= runningTotalProjectedAnnualRetirementIncome * rrspPercentage;
            runningTotalInvestmentFHSALow -= 0;
            currentResultRecord.AnnualCompoundedInvestmentLowTFSA = runningTotalInvestmentTFSALow;
            currentResultRecord.AnnualCompoundedInvestmentLowRRSP = runningTotalInvestmentRRSPLow;
            currentResultRecord.AnnualCompoundedInvestmentLowFHSA = runningTotalInvestmentFHSALow;

            currentResultRecord.AnnualCompoundedInvestmentLow = currentResultRecord.AnnualCompoundedInvestmentLowTFSA + currentResultRecord.AnnualCompoundedInvestmentLowRRSP + currentResultRecord.AnnualCompoundedInvestmentLowFHSA;

            if (currentResultRecord.Age > currentResultRecord.RRIFAge)
            {
                currentResultRecord.AnnualCompoundedInvestmentLowStyle = "color: #7A4E61";
            }

            if (currentResultRecord.AnnualCompoundedInvestmentLow < 0)
            {
                currentResultRecord.AnnualCompoundedInvestmentLowStyle = "color: Red";
                currentResultRecord.InitialInvestment += "Neg Low Rate; ";
                currentResultRecord.InitialInvestmentStyle = "color: Red; font-weight: bold; font-size: x-small;";
                //result.IIFontSize = "x-small";
            }
            if (currentResultRecord.AnnualCompoundedInvestmentLowTFSA < 0)
            {
                currentResultRecord.AnnualCompoundedInvestmentLowTFSAStyle = "color: Red";
                currentResultRecord.InitialInvestment += "Neg TFSA Low Rate; ";
                currentResultRecord.InitialInvestmentStyle = "color: Red; font-weight: bold; font-size: x-small;";
                //currentResultRecord.IIFontSize = "x-small";
            }
            if (currentResultRecord.AnnualCompoundedInvestmentLowRRSP < 0)
            {
                currentResultRecord.AnnualCompoundedInvestmentLowRRSPStyle = "color: Red";
                currentResultRecord.InitialInvestment += "Neg RRSP Low Rate; ";
                currentResultRecord.InitialInvestmentStyle = "color: Red; font-weight: bold; font-size: x-small;";
                //currentResultRecord.IIFontSize = "x-small";
            }
        }

        /// <summary>Calculate Bank and Dump Strategy for Contributions &amp; Contribution Room (without Interest)</summary>
        /// <param name="model">The model.</param>
        /// <param name="currentResultRecord">The current result record.</param>
        /// <param name="yearlyInsuranceDeduct">The yearly insurance deduct.</param>
        /// <param name="bankAndDumpFlag">The bank and dump flag.</param>
        /// <param name="dumpAmount">The dump amount.</param>
        /// <param name="runningTotalProjectedAnnualRetirementIncome">The running total projected annual retirement income.</param>
        private static void BankAndDumpNoInterest(FinancialStrategyModel model, FinancialResultsModel currentResultRecord, decimal yearlyInsuranceDeduct, int bankAndDumpFlag, decimal dumpAmount, decimal runningTotalProjectedAnnualRetirementIncome)
        {
            //Annual Compounded Investment Running Total
            runningTotalInvestmentNoInterest = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentNoInterest, 0, currentResultRecord.YearlyInvestmentContribution);

            //Contribution Room Behaves OPPOSITE of the others
            FinancialResultsModel lastResult = GetPreviousRecordResultForContributionRoom(model);

            //Base Contributions & Contribution Room
            if (bankAndDumpFlag == 0)
            {
                //Base Contributions
                runningTotalInvestmentTFSANoInterest = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentTFSANoInterest, 0, YearlyAllocationContribution(currentResultRecord, "TFSA"));
                runningTotalInvestmentRRSPNoInterest = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentRRSPNoInterest, 0, YearlyAllocationContribution(currentResultRecord, "RRSP"));
                runningTotalInvestmentFHSANoInterest = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentFHSANoInterest, 0, YearlyAllocationContribution(currentResultRecord, "FHSA"));

                //Contribution Room (This year + last year)
                decimal currentTFSAContributionRoom = model.TFSARemainingContributionRoom + lastResult.TFSARemainingContributionRoom;
                runningTotalTFSARemainingContributionRoom = currentTFSAContributionRoom - YearlyAllocationContribution(currentResultRecord, "TFSA");
                if (currentResultRecord.Age < 71)
                {
                    decimal currentRRSPContributionRoom = currentResultRecord.AnnualIncome * model.RRSPAnnualRate + lastResult.RRSPRemainingContributionRoom;
                    runningTotalRRSPRemainingContributionRoom = currentRRSPContributionRoom - YearlyAllocationContribution(currentResultRecord, "RRSP");
                }
                decimal currentFHSAContributionRoom = model.FHSARemainingContributionRoom + lastResult.FHSARemainingContributionRoom;
                runningTotalFHSARemainingContributionRoom = currentFHSAContributionRoom - YearlyAllocationContribution(currentResultRecord, "FHSA");

                ProcessAdditionalDumpDue2NegTFSA(model, currentResultRecord, dumpAmount, lastResult);

            }
            else if (bankAndDumpFlag == 1)
            {
                if (currentResultRecord.Age > model.AgeToMoveToRRIF)
                {
                    //Set Amount to 0 because you can no longer dump to RRIF
                    dumpAmount = 0;
                }

                //Base Contributions
                runningTotalInvestmentTFSANoInterest = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentTFSANoInterest - dumpAmount, 0, YearlyAllocationContribution(currentResultRecord, "TFSA"));
                runningTotalInvestmentRRSPNoInterest = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentRRSPNoInterest + dumpAmount, 0, YearlyAllocationContribution(currentResultRecord, "RRSP"));
                runningTotalInvestmentFHSANoInterest = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentFHSANoInterest, 0, YearlyAllocationContribution(currentResultRecord, "FHSA"));

                //Contribution Room (dumpAmounts are opposite)
                decimal currentTFSAContributionRoom = model.TFSARemainingContributionRoom + lastResult.TFSARemainingContributionRoom;
                runningTotalTFSARemainingContributionRoom = currentTFSAContributionRoom - YearlyAllocationContribution(currentResultRecord, "TFSA") + dumpAmount;
                if (currentResultRecord.Age <= model.AgeToMoveToRRIF)
                {
                    decimal currentRRSPContributionRoom = currentResultRecord.AnnualIncome * model.RRSPAnnualRate + lastResult.RRSPRemainingContributionRoom;
                    runningTotalRRSPRemainingContributionRoom = currentRRSPContributionRoom - YearlyAllocationContribution(currentResultRecord, "RRSP") - dumpAmount;
                }
                decimal currentFHSAContributionRoom = model.FHSARemainingContributionRoom + lastResult.FHSARemainingContributionRoom;
                runningTotalFHSARemainingContributionRoom = currentFHSAContributionRoom - YearlyAllocationContribution(currentResultRecord, "FHSA");

                ProcessAdditionalDumpDue2NegTFSA(model, currentResultRecord, dumpAmount, lastResult);

                //Dump Style
                if (dumpAmount > 0)
                {
                    currentResultRecord.AnnualCompoundedInvestmentNoInterestTFSAStyle = "color: blue; font-weight: bold;";
                    currentResultRecord.AnnualCompoundedInvestmentNoInterestRRSPStyle = "color: blue; font-weight: bold;";
                }
            }

            #region SEND FINAL AMOUNTS BACK TO currentResultRecord (current Record)
            //SEND FINAL AMOUNTS BACK TO currentResultRecord (current Record) -=

            decimal totalInvestmentNoInterest = runningTotalInvestmentTFSANoInterest + runningTotalInvestmentRRSPNoInterest;

            decimal tfsaPercentage = totalInvestmentNoInterest != 0
                                     ? Math.Round(runningTotalInvestmentTFSANoInterest / totalInvestmentNoInterest, 1)
                                     : 1; // Default to 1 for tfsaPercentage

            decimal rrspPercentage = totalInvestmentNoInterest != 0
                                     ? Math.Round(runningTotalInvestmentRRSPNoInterest / totalInvestmentNoInterest, 1)
                                     : 1; // Default to 1 for rrspPercentage

            decimal remainder = 1 - tfsaPercentage - rrspPercentage;
            tfsaPercentage += remainder;

            runningTotalInvestmentTFSANoInterest -= runningTotalProjectedAnnualRetirementIncome * tfsaPercentage + yearlyInsuranceDeduct;  //You pay Insurance Premiums from TFSA not RRSP
            runningTotalInvestmentRRSPNoInterest -= runningTotalProjectedAnnualRetirementIncome * rrspPercentage;
            runningTotalInvestmentFHSANoInterest -= 0;
            currentResultRecord.AnnualCompoundedInvestmentNoInterestTFSA = runningTotalInvestmentTFSANoInterest;
            currentResultRecord.AnnualCompoundedInvestmentNoInterestRRSP = runningTotalInvestmentRRSPNoInterest;
            currentResultRecord.AnnualCompoundedInvestmentNoInterestFHSA = runningTotalInvestmentFHSANoInterest;
            currentResultRecord.AnnualCompoundedInvestmentNoInterest = currentResultRecord.AnnualCompoundedInvestmentNoInterestTFSA + currentResultRecord.AnnualCompoundedInvestmentNoInterestRRSP + currentResultRecord.AnnualCompoundedInvestmentNoInterestFHSA;
            currentResultRecord.RunningTotalInvestmentContributed = currentResultRecord.AnnualCompoundedInvestmentNoInterestTFSA + currentResultRecord.AnnualCompoundedInvestmentNoInterestRRSP + currentResultRecord.AnnualCompoundedInvestmentNoInterestFHSA; //This is the No Interest Column After Initial Investment

            //Opposite effect += here
            decimal totalTFSARRSP = runningTotalTFSARemainingContributionRoom + runningTotalRRSPRemainingContributionRoom > 0 ? runningTotalTFSARemainingContributionRoom + runningTotalRRSPRemainingContributionRoom : runningTotalTFSARemainingContributionRoom != 0 ? runningTotalTFSARemainingContributionRoom : 1;
            decimal tfsaRCPercentage = totalTFSARRSP != 0
                                       ? runningTotalTFSARemainingContributionRoom / totalTFSARRSP
                                       : 1; // Default to 1 for tfsaRCPercentage

            decimal rrsRCpPercentage = totalTFSARRSP != 0
                                       ? runningTotalRRSPRemainingContributionRoom / totalTFSARRSP
                                       : 1; // Default to 1 for rrsRCpPercentage

            runningTotalTFSARemainingContributionRoom += runningTotalProjectedAnnualRetirementIncome * tfsaRCPercentage - yearlyInsuranceDeduct;
            runningTotalRRSPRemainingContributionRoom += runningTotalProjectedAnnualRetirementIncome * rrsRCpPercentage;
            runningTotalFHSARemainingContributionRoom += 0;
            currentResultRecord.TFSARemainingContributionRoom = runningTotalTFSARemainingContributionRoom;
            currentResultRecord.RRSPRemainingContributionRoom = runningTotalRRSPRemainingContributionRoom;
            currentResultRecord.FHSARemainingContributionRoom = runningTotalFHSARemainingContributionRoom;

            //Override if Age above RRIF
            if (currentResultRecord.Age > model.AgeToMoveToRRIF)
            {
                runningTotalRRSPRemainingContributionRoom = 0;
                runningTotalFHSARemainingContributionRoom = 0;
                currentResultRecord.RRSPRemainingContributionRoom = runningTotalRRSPRemainingContributionRoom;
                currentResultRecord.FHSARemainingContributionRoom = runningTotalFHSARemainingContributionRoom;
                decimal maxTFSARemainingContributionRoom = (currentResultRecord.Age - model.Age) * model.TFSAAnnualMaximumum;
                if (runningTotalTFSARemainingContributionRoom > maxTFSARemainingContributionRoom)
                {
                    runningTotalTFSARemainingContributionRoom = maxTFSARemainingContributionRoom;
                    currentResultRecord.TFSARemainingContributionRoom = runningTotalTFSARemainingContributionRoom;
                }
            }

            #endregion

            //TAX SHIELDED Annual Compounded Investment Running Total
            //CPP
            if (currentResultRecord.Age > currentResultRecord.RRIFAge)
            {
                currentResultRecord.RunningTotalInvestmentContributedStyle = "color: #7A4E61";
            }

            //checking AnnualCompoundedInvestmentNoInterest but setting RunningTotalInvestmentContributedStyle
            if (currentResultRecord.AnnualCompoundedInvestmentNoInterest < 0)
            {
                currentResultRecord.RunningTotalInvestmentContributedStyle = "color: Brown";
                currentResultRecord.InitialInvestment += "Neg 0 Rate/Cont; ";
                currentResultRecord.InitialInvestmentStyle = "color: Brown; font-weight: bold; font-size: x-small;";
            }

            //TAX SHIELDED Running Total Contribution
            if (currentResultRecord.AnnualCompoundedInvestmentNoInterestTFSA < 0)
            {
                currentResultRecord.AnnualCompoundedInvestmentNoInterestTFSAStyle = "color: brown";
                currentResultRecord.InitialInvestment += "Neg TFSA Base Bal; ";
                currentResultRecord.InitialInvestmentStyle = "color: Brown; font-weight: bold; font-size: x-small;";
            }
            if (currentResultRecord.AnnualCompoundedInvestmentNoInterestRRSP < 0)
            {
                currentResultRecord.AnnualCompoundedInvestmentNoInterestRRSPStyle = "color: Brown";
                currentResultRecord.InitialInvestment += "Neg RRSP Base Bal; ";
                currentResultRecord.InitialInvestmentStyle = "color: Brown; font-weight: bold; font-size: x-small;";
            }

            if (currentResultRecord.RRSPRemainingContributionRoom < 0)
            {
                currentResultRecord.RRSPRemainingContributionRoomStyle = "color: Brown";
                currentResultRecord.InitialInvestment += "Neg RRSP Cont; ";
                currentResultRecord.InitialInvestmentStyle = "color: Brown; font-weight: bold; font-size: x-small;";
                currentResultRecord.IIFontSize = "x-small";
            }
            if (currentResultRecord.TFSARemainingContributionRoom < 0)
            {
                currentResultRecord.TFSARemainingContributionRoomStyle = "color: Brown";
                currentResultRecord.InitialInvestment += "Neg TFSA Cont; ";
                currentResultRecord.InitialInvestmentStyle = "color: Brown; font-weight: bold; font-size: x-small;";
                currentResultRecord.IIFontSize = "x-small";
            }
        }

        private static void ProcessAdditionalDumpDue2NegTFSA(FinancialStrategyModel model, FinancialResultsModel currentResultRecord, decimal dumpAmount, FinancialResultsModel lastResult)
        {
            if (currentResultRecord.Age <= model.AgeToMoveToRRIF)
            {

                //DUMP MORE Until 10
                int dumpCount = 1;
                while (dumpCount < 10 && runningTotalTFSARemainingContributionRoom < 0 && currentResultRecord.YearlyInvestmentContribution > 0)
                {
                    //Base Contributions
                    runningTotalInvestmentTFSANoInterest = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentTFSANoInterest - dumpAmount, 0, 0);
                    runningTotalInvestmentRRSPNoInterest = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentRRSPNoInterest + dumpAmount, 0, 0);
                    runningTotalInvestmentFHSANoInterest = CalculateIncDecInContributionsAndInterest(runningTotalInvestmentFHSANoInterest, 0, 0);

                    //Contribution Room (dumpAmounts are opposite)
                    decimal currentTFSAContributionRoom = model.TFSARemainingContributionRoom + lastResult.TFSARemainingContributionRoom;
                    runningTotalTFSARemainingContributionRoom = currentTFSAContributionRoom + dumpAmount;
                    if (currentResultRecord.Age <= model.AgeToMoveToRRIF)
                    {
                        decimal currentRRSPContributionRoom = currentResultRecord.AnnualIncome * model.RRSPAnnualRate + lastResult.RRSPRemainingContributionRoom;
                        runningTotalRRSPRemainingContributionRoom = currentRRSPContributionRoom - dumpAmount;
                    }
                    decimal currentFHSAContributionRoom = model.FHSARemainingContributionRoom + lastResult.FHSARemainingContributionRoom;
                    runningTotalFHSARemainingContributionRoom = currentFHSAContributionRoom;

                    //Dump Style
                    if (dumpAmount > 0)
                    {
                        currentResultRecord.AnnualCompoundedInvestmentNoInterestTFSAStyle = "color: blue; font-weight: bold;";
                        currentResultRecord.AnnualCompoundedInvestmentNoInterestRRSPStyle = "color: blue; font-weight: bold;";
                    }
                    dumpCount++;
                }
            }
        }

        /// <summary>Gets the previous record result for contribution room.</summary>
        /// <param name="model">The model.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        private static FinancialResultsModel GetPreviousRecordResultForContributionRoom(FinancialStrategyModel model)
        {
            if (FinancialResultsModel.Count > 0)
            {
                return FinancialResultsModel.LastOrDefault();
            }
            else
            {
                //For First Record Only
                return new FinancialResultsModel()
                {
                    TFSARemainingContributionRoom = model.TFSAStartingContributionRoom,
                    RRSPRemainingContributionRoom = model.RRSPStartingContributionRoom,
                    FHSARemainingContributionRoom = model.FHSAStartingContributionRoom
                };
            }
        }

        /// <summary>Computes the yearly allocation contribution.</summary>
        /// <param name="result">The result.</param>
        /// <param name="fundname">The fundname.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        private static decimal YearlyAllocationContribution(FinancialResultsModel result, string fundname)
        {
            decimal amount = 0;
            switch (fundname)
            {
                case "TFSA":
                    amount = result.YearlyInvestmentContribution;
                    break;
                case "RRSP":
                    amount = 0;
                    break;
                case "FHSA":
                    amount = 0;
                    break;
                default:
                    break;
            }

            return amount;
        }

        /// <summary>Common Method to calculate the Increase or Decrease in contributions and Investment Interest Earned (Future Value).</summary>
        /// <param name="runningTotalContribution">The running total contribution.</param>
        /// <param name="interestRateNumerator">The interest rate numerator.</param>
        /// <param name="yearlyContribution">The yearly contribution.</param>
        /// <returns>Compounded Returns<br /></returns>
        private static decimal CalculateIncDecInContributionsAndInterest(decimal runningTotalContribution, decimal interestRateNumerator, decimal yearlyContribution)
        {
            int years = 1;
            decimal monthlyContribution = yearlyContribution / 12;
            decimal interestRate = interestRateNumerator / 100;

            if (interestRate > 0)
            {
                // Calculate future value of initial contribution
                decimal futureValueInitial = runningTotalContribution * (decimal)Math.Pow((double)(1 + interestRate), years);

                // Calculate future value of monthly contributions
                decimal futureValueAnnuity = 0M;
                for (int month = 1; month <= 12; month++)
                {
                    // Calculate how many months are left until the end of the year for each contribution
                    int monthsUntilEndOfYear = 13 - month; //Should be 13 not 12 as last month should be compounded 1/12
                    futureValueAnnuity += monthlyContribution * (decimal)Math.Pow((double)(1 + interestRate), monthsUntilEndOfYear / 12.0);
                }

                // Total future value
                decimal totalFutureValue = futureValueInitial + futureValueAnnuity;

                return totalFutureValue;
            }
            else
            {
                runningTotalContribution += yearlyContribution;
                return runningTotalContribution;
            }
        }

        /// <summary>Computation for Free Insurance using YRT/ART Strategies Option</summary>
        /// <param name="model"></param>
        /// <param name="runningTotalInsurancePaid"></param>
        /// <param name="termLength"></param>
        /// <param name="age"></param>
        /// <param name="result"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        private static decimal CalculateFreeInsuranceStrategies(FinancialStrategyModel model, ref decimal runningTotalInsurancePaid, int termLength, int age, FinancialResultsModel result)
        {
            bool deductyearly = false;

            result.FaceAmount = model.FaceAmount;


            if (model != null && model.HasARTYRTFileLoaded && model.InsurancePlan == 1)
            {
                deductyearly = CalculateYRTARTInsurance(model, termLength, age, result);

            }
            else if (model != null && model.HasDRTFileLoaded && model.InsurancePlan == 2)
            {
                deductyearly = CalcualteDRTInsurance(model, termLength, age, result);

            }
            else if (model != null && model.HasDRTFileLoaded && model.InsurancePlan == 3)
            {
                deductyearly = CalcualteTenDRTInsurance(model, termLength, age, result);

            }

            if (!deductyearly && termLength >= model.TermLength)
            {
                result.MonthlyInsurancePremium = 0;
                result.Color = "Red";
                result.InitialInvestment += "No Insurance; ";
                result.InitialInvestmentStyle = "color: Red; font-weight: bold; font-size: x-small;";
                result.IIFontSize = "x-small";
                result.RRSPDumpStyle = "color: Red;";
                result.TFSADumpStyle = "color: Red;";
            }

            decimal yearlyInsuranceDeduct = 0;
            if (model.InvestmentPayInsurance && age <= 71 && deductyearly)
            {
                yearlyInsuranceDeduct = result.YearlyInsurancePremium;
                result.Color = "Blue";
                result.InitialInvestment += "Free Insurance; ";
                result.InitialInvestmentStyle = "color: Blue; font-weight: bold; font-size: x-small;";
                result.IIFontSize = "x-small";
                result.RRSPDumpStyle = "color: blue;";
                result.TFSADumpStyle = "color: blue;";
            }
            else if (model.InvestmentPayInsurance && age > 71)
            {
                yearlyInsuranceDeduct = result.YearlyInsurancePremium;
                result.Color = "Blue";
                result.InitialInvestment += "Free Insurance; ";
                result.InitialInvestmentStyle = "color: Blue; font-weight: bold; font-size: x-small;";
                result.IIFontSize = "x-small";
                result.RRSPDumpStyle = "color: blue;";
                result.TFSADumpStyle = "color: blue;";
            }
            else if (!model.InvestmentPayInsurance && deductyearly)
            {
                result.Color = "Brown";
                result.InitialInvestment += model.InsurancePlan == 1 ? "ART/YRT; " : model.InsurancePlan == 2 ? "DRT; " : "10FDRT; ";
                result.InitialInvestmentStyle = "color: Brown; font-weight: bold; font-size: x-small;";
                result.IIFontSize = "x-small";
                result.RRSPDumpStyle = "color: brown;";
                result.TFSADumpStyle = "color: brown;";
            }
            else if (result.Color != "Red")
            {
                result.Color = "Black";
                result.RRSPDumpStyle = "color: black;";
                result.TFSADumpStyle = "color: black;";
            }

            if (model.RefundPremiums && yearlyInsuranceDeduct > 0 && runningTotalInsurancePaid > 0)
            {
                yearlyInsuranceDeduct += runningTotalInsurancePaid;
            }

            runningTotalInsurancePaid += result.YearlyInsurancePremium - yearlyInsuranceDeduct;
            result.RunningTotalInsurancePaid = runningTotalInsurancePaid;
            return yearlyInsuranceDeduct;
        }

        /// <summary>Calculates the yrtart insurance.</summary>
        /// <param name="model">The model.</param>
        /// <param name="termLength">Length of the term.</param>
        /// <param name="age">The age.</param>
        /// <param name="result">The result.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        private static bool CalculateYRTARTInsurance(FinancialStrategyModel model, int termLength, int age, FinancialResultsModel result)
        {
            bool deductyearly = false;
            decimal GetRateForInsuranceType1YRTART(ARTYRTInsuranceData data)
            {
                return model.FaceAmount switch
                {
                    <= 149999 => data.SelectNTU149k,
                    <= 249999 => data.PreferredPlus150kto249k,
                    <= 499999 => data.PreferredPlus250kto499k,
                    <= 749999 => data.PreferredPlus500k,
                    _ => data.PreferredPlus500k
                };
            }

            decimal GetRateForInsuranceType2YRTART(ARTYRTInsuranceData data)
            {
                return model.FaceAmount switch
                {
                    <= 149999 => data.SelectNTU149k,
                    <= 249999 => data.SelectNTU150kto249k,
                    <= 499999 => data.SelectNTU250kto499k,
                    <= 749999 => data.SelectNTU500kto749k,
                    _ => data.SelectNTU750k
                };
            }

            decimal GetRateForInsuranceType3YRTART(ARTYRTInsuranceData data)
            {
                return model.FaceAmount switch
                {
                    <= 149999 => data.TakenUpTU149k,
                    <= 249999 => data.TakenUpTU150kto249k,
                    <= 499999 => data.TakenUpTU250kto499k,
                    <= 749999 => data.TakenUpTU500kto749k,
                    _ => data.TakenUpTU750k
                };
            }

            if (termLength >= model.TermLength && model.ContinueInvestment)
            {
                if (InsuranceARTYRTDataSettings.Count > 0)
                {
                    var insuranceDataForAge = InsuranceARTYRTDataSettings.FirstOrDefault(i => i.AgeRange.Contains(age.ToString()));
                    if (insuranceDataForAge != null)
                    {
                        decimal rate = 0;

                        var insuranceTypeMapping = new Dictionary<int, Func<ARTYRTInsuranceData, decimal>>
                        {
                            { 1, GetRateForInsuranceType1YRTART },
                            { 2, GetRateForInsuranceType2YRTART },
                            { 3, GetRateForInsuranceType3YRTART }
                        };

                        if (insuranceTypeMapping.ContainsKey(model.InsuranceType))
                        {
                            rate = insuranceTypeMapping[model.InsuranceType](insuranceDataForAge);
                        }

                        result.MonthlyInsurancePremium = rate * model.FaceAmount / 1000 / 12;
                        deductyearly = true;
                    }
                }
            }

            result.YearlyInsurancePremium = result.MonthlyInsurancePremium * 12;

            result.FaceAmount = model.FaceAmount;

            return deductyearly;
        }

        /// <summary>Calcualtes the DRT insurance.</summary>
        /// <param name="model">The model.</param>
        /// <param name="termLength">Length of the term.</param>
        /// <param name="age">The age.</param>
        /// <param name="result">The result.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        /// <exception cref="Exception">Age not found in the data.</exception>
        private static bool CalcualteDRTInsurance(FinancialStrategyModel model, int termLength, int age, FinancialResultsModel result)
        {
            bool deductyearly = false;
            if (termLength >= model.TermLength && model.ContinueInvestment)
            {
                var rowData = InsuranceDRTDataSettings.Find(row => row.AgeRange == age.ToString());

                if (rowData == null)
                {
                    throw new Exception("Age not found in the data.");
                }

                decimal coverageFor100Annual = model.FaceAmount switch
                {
                    <= 149999 => rowData.SelectNTU149k,
                    <= 249999 => rowData.SelectNTU150kto249k,
                    <= 499999 => rowData.SelectNTU250kto499k,
                    <= 749999 => rowData.SelectNTU500kto749k,
                    _ => rowData.SelectNTU750k,
                };

                // Calculate the premium for the given face amount
                decimal units = result.YearlyInsurancePremium / 100;
                result.FaceAmount = units * coverageFor100Annual;

                deductyearly = true;
            }

            return deductyearly;
        }

        /// <summary>Calcualtes the ten DRT insurance.</summary>
        /// <param name="model">The model.</param>
        /// <param name="termLength">Length of the term.</param>
        /// <param name="age">The age.</param>
        /// <param name="result">The result.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        /// <exception cref="Exception">Age not found in the data.</exception>
        private static bool CalcualteTenDRTInsurance(FinancialStrategyModel model, int termLength, int age, FinancialResultsModel result)
        {
            bool deductyearly = false;
            if (termLength >= model.TermLength && model.ContinueInvestment)
            {
                var rowData = InsuranceDRTDataSettings.Find(row => row.AgeRange == age.ToString());

                if (rowData == null)
                {
                    throw new Exception("Age not found in the data.");
                }

                decimal coverageFor100Annual = model.FaceAmount switch
                {
                    <= 149999 => rowData.SelectNTU149k,
                    <= 249999 => rowData.SelectNTU150kto249k,
                    <= 499999 => rowData.SelectNTU250kto499k,
                    <= 749999 => rowData.SelectNTU500kto749k,
                    _ => rowData.SelectNTU750k,
                };

                // Calculate the premium for the given face amount
                if (tenCountAfterTerm == 0)
                {
                    tenUnitsAfterTerm = result.FaceAmount / coverageFor100Annual;
                }

                tenCountAfterTerm++;
                if (tenCountAfterTerm > 10)
                {
                    result.FaceAmount = tenUnitsAfterTerm * coverageFor100Annual;
                }

                result.YearlyInsurancePremium = tenUnitsAfterTerm * 100;
                result.MonthlyInsurancePremium = result.YearlyInsurancePremium / 12;

                deductyearly = true;
            }

            return deductyearly;
        }

        /// <summary>
        ///   <br />
        /// </summary>
        /// <param name="model"></param>
        /// <param name="currentResultRecord"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        private static int SetupBankNDumpContributionOnly(FinancialStrategyModel model, FinancialResultsModel currentResultRecord)
        {
            decimal bankDumpAmount = model.AmountToBankInTFSA;

            //Not Enough Money so Bank
            if (runningTotalTFSAStrategicContribution <= model.AmountToBankInTFSA)
            {
                //Contribution Room Behaves OPPOSITE of the others
                ContributionRooms remainingContRooms = CalculateBaseRemainingContributionRoom(currentResultRecord, 1, bankDumpAmount);
                if (remainingContRooms.TFSARemainingContributionRoom >= bankDumpAmount)
                {
                    return 0;
                }
                else
                {
                    //No more TFS Contribution Room Do and Early Dump
                    return 1;
                }
            }
            else
            {
                //Contribution Room Behaves OPPOSITE of the others
                ContributionRooms remainingContRooms = CalculateBaseRemainingContributionRoom(currentResultRecord, 1, bankDumpAmount);

                if (remainingContRooms.RRSPRemainingContributionRoom >= 0)
                {
                    return 1;
                }
                else
                {
                    //No More RRSP Contribution Room Do a Late Dump
                    return 0;
                }
            }
        }
        #endregion
    }
}
