// -----------------------------------------------------------------------------
// <copyright file=”OctaneGherkinReportFileManager.cs" company=”Microfocus, Inc”>
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
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using NLog;

    public class OctaneGherkinReportFileManager
    {
        public String ResultsFileNamePostfix = "OctaneGherkinResults.xml";

        public String ResultsFolder = "gherkin-results";

        public String ErrorPrefix = "<HPEAlmOctaneGherkinFormatter Error>";

        public String XmlVersion = "1";

        private static readonly Logger log = LogManager.GetCurrentClassLogger();


        public OctaneGherkinReportFileManager()
        {
            this.OctaneSpecflowReport = new OctaneSpecflowFormat();
            this.OctaneSpecflowReport.Version = this.XmlVersion;
        }

        public OctaneSpecflowFormat OctaneSpecflowReport { get; set; }


        public FeatureElement CurrentFeature { get; set; }
        public ScenarioElement CurrentScenario { get; set; }


        public StepElement CurrentStep { get; set; }


        public static long GetStartUnixTime()
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((DateTime.UtcNow.ToUniversalTime() - epoch).TotalSeconds);
        }


        #region Writing xml content into file

        public void UpdateCurrentFeatureWithFileContent(string featureFilePath, string fullFeatureFilePath)
        {
            try
            {
                if (this.CurrentFeature != null && this.OctaneSpecflowReport != null)
                {
                    if (string.IsNullOrEmpty(this.CurrentFeature.Path))
                    {
                        var featureFileContent = string.Empty;

                        if (!string.IsNullOrEmpty(featureFilePath) & !string.IsNullOrEmpty(fullFeatureFilePath))
                        {
                            log.Debug("Read feature file " + fullFeatureFilePath);
                            featureFileContent = File.ReadAllText(fullFeatureFilePath);
                            log.Debug("File content: " + featureFileContent);
                        }

                        this.CurrentFeature.Path = featureFilePath;
                        this.CurrentFeature.FileContentString = featureFileContent;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn(ex);
            }
        }

        public void WriteToXml(string testResultsDirectory)
        {
            log.Trace(MethodBase.GetCurrentMethod().Name);
            log.Debug(this.OctaneSpecflowReport);
            try
            {
                //Create our own namespaces for the output
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

                //Add an empty namespace and empty value
                ns.Add("", "");
                var xmlSerializer = new XmlSerializer(typeof(OctaneSpecflowFormat));

                if (!string.IsNullOrEmpty(testResultsDirectory))
                {
                    log.Trace("Results directory: " + testResultsDirectory);

                    var fileName = string.Format("{0}\\{1}\\{2}", testResultsDirectory, this.ResultsFolder,
                        this.ResultsFileNamePostfix);
                    var resultsDir = Path.GetDirectoryName(fileName);
                    if (!Directory.Exists(resultsDir))
                    {
                        Directory.CreateDirectory(resultsDir);
                    }
                    log.Info("Saving results to file: " + fileName);

                    using (FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        xmlSerializer.Serialize(fs, this.OctaneSpecflowReport, ns);
                    }
                }
            }
            catch (IOException ex)
            {
                log.Error(ex);
                Console.WriteLine(this.ErrorPrefix + "IO exception: {0}", ex.Message);
            }
            catch (SerializationException ex)
            {
                log.Error(ex);
                Console.WriteLine(this.ErrorPrefix + "Serialization exception: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
                Console.WriteLine(this.ErrorPrefix + ex);
            }
        }

        #endregion
    }
}
