//  https://github.com/freedot/SimpleJSON.cs.git
using System.Collections.Generic;
using System.Text;

namespace SimpleJSON
{
    public enum JSONObjectType
    {
        NUMBER, STRING, BOOLEAN, OBJECT, ARRAY, NULL
    }

    public class JSONObject
    {
        public static JSONObject Parse(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new JSONError("empty string!");
            return new PARSER.Parser(json).Parse();
        }

        protected object value;
        protected Dictionary<string, JSONObject> keyvalues;

        public virtual JSONObjectType Type
        {
            get
            {
                if (value == null)
                    return JSONObjectType.OBJECT;

                var type = value.GetType();
                if (type == typeof(double) || type == typeof(int) || type == typeof(float))
                    return JSONObjectType.NUMBER;
                else if (type == typeof(string))
                    return JSONObjectType.STRING;
                else if (type == typeof(bool))
                    return JSONObjectType.BOOLEAN;
                else
                    return JSONObjectType.NULL;
            }
        }

        public JSONObject(float value) { this.value = value; }
        public JSONObject(int value) { this.value = value; }
        public JSONObject(double value) { this.value = value; }
        public JSONObject(bool value) { this.value = value; }
        public JSONObject(string value) { this.value = value; }
        public JSONObject() { keyvalues = new Dictionary<string, JSONObject>(); }

        public virtual string Error { get { return null; } }

        public JSONObject this[string index]
        {
            get
            {
                JSONObject outval = null;
                keyvalues.TryGetValue(index, out outval);
                return outval;
            }
            set { keyvalues[index] = value; }
        }

        public float GetFloat(string key, float defaultval = 0f)
        {
            JSONObject v = this[key];
            if (v != null) return v.ToFloat();
            return defaultval;
        }

        public int GetInteger(string key, int defaultval = 0)
        {
            JSONObject v = this[key];
            if (v != null) return v.ToInteger();
            return defaultval;
        }

        public bool GetBealoon(string key, bool defaultval = false)
        {
            JSONObject v = this[key];
            if (v != null) return v.ToBealoon();
            return defaultval;
        }

        public string GetString(string key, string defaultval = "")
        {
            JSONObject v = this[key];
            if (v != null) return v.ToString();
            return defaultval;
        }

        public JSONObject GetJSONObject(string key)
        {
            return this[key];
        }

        public JSONArray GetJSONArray(string key)
        {
            return this[key] as JSONArray;
        }

        public float ToFloat() { return (float)value; }
        public int ToInteger() { return (int)value; }
        public bool ToBealoon() { return (bool)value; }
        public override string ToString()
        {
            return InnerToString(false);
        }
        public string ToJSONString()
        {
            return InnerToString(true);
        }

        protected virtual string InnerToString(bool json)
        {
            var valueType = this.Type;
            if (valueType == JSONObjectType.STRING)
                return json ? ((string)value).Replace("\"", "\\\"") : (string)value;
            else if (valueType == JSONObjectType.NULL)
                return "null";
            else if (valueType == JSONObjectType.NUMBER)
                return value.ToString();
            else if (valueType == JSONObjectType.BOOLEAN)
                return (bool)value ? "true" : "false";
            else if (valueType == JSONObjectType.OBJECT)
            {
                StringBuilder str = new StringBuilder();
                str.Append("{");
                bool firstElemet = true;
                foreach (var obj in keyvalues)
                {
                    if (!firstElemet)
                        str.Append(",");

                    str.AppendFormat("\"{0}\":", obj.Key);
                    AppendElement(str, obj.Value, json);

                    firstElemet = false;
                }
                str.Append("}");
                return str.ToString();
            }
            else
            {
                return "{}";
            }
        }

        protected void AppendElement(StringBuilder str, JSONObject value, bool json)
        {
            if (value.Type == JSONObjectType.STRING)
                str.AppendFormat("\"{0}\"", value.InnerToString(json));
            else
                str.Append(value.InnerToString(json));
        }
    }

    public class JSONNull : JSONObject
    {
        public override JSONObjectType Type
        {
            get { return JSONObjectType.NULL; }
        }
    }

    public class JSONError : JSONObject
    {
        private string error;
        public JSONError(string error) { this.error = error; }
        public override string Error { get { return (string)error; } }
    }

    public class JSONArray : JSONObject
    {
        protected List<JSONObject> values;

        public override JSONObjectType Type
        {
            get { return JSONObjectType.ARRAY; }
        }

        public JSONArray() { values = new List<JSONObject>(); }

        public JSONObject this[int index]
        {
            get { return values[index]; }
            set
            {
                while (index >= values.Count)
                    values.Add(null);
                values[index] = value;
            }
        }

        protected override string InnerToString(bool json)
        {
            StringBuilder str = new StringBuilder();
            str.Append("[");
            for (int i = 0, n = values.Count; i < n; ++i)
            {
                var value = values[i];
                if (i > 0)
                    str.Append(",");
                AppendElement(str, value, json);
            }
            str.Append("]");
            return str.ToString();
        }

        public int Length
        {
            get { return values.Count; }
        }
    }
}

namespace SimpleJSON.PARSER
{
    internal enum TokenType
    {
        TokLBracket, TokRBracket, TokLBrace, TokRBrace,
        TokString, TokNumber, TokColon,
        TokComma, TokTrue, TokFalse,
        TokNull, TokEnd, TokError
    }

    internal class Token
    {
        public TokenType type;
        public int start;
        public int end;
        public double numberToken;
        public string stringToken;
    }

    internal class Lexer
    {
        private string json = null;
        private int end = 0;
        private int cur = 0;
        public Token currentToken { private set; get; }

        public Lexer(string json)
        {
            this.json = json;
            cur = 0;
            end = json.Length;
            currentToken = new Token();
        }

        public TokenType Next()
        {
            while (cur < end && isWhiteSpace())
                ++cur;

            if (cur >= end)
            {
                currentToken.type = TokenType.TokEnd;
                currentToken.start = currentToken.end = cur;
                return TokenType.TokEnd;
            }

            currentToken.type = TokenType.TokError;
            currentToken.start = cur;
            char c = json[cur];
            switch (c)
            {
                case '[':
                    currentToken.type = TokenType.TokLBracket;
                    currentToken.end = ++cur;
                    return TokenType.TokLBracket;
                case ']':
                    currentToken.type = TokenType.TokRBracket;
                    currentToken.end = ++cur;
                    return TokenType.TokRBracket;
                case '{':
                    currentToken.type = TokenType.TokLBrace;
                    currentToken.end = ++cur;
                    return TokenType.TokLBrace;
                case '}':
                    currentToken.type = TokenType.TokRBrace;
                    currentToken.end = ++cur;
                    return TokenType.TokRBrace;
                case ',':
                    currentToken.type = TokenType.TokComma;
                    currentToken.end = ++cur;
                    return TokenType.TokComma;
                case ':':
                    currentToken.type = TokenType.TokColon;
                    currentToken.end = ++cur;
                    return TokenType.TokColon;
                case '"':
                    return LexString();
                case 't':
                    if (end - cur >= 4 && json.IndexOf("rue", cur + 1) == cur + 1)
                    {
                        cur += 4;
                        currentToken.type = TokenType.TokTrue;
                        currentToken.end = cur;
                        return TokenType.TokTrue;
                    }
                    break;
                case 'f':
                    if (end - cur >= 5 && json.IndexOf("alse", cur + 1) == cur + 1)
                    {
                        cur += 5;
                        currentToken.type = TokenType.TokFalse;
                        currentToken.end = cur;
                        return TokenType.TokFalse;
                    }
                    break;
                case 'n':
                    if (end - cur >= 4 && json.IndexOf("ull", cur + 1) == cur + 1)
                    {
                        cur += 4;
                        currentToken.type = TokenType.TokNull;
                        currentToken.end = cur;
                        return TokenType.TokNull;
                    }
                    break;
                case '-':
                case '.':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return LexNumber();
            }

            return TokenType.TokError;
        }

        private bool isWhiteSpace()
        {
            char c = json[cur];
            return c == ' ' || c == '\r' || c == '\n' || c == '\t';
        }

        private TokenType LexString()
        {
            ++cur;
            currentToken.type = TokenType.TokString;
            currentToken.start = cur;

            while (cur < end && (json[cur] != '"' || json[cur - 1] == '\\'))
                ++cur;

            if (cur < end)
            {
                currentToken.end = cur;
                currentToken.stringToken = json.Substring(currentToken.start, cur - currentToken.start).Replace("\\\"", "\"");
                ++cur;
                return TokenType.TokString;
            }
            else
            {
                return TokenType.TokError;
            }
        }

        private TokenType LexNumber()
        {
            currentToken.type = TokenType.TokNumber;
            currentToken.start = cur;
            while (cur < end)
            {
                char c = json[cur];
                if (c == '-' || c == '.' || c == 'e' || c == 'E' || (c >= '0' && c <= '9'))
                    ++cur;
                else
                    break;
            }

            if (cur < end)
            {
                currentToken.end = cur;
                currentToken.numberToken = 0;
                string token = json.Substring(currentToken.start, cur - currentToken.start);
                double.TryParse(token, out currentToken.numberToken);
                return TokenType.TokNumber;
            }
            else
            {
                return TokenType.TokError;
            }
        }
    }

    internal enum ParserState
    {
        None,
        StartArray, StartArrayElement, StartArrayNextElement,
        StartObject, StartObjectElement, StartObjectKey, StartObjectValue, StartObjectNextElement
    }

    internal class Parser
    {
        private Lexer lexer = null;
        public Parser(string json)
        {
            lexer = new Lexer(json);
        }

        public JSONObject Parse()
        {
            Stack<JSONObject> objects = new Stack<JSONObject>();
            JSONObject lastValue = null;

            Stack<ParserState> states = new Stack<ParserState>();
            ParserState state = ParserState.None;

            JSONObject curValue = null;

            int index = 0;
            string key = null;

            do
            {
                var type = lexer.Next();
                if (!CheckSyntaxByState(state, type))
                    return CreateErrorObject(lexer.currentToken);

                switch (type)
                {
                    case TokenType.TokLBracket:
                        objects.Push(lastValue);
                        states.Push(state);

                        lastValue = new JSONArray();
                        state = ParserState.StartArray;
                        break;

                    case TokenType.TokLBrace:
                        objects.Push(lastValue);
                        states.Push(state);

                        lastValue = new JSONObject();
                        state = ParserState.StartObject;
                        break;

                    case TokenType.TokRBracket:
                    case TokenType.TokRBrace:
                        curValue = lastValue;
                        lastValue = objects.Pop();
                        state = states.Pop();
                        break;

                    case TokenType.TokString:
                        if (state == ParserState.StartObjectElement)
                        {
                            key = lexer.currentToken.stringToken;
                            var nextType = lexer.Next();
                            if (nextType != TokenType.TokColon)
                                return CreateErrorObject(lexer.currentToken);
                            state = ParserState.StartObjectKey;
                        }
                        else
                        {
                            curValue = new JSONObject(lexer.currentToken.stringToken);
                        }
                        break;

                    case TokenType.TokNumber:
                        curValue = new JSONObject(lexer.currentToken.numberToken);
                        break;

                    case TokenType.TokTrue:
                        curValue = new JSONObject(true);
                        break;

                    case TokenType.TokFalse:
                        curValue = new JSONObject(false);
                        break;

                    case TokenType.TokNull:
                        curValue = new JSONNull();
                        break;

                    case TokenType.TokComma:
                        break;

                    case TokenType.TokEnd:
                        return CreateErrorObject(lexer.currentToken);
                }

                if (state == ParserState.None)
                    return curValue;

                if (!SwitchStateAndSetElement(ref state, ref curValue, ref lastValue, ref index, ref key))
                    return CreateErrorObject(lexer.currentToken);

            } while (true);
        }

        private bool IsValueType(TokenType type)
        {
            return type == TokenType.TokString || type == TokenType.TokNumber
                || type == TokenType.TokLBrace || type == TokenType.TokLBracket // object , array
                || type == TokenType.TokTrue || type == TokenType.TokFalse || type == TokenType.TokNull;
        }

        private bool CheckSyntaxByState(ParserState state, TokenType type)
        {
            if (type == TokenType.TokEnd)
                return true;

            switch (state)
            {
                case ParserState.None:
                    return (type == TokenType.TokLBrace || type == TokenType.TokLBracket);

                case ParserState.StartArray:

                    return true;
                case ParserState.StartArrayElement:
                    return IsValueType(type) || type == TokenType.TokRBracket;

                case ParserState.StartArrayNextElement:
                    return (type == TokenType.TokComma || type == TokenType.TokRBracket);

                case ParserState.StartObject:
                    return true;

                case ParserState.StartObjectElement:
                    return (type == TokenType.TokString || type == TokenType.TokRBrace);

                case ParserState.StartObjectValue:
                    return IsValueType(type);

                case ParserState.StartObjectNextElement:
                    return (type == TokenType.TokComma || type == TokenType.TokRBrace);

                default:
                    return false;
            }
        }

        private bool SwitchStateAndSetElement(ref ParserState state, ref JSONObject curValue, ref JSONObject lastValue, ref int index, ref string key)
        {
            switch (state)
            {
                case ParserState.StartArray:
                    state = ParserState.StartArrayElement;
                    return true;

                case ParserState.StartArrayElement:
                    ((JSONArray)lastValue)[index++] = curValue;
                    curValue = null;
                    state = ParserState.StartArrayNextElement;
                    return true;

                case ParserState.StartArrayNextElement:
                    state = ParserState.StartArrayElement;
                    return true;

                case ParserState.StartObject:
                    state = ParserState.StartObjectElement;
                    return true;

                case ParserState.StartObjectKey:
                    state = ParserState.StartObjectValue;
                    return true;

                case ParserState.StartObjectValue:
                    lastValue[key] = curValue;
                    curValue = null;
                    state = ParserState.StartObjectNextElement;
                    return true;

                case ParserState.StartObjectNextElement:
                    state = ParserState.StartObjectElement;
                    return true;

                default:
                    return false;
            }
        }

        private JSONObject CreateErrorObject(Token curToken, string error = "")
        {
            return new JSONError(string.Format("Syntax error> position:({0}-{1}); token type:{2}! {3}", curToken.start, curToken.end, curToken.type, error));
        }
    }
}