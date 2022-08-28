﻿using PlanScoreCard.Events.HelperWindows;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;

namespace PlanScoreCard.ViewModels.VMHelpers
{
    public class EULAViewModel : BindableBase
    {
        private IEventAggregator _eventAggregator;

        private string _licenseText;
        public string LicenseText
        {
            get { return _licenseText; }
            set { _licenseText = value; }
        }

        public DelegateCommand EULAAgreeCommand { get; private set; }
        public DelegateCommand EULADisagreeCommand { get; private set; }

        public EULAViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            SetLicenseText();
            EULAAgreeCommand = new DelegateCommand(OnAgree);
            EULADisagreeCommand = new DelegateCommand(OnDisagree);
        }

        private void OnDisagree()
        {
            _eventAggregator.GetEvent<CloseEulaEvent>().Publish();
        }

        private void OnAgree()
        {
            //check if config can be modified
            var exePath = Assembly.GetExecutingAssembly().Location;
            var configPath = $"{exePath}.config";
            using (var fileStream = new FileStream(configPath, FileMode.Open))
            {
                if (!fileStream.CanWrite)
                {
                    System.Windows.MessageBox.Show($"Cannot update config file. \nUser does not have rights to {configPath}");
                    return;
                }
            }

            UpdateAppSettings("EULAAgree", "true");

            OnDisagree();
        }

        private void UpdateAppSettings(string appSettingsKey, string newValue) 
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var configPath = $"{exePath}.config";

            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(configPath);
            XmlNode eulaAgreeNode = XmlDoc.SelectSingleNode($"configuration/appSettings/add[@key='{appSettingsKey}']");
            eulaAgreeNode.Attributes["value"].Value = newValue;
            XmlDoc.Save(configPath);

            ConfigurationManager.RefreshSection("appSettings");
        }

        private void SetLicenseText()
        {
            LicenseText = "This Limited Use Software License Agreement(the \"Agreement\") is a legal agreement between you , the " +
    "user(\"You\"), and Varian Medical Systems, Inc. (\"Varian\").By downloading or otherwise accessing the " +
    "software material, which includes source code(the \"Source Code\") and related software tools(collectively, " +
    "the \"Software\"), You are agreeing to be bound by the terms of this Agreement.If You are entering into this " +
    "Agreement on behalf of an institution or company, You represent and warrant that You are authorized to do " +
                "so. If You do not agree to the terms of this Agreement, You may not use the Software and must immediately" +
               "destroy any Software You may have downloaded or copied.\n\n" +
    "SOFTWARE LICENSE\n\n" +
    "1.\tGrant of License. Varian grants to You a non-transferable, non - sublicensable license to use " +
    "the Software solely as provided in Section 2(Permitted Uses) below.Access to the Software will be " +
    "facilitated through a source code repository provided by Varian.\n\n" +
    "2.\tPermitted Uses.You may download, compile and use the Software, You may(but are not required to do " +
                "so) suggest to Varian improvements or otherwise provide feedback to Varian with respect to the " +
    "Software. You may modify the Software solely in support of such use, and You may upload such " +
    "modified Software to Varian’s source code repository. Any derivation of the Software(including compiled " +
    "binaries) must display prominently the terms and conditions of this Agreement in the interactive user " +
    "interface, such that use of the Software cannot continue until the user has acknowledged having read " +
    "this Agreement via click-through.\n\n" +
    "3.\t Publications.Solely in connection with your use of the Software as permitted under this Agreement, You " +
    "may make reference to this Software in connection with such use in academic research publications " +
    "after notifying an authorized representative of Varian in writing in each instance. Notwithstanding the " +
    "foregoing, You may not make reference to the Software in any way that may indicate or imply any " +
    "approval or endorsement by Varian of the results of any use of the Software by You.\n\n " +
    "4.\t Prohibited Uses. Under no circumstances are You permitted, allowed or authorized to distribute the " +
    "Software or any modifications to the Software for any purpose, including, but not limited to, renting, " +
    "selling, or leasing the Software or any modifications to the Software, for free or otherwise.You may not " +
    "disclose the Software to any third party without the prior express written consent of an authorized " +
    "representative of Varian. You may not reproduce, copy or disclose to others, in whole or in any part, the " +
    "Software or modifications to the Software, except within Your own institution or company, as applicable, " +
    "to facilitate Your permitted use of the Software. You agree that the Software will not be shipped, " +
    "transferred or exported into any country in violation of the U.S.Export Administration Act (or any other " +
    "law governing such matters) and that You will not utilize, in any other manner, the Software in " +
    "violation of any applicable law. \n\n" +
    "5.\t Intellectual Property Rights. All intellectual property rights in the Software and any modifications to the " +
    "Software are owned solely and exclusively by Varian, and You shall have no ownership or other " +
    "proprietary interest in the Software or any modifications. You hereby transfer and assign to Varian all " +
    "right, title and interest in any such modifications to the Software that you may have made or contributed. " +
    "You hereby waive any and all moral rights that you may have with respect to such modifications, and " +
    "hereby waive any rights of attribution relating to any modifications of the Software. You acknowledge " +
    "that Varian will have the sole right to commercialize and otherwise use, whether directly or through thir " +
    "parties, any modifications to the Software that you provide to Varian’s repository. Varian may make any " +
    "use it determines to be appropriate with respect to any feedback, suggestions or other communications " +
    "that You provide with respect to the Software or any modifications. \n\n" +
    "Varian Confidential\n\n" +
    "6.\t No Support Obligations. Varian is under no obligation to provide any support or technical assistance in connection with the Software or any modifications. Any such support or technical assistance is entirely discretionary on the part of Varian, and may be discontinued at any time without liability.\n\n" +
    "7.\t NO WARRANTIES. THE SOFTWARE AND ANY SUPPORT PROVIDED BY VARIAN ARE PROVIDED " +
    "\"AS IS\" AND \"WITH ALL FAULTS.\" VARIAN DISCLAIMS ALL WARRANTIES, BOTH EXPRESS AND " +
    "IMPLIED, INCLUDING BUT NOT LIMITED TO IMPLIED WARRANTIES OF MERCHANTABILITY, " +
    "FITNESS FOR A PARTICULAR PURPOSE, AND NON-INFRINGEMENT WITH RESPECT TO THE " +
    "SOFTWARE AND ANY SUPPORT. VARIAN DOES NOT WARRANT THAT THE OPERATION OF THE " +
    "SOFTWARE WILL BE UNINTERRUPTED, ERROR FREE OR MEET YOUR SPECIFIC " +
    "REQUIREMENTS OR INTENDED USE. THE AGENTS AND EMPLOYEES OF VARIAN ARE NOT " +
    "AUTHORIZED TO MAKE MODIFICATIONS TO THIS PROVISION, OR PROVIDE ADDITIONAL " +
    "WARRANTIES ON BEHALF OF VARIAN. \n\n" +
    "8.\t No Regulatory Clearance. The Software is not cleared or approved for use by any regulatory body in any jurisdiction.\n\n" +
    "9.\t Termination. You may terminate this Agreement, and the right to use the Software, at any time upon" +
    "written notice to Varian. Varian may terminate this Agreement, and the right to use the Software, at any " +
    "time upon notice to You in the event that Varian determines that you are not using the Software in " +
    "accordance with this Agreement or have otherwise breached any provision of this Agreement.The " +
    "Software, together with any modifications to it or any permitted archive copy thereof, shall be destroyed " +
    "when no longer used in accordance with this Agreement, or when the right to use the Software is terminated.\n\n " +
    "10.\t Limitation of Liability.IN NO EVENT SHALL VARIAN BE LIABLE FOR LOSS OF DATA, LOSS OF " +
    "PROFITS, LOST SAVINGS, SPECIAL, INCIDENTAL, CONSEQUENTIAL, INDIRECT OR " +
    "OTHER SIMILAR DAMAGES ARISING FROM BREACH OF WARRANTY, BREACH OF " +
    "CONTRACT, NEGLIGENCE, OR OTHER LEGAL THEORY EVEN IF VARIAN OR ITS AGENT HAS " +
    "BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES, OR FOR ANY CLAIM BY ANY OTHER PARTY.\n\n" +
    "11.\t Indemnification.You will defend, indemnify and hold harmless Varian, its affiliates and their respective " +
    "officers, directors, employees, sublicensees, contractors, users and agents from any and all claims, " +
    "losses, liabilities, damages, expenses and costs (including attorneys’ fees and court costs) arising out of " +
    "any third-party claims related to or arising from your use of the Software or any modifications to the Software. \n\n" +
    "12.\t Assignment. You may not assign any of Your rights or obligations under this Agreement without the written consent of Varian. \n\n" +
    "13.\t Governing Law. This Agreement will be governed and construed under the laws of the State of California " +
    "and the United States of America without regard to conflicts of law provisions. The parties agree to the " +
    "exclusive jurisdiction of the state and federal courts located in Santa Clara County, California with " +
    "respect to any disputes under or relating to this Agreement. \n\n" +
    "14. Entire Agreement. This Agreement is the entire agreement of the parties as to the subject matter and " +
    "supersedes all prior written and oral agreements and understandings relating to same. The Agreement " +
    "may only be modified or amended in a writing signed by the parties that makes specific reference to the " +
    "Agreement and the provision the parties intend to modify or amend. ";
        }
    }
}
