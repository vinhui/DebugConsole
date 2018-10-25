using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebuggingConsole
{
    public static class BasicCommands
    {
        [ConsoleCommand("quit", "Quit the application")]
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }

        [ConsoleCommand("exit", "Close the console")]
        [ConsoleCommand("close", "Close the console")]
        // ReSharper disable once UnusedMember.Local
        private static void HideConsole()
        {
            DebugConsole.Hide();
        }

        #region Graphics

        [ConsoleCommand("graphics.fov", "Set the field of view of the main camera")]
        // ReSharper disable once UnusedMember.Local
        private static void SetFov(float fov)
        {
            if (Camera.main != null)
                Camera.main.fieldOfView = fov;
        }

        [ConsoleCommand("graphics.fov", "Get the field of view of the main camera")]
        // ReSharper disable once UnusedMember.Local
        private static float GetFov()
        {
            return Camera.main != null ? Camera.main.fieldOfView : 0;
        }

        [ConsoleCommand("graphics.fullscreen", "Toggle fullscreen")]
        // ReSharper disable once UnusedMember.Local
        private static void FullScreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

        [ConsoleCommand("graphics.quality", "Get the current quality level")]
        // ReSharper disable once UnusedMember.Local
        private static int GetQuality()
        {
            return QualitySettings.GetQualityLevel();
        }

        [ConsoleCommand("graphics.quality", "Set the quality level")]
        // ReSharper disable once UnusedMember.Local
        private static void SetQuality(int level)
        {
            QualitySettings.SetQualityLevel(level, true);
        }

        [ConsoleCommand("graphics.resolution", "Get the current resolution")]
        // ReSharper disable once UnusedMember.Local
        private static string GetResolution()
        {
            return Screen.currentResolution.ToString();
        }

        [ConsoleCommand("graphics.resolution", "Set the resolution")]
        // ReSharper disable once UnusedMember.Local
        private static void SetResolution(int width, int height)
        {
            Screen.SetResolution(width, height, Screen.fullScreen);
        }

        #endregion Graphics

        [ConsoleCommand("list.hierarchy", "Show the hierarchy tree")]
        private static string ListHierarchy()
        {
            string str = "";
            foreach (Transform obj in Object.FindObjectsOfType(typeof(Transform)))
            {
                if (obj.transform.parent == null)
                {
                    str += ListHierarchy(0, obj);
                }
            }
            return str;
        }

        private static string ListHierarchy(int depth, Transform go)
        {
            string str = "";

            str += string.Format("{0}{1}{2} \n",
                string.Join(
                    "",
                    Enumerable.Repeat("|    ", depth).ToArray()),
                depth > 0 ? "|-- " : "",
                go.name);
            foreach (Transform o in go)
            {
                str += ListHierarchy(depth + 1, o);
            }

            return str;
        }

        [ConsoleCommand("list.components", "List all the components on a game object")]
        private static string ListComponents(string gameObject)
        {
            IEnumerable<GameObject> go = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(obj => obj.name == gameObject);

            string str = "";
            if (!go.Any())
                str = string.Format("There are no objects with the name '{0}'", gameObject);

            foreach (GameObject o in go)
            {
                str += string.Format("Components on '{0}':\n", gameObject);
                foreach (Component component in o.GetComponents<Component>())
                {
                    str += string.Format("- {0}\n", component.GetType().Name);
                }
            }
            return str;
        }

        [ConsoleCommand("list.vars", "List all the variables and values of a component that are serialized")]
        private static string ListVariables(string gameObject, string component)
        {
            GameObject go = GameObject.Find(gameObject);

            if (go == null)
                return string.Format("Can't find game object with the name '{0}'", gameObject);

            Component comp = go.GetComponent(component);

            if (comp == null)
                return string.Format("'{0}' doesn't have a component of type '{1}'", gameObject, component);

            string str = string.Format("Field values of '{0}':\n", component);

            if (comp is Transform)
            {
                Transform transform = comp as Transform;
                str += string.Format("Position".PadRight(35, ' ') + "\t{0}\n" +
                                     "Rotation".PadRight(31, ' ') + "\t{1}\n" +
                                     "Scale".PadRight(35, ' ') + "\t{2}\n",
                    transform.localPosition,
                    transform.localRotation.eulerAngles,
                    transform.localScale);
            }
            else
            {
                FieldInfo[] fields = comp.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                foreach (FieldInfo fieldInfo in fields)
                {
                    if (fieldInfo.IsPrivate && !Attribute.IsDefined(fieldInfo, typeof(SerializeField)))
                        continue;

                    str += string.Format("{0}\t {1}\n", fieldInfo.Name.PadRight(35, ' '), fieldInfo.GetValue(comp));
                }
            }

            return str;
        }
    }
}