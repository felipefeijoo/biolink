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
    /// Keyword Type
    /// </summary>
    public enum KeywordType {
        Attribute, Character, Destination, Special
    }

    public class CharacterKeyword : Keyword {
	
	    private char _outputChar;

        public CharacterKeyword(String keyword, char outputChar) : base(keyword, KeywordType.Character) {
		    _outputChar = outputChar;
	    }
	
	    public char getOutputChar() {
		    return _outputChar;
	    }

    }

    public class DestinationKeyword : Keyword {
	
	    private DestinationState _destState;

        public DestinationKeyword(String keyword, DestinationState destState) : base(keyword, KeywordType.Destination) {		    
		    _destState = destState;
	    }
	
	    public DestinationState getDestinationState() {
		    return _destState;
	    }

    }

    public class AttributeKeyword : Keyword {
	
	    private int _default;
	    private bool _useDefault;
	    private CharacterAttributeType _charAttrType;

	    public AttributeKeyword(String keyword, CharacterAttributeType type, int defval, bool useDefault) : base(keyword, KeywordType.Attribute) {
		    _default = defval;
		    _useDefault = useDefault;
		    _charAttrType = type;
	    }
	
	    public int getDefaultValue() {
		    return _default;
	    }
	
	    public bool useDefault() {
		    return _useDefault;
	    }
	
	    public CharacterAttributeType getAttributeType() {
		    return _charAttrType;
	    }

    }

    public abstract class SpecialKeyword : Keyword {

	    public SpecialKeyword(String keyword) : base(keyword, KeywordType.Special) {
	    }

	    public abstract char[] process(int param, RTFReader reader);	
    }

    public class UnicodeKeyword : SpecialKeyword {

	    public UnicodeKeyword(String keyword) : base(keyword) {
	    }
	
	    public override char[] process(int param, RTFReader reader) {
				
		    if ((param < 0) && (param >= short.MinValue)) { 
			    // in this case the value has been written as a signed 16 bit number.  We need to convert to an
			    // unsigned value as negative code points are invalid.
			    param = (short)param & 0xFFFF;
		    }
		
		    // Convert the code point to one or more characters.
		    var characters = char.ConvertFromUtf32(param);
		
		    // skip the next character.
		    reader.read();
		
		    return characters.ToArray();
		
	    }
    }

    public class CodePageKeyword : SpecialKeyword {
	
	    public CodePageKeyword() : base("'") {
	    }

        public override char[] process(int param, RTFReader reader) {
		    char[] hex = new char[2];
		    hex[0] = (char) reader.read();
		    hex[1] = (char) reader.read();

            uint value = Convert.ToUInt32(new String(hex), 16);
		    return new char[]  { (char) value };		
	    }

    }

    public abstract class Keyword {

	public static Dictionary<String, Keyword> KEYWORDS = new Dictionary<String, Keyword>();

	public static void registerKeyword(Keyword keywordDesc) {
		KEYWORDS[keywordDesc.getKeyword()] = keywordDesc;
	}

	static Keyword() {

		// Attribute keywords (keywords that alter character/section or document attributes)
		foreach (CharacterAttributeType attribute in CharacterAttributeType.Values) {
			registerKeyword(new AttributeKeyword(attribute.Keyword, attribute, 1, false));
		}

		// Character literal keywords...
		registerKeyword(new CharacterKeyword("\r", '\r'));
		registerKeyword(new CharacterKeyword("\n", '\n'));
		registerKeyword(new CharacterKeyword("line", '\n'));
		registerKeyword(new CharacterKeyword("tab", '\t'));
		registerKeyword(new CharacterKeyword("page", '\f'));
		registerKeyword(new CharacterKeyword("lquote", (char) 0x2018));
		registerKeyword(new CharacterKeyword("rquote", (char) 0x2019));
		registerKeyword(new CharacterKeyword("ldblquote", (char) 0x201c));
		registerKeyword(new CharacterKeyword("rdblquote", (char) 0x201d));
		registerKeyword(new CharacterKeyword("bullet", (char) 0x2022));
		registerKeyword(new CharacterKeyword("endash", (char) 0x2013));
		registerKeyword(new CharacterKeyword("emdash", (char) 0x2014));
		registerKeyword(new CharacterKeyword("enspace", (char) 0x2002));
		registerKeyword(new CharacterKeyword("emspace", (char) 0x2003));
		
		// This is the cheats way of allowing an escaped grouping bracket.
		registerKeyword(new CharacterKeyword("{", '{'));
		registerKeyword(new CharacterKeyword("}", '}'));
		
		

		// Destinations...
		registerKeyword(new DestinationKeyword("fonttbl", DestinationState.Header));
		registerKeyword(new DestinationKeyword("colortbl", DestinationState.Header));
		registerKeyword(new DestinationKeyword("info", DestinationState.Header));
		registerKeyword(new DestinationKeyword("stylesheet", DestinationState.Header));

		// Special keywords
		registerKeyword(new UnicodeKeyword("u"));
		registerKeyword(new CodePageKeyword());
	}

	protected String _keyword;
	protected KeywordType _type;

	public Keyword(String keyword, KeywordType type) {
		_keyword = keyword;
		_type = type;
	}

	public String getKeyword() {
		return _keyword;
	}

	public KeywordType getKeywordType() {
		return _type;
	}

	public static AttributeKeyword findAttributeKeyword(CharacterAttributeType attrType) {
		foreach (Keyword kwd in KEYWORDS.Values) {
			if (kwd is AttributeKeyword) {
				AttributeKeyword attrKwd = (AttributeKeyword) kwd;
				if (attrKwd.getAttributeType() == attrType) {
					return attrKwd;
				}
			}
		}

		return null;
	}

	public static CharacterKeyword findKeywordForCharacter(char ch) {
		foreach (Keyword kwd in KEYWORDS.Values) {
			if (kwd is CharacterKeyword) {
				CharacterKeyword charKwd = (CharacterKeyword) kwd;
				if (charKwd.getOutputChar() == ch) {
					return charKwd;
				}
			}
		}

		return null;
	}

}}
