using FinancialStrategyCalculator.Data;
using FinancialStrategyCalculator.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
//using Network;
using Radzen.Blazor;
//using PdfSharp;
//using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace FinancialStrategyCalculator.Shared
{
    public partial class PlanDetailsView
    {
        #region Fields
        private List<ARTYRTInsuranceData> insuranceARTYRTDataSettings = new List<ARTYRTInsuranceData>();
        private List<TFSARRSPFHSAData> tfsaRRSPFHSADataSettings = new List<TFSARRSPFHSAData>();

        //Boostrap Icons HTML
        const string constForwardButton = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-caret-right-fill\" viewBox=\"0 0 16 16\">  <path d=\"m12.14 8.753-5.482 4.796c-.646.566-1.658.106-1.658-.753V3.204a1 1 0 0 1 1.659-.753l5.48 4.796a1 1 0 0 1 0 1.506z\"/></svg>";
        const string constBackwardButton = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-caret-left-fill\" viewBox=\"0 0 16 16\">  <path d=\"m3.86 8.753 5.482 4.796c.646.566 1.658.106 1.658-.753V3.204a1 1 0 0 0-1.659-.753l-5.48 4.796a1 1 0 0 0 0 1.506z\"/></svg>";

        private bool renderState = false;
        private RadzenButton toggleButton;
        private MarkupString toggleButtonText = new MarkupString(constForwardButton);
        private bool renderInsState = false;
        private RadzenButton yearlyInsToggleButton;
        private MarkupString yearlyInsToggleButtonText = new MarkupString(constForwardButton);
        private bool renderInvState = false;
        private RadzenButton yearlyInvToggleButton;
        private MarkupString yearlyInvToggleButtonText = new MarkupString(constForwardButton);

        private MarkupString popOutInButton = new(Utility.PopOutInText);

        #endregion

        #region Methods
        public void RefreshContent()
        {
            // Logic to refresh the content of the component
            // For instance, you might fetch data again or reset certain properties.
            StateHasChanged(); // This will request the component to re-render.
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            Utility.EditMode = false;
            Utility.SubmitDisabledState = false;

            StateHasChanged();

            InitializeRenderState();

            //Submit for Computation
            Utility.ErroMessages = Utility.SubmitNext(FinancialDataService.FinancialStrategyModel.AnnualIncreaseOnMonthlyContributionsStr, FinancialDataService.FinancialStrategyModel.AgeToStopIncreaseStr, "", 0);

        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (renderState)
            {
                //Collapse Columns by default when entering into the page
                await JSRuntime.InvokeVoidAsync("collapseColumn", "columnClassToToggle");
            }

            if (renderInsState)
            {
                //Collapse Columns by default when entering into the page
                await JSRuntime.InvokeVoidAsync("collapseColumn", "columnInsClassToToggle");
            }

            if (renderInvState)
            {
                //Collapse Columns by default when entering into the page
                await JSRuntime.InvokeVoidAsync("collapseColumn", "columnInvClassToToggle");
            }
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

        private async Task ToggleColumns()
        {
            renderState = false;

            if (toggleButtonText.Equals(new MarkupString(constForwardButton)))      //>>
            {
                toggleButtonText = new MarkupString(constBackwardButton);       //<<
            }
            else
            {
                toggleButtonText = new MarkupString(constForwardButton);
            }
            await JSRuntime.InvokeVoidAsync("toggleColumn", "columnClassToToggle");

        }

        private async Task yearlyInsToggleColumns()
        {
            renderInsState = false;

            if (yearlyInsToggleButtonText.Equals(new MarkupString(constForwardButton)))
            {
                yearlyInsToggleButtonText = new MarkupString(constBackwardButton);
            }
            else
            {
                yearlyInsToggleButtonText = new MarkupString(constForwardButton);
            }
            await JSRuntime.InvokeVoidAsync("toggleColumn", "columnInsClassToToggle");
        }

        private async Task yearlyInvToggleColumns()
        {
            renderInvState = false;

            if (yearlyInvToggleButtonText.Equals(new MarkupString(constForwardButton)))
            {
                yearlyInvToggleButtonText = new MarkupString(constBackwardButton);
            }
            else
            {
                yearlyInvToggleButtonText = new MarkupString(constForwardButton);
            }
            await JSRuntime.InvokeVoidAsync("toggleColumn", "columnInvClassToToggle");
        }

        #endregion
    }
}
