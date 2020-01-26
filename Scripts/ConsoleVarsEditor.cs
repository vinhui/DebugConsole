using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebuggingConsole
{
    public static class ConsoleVarsEditor
    {
        [ConsoleCommand("var", "Get or set the value of a variable on a component")]
        // ReSharper disable once UnusedMember.Local
        private static object GetVar(string gameObject, string component, string var)
        {
            var go = GetGameObject(gameObject);

            if (go != null)
            {
                var c = GetComponent(go, component);

                if (c != null)
                {
                    return GetVarOnUObject(c, var);
                }
            }

            return null;
        }

        [ConsoleCommand("var", "Get or set the value of a variable on a component")]
        // ReSharper disable once UnusedMember.Local
        private static void SetVar(string gameObject, string component, string var, object value)
        {
            var go = GetGameObject(gameObject);

            if (go != null)
            {
                var c = GetComponent(go, component);

                if (c != null)
                {
                    SetVarOnUObject(c, var, value);
                }
            }
        }

        [ConsoleCommand("call", "Call a method that's on a component")]
        // ReSharper disable once UnusedMember.Local
        private static object Call(string gameObject, string component, string method)
        {
            return CallOverloads(gameObject, component, method, null, null, null, null);
        }

        [ConsoleCommand("call", "Call a method that's on a component")]
        // ReSharper disable once UnusedMember.Local
        private static object Call(string gameObject, string component, string method, string arg1)
        {
            return CallOverloads(gameObject, component, method, arg1, null, null, null);
        }

        [ConsoleCommand("call", "Call a method that's on a component")]
        // ReSharper disable once UnusedMember.Local
        private static object Call(string gameObject, string component, string method, string arg1, string arg2)
        {
            return CallOverloads(gameObject, component, method, arg1, arg2, null, null);
        }

        [ConsoleCommand("call", "Call a method that's on a component")]
        // ReSharper disable once UnusedMember.Local
        private static object Call(string gameObject, string component, string method, string arg1, string arg2, string arg3)
        {
            return CallOverloads(gameObject, component, method, arg1, arg2, arg3, null);
        }

        [ConsoleCommand("call", "Call a method that's on a component")]
        // ReSharper disable once UnusedMember.Local
        private static object Call(string gameObject, string component, string method, string arg1, string arg2, string arg3, string arg4)
        {
            return CallOverloads(gameObject, component, method, arg1, arg2, arg3, arg4);
        }

        private static object CallOverloads(string gameObject, string component, string method, string arg1, string arg2, string arg3, string arg4)
        {
            var go = GetGameObject(gameObject);

            if (go != null)
            {
                var c = GetComponent(go, component);

                if (c != null)
                {
                    return CallMethodOnComponent(c, method, arg1, arg2, arg3, arg4);
                }
            }

            return null;
        }

        private static GameObject GetGameObject(string name)
        {
            var go = GameObject.Find(name);

            if (go == null)
                DebugConsole.WriteErrorLine("There is no game object with the name '" + name + "' in the scene");

            return go;
        }

        private static Component GetComponent(GameObject go, string name)
        {
            var c = go.GetComponent(name);

            if (c == null)
                DebugConsole.WriteErrorLine("There is no component called '" + name + "' on game object '" + go.name + "'");

            return c;
        }

        private static object CallMethodOnComponent(Component c, string name, params object[] @params)
        {
            var cType = c.GetType();

            var method = cType.GetMethod(name, BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (method != null)
            {
                var nonNullParams = @params.Where(x => x != null).ToArray();

                var paramsInfo = method.GetParameters();

                if (nonNullParams.Length == paramsInfo.Length)
                {
                    for (var i = 0; i < paramsInfo.Length; i++)
                    {
                        nonNullParams[i] = Convert.ChangeType(nonNullParams[i], paramsInfo[i].ParameterType);
                    }

                    return method.Invoke(c, nonNullParams);
                }
                else
                    DebugConsole.WriteErrorLine("The method you're trying to call has " + paramsInfo.Length + " parameters, not " + nonNullParams.Length);
            }
            else
                DebugConsole.WriteErrorLine("There is no method '" + name + "' on component '" + c.name + "'");

            return null;
        }

        private static void SetVarOnUObject(Object c, string name, object value)
        {
            var cType = c.GetType();
            PropertyInfo property = null;
            var field = cType.GetField(name, BindingFlags.Instance | BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (field == null)
                property = cType.GetProperty(name, BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (field != null || property != null)
            {
                try
                {
                    if (field != null)
                        field.SetValue(c, Convert.ChangeType(value, field.FieldType));
                    else
                        property.SetValue(c, Convert.ChangeType(value, property.PropertyType), null);
                }
                catch (InvalidCastException)
                {
                    DebugConsole.WriteErrorLine("Failed to cast the value to the right type");
                }
                return;
            }

            DebugConsole.WriteErrorLine("There is no var called '" + name + "' on component '" + c.name + "'");
        }

        private static object GetVarOnUObject(Object c, string name)
        {
            var cType = c.GetType();
            PropertyInfo property = null;
            var field = cType.GetField(name, BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (field == null)
                property = cType.GetProperty(name, BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (field != null || property != null)
            {
                if (field != null)
                    return field.GetValue(c);
                else
                    return property.GetValue(c, null);
            }

            DebugConsole.WriteErrorLine("There is no var called '" + name + "' on component '" + c.name + "'");
            return null;
        }
    }
}