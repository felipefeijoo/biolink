/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BioLink.Client.Extensibility {

    /// <summary>
    /// Optional questions are those that the use can nominate not to be asked again. The answer to the question is saved in the configuration store.
    /// 
    /// </summary>
    public class OptionalQuestion {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configKey">The preference key to use to store the answer to the question</param>
        /// <param name="configQuestion">The text of the configuration setting questoin</param>
        /// <param name="questionText">The actual question text</param>
        /// <param name="questionTitle">The window title of the question</param>
        public OptionalQuestion(string configKey, string configQuestion, string questionText, string questionTitle) {
            ConfigurationKey = configKey;
            ConfigurationText = configQuestion;
            QuestionText = questionText;
            QuestionTitle = questionTitle;
        }

        public string ConfigurationKey { get; set; }        
        public string ConfigurationText { get; set; }
        public string QuestionText { get; set; }
        public string QuestionTitle { get; set; }

        public string AskQuestionConfigurationKey {
            get { return ConfigurationKey + ".AskQuestion"; }
        }

        /// <summary>
        /// Asks the question, and optionally remembers the answer so that the question will be suppressed subsequently
        /// </summary>
        /// <param name="parentWindow"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool Ask(Window parentWindow, params object[] args) {
            return OptionalQuestionWindow.AskOrDefault(parentWindow, this, args);
        }

    }

    /// <summary>
    /// A repository of known optional questions
    /// </summary>
    public class OptionalQuestions {

        public static readonly OptionalQuestion MaterialIDHistoryQuestion = new OptionalQuestion (
            "Material.DefaultRecordIDHistory",
            "Ask to add an Identification History when material identificaton changes?",            
            "Do you wish to record a history of this identification change?",
            "Record identification history?"
        );

        public static readonly OptionalQuestion UpdateLocalityQuestion = new OptionalQuestion (
            "Material.UpdateLocalityFromEGaz",
            "Ask to update site locality when a place name is selected from eGaz?",
            "Do you wish to update the locality from '{0}' to '{1}'?",             
            "Update locality?"
        );

        public static readonly OptionalQuestion UpdateElevationQuestion = new OptionalQuestion ( 
            "Site.UpdateElevationFromGoogleCode",
            "Ask to update elevation when a location is selected from Google Earth™?",
            "The altitude of the selected location is {0} metres. Do you wish to update the elevation for this site?",
            "Update elevation?"            
        );

    }
}
