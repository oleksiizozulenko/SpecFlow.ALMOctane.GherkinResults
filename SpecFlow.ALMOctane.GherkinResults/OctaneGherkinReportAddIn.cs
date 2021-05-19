// -----------------------------------------------------------------------------
// <copyright file=”OctaneGherkinReportAddIn.cs" company=”Microfocus, Inc”>
// 
// Copyright (c) Microfocus, Inc. 2006-2017. All Rights Reserved.
// 
// This computer software is Licensed Material belonging to Microfocus.
// It is considered a trade secret and not to be used or divulged by parties
// who have not received written authorization from Microfocus.
// 
// This file and its contents are protected by United States and 
// International copyright laws. Unauthorized reproduction and/or 
// distribution of all or any portion of the code contained herein 
// is strictly prohibited and will result in severe civil and criminal 
// penalties. Any violations of this copyright will be prosecuted 
// to the fullest extent possible under law. 
//  
// THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE. 
// 
// </copyright>
// ----------------------------------------------------------------------------

using SpecFlow.ALMOctane.GherkinResults;

namespace SBM.OctaneGherkinResults.SpecFlowPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using NLog;
    using TechTalk.SpecFlow;
    using TechTalk.SpecFlow.Bindings;
    using TechTalk.SpecFlow.Bindings.Reflection;
    using TechTalk.SpecFlow.Tracing;

    [Binding]
    public class OctaneGherkinReportAddIn : ITestTracer
    {
        private static OctaneGherkinReportFileManager _reportFileInstance;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static OctaneGherkinReportFileManager ReportFileInstance
        {
            get
            {
                if (_reportFileInstance == null)
                {
                    _reportFileInstance = new OctaneGherkinReportFileManager();
                }
                return _reportFileInstance;
            }
        }

        public static string TestResultsDirectory
        {
            get
            {
                var testResultsDirectory = Path.GetDirectoryName(typeof(OctaneGherkinReportAddIn).Assembly.Location);

               
                return testResultsDirectory;
            }
            private set { }
        }


        #region Logging Hooks

        [BeforeTestRun(Order = -20000)]
        public static void BeforeTestRun()
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);

            //nothing to execute
        }


        [AfterTestRun(Order = 20000)]
        public static void AfterTestRun()
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
        }


        [BeforeFeature(Order = -20000)]
        public static void BeforeFeature()
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            if (FeatureContext.Current != null)
            {
                ReportFileInstance.CurrentFeature = new FeatureElement();
                ReportFileInstance.CurrentFeature.Started = GetStartUnixTime();
                ReportFileInstance.CurrentFeature.Name = FeatureContext.Current.FeatureInfo.Title;
                var tidTag = FeatureContext.Current.FeatureInfo.Tags.FirstOrDefault(tag => tag.StartsWith("@TID"));
                ReportFileInstance.CurrentFeature.Tag = string.IsNullOrEmpty(tidTag) ? string.Empty : tidTag;
                   
                ReportFileInstance.OctaneSpecflowReport.Features.Add(ReportFileInstance.CurrentFeature);

                ReportFileInstance.UpdateCurrentFeatureWithFileContent(FeatureFilePath, GetFullFeaturePath(FeatureFilePath));
                ReportFileInstance.WriteToXml(TestResultsDirectory);
            }
        }

        private static string GetFullFeaturePath(string featureFilePath)
        {
            string fullFeatureFilePath = string.Empty;

            if (!string.IsNullOrEmpty(featureFilePath))
            {
                var assemblyLocation = Path.GetDirectoryName(Assembly.GetAssembly(typeof(OctaneGherkinReportAddIn)).Location);
                var generationRoot = new FileInfo(new FileInfo(assemblyLocation).DirectoryName).DirectoryName
                                     + Path.DirectorySeparatorChar;


                fullFeatureFilePath = generationRoot + Path.DirectorySeparatorChar + featureFilePath;
            }
            return fullFeatureFilePath;
        }

        public static string FeatureFilePath
        {
            get
            {
                var featureFilePath = string.Empty;
                if (FeatureContext.Current != null)
                {
                    if (FeatureContext.Current.ContainsKey("FeaturePath"))
                    {
                        featureFilePath = FeatureContext.Current.Get<string>("FeaturePath");
                    }
                }
                return featureFilePath;
            }
            private set { }
        }


        private static long GetStartUnixTime()
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((DateTime.UtcNow.ToUniversalTime() - epoch).TotalSeconds);
        }

        [AfterFeature(Order = 20000)]
        public static void AfterFeature()
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            if (ReportFileInstance.CurrentFeature != null)
            {
                ReportFileInstance.UpdateCurrentFeatureWithFileContent(FeatureFilePath, GetFullFeaturePath(FeatureFilePath));
                ReportFileInstance.WriteToXml(TestResultsDirectory);
                ReportFileInstance.CurrentFeature = null;
            }
        }

        [BeforeScenario(Order = -20000)]
        public void BeforeScenario()
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            if (ReportFileInstance.CurrentFeature != null && ScenarioContext.Current != null)
            {
                ReportFileInstance.CurrentScenario = new ScenarioElement();

                ReportFileInstance.CurrentScenario.Name = ScenarioContext.Current.ScenarioInfo.Title;

                ReportFileInstance.CurrentFeature.Scenarios.Add(ReportFileInstance.CurrentScenario);
                if (ReportFileInstance.OctaneSpecflowReport.Features.Count == 0)
                {
                    ReportFileInstance.OctaneSpecflowReport.Features.Add(ReportFileInstance.CurrentFeature);
                }
                ReportFileInstance.UpdateCurrentFeatureWithFileContent(FeatureFilePath, GetFullFeaturePath(FeatureFilePath));
                ReportFileInstance.WriteToXml(TestResultsDirectory);
            }
        }


        [AfterScenario(Order = 20000)]
        public void AfterScenario()
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            if (ReportFileInstance.CurrentScenario != null)
            {
                ReportFileInstance.UpdateCurrentFeatureWithFileContent(FeatureFilePath, GetFullFeaturePath(FeatureFilePath));
                ReportFileInstance.WriteToXml(TestResultsDirectory);
                ReportFileInstance.CurrentScenario = null;
            }
        }


        public void TraceStep(StepInstance stepInstance, bool showAdditionalArguments)
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            if (ReportFileInstance.CurrentScenario != null)
            {
                ReportFileInstance.CurrentStep = new StepElement();
                ReportFileInstance.CurrentStep.Status = TestResultStatus.Started;


                string stepTitle = String.Empty;

                stepTitle += stepInstance.Keyword + " " + stepInstance.Text;

                if (stepInstance.MultilineTextArgument != null)
                {
                    stepTitle += stepInstance.MultilineTextArgument;
                }

                ReportFileInstance.CurrentStep.Name = stepTitle;
                ReportFileInstance.CurrentScenario.Steps.Add(ReportFileInstance.CurrentStep);

                ReportFileInstance.WriteToXml(TestResultsDirectory);
            }
        }

        public void TraceWarning(string text)
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            if (ReportFileInstance.CurrentStep != null)
            {
                ReportFileInstance.CurrentStep.Status = TestResultStatus.Passed;
                ReportFileInstance.CurrentStep.ErrorMessageString = text;

                ReportFileInstance.WriteToXml(TestResultsDirectory);
            }
        }

        public void TraceStepDone(BindingMatch match, object[] arguments, TimeSpan duration)
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            if (ReportFileInstance.CurrentStep != null)
            {
                ReportFileInstance.CurrentStep.Status = TestResultStatus.Passed;
                ReportFileInstance.CurrentStep.Duration = duration.Ticks * 100;
                //
                ReportFileInstance.WriteToXml(TestResultsDirectory);
            }
            ReportFileInstance.CurrentStep = null;
        }

        public void TraceStepSkipped()
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            if (ReportFileInstance.CurrentStep != null)
            {
                ReportFileInstance.CurrentStep.Status = TestResultStatus.Skipped;
                //
                ReportFileInstance.WriteToXml(TestResultsDirectory);
            }
            ReportFileInstance.CurrentStep = null;
        }

        public void TraceStepPending(BindingMatch match, object[] arguments)
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            if (ReportFileInstance.CurrentStep != null)
            {
                ReportFileInstance.CurrentStep.Status = TestResultStatus.Skipped;
                ReportFileInstance.CurrentStep.ErrorMessageString =
                    "One or more step definitions are not implemented yet.\r"
                    + match.StepBinding.Method.Name + "(" + ")";

                //
                ReportFileInstance.WriteToXml(TestResultsDirectory);
            }
            ReportFileInstance.CurrentStep = null;
        }

        public void TraceBindingError(BindingException ex)
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            if (ReportFileInstance.CurrentStep != null)
            {
                ReportFileInstance.CurrentStep.Status = TestResultStatus.Skipped;
                ReportFileInstance.CurrentStep.ErrorMessageString = ex.ToString();
                //
                ReportFileInstance.WriteToXml(TestResultsDirectory);
            }
            ReportFileInstance.CurrentStep = null;
        }

        public void TraceError(Exception ex)
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            if (ReportFileInstance.CurrentStep != null)
            {
                ReportFileInstance.CurrentStep.Status = TestResultStatus.Failed;
                ReportFileInstance.CurrentStep.ErrorMessageString = ex.ToString();

                //
                ReportFileInstance.WriteToXml(TestResultsDirectory);
            }
        }

        public void TraceNoMatchingStepDefinition(StepInstance stepInstance, ProgrammingLanguage targetLanguage,
            CultureInfo bindingCulture, List<BindingMatch> matchesWithoutScopeCheck)
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            if (ReportFileInstance.CurrentStep != null)
            {
                ReportFileInstance.CurrentStep.Status = TestResultStatus.Failed;
                ReportFileInstance.CurrentStep.ErrorMessageString = stepInstance.ToString();


                //
                ReportFileInstance.WriteToXml(TestResultsDirectory);
            }
        }

        public void TraceDuration(TimeSpan elapsed, IBindingMethod method, object[] arguments)
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            if (ReportFileInstance.CurrentStep != null)
            {
                ReportFileInstance.CurrentStep.Duration = elapsed.Ticks * 100;
                //
                ReportFileInstance.WriteToXml(TestResultsDirectory);
            }
        }

        public void TraceDuration(TimeSpan elapsed, string text)
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            if (ReportFileInstance.CurrentStep != null)
            {
                ReportFileInstance.CurrentStep.Duration = elapsed.Ticks * 100;
                //
                ReportFileInstance.WriteToXml(TestResultsDirectory);
            }
        }

        #endregion
    }
}
