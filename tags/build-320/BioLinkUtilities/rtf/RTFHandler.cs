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
