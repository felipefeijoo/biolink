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
using System.IO;

namespace BioLink.Client.Utilities {

    public class RTFReader {

        private int _cGroup = 0;
        private PushbackReader _stream;
        private Stack<ParserState> _stateStack = new Stack<ParserState>();
        private ParserState _parserState;
        private long _cbBin = 0;
        private StringBuilder _headerGroupBuffer = new StringBuilder();

        private RTFHandler _handler;

        public RTFReader(TextReader reader, RTFHandler handler) {
            _stream = new PushbackReader(reader);
            _handler = handler;
        }

        public RTFReader(String rtf, RTFHandler handler) {
            _stream = new PushbackReader(new StringReader(rtf));
            _handler = handler;
        }

        public void parse() {
            int cNibble = 2;
            int intCh = 0;
            short b = 0;
            _parserState = new ParserState();

            if (_handler != null) {
                _handler.startParse();
            }

            while ((intCh = _stream.Read()) >= 0) {
                char ch = (char)intCh;
                if (_cGroup < 0) {
                    throw new Exception("Group stack underflow exception");
                }

                if (_parserState.ris == ParserInternalState.Binary) {
                    parseChar(ch);
                } else {
                    switch (ch) {
                        case '{':
                            pushParserState();
                            break;
                        case '}':
                            popParserState();
                            break;
                        case '\\':
                            parseRtfKeyword();
                            break;
                        case '\n':
                        case '\r':
                            break;
                        default:
                            if (_parserState.ris == ParserInternalState.Normal) {
                                parseChar(ch);
                            } else {
                                if (_parserState.ris != ParserInternalState.Hex) {
                                    throw new Exception("Invalid parser state - expected Hex");
                                }

                                b = (short)(b << 4);
                                if (char.IsDigit(ch)) {
                                    b += (short)(ch - '0');
                                } else {
                                    if (char.IsLower(ch)) {
                                        if (ch < 'a' || ch > 'f') {
                                            throw new Exception("Invalid Hex char: " + ch);
                                        }
                                        b += (short)(ch - 'a');
                                    } else {
                                        if (ch < 'A' || ch > 'F') {
                                            throw new Exception("Invalid Hex char: " + ch);
                                        }
                                        b += (short)(ch - 'A');
                                    }
                                }
                                cNibble--;
                                if (cNibble == 0) {
                                    parseChar((char)b);
                                    cNibble = 2;
                                    b = 0;
                                    _parserState.ris = ParserInternalState.Normal;
                                }
                            }
                            break;
                    }
                }
            }
            if (_cGroup < 0) {
                throw new Exception("Group Stack Underflow");
            }

            if (_cGroup > 0) {
                throw new Exception("Unmatched '{'");
            }

            if (_handler != null) {
                _handler.endParse();
            }

        }

        private void parseRtfKeyword() {
            int ch;
            bool isNeg = false;
            bool hasParam = false;
            int param = 0;

            if ((ch = _stream.Read()) < 0) {
                return;
            }

            if (!char.IsLetter((char)ch)) {
                // Control symbol...
                translateKeyword("" + (char)ch, 0, false);
                return;
            }

            StringBuilder keyword = new StringBuilder();
            for (; char.IsLetter((char)ch) && ch >= 0; ch = _stream.Read()) {
                keyword.Append((char)ch);
            }

            if ((char)ch == '-') {
                isNeg = true;
                if ((ch = _stream.Read()) < 0) {
                    return;
                }
            }

            if (char.IsDigit((char)ch)) {
                StringBuilder strParam = new StringBuilder();
                hasParam = true;
                for (; char.IsDigit((char)ch); ch = _stream.Read()) {
                    strParam.Append((char)ch);
                }
                param = Int32.Parse(strParam.ToString());
                if (isNeg) {
                    param = -param;
                }
            }

            if (ch != ' ' && ch >= 0) {
                _stream.Unread(ch);
            }

            translateKeyword(keyword.ToString(), param, hasParam);
        }

        public int read() {
            return _stream.Read();
        }

        ParserState currentState() {
            return _parserState;
        }

        private void translateKeyword(String keyword, int param, bool hasParam) {

            if (Keyword.KEYWORDS.ContainsKey(keyword)) {
                Keyword kwd = Keyword.KEYWORDS[keyword];
                switch (kwd.getKeywordType()) {
                    case KeywordType.Character:
                        parseChar(((CharacterKeyword)kwd).getOutputChar());
                        break;
                    case KeywordType.Destination:
                        _parserState.rds = ((DestinationKeyword)kwd).getDestinationState();
                        if (_parserState.rds == DestinationState.Header) {
                            _headerGroupBuffer = new StringBuilder(keyword);
                        }
                        break;
                    case KeywordType.Attribute:
                        AttributeKeyword attrKwd = (AttributeKeyword)kwd;

                        if (hasParam) {
                            _parserState.CharacterAttributes.set(attrKwd.getAttributeType(), param);
                        } else {
                            _parserState.CharacterAttributes.set(attrKwd.getAttributeType(), attrKwd.getDefaultValue());
                        }

                        AttributeValue val = new AttributeValue(attrKwd.getKeyword(), hasParam, param);
                        if (_handler != null) {
                            List<AttributeValue> values = new List<AttributeValue>();
                            values.Add(val);
                            _handler.onCharacterAttributeChange(values);
                        }
                        break;
                    case KeywordType.Special:
                        char[] output = ((SpecialKeyword)kwd).process(param, this);
                        foreach (char ch in output) {
                            parseChar(ch);
                        }
                        break;
                    default:
                        break;
                }
            } else {
                if (_parserState.rds == DestinationState.Header) {
                    _headerGroupBuffer.Append("\\").Append(keyword);
                    if (hasParam) {
                        _headerGroupBuffer.Append(param);
                    }
                } else {
                    if (_handler != null) {
                        _handler.onKeyword(keyword, hasParam, param);
                    }
                }
            }

        }

        private void parseChar(char ch) {
            if (_parserState.ris == ParserInternalState.Binary && --_cbBin < 0) {
                _parserState.ris = ParserInternalState.Normal;
            }

            switch (_parserState.rds) {
                case DestinationState.Skip:
                    return;
                case DestinationState.Normal:
                    printChar(ch);
                    return;
                case DestinationState.Header:
                    _headerGroupBuffer.Append(ch);
                    break;
                default:
                    // TODO handle other destination types
                    break;
            }

        }

        private void printChar(char ch) {
            if (_handler != null) {
                _handler.onTextCharacter(ch);
            }
        }

        private void pushParserState() {
            // Save the current state,
            _stateStack.Push(_parserState);
            // and create a new one based on this one
            _parserState = new ParserState(_parserState);
            // except that RIS is reset
            _parserState.ris = ParserInternalState.Normal;
            // increment group depth
            _cGroup++;
            if (_parserState.rds == DestinationState.Header) {
                _headerGroupBuffer.Append("{");
            }
        }

        private void popParserState() {
            if (_stateStack.Count == 0) {
                throw new Exception("Group Stack underflow!");
            }

            ParserState prevState = _stateStack.Pop();
            if (_parserState.rds == DestinationState.Normal) {
                endGroupAction(_parserState, prevState);
            }

            _parserState = prevState;
            _cGroup--;
            if (_parserState.rds == DestinationState.Header) {
                _headerGroupBuffer.Append("}");
            }

        }

        private void endGroupAction(ParserState currentState, ParserState previousState) {
            switch (currentState.rds) {
                case DestinationState.Header:
                    emitHeaderGroup(_headerGroupBuffer.ToString());
                    _headerGroupBuffer = new StringBuilder();
                    break;
                default:
                    List<AttributeValue> changes = new List<AttributeValue>();

                    foreach (CharacterAttributeType attrType in CharacterAttributeType.Values) {
                        int currentVal = currentState.CharacterAttributes.Get(attrType);
                        int previousVal = previousState.CharacterAttributes.Get(attrType);
                        if (previousVal != currentVal) {
                            AttributeKeyword attrKeyword = Keyword.findAttributeKeyword(attrType);
                            if (attrKeyword != null) {
                                changes.Add(new AttributeValue(attrKeyword.getKeyword(), true, previousVal));
                            }
                        }
                    }

                    if (_handler != null && changes.Count > 0) {
                        _handler.onCharacterAttributeChange(changes);
                    }
                    break;
            }
        }

        private void emitHeaderGroup(String group) {
            if (_handler != null) {
                _handler.onHeaderGroup(group);
            }
        }

    }
}
