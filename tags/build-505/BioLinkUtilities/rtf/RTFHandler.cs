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

    public interface RTFHandler {

        void startParse();

        void onKeyword(String keyword, bool hasParam, int param);

        void onHeaderGroup(String group);

        void onTextCharacter(char ch);

        void onCharacterAttributeChange(List<AttributeValue> changes);

        void endParse();

    }

    public class RTFHandlerAdapter : RTFHandler {

        public void startParse() {            
        }

        public void onKeyword(string keyword, bool hasParam, int param) {
        }

        public void onHeaderGroup(string group) {
        }

        public void onTextCharacter(char ch) {
        }

        public void onCharacterAttributeChange(List<AttributeValue> changes) {
        }

        public void endParse() {
        }

    }

    public class AttributeValue {

        public AttributeValue(String keyword, bool hasParam, int param) {
            this.Keyword = keyword;
            this.Param = param;
            this.HasParam = hasParam;
        }

        public String Keyword { get; set; }
        public int Param { get; set; }
        public bool HasParam { get; set; }

    }
}
