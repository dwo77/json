# dwo.Json
dwo.Json is a lightweight Json parser, if you just need to read or write a few values and
do not want to integrate a full library.

## License
Use at your own risk! Do what you want with it.

## Usage
Find your way around using `JsonObject` and `JsonValue`. Every object consists of a dictionary
of values, which may be an object again, or just a string value or an array of values.

### JsonObject

__GetValue(string)__ Returns the JsonValue with the given name or a new one if not found.  
__SetValue(string,JsonValue)__ Sets the value with the given name.  
__Parse(string)__ Parses the given string. Throws a basic `Exception` on error.  
__ToJson()__ Outputs the whole object tree as Json.  
__ValuesAsDict(Dictionary<string,string>)__ Fills the dictionary with formatted values.

### JsonValue

__ToString()__ Output the formatted value.  
__GetSafeObject()__ If the value is not an object, this will return a new JsonValue. Used for
chaining calls to travel nested objects:
`jo.GetValue('nested1').GetSafeObject().GetValue('nested2').GetSafeObject()`...
As `GetValue` returns a new `JsonValue` if not found, so this construction may not fail. 

### More

Look at the example and / or read the source code!
