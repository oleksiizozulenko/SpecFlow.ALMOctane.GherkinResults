// -----------------------------------------------------------------------------
// <copyright file=”OctaneGerkinResultsPlugin.cs" company=”Microfocus, Inc”>
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

using ALMOctane.GherkinResults.SpecFlowPlugin;
using SBM.OctaneGherkinResults.SpecFlowPlugin;
using TechTalk.SpecFlow.Plugins;
using TechTalk.SpecFlow.Tracing;
using TechTalk.SpecFlow.UnitTestProvider;

[assembly: RuntimePlugin(typeof(OctaneGerkinResultsPlugin))]

namespace ALMOctane.GherkinResults.SpecFlowPlugin
{
    public class OctaneGerkinResultsPlugin : IRuntimePlugin
    {

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters,
            UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeTestThreadDependencies += (sender, e) =>
            {
                e.ObjectContainer.RegisterTypeAs<OctaneGherkinReportAddIn, ITestTracer>();
            };
        }
    }
}
