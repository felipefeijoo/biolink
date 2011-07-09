using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BioLink.Client.Utilities {

class FilteringRTFHandler : RTFHandler {

	private HashSet<String> _allowedKeywords = new HashSet<String>();

	private StringBuilder _buffer;
	private bool _newlinesToSpace;

	public FilteringRTFHandler(bool newlinesToSpace, params String[] allowed) {
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

}}
