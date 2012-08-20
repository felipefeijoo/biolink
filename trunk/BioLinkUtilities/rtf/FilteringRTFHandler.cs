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

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Specialized RTF handler that strips out all control words that do not match a supplied list.
    /// </summary>
    class PositiveVetFilteringRTFHandler : RTFHandler {

        private HashSet<String> _allowedKeywords = new HashSet<String>();

        private StringBuilder _buffer;
        private bool _newlinesToSpace;

        public PositiveVetFilteringRTFHandler(bool newlinesToSpace, params String[] allowed) {
            _newlinesToSpace = newlinesToSpace;
            foreach (String word in allowed) {
                _allowedKeywords.Add(word);
            }
            _buffer = new StringBuilder();
        }

        public void startParse() {
        }

        public void onKeyword(String keyword, bool hasParam, int param) {

            if (_newlinesToSpace && keyword.Equals("par")) {
                _buffer.Append(" ");
            }

            if (_allowedKeywords.Contains(keyword)) {
                _buffer.Append("\\").Append(keyword);
                if (hasParam) {
                    _buffer.Append(param);
                }
                _buffer.Append(" ");
            }
        }

        public void onHeaderGroup(String group) {
        }

        public void onTextCharacter(char ch) {
            _buffer.Append(ch);
        }

        public void endParse() {
        }

        public String getFilteredText() {
            return _buffer.ToString();
        }

        public void onCharacterAttributeChange(List<AttributeValue> values) {
            bool atLeastOneAllowed = false;
            foreach (AttributeValue val in values) {
                if (_allowedKeywords.Contains(val.Keyword)) {
                    atLeastOneAllowed = true;
                    _buffer.Append("\\").Append(val.Keyword);
                    if (val.HasParam) {
                        _buffer.Append(val.Param);
                    }
                }
            }
            if (atLeastOneAllowed) {
                _buffer.Append(" "); // terminate the string of control words...
            }
        }
    }

    /// <summary>
    /// Specialized RTF handler that strips out all control words that do not match a supplied list.
    /// </summary>
    class NegativeVetFilteringRTFHandler : RTFHandler {

        private HashSet<String> _disallowedKeywords = new HashSet<String>();

        private StringBuilder _buffer;
        private bool _newlinesToSpace;

        public NegativeVetFilteringRTFHandler(bool newlinesToSpace, params String[] disallowed) {
            _newlinesToSpace = newlinesToSpace;
            foreach (String word in disallowed) {
                _disallowedKeywords.Add(word);
            }
            _buffer = new StringBuilder();
        }

        public void startParse() {
        }

        public void onKeyword(String keyword, bool hasParam, int param) {

            if (_newlinesToSpace && keyword.Equals("par")) {
                _buffer.Append(" ");
            }

            if (!_disallowedKeywords.Contains(keyword)) {
                _buffer.Append("\\").Append(keyword);
                if (hasParam) {
                    _buffer.Append(param);
                }
                _buffer.Append(" ");
            }
        }

        public void onHeaderGroup(String group) {
        }

        public void onTextCharacter(char ch) {
            _buffer.Append(ch);
        }

        public void endParse() {
        }

        public String getFilteredText() {
            return _buffer.ToString();
        }

        public void onCharacterAttributeChange(List<AttributeValue> values) {
            bool atLeastOneAllowed = false;
            foreach (AttributeValue val in values) {
                if (!_disallowedKeywords.Contains(val.Keyword)) {
                    atLeastOneAllowed = true;
                    _buffer.Append("\\").Append(val.Keyword);
                    if (val.HasParam) {
                        _buffer.Append(val.Param);
                    }
                }
            }
            if (atLeastOneAllowed) {
                _buffer.Append(" "); // terminate the string of control words...
            }
        }
    }


}
