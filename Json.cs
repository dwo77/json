using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace dwo.Json
{
	internal class TokenHelper
	{
		public int index;
		public string[] data;
		public string Value { get { return data[index]; } }
		public void Next() { ++index; }
		public string RemoveQuotes(string s) { return s.Substring(1,s.Length - 2); }
	}
	
	public class JsonObject
	{
		private Dictionary<string,JsonValue> _values = new Dictionary<string,JsonValue>();
        private List<string> MakeTokens(string json)
        {
            List<string> list = new List<string>();
            bool inQuotes = false;
            int i = 0;
            int len = json.Length;
            string lastToken = "";

            while(i < len)
            {
                char c = json[i];

                if(inQuotes)
                {
                    if(c == '"' && json[i - 1] != '\\')
                    {
                        list.Add(lastToken + c);
                        lastToken = "";
                        inQuotes = false;
                    }
                    else
                    {
                        lastToken += c;
                    }

                    ++i;
                    continue;
                }

                switch(c)
                {
                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                        if(lastToken.Length > 0)
                        {
                            list.Add(lastToken);
                            lastToken = "";
                        }

                        break;

                    case '"':
                        inQuotes = true;
                        lastToken += c;
                        break;

                    case ':':
                    case ',':
                    case '{':
                    case '}':
                    case '[':
                    case ']':
                        if(lastToken.Length > 0)
                        {
                            list.Add(lastToken);
                            lastToken = "";
                        }
                        list.Add(lastToken + c);
                        lastToken = "";
                        break;

                    default:
                        lastToken += c;
                        break;
                } // switch c

                ++i;
            }

            return list;
        }
		private void Parse(TokenHelper th)
		{
			while(true)
			{
				// first token has to be a key
				if(th.Value[0] != '"')
					throw new Exception("Parse error: missing \"");

				string key = th.RemoveQuotes(th.Value);
				th.Next();

				if(th.Value != ":")
					throw new Exception(string.Format("Parse error: expected ':', encountered '{0}'",th.Value));

				th.Next();

				JsonValue jv = new JsonValue();
                ParseValue(jv,th);
				_values[key] = jv;
				
				// after each value must be a , or }
				if(th.Value == ",")
				{
					th.Next();
				    continue;
				}

				if(th.Value != "}")
					throw new Exception("Parse error: missing }");	

				th.Next();
				break;
			}
		}
        private void ParseValue(JsonValue jv,TokenHelper th)
        {
            switch(th.Value)
            {
                case "{":
                    jv.Vt = JsonValue.ValueType.Object;
                    th.Next();
                    jv.ObjectVal = new JsonObject();
                    jv.ObjectVal.Parse(th);
                    break;

                case "[":
                    jv.Vt = JsonValue.ValueType.Array;
                    jv.ArrayVal = new List<JsonValue>();
                    th.Next();

                    while(true)
                    {
                        if(th.Value == "]")
                        {
                            th.Next();
                            break;
                        }

                        JsonValue av = new JsonValue();
                        ParseValue(av,th);
                        jv.ArrayVal.Add(av);

                        if(th.Value == ",")
                        {
                            th.Next();
                            continue;
                        }

                        if(th.Value != "]")
                            throw new Exception("Parse error: missing ]");

                        th.Next();
                        break;
                    }
                    break;

                case "null":
                    jv.Vt = JsonValue.ValueType.Fixed;
                    jv.FixedVal = JsonValue.FixedValue.Null;
                    th.Next();
                    break;

                case "true":
                    jv.Vt = JsonValue.ValueType.Fixed;
                    jv.FixedVal = JsonValue.FixedValue.True;
                    th.Next();
                    break;

                case "false":
                    jv.Vt = JsonValue.ValueType.Fixed;
                    jv.FixedVal = JsonValue.FixedValue.False;
                    th.Next();
                    break;

                default:
                    if(th.Value[0] == '"') // String
                    {
                        jv.Vt = JsonValue.ValueType.String;
                        jv.StrVal = th.RemoveQuotes(th.Value);
                        th.Next();
                    }
                    else // Number
                    {
                        Regex r = new Regex("^-?[0-9]+$");
                        
                        if(r.IsMatch(th.Value))
                        {
                            jv.Vt = JsonValue.ValueType.Int;
                            jv.IntVal = int.Parse(th.Value);
                        }
                        else
                        {
                            jv.Vt = JsonValue.ValueType.Float;
                            jv.FloatVal = float.Parse(th.Value,CultureInfo.InvariantCulture.NumberFormat);
                        }
                        
                        th.Next();
                    }
                    break;
            } // switch Value
        }

		public void Parse(string json)
        {
            _values.Clear();
            List<string> tokens = MakeTokens(json);

            TokenHelper th = new TokenHelper();
            th.index = 0;
            th.data = tokens.ToArray();

            // start the recursive token parsing
            if(th.Value != "{")
                throw new Exception("Parse error: start not {");

            th.Next();
            Parse(th);
        }
        public string ToJson()
		{
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n");

            bool first = true;

            foreach(string s in _values.Keys)
            {
                if(!first)
                    sb.Append(",\n");

                first = false;
                sb.Append("\"").Append(s).Append("\":");
                sb.Append(_values[s].ToJson());
            }

            sb.Append(" }\n");
            return sb.ToString();
		}
        public JsonValue GetValue(string key)
        {
            if(!_values.ContainsKey(key))
                return new JsonValue();

            return _values[key];
        }
        public void SetValue(string key,JsonValue jv)
        {
            _values[key] = jv;
        }
        public void ValuesAsDict(Dictionary<string,string> vals)
        {
            foreach(string key in _values.Keys)
            {
                vals[key] = _values[key].ToString();
            }
        }
	}

	public class JsonValue
	{
		public enum ValueType { Fixed, String, Int, Float, Object, Array };
		public enum FixedValue { Null, True, False };
		public ValueType Vt;
		public FixedValue FixedVal;
		public string StrVal;
		public int IntVal;
		public float FloatVal;
		public JsonObject ObjectVal;
		public List<JsonValue> ArrayVal;

		public JsonValue()
		{
		    Vt = ValueType.Fixed;
			FixedVal = FixedValue.Null;
			StrVal = "";
			ObjectVal = null;
			ArrayVal = null;
			IntVal = 0;
			FloatVal = 0;
		}
        public JsonValue(string s) : this()
        {
            Vt = ValueType.String;
            StrVal = s;
        }
        public JsonValue(int i) : this()
        {
            Vt = ValueType.Int;
            IntVal = i;
        }
        public JsonValue(FixedValue fv) : this()
        {
            Vt = ValueType.Fixed;
            FixedVal = fv;
        }
        public JsonValue(float f) : this()
        {
            Vt = ValueType.Float;
            FloatVal = f;
        }
        public JsonValue(JsonObject o) : this()
        {
            Vt = ValueType.Object;
            ObjectVal = o;
        }
        public JsonValue(List<JsonValue> arr) : this()
        {
            Vt = ValueType.Array;
            ArrayVal = arr;
        }
        public JsonValue(bool b) : this()
        {
            Vt = ValueType.Fixed;
            FixedVal = b ? FixedValue.True : FixedValue.False;
        }

        public override string ToString()
        {
            switch(Vt)
            {
                case ValueType.String: return StrVal;
                case ValueType.Int: return IntVal.ToString();
                case ValueType.Float: return FloatVal.ToString();
                case ValueType.Fixed:
                    switch(FixedVal)
                    {
                        case FixedValue.False: return "False";
                        case FixedValue.True: return "True";
                        default: return "Null";
                    }
                case ValueType.Array: return "<Array>";
                case ValueType.Object: return "<Object>";
            }

            return "";
        }
		public string ToJson()
		{
            StringBuilder sb = new StringBuilder();

            switch(Vt)
            {
                case ValueType.Fixed:
                    switch(FixedVal)
                    {
                        case FixedValue.False:
                            sb.Append("false");
                            break;

                        case FixedValue.True:
                            sb.Append("true");
                            break;

                        case FixedValue.Null:
                            sb.Append("null");
                            break;
                    } // switch FixedVal
                    break;

                case ValueType.String:
                    sb.Append("\"").Append(StrVal).Append("\"");
                    break;

                case ValueType.Int:
                    sb.Append(IntVal);
                    break;

                case ValueType.Float:
                    sb.Append(FloatVal.ToString().Replace(',','.'));
                    break;

                case ValueType.Object:
                    sb.Append(ObjectVal.ToJson());
                    break;

                case ValueType.Array:
                    sb.Append("[");

                    if(ArrayVal.Count > 0)
                    {
                        sb.Append(ArrayVal[0].ToJson());

                        for(int i = 1;i < ArrayVal.Count;i++)
                            sb.Append(",").Append(ArrayVal[i].ToJson());
                    }

                    sb.Append("]");
                    break;
            } // switch Vt

            return sb.ToString();
		}
        public JsonObject GetSafeObject()
        {
            if(ObjectVal == null)
                return new JsonObject();

            return ObjectVal;
        }
        public bool IsNull
        {
            get { return Vt == ValueType.Fixed && FixedVal == FixedValue.Null; }
        }
	}
}
