using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace ThreeByte.Network.Util {

    /// <summary>
    /// Parses javascript object syntax
    /// </summary>
    public static class JavascriptParser<T> where T : new() {

        enum tokenizerState { key, value, syntaxChar };
        private static HashSet<char> syntaxChars = new HashSet<char>() { ';', ',', '[', ']', '{', '}', };

        public static List<string> Tokenize(string content) {
            List<string> allTokens = new List<string>();
            string currentToken = "";
            tokenizerState currentState = tokenizerState.syntaxChar;
            for (int i = 0; i < content.Length; i++) {
                char c = content[i];
                if (c == ':') {

                }
                if (syntaxChars.Contains(c)) {
                    if (currentToken != "") {
                        allTokens.Add(currentToken);
                        currentToken = "";
                    }
                    allTokens.Add(c.ToString());
                    continue;
                }



                if(c == '"'){
                    if (currentState == tokenizerState.value) {
                        allTokens.Add(currentToken);
                        currentToken = "";
                        currentState = tokenizerState.key;
                    } else if (currentState == tokenizerState.key) {
                        //currentToken += c;
                        currentState = tokenizerState.value;
                    }
                    continue;
                }

                if (currentState == tokenizerState.key) {
                    if (c == ':') {
                        if (currentToken != "") {
                            allTokens.Add(currentToken);
                            currentToken = "";
                        }
                        allTokens.Add(c.ToString());
                        //currentState = tokenizerState.value;
                    } else {
                        currentToken += c;
                    }
                    continue;
                }

                if (currentState == tokenizerState.value) {
                    currentToken += c;
                }

                if (char.IsLetter(c) && currentState != tokenizerState.value) {
                    currentState = tokenizerState.key;
                    currentToken += c;
                }
            }
            return allTokens.Select(i => i.Trim()).Where(i => !string.IsNullOrWhiteSpace(i)).ToList();

        }

        public static List<T> Parse(string content) {
            List<T> toReturn = new List<T>();
            string currentKey = "";
            T inspectionClip = new T();
            var tokens = Tokenize(content);
            for (int i = 0; i < tokens.Count(); i++) {
                var token = tokens[i];
                if (token == "[" || token == "]" || string.IsNullOrWhiteSpace(token)) {
                    continue;
                }
                if (token.Contains("}")) {
                    toReturn.Add(inspectionClip);
                    inspectionClip = new T();
                    continue;
                }
                if (token == ":") {
                    var val = tokens[++i];
                    var prop = inspectionClip.GetType().GetProperty(currentKey);
                    if (prop != null) {
                        prop.SetValue(inspectionClip, val, null);
                    }
                    continue;
                }

                if (char.IsLetter(token.First())) {
                    currentKey = token;
                    continue;
                }

            }
            return toReturn;
        }
    }
}
