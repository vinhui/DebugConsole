using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace DebugConsole
{
    /// <summary>
    /// The debug console
    /// </summary>
    internal class DebugConsole : MonoBehaviour
    {
        /// <summary>
        /// Instance of the monobehaviour
        /// </summary>
        private static DebugConsole instance;

        [SerializeField]
        private bool dontDestroyOnLoad = true;

        [SerializeField]
        private GameObject dontDestroyObject;

        /// <summary>
        /// The key to press to toggle the debug console
        /// </summary>
        [SerializeField]
        private KeyCode toggleKey = KeyCode.BackQuote;

        /// <summary>
        /// The textfield that should have all the messages
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI consoleText;

        /// <summary>
        /// The panel that contains everything
        /// This is the panel that gets enabled and disabled
        /// </summary>
        [SerializeField]
        private RectTransform consolePanel;

        /// <summary>
        /// The scrollrect that contains the ConsoleText
        /// </summary>
        [SerializeField]
        private ScrollRect scrollRect;

        /// <summary>
        /// The inputfield where we can type commands
        /// </summary>
        [SerializeField]
        [Tooltip("The inputfield where we can type in all the commands. This will add all the listeners needed.")]
        private TMP_InputField inputField;

        /// <summary>
        /// Should the console pop up when there is an error
        /// </summary>
        [SerializeField]
        [Tooltip("Should the console pop up when there is an error")]
        private bool showOnError;

        /// <summary>
        /// Should all messages send to the console, also be send to the unity console?
        /// </summary>
        internal static bool logToUnity = false;

        /// <summary>
        /// Field that contains all the text that is send to the console before the console is actually loaded
        /// </summary>
        private static string cachedText;

        /// <summary>
        /// Is the console open?
        /// </summary>
        public static bool isOpen
        {
            get { return instance != null && instance.consolePanel.gameObject.activeSelf; }
        }

        /// <summary>
        /// All the commands and the method to call are stored here
        /// </summary>
        private Dictionary<ConsoleCommandAttribute, MethodInfo> consoleCommands;

        private static ILogHandler defaultLogHandler;
        private static UnityLogHandler unityLogHandler;

        /// <summary>
        /// List of all the user inputs in order of new to old
        /// </summary>
        private readonly List<string> inputHistory = new List<string>();

        private int currentInputHistory = -1;
        private string lastAutoCompleteInput = string.Empty;

        private void Awake()
        {
            // Set the instance and make sure there arent multiple instances of this thing
            if (instance == null)
            {
                instance = this;
                if (dontDestroyOnLoad)
                    DontDestroyOnLoad(dontDestroyObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Do some boring null checks
            if (consoleText == null)
                Debug.LogError("The Console Text is not assinged at the DebugConsole", this);
            else
            {
                consoleText.text = cachedText;
                cachedText = null;
            }

            if (consolePanel == null)
                Debug.LogError("The Console Panel is not assinged at the DebugConsole", this);
            if (scrollRect == null)
                Debug.LogError("The ScrollRect is not assinged at the DebugConsole", this);

            if (inputField == null)
                Debug.LogError("The InputField is not assinged at the DebugConsole", this);
            else
                inputField.onEndEdit.AddListener(RunCommand);

            unityLogHandler = new UnityLogHandler();

            defaultLogHandler = Debug.logger.logHandler;
            //Debug.logger.logHandler = unityLogHandler;
            Application.logMessageReceived += Application_logMessageReceived;

            // And finally, start indexing all the commands in this assembly
            StartCommandIndexing(new[] {Assembly.GetExecutingAssembly()});

            ScrollToBottom();
        }

        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (logToUnity)
                defaultLogHandler.LogFormat(LogType.Log, null, stackTrace);
            if (type == LogType.Exception)
            {
                Debug.LogError(condition + Environment.NewLine + stackTrace, null);
            }
        }

        #region Handle keyboard input

        private void Update()
        {
            InputHandleToggle();
            InputHandleArrows();
            InputHandleAutoCompletion();
            InputHandleCtrlBackspace();
        }

        private void InputHandleAutoCompletion()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                DoAutocompletion();
            }
        }

        public void DoAutocompletion()
        {
            if (inputField.text.Length > 0)
            {
                if (inputField.text != lastAutoCompleteInput)
                {
                    string[] matchingCommands = consoleCommands
                        .Select(x => x.Key.command)
                        .Distinct()
                        .Where(x => x.ToLower().StartsWith(inputField.text.ToLower()))
                        .ToArray();

                    if (matchingCommands.Length == 1)
                    {
                        inputField.text = matchingCommands[0];
                        inputField.MoveTextEnd(false);
                    }
                    else
                    {
                        if (matchingCommands.Length > 0)
                        {
                            int matchingTillIndex = Enumerable
                                .Range(0, matchingCommands.Min(x => x.Length))
                                .Count(i => matchingCommands.All(x => x[i] == matchingCommands[0][i]));

                            if (matchingTillIndex > 0)
                            {
                                inputField.text = matchingCommands[0].Substring(0, matchingTillIndex);
                                inputField.MoveTextEnd(false);
                            }

                            if (matchingCommands.Length > 1)
                            {
                                WriteLine("Available commands:");
                                foreach (var item in matchingCommands)
                                    WriteLine("- " + item);
                            }
                        }
                        else
                        {
                            WriteLine("There are no matches for autocompletion");
                        }
                    }
                }

                lastAutoCompleteInput = inputField.text;
            }
        }

        private void InputHandleCtrlBackspace()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Backspace))
            {
                while (inputField.text.Length > 0 &&
                       !char.IsWhiteSpace(inputField.text[inputField.text.Length - 1]) &&
                       !char.IsUpper(inputField.text[inputField.text.Length - 1]) &&
                       !char.IsPunctuation(inputField.text[inputField.text.Length - 1]))
                {
                    inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
                }
            }
        }

        private void InputHandleArrows()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (inputHistory.Count > 0)
                {
                    currentInputHistory = Mathf.Min(currentInputHistory + 1, inputHistory.Count - 1);
                    inputField.text = inputHistory[currentInputHistory];
                    inputField.caretPosition = inputField.text.Length;
                }
                else
                    inputField.text = string.Empty;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentInputHistory = Mathf.Max(currentInputHistory - 1, -1);
                if (currentInputHistory == -1)
                {
                    inputField.text = string.Empty;
                }
                else
                {
                    if (inputHistory.Count > 0)
                    {
                        inputField.text = inputHistory[currentInputHistory];
                        inputField.caretPosition = inputField.text.Length;
                    }
                }
            }
        }

        private void InputHandleToggle()
        {
            // Toggle the visibility of the console when the tilde key is pressed
            if (Input.GetKeyDown(toggleKey))
            {
                bool newValue = !consolePanel.gameObject.activeSelf;
                consolePanel.gameObject.SetActive(newValue);

                inputField.text = string.Empty;

                if (newValue)
                {
                    // Activate the input when the console is shown
                    inputField.Select();
                    inputField.ActivateInputField();
                }
            }
        }

        #endregion Handle keyboard input

        #region Commands stuff

        /// <summary>
        /// Go through the assemblies to search for the ConsoleCommandAttribute
        /// </summary>
        /// <param name="assemblies">Assemblies to include in the search</param>
        private void StartCommandIndexing(Assembly[] assemblies)
        {
            // Start a timer so we know how long the indexing takes
            Stopwatch s = new Stopwatch();
            s.Start();

            // Clear the already set commands
            consoleCommands = new Dictionary<ConsoleCommandAttribute, MethodInfo>();

            // Loop through the assemblies and index them individually
            foreach (Assembly assembly in assemblies)
            {
                StartCommandIndexing(assembly);
            }
            s.Stop();
            // And show how much time has passed
            WriteLine("Indexed all commands in " + s.ElapsedMilliseconds + " milliseconds");
        }

        /// <summary>
        /// Go through a single assembly to search for the ConsoleCommandAttribute
        /// </summary>
        /// <param name="assembly">Assembly to search in</param>
        private void StartCommandIndexing(Assembly assembly)
        {
            // Just a null check, you never know
            if (consoleCommands == null)
                consoleCommands = new Dictionary<ConsoleCommandAttribute, MethodInfo>();

            CommandIndexer indexer = new CommandIndexer(assembly);
            indexer.StartIndexing();
            foreach (KeyValuePair<ConsoleCommandAttribute, MethodInfo> item in indexer.commands)
            {
                consoleCommands.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Easier access for running a command
        /// </summary>
        /// <param name="cmd">Command to run</param>
        internal static void RunCommandStatic(string cmd)
        {
            instance.RunCommand(cmd);
        }

        /// <summary>
        /// Run a command
        /// </summary>
        /// <param name="cmd">Command to run</param>
        internal void RunCommand(string cmd)
        {
            // Do some cleanup
            cmd = cmd.Trim();
            cmd = cmd.TrimStart('/', '\\', '`');
            cmd = cmd.Trim();

            // Make sure there is something left
            if (!string.IsNullOrEmpty(cmd))
            {
                WriteLine("> " + cmd);
                currentInputHistory = -1;
                if ((inputHistory.Count > 0 && inputHistory[0] != cmd) || inputHistory.Count == 0)
                    inputHistory.Insert(0, cmd);

                // Split the command in to smaller parts
                string[] text = cmd.Split('"')
                    .Select((element, index) => index % 2 == 0 // If even index
                        ? element.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries) // Split the item
                        : new[] {element}) // Keep the entire item
                    .SelectMany(element => element)
                    .ToArray();

                // Again, make sure there is something
                if (text.Length > 0)
                {
                    // Find matches for the input from the indexed commands
                    Dictionary<ConsoleCommandAttribute, MethodInfo> commands = consoleCommands
                        .Where(x => x.Key.command.ToLower() == text[0].ToLower())
                        .ToDictionary(x => x.Key, x => x.Value);

                    // Are there any commands?
                    if (commands.Count > 0)
                    {
                        // Are there parameters, and if so, does the input have the same amount?
                        if (commands.Any(x => x.Value.GetParameters().Length == text.Length - 1))
                        {
                            // There might be multiple ConsoleCommandAttributes on the same method, just pick the first one
                            KeyValuePair<ConsoleCommandAttribute, MethodInfo> command =
                                commands.FirstOrDefault(x => x.Value.GetParameters().Length == text.Length - 1);

                            try
                            {
                                object[] parameters = new object[text.Length - 1];
                                int i = 0;
                                // Fill the parameters from the input and do casting of the text
                                foreach (ParameterInfo parameterInfo in command.Value.GetParameters())
                                {
                                    try
                                    {
                                        if (parameterInfo.ParameterType == typeof(object))
                                            parameters[i] = text[i + 1];
                                        else
                                            parameters[i] =
                                                parameterInfo.ParameterType.IsEnum
                                                    ? Enum.Parse(parameterInfo.ParameterType, text[i + 1], true)
                                                    : Convert.ChangeType(text[i + 1], parameterInfo.ParameterType);
                                    }
                                    catch (InvalidCastException)
                                    {
                                        WriteErrorLine("Failed to cast " + text[i + 1] + " to " +
                                                       parameterInfo.ParameterType);
                                    }
                                    i++;
                                }

                                // Call the method with the parameters
                                object returnValue;
                                if (command.Value.DeclaringType.IsSubclassOf(typeof(MonoBehaviour)))
                                {
                                    returnValue =
                                        command.Value.Invoke(FindObjectOfType(command.Value.DeclaringType),
                                            parameters);
                                }
                                else
                                    returnValue = command.Value.Invoke(null, parameters);

                                // If theres a return value, output it to the console
                                if (returnValue != null)
                                    WriteLine(returnValue.ToString());
                            }
                            catch (Exception e)
                            {
                                // Show the user running the command failed and for what reason
                                WriteErrorLine("Failed to run command: ");
                                if (e is TargetParameterCountException)
                                    WriteErrorLine("Parameters are invalid");
                                else
                                {
                                    WriteErrorLine(e.Message);
                                    throw;
                                }
                                // Show the help info for the command if its available
                                if (!string.IsNullOrEmpty(command.Key.help))
                                {
                                    WriteLine(command.Key.help);
                                }
                            }
                        }
                        else
                        {
                            // Show the user what went wrong
                            WriteWarningLine("The amount of parameters doesnt match");
                            string help = commands.Select(x => x.Key.help)
                                .FirstOrDefault(x => !string.IsNullOrEmpty(x));
                            if (!string.IsNullOrEmpty(help))
                            {
                                WriteLine(help);
                            }
                        }
                    }
                    else
                    {
                        WriteWarningLine("Unknown command");
                    }
                }
            }

            // Clear the inputfield
            inputField.text = string.Empty;
            inputField.Select();
            inputField.ActivateInputField();
        }

        // Show all the commands, and if they contain help info, show that too
        [ConsoleCommand("help", "Shows all the registered commands")]
        [ConsoleCommand("?", "Same as help, shows all the registered commands")]
        [ConsoleCommand("commandslist", "Same as help, shows all the registered commands")]
        // ReSharper disable once UnusedMember.Local
        private void ShowCommandsList()
        {
            foreach (ConsoleCommandAttribute attribute in instance.consoleCommands.Keys)
            {
                if (attribute.excludeFromCommandList)
                    continue;
                WriteLine("\r\n" + attribute.command + "\r\n\t" + attribute.help);
            }
        }

        #endregion Commands stuff

        /// <summary>
        /// Write an error line to the console
        /// </summary>
        /// <param name="line">The line to show</param>
        public static void WriteErrorLine(string line)
        {
            WriteLine("<color=red>" + line + "</color>", false);

            if (logToUnity && defaultLogHandler != null)
                defaultLogHandler.LogFormat(LogType.Error, null, "{0}", line);

            // If the console should be shown on errors, nows the time to do so
            if (instance != null && instance.showOnError)
            {
                instance.consolePanel.gameObject.SetActive(true);
                instance.inputField.Select();
                instance.inputField.ActivateInputField();
            }
        }

        /// <summary>
        /// Write a warning to the console, lets hope the user wont ignore it
        /// </summary>
        /// <param name="line">The line to show</param>
        public static void WriteWarningLine(string line)
        {
            WriteLine("<color=yellow>" + line + "</color>", false);

            if (logToUnity && defaultLogHandler != null)
                defaultLogHandler.LogFormat(LogType.Warning, null, "{0}", line);
        }

        /// <summary>
        /// Write some text to the console
        /// </summary>
        /// <param name="line">The line to show</param>
        public static void WriteLine(string line)
        {
            WriteLine(line, logToUnity);
        }

        private static void WriteLine(string line, bool logToUnity)
        {
            line += Environment.NewLine;

            Write(line);

            if (logToUnity && defaultLogHandler != null)
                defaultLogHandler.LogFormat(LogType.Log, null, "{0}", line);
        }

        /// <summary>
        /// Write some text to the console without adding the new line character
        /// </summary>
        /// <param name="text">Text to show</param>
        public static void Write(string text)
        {
            if (instance == null)
                cachedText += text;
            else
            {
                if (instance.consoleText != null)
                {
                    if (instance.consoleText.text == null)
                        instance.consoleText.text = string.Empty;

                    if (!instance.consoleText.text.EndsWith(text))
                    {
                        instance.consoleText.text += text;
                    }
                }

                ScrollToBottom();
            }
        }

        private static void ScrollToBottom()
        {
            if (instance.scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                instance.scrollRect.verticalNormalizedPosition = 0f;
                Canvas.ForceUpdateCanvases();
            }
        }

        /// <summary>
        /// Show the debug console
        /// </summary>
        public static void Show()
        {
            if (instance != null)
            {
                instance.consolePanel.gameObject.SetActive(true);
                instance.inputField.Select();
                instance.inputField.ActivateInputField();
                instance.inputField.text = string.Empty;
            }
        }

        /// <summary>
        /// Hide the debug console
        /// </summary>
        public static void Hide()
        {
            if (instance != null)
            {
                instance.consolePanel.gameObject.SetActive(false);
                instance.inputField.text = string.Empty;
            }
        }
    }
}