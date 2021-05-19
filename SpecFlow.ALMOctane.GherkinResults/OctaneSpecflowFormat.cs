// -----------------------------------------------------------------------------
// <copyright file=”OctaneSpecflowFormat.cs" company=”Microfocus, Inc”>
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace SpecFlow.ALMOctane.GherkinResults
{
    [Serializable]
    [XmlRoot("features")]
    public class OctaneSpecflowFormat
    {
        public OctaneSpecflowFormat()
        {
            this.Features = new List<FeatureElement>();
            this.Version = "1";
        }
        [XmlElement("feature")]
        public List<FeatureElement> Features { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        public override string ToString()
        {
            return string.Format("Features: [{0}], Version: {1}", string.Join(", ",this.Features), this.Version);
        }
    }

    [Serializable]
    public class FeatureElement
    {
        public override string ToString()
        {
            return string.Format("Scenarios: [{0}], FileContentString: {1}, Name: {2}, Path: {3}, Started: {4}, Tag: {5}", string.Join(", ", this.Scenarios), this.FileContentString, this.Name, this.Path, this.Started, this.Tag);
        }

        public FeatureElement()
        {
            this.Scenarios = new List<ScenarioElement>();
        }

        [XmlArray("scenarios", Order = 2)]
        [XmlArrayItem("scenario", IsNullable = false)]
        public List<ScenarioElement> Scenarios { get; set; }


        private string _FileContent;
        
        [XmlIgnore]
        public string FileContentString
        {
            get { return this._FileContent; }
            set { this._FileContent = value; }
        }

        [XmlElement("file", Order = 1)]
        public XmlNode[] FileContent {  get
            {
                if (this.FileContentString == null) return null;

                var dummy = new XmlDocument();
                return new XmlNode[] { dummy.CreateCDataSection(this.FileContentString) };
            }
            set
            {
                if (value == null)
                {
                    this.FileContentString = null;
                    return;
                }

                this.FileContentString = string.Join("\r\n", value.Select(v => v.Value));
            }}

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("path")]
        public string Path { get; set; }

        [XmlAttribute("started")]
        public long Started { get; set; }

        [XmlAttribute("tag")]
        public string Tag { get; set; }
    }

    [Serializable]
    [XmlRoot("background")]
    public class BackgroundElement : ScenarioElement
    {
    }

    [Serializable]
    [XmlRoot("scenario")]
    public class ScenarioElement
    {
        public override string ToString()
        {
            return string.Format("Steps: [{0}], Name: {1}, OutlineIndex: {2}", string.Join(", ",this.Steps), this.Name, this.OutlineIndex);
        }

        public ScenarioElement()
        {
            this.Steps = new List<StepElement>();
        }

        [XmlArray("steps")]
        [XmlArrayItem("step", IsNullable = false)]
        public List<StepElement> Steps { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("outlineIndex")]
        public string OutlineIndex { get; set; }
    }

    [Serializable]
    public class StepElement
    {
        public override string ToString()
        {
            return string.Format("Name: {0}, Duration: {1}, StatusString: {2}, ErrorMessageString: {3}", this.Name, this.Duration, this.StatusString, this.ErrorMessageString);
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("duration")]
        public long Duration { get; set; }

        [XmlIgnore]
        public TestResultStatus Status { get; set; }

        [XmlAttribute("status")]
        public string StatusString
        {
            get { return this.Status.ToString(); }
            set { this.Status = (TestResultStatus) Enum.Parse(typeof(TestResultStatus), value, true); }
        }

        [XmlIgnore]
        public string ErrorMessageString { get; set; }

        [XmlElement("error_message")]
        public XmlNode[] ErrorMessage
        {
            get
            {
                if (this.ErrorMessageString == null) return null;

                var dummy = new XmlDocument();
                return new XmlNode[] { dummy.CreateCDataSection(this.ErrorMessageString) };
            }
            set
            {
                if (value == null)
                {
                    this.ErrorMessageString = null;
                    return;
                }

                this.ErrorMessageString = string.Join("\r\n", value.Select(v => v.Value));
            }
        }
    }

    [Serializable]
    public enum TestResultStatus
    {
      Passed,Skipped,Failed,
        Started
    }
}
