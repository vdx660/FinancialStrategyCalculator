using CommunityToolkit.Maui.Alerts;
using FinancialStrategyCalculator.Data;
using FinancialStrategyCalculator.Services;
using FinancialStrategyCalculator.Shared;
using Microsoft.JSInterop;
using System.Data;
using System.Text;

namespace FinancialStrategyCalculator.Pages
{
    public partial class Settings
    {
        private FinancialStrategyModel financialStrategyModel { get; set; }

        public IEnumerable<DropdownValue> CPPPayoutTypes { get; set; }

        public string HtmlContentCPPContribution { get; set; }
        public bool IsCPPContributionDisabled { get; set; }

        public string HtmlContentCPPPayout { get; set; }
        public bool IsCPPPayoutDisabled { get; set; }

        public string HtmlContentCPPID { get; set; }
        public bool IsCPPIDDisabled { get; set; }

        public string HtmlContentOAS { get; set; }
        public bool IsOASDisabled { get; set; }

        private List<string> ParseRadzenFileInputFinancialResultsModel(string fileFinancialResultsModel)
        {
            // Check if it's a valid data URI
            if (!fileFinancialResultsModel.StartsWith("data:"))
            {
                // Handle error: not a valid data URI
                return null;
            }

            // Find the base64 delimiter; the actual content starts after this
            int base64DelimiterIndex = fileFinancialResultsModel.IndexOf("base64,");
            if (base64DelimiterIndex < 0)
            {
                // Handle error: not a valid base64 encoded data URI
                return null;
            }

            // Extract the base64 part from the data URI
            var base64Content = fileFinancialResultsModel.Substring(base64DelimiterIndex + 7); // 7 is the length of "base64,"

            // Decode the base64 string into a regular string
            var decodedContent = Encoding.UTF8.GetString(Convert.FromBase64String(base64Content));

            // Split the content into lines and add to a List
            List<string> csvLines = decodedContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return csvLines;
        }


        protected override void OnInitialized()
        {
            CPPPayoutTypes = new[] {
                new DropdownValue { Id = 1, Text = "Average CPP Benefits Payout" },
                new DropdownValue { Id = 2, Text = "Maximum CPP Benefits Payout" },
            };
            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;
        }

        private async void OnARTYRTFileChange(string fileFinancialResultsModel)
        {
            try
            {
                if (!string.IsNullOrEmpty(fileFinancialResultsModel))
                {
                    List<string> csvLines = ParseRadzenFileInputFinancialResultsModel(fileFinancialResultsModel);
                    string filePath = Utility.ReturnDeviceLocalPathForARTYRT();

                    string csv = string.Join(Environment.NewLine, csvLines);

                    if (csvLines.Count > 0)
                    {
                        try
                        {
                            File.WriteAllText(filePath, csv);
                            await Toast.Make($"The ART/YRT Custom Exchange Insurance Data was saved successfully!").Show();
                        }
                        catch (Exception ex)
                        {
                            await Toast.Make($"The ART/YRT Custom Exchange Insurance Data was not saved successfully with error: {ex.Message}").Show();
                        }
                    }
                    StateHasChanged();
                }
                else
                {
                    await Toast.Make($"The ART/YRT Insurance Table NOT Loaded!").Show();
                }
            }
            catch (Exception ex)
            {
                await Toast.Make($"Error in !" + ex.Message).Show();
            }

        }

        private async void OnDRTFileChange(string fileFinancialResultsModel)
        {
            try
            {
                if (!string.IsNullOrEmpty(fileFinancialResultsModel))
                {
                    List<string> csvLines = ParseRadzenFileInputFinancialResultsModel(fileFinancialResultsModel);
                    string filePath = Utility.ReturnDeviceLocalPathForDRT();

                    string csv = string.Join(Environment.NewLine, csvLines);

                    if (csvLines.Count > 0)
                    {
                        try
                        {
                            File.WriteAllText(filePath, csv);
                            await Toast.Make($"The DRT Custom Exchange Insurance Data was saved successfully!").Show();
                        }
                        catch (Exception ex)
                        {
                            await Toast.Make($"The DRT Custom Exchange Insurance Data was not saved successfully with error: {ex.Message}").Show();
                        }
                    }
                    StateHasChanged();
                }
                else
                {
                    await Toast.Make($"The ART/YRT Insurance Table NOT Loaded!").Show();
                }
            }
            catch (Exception ex)
            {
                await Toast.Make($"Error in !" + ex.Message).Show();
            }

        }

        private async void OnTFSARRSPFHSAFileChange(string fileFinancialResultsModel)
        {
            try
            {
                if (!string.IsNullOrEmpty(fileFinancialResultsModel))
                {
                    List<string> csvLines = ParseRadzenFileInputFinancialResultsModel(fileFinancialResultsModel);
                    string filePath = Utility.ReturnDeviceLocalPathForTFSARRSPFHSA();

                    string csv = string.Join(Environment.NewLine, csvLines);

                    if (csvLines.Count > 0)
                    {
                        try
                        {
                            File.WriteAllText(filePath, csv);
                            await Toast.Make($"The TFSA/RRSP/FHSA Data was saved successfully!").Show();
                        }
                        catch (Exception ex)
                        {
                            await Toast.Make($"The TFSA/RRSP/FHSA Data was not saved successfully with error: {ex.Message}").Show();
                        }
                    }
                    StateHasChanged();
                }
                else
                {
                    await Toast.Make($"The TFSA/RRSP/FHSA NOT Loaded!").Show();
                }
            }
            catch (Exception ex)
            {
                await Toast.Make($"Error in !" + ex.Message).Show();
            }

        }

        private async void GetCPPContributionFromFedCAD()
        {
            IsCPPContributionDisabled = true;
            StateHasChanged();
            await Task.Delay(100);

            KeyValuePair<string, List<CPPContribution>> result = await Utility.ReadCPPContributionFromFederalWebsiteAsync();
            HtmlContentCPPContribution = result.Key;

            if (result.Value != null && result.Value.Count > 0)
            {
                string localFilePath = Utility.ReturnDeviceLocalPathForCPPContributions();

                using (var writer = new StreamWriter(localFilePath))
                {
                    writer.WriteLine("Year,Maximum Annual Pensionable Earnings,Basic Exemption Amount,Maximum Contributory Earnings,Contribution Rate,Maximum Annual Employee and Employer Contribution,Maximum Annual Self-Employed Contribution");

                    foreach (var contribution in result.Value)
                    {
                        writer.WriteLine($"{contribution.Year},{contribution.MaximumAnnualPensionableEarnings},{contribution.BasicExemptionAmount},{contribution.MaximumContributoryEarnings},{contribution.ContributionRate},{contribution.MaximumAnnualEmployeeAndEmployerContribution},{contribution.MaximumAnnualSelfEmployedContribution}");
                    }
                }

                Utility.CPPContributionExisting = 4;
                Utility.ReadCPPContributionFromLocalStorage();
                StateHasChanged();
            }
            else
            {
                await Toast.Make($"The CPP Annual Contribution NOT Loaded!").Show();
            }

            IsCPPContributionDisabled = false;
            StateHasChanged();
        }

        private async void GetCPPBenefitPayoutFromFedCAD()
        {
            IsCPPPayoutDisabled = true;
            StateHasChanged();
            await Task.Delay(50);

            KeyValuePair<string, List<string[]>> result = await Utility.ReadCPPBenefitPayoutFromFederalWebsiteAsync();
            HtmlContentCPPPayout = result.Key;

            if (result.Value != null && result.Value.Count > 0)
            {
                List<string> csvLines = result.Value.Select(lineArray => string.Join(",", lineArray)).ToList();
                string filePath = Utility.ReturnDeviceLocalPathForCPPRetirement();

                string csv = string.Join(Environment.NewLine, csvLines);

                if (csvLines.Count > 0)
                {
                    try
                    {
                        File.WriteAllText(filePath, csv);
                        await Toast.Make($"The CPP Annual Contribution Data was saved successfully!").Show();
                    }
                    catch (Exception ex)
                    {
                        await Toast.Make($"The CPP Annual Contribution Data was not saved successfully with error: {ex.Message}").Show();
                    }
                }
                Utility.CPPBenefitsExisting = 4;
                Utility.ReadCPPBenefitsFromLocalStorage();
                StateHasChanged();
            }
            else
            {
                await Toast.Make($"The CPP Annual Contribution NOT Loaded!").Show();
            }

            IsCPPPayoutDisabled = false;
            StateHasChanged();
        }

        private async void GetCPPIncDecFormulaFromFedCAD()
        {
            IsCPPIDDisabled = true;
            StateHasChanged();
            await Task.Delay(50);

            KeyValuePair<string, CPPPayoutRule> result = await Utility.GetCPPIncreaseDecreaseComputation();
            HtmlContentCPPID = result.Key;
            CPPPayoutRule rule = result.Value;
            FinancialDataService.CPPPayoutRule = rule;

            string localFilePath = Utility.ReturnDeviceLocalPathForCPPIncreaseDecrease();
            using (var writer = new StreamWriter(localFilePath))
            {
                writer.WriteLine("PercentageDecrease,PercentageIncrease,MinimumAge,MaximumAge,MaximumReduction,MaximumIncrease");
                writer.WriteLine($"{rule.PercentageDecrease},{rule.PercentageIncrease},{rule.MinimumAge},{rule.MaximumAge},{rule.MaximumReduction},{rule.MaximumIncrease}");
            }

            Utility.CPPIncDecExisting = 4;
            Utility.ReadCPPIncreaseDecreaseFromLocalStorage();

            IsCPPIDDisabled = false;
            StateHasChanged();
        }

        private async void GetOASBenefitPayoutFromFedCAD()
        {
            IsOASDisabled = true;
            StateHasChanged();
            await Task.Delay(50);

            KeyValuePair<string, List<OASCalculation>> results = await Utility.GetOASBenefitPayoutFromFederalWebsiteAsync();
            HtmlContentOAS = results.Key;
            List<OASCalculation> oasResults = results.Value;

            string localFilePath = Utility.ReturnDeviceLocalPathForOASBenefits();
            using (var writer = new StreamWriter(localFilePath))
            {
                // Write the CSV header
                writer.WriteLine("Your Situation,Annual Net Income,Maximum Monthly Payment");

                foreach (var result in oasResults)
                {
                    // Escape and format values as needed (e.g., handle commas, quotes, etc.)
                    string yourSituation = Utility.EscapeAndFormatCsvValue(result.YourSituation);
                    string annualNetIncome = Utility.EscapeAndFormatCsvValue(result.AnnualNetIncome.ToString()); // Format as currency
                    string maximumMonthlyPayment = Utility.EscapeAndFormatCsvValue(result.MaximumMonthlyPayment.ToString()); // Format as currency

                    // Write the CSV data
                    writer.WriteLine($"{yourSituation},{annualNetIncome},{maximumMonthlyPayment}");
                }
            }
            Utility.OASBenefitExisting = 4;
            Utility.ReadOASBenefitsFromLocalStorage();

            IsOASDisabled = false;
            StateHasChanged();
        }

        private async Task ZoomIn()
        {
            await JSRuntime.InvokeVoidAsync("zoomFunctions.zoomIn");
            Utility.ScreenWidth = await JSRuntime.InvokeAsync<decimal>("getScreenWidth");
            Utility.ScreenHeight = await JSRuntime.InvokeAsync<decimal>("getScreenHeight");
        }

        private async Task ZoomOut()
        {
            await JSRuntime.InvokeVoidAsync("zoomFunctions.zoomOut");
            Utility.ScreenWidth = await JSRuntime.InvokeAsync<decimal>("getScreenWidth");
            Utility.ScreenHeight = await JSRuntime.InvokeAsync<decimal>("getScreenHeight");
        }

    }
}
