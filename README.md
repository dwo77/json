# dwo.Json
dwo.Json is a lightweight Json parser, if you just need to read or write a few values and
do not want to integrate a full library.

## License
Use at your own risk! Do what you want with it.

## Usage
Find your way around using `JsonObject` and `JsonValue`. Every object consists of a dictionary
of values, which may be an object again, or an array of values or just a simple value like
a string or a number.

### JsonObject

| Method | Description |
| :--- | :--- |
| GetValue(string) | Returns the JsonValue with the given name or a new one if not found.  |
| SetValue(string,JsonValue) | Sets the value with the given name. |
| Parse(string) | Parses the given string. Throws a basic `Exception` on error. |
| ToJson() | Outputs the whole object tree as Json. |
| ValuesAsDict(Dictionary<string,string>) | Fills the dictionary with formatted values. |

### JsonValue
This class works somewhat like a variant. There are public members for the value, constructors
for all the possible content types and a `ValueType` enum to detect which type is currently stored.

| Method | Description |
| :--- | :--- |
| ToString() | Output the formatted value. |
| GetSafeObject() | If the value is not an object, this will return a new JsonValue. Used for chaining calls to travel nested objects: `jo.GetValue('nested1').GetSafeObject().GetValue('nested2').GetSafeObject()`...  As `GetValue` returns a new `JsonValue` if not found, so this construction may not fail. |

### More

Look at the example and / or read the source code!
