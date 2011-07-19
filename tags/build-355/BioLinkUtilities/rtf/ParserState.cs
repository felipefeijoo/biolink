using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Utilities {

    public class ParserState {

        public CharacterAttributes CharacterAttributes;
        public ParagraphAttributes ParagraphAttributes;
        public SectionAttributes SectionAttributes;
        public DocumentAttributes DocumentAttributes;
        public DestinationState rds;
        public ParserInternalState ris;

        public ParserState() {
            CharacterAttributes = new CharacterAttributes();
            ParagraphAttributes = new ParagraphAttributes();
            SectionAttributes = new SectionAttributes();
            DocumentAttributes = new DocumentAttributes();
            rds = DestinationState.Normal;
            ris = ParserInternalState.Normal;
        }

        public ParserState(ParserState other) {
            rds = other.rds;
            ris = other.ris;
            CharacterAttributes = new CharacterAttributes(other.CharacterAttributes);
            ParagraphAttributes = new ParagraphAttributes(other.ParagraphAttributes);
            SectionAttributes = new SectionAttributes(other.SectionAttributes);
            DocumentAttributes = new DocumentAttributes(other.DocumentAttributes);
        }

    }

    public enum ParserInternalState {
        Normal, Binary, Hex
    }

    public enum DestinationState {
        Normal, Skip, Header
    }

    public class CharacterAttributes {

        private Dictionary<CharacterAttributeType, Int32> _attrMap = new Dictionary<CharacterAttributeType, Int32>();

        public CharacterAttributes() {
		    foreach (CharacterAttributeType attrType in CharacterAttributeType.Values) {
                _attrMap[attrType] = 0;
		    }
	    }

        public CharacterAttributes(CharacterAttributes other) {
            foreach (CharacterAttributeType attrType in CharacterAttributeType.Values) {
			    _attrMap[attrType] = other.Get(attrType);
			
		    }
	    }

        public int Get(CharacterAttributeType attrType) {
            return _attrMap[attrType];
        }

        public void set(CharacterAttributeType attrType, int value) {
            _attrMap[attrType] = value;
        }

    }

    public class DocumentAttributes {

        public DocumentAttributes() {
        }

        public DocumentAttributes(DocumentAttributes other) {
            PageWidth = other.PageWidth;
            PageHeight = other.PageHeight;
            LeftMargin = other.LeftMargin;
            TopMargin = other.TopMargin;
            RightMargin = other.RightMargin;
            BottomMargin = other.BottomMargin;
            StartingPageNumber = other.StartingPageNumber;
            FacingPages = other.FacingPages;
            Landscape = other.Landscape;
        }

        public int PageWidth { get; set; }
        public int PageHeight { get; set; }
        public int LeftMargin { get; set; }
        public int TopMargin { get; set; }
        public int RightMargin { get; set; }
        public int BottomMargin { get; set; }
        public int StartingPageNumber { get; set; }
        public bool FacingPages { get; set; }
        public bool Landscape { get; set; }

    }


    public class SectionAttributes {

        public int Columns { get; set; }
        public SectionBreakType SectionBreak { get; set; }
        public int XPgn { get; set; }
        public int YPgn { get; set; }
        public PageNumberFormatType PageNumberFormat { get; set; }

        public SectionAttributes() {
            SectionBreak = SectionBreakType.NonBreaking;
        }

        public SectionAttributes(SectionAttributes other) {
            Columns = other.Columns;
            SectionBreak = other.SectionBreak;
            XPgn = other.XPgn;
            YPgn = other.YPgn;
            PageNumberFormat = other.PageNumberFormat;
        }
    }

    public enum PageNumberFormatType {
        Dec, URom, LRom, ULtr, LLtr
    }

    public enum SectionBreakType {
        NonBreaking, Column, Even, Odd, Page
    }

    public class ParagraphAttributes {

        public ParagraphAttributes() {
            this.Justification = JustificationType.Left;
        }

        public ParagraphAttributes(ParagraphAttributes other) {
            LeftIndent = other.LeftIndent;
            RightIndent = other.RightIndent;
            FirstLineIndent = other.FirstLineIndent;
            Justification = other.Justification;
        }

        public int LeftIndent { get; set; }
        public int RightIndent { get; set; }
        public int FirstLineIndent { get; set; }
        public JustificationType Justification { get; set; }

    }

    public enum JustificationType {
        Left, Right, Center, Full
    }

    public class CharacterAttributeType {

	    public static CharacterAttributeType Bold = new CharacterAttributeType("b");
        public static CharacterAttributeType Underline = new CharacterAttributeType("ul");
        public static CharacterAttributeType Italic = new CharacterAttributeType("i");
        public static CharacterAttributeType Superscript  = new CharacterAttributeType("super");
        public static CharacterAttributeType Subscript = new CharacterAttributeType("sub");
	
	    private CharacterAttributeType(String keyword) {
		    Keyword = keyword;
	    }
	
	    public String Keyword { get; private set; }

        public static CharacterAttributeType[] Values {
            get { return new CharacterAttributeType[] { Bold, Underline, Italic, Superscript, Subscript }; }
        }
    }

}
