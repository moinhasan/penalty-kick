using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace GamedockSDK.Unity.Json
{
    /// <summary>
    /// Use this class to turn your JSON strings into objects and vice versa.
    /// Check the JSONHelper.cs file in the Helpers directory for an example of 
    /// how to do this with your game config!
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// If you have a JSON string then you can design a class that mirrors the structure of the JSON.
        /// You can then call this method and pass the JSON string as a parameter and your class as T.
        /// Check the JSONHelper.cs file in the Helpers directory for an example of how to do this with your
        /// game config!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns>A new instance (object) of your class containing. all the values from the JSON data</returns>
        public static T getObjectFromJson<T>(string jsonString) where T : new()
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch (System.MissingMethodException ex)
            {
                Debug.Log(
                    "Something has gone wrong while loading NewtonSoft.json. Most likely the file \"link.xml\" is missing from the /Assets directory of your Unity project. Please copy the \"link.xml\" file included with the Unity GamedockSDK download to the /Assets folder of your project.\n" +
                    ex.Message);
                throw new System.Exception();
            }
            catch (System.Exception ex)
            {
                Debug.Log(
                    "Something has gone wrong while loading NewtonSoft.json or parsing the supplied JSON. Exception: " +
                    ex.Message);
                throw new System.Exception();
            }
        }

        /// <summary>
        /// Turns an object into a JSON string so it can be easily stored or sent to another application.
        /// </summary>
        /// <param name="_object"></param>
        /// <returns></returns>
        public static string getJSONFromObject(object _object)
        {
            try
            {
                return JsonConvert.SerializeObject(_object);
            }
            catch (System.MissingMethodException ex)
            {
                Debug.Log(
                    "Something has gone wrong while loading NewtonSoft.json. Most likely the file \"link.xml\" is missing from the /Assets directory of your Unity project. Please copy the \"link.xml\" file included with the Unity GamedockSDK download to the /Assets folder of your project.\n" +
                    ex.Message);
                throw new System.Exception();
            }
            catch (System.Exception ex)
            {
                Debug.Log(
                    "Something has gone wrong while loading NewtonSoft.json or parsing the supplied JSON. Exception: " +
                    ex.Message);
                throw new System.Exception();
            }
        }

        public static JSONObject DictToJSONObject(IDictionary<string, object> dict)
        {
            JSONObject jsonObject = new JSONObject(JSONObject.Type.OBJECT);

            foreach (KeyValuePair<string, object> kvp in dict)
            {
                if (kvp.Value != null)
                {
                    if (kvp.Value is Dictionary<string, object>)
                    {
                        JSONObject childNode = DictToJSONObject((Dictionary<string, object>) kvp.Value);
                        jsonObject.AddField(kvp.Key, childNode);
                    }
                    else if (kvp.Value is JSONObject)
                    {
                        jsonObject.AddField(kvp.Key, (JSONObject) kvp.Value);
                    }
                    else if (kvp.Value is List<string> || kvp.Value is List<bool> || kvp.Value is List<float> ||
                             kvp.Value is List<int> || kvp.Value is string[] || kvp.Value is bool[] ||
                             kvp.Value is float[] || kvp.Value is int[])
                    {
                        string itemsListJSON = getJSONFromObject(kvp.Value);
                        jsonObject.AddField(kvp.Key, itemsListJSON);
                    }
                    else
                    {
                        if (kvp.Value is string)
                        {
                            jsonObject.AddField(kvp.Key, (string) kvp.Value);
                        }
                        else if (kvp.Value is bool)
                        {
                            jsonObject.AddField(kvp.Key, (bool) kvp.Value);
                        }
                        else if (kvp.Value is float)
                        {
                            jsonObject.AddField(kvp.Key, (float) kvp.Value);
                        }
                        else if (kvp.Value is int)
                        {
                            jsonObject.AddField(kvp.Key, (int) kvp.Value);
                        }
                        else if (kvp.Value is long)
                        {
                            jsonObject.AddField(kvp.Key, (long) kvp.Value);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError(
                                "Tried to add a value with an illegal type to a JSONObject: Key \"" + kvp.Key +
                                "\" with value type " + kvp.Value.GetType() +
                                ". Only values of type string, bool, float, int, JSONOBject or DIctionary<string, object> are allowed.");
                        }
                    }
                }
                else
                {
                    jsonObject.AddField(kvp.Key, (string) null);
                }
            }

            return jsonObject;
        }

        public static string SortJSONAlphabetically(string jsonString)
        {
            return SortJsonNodesAlphabetically(new JSONObject(jsonString)).ToString();
        }

        public static JSONObject SortJsonNodesAlphabetically(JSONObject jsonObject)
        {
            if (jsonObject.IsArray)
            {
                List<JSONObject> newJsonObjectList = new List<JSONObject>();
                foreach (JSONObject field in jsonObject.list)
                {
                    if (field.isContainer || field.IsObject || field.IsArray)
                    {
                        JSONObject newField = SortJsonNodesAlphabetically(field);
                        newJsonObjectList.Add(newField);
                    }
                    else
                    {
                        newJsonObjectList.Add(field);
                    }
                }

                jsonObject.list = newJsonObjectList;
                return jsonObject;
            }
            else
            {
                SortedDictionary<string, object> jsonAsObjectDict = new SortedDictionary<string, object>();
                foreach (string key in jsonObject.keys)
                {
                    JSONObject field = jsonObject.GetField(key);

                    if (field.isContainer || field.IsObject || field.IsArray)
                    {
                        field = SortJsonNodesAlphabetically(field);
                    }

                    jsonAsObjectDict.Add(key, field);
                }

                return DictToJSONObject(jsonAsObjectDict);
            }
        }
    }
}