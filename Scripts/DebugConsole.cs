using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace DebuggingConsole
{
    /// <summary>
    /// The debug console
    /// </summary>
    public class DebugConsole : MonoBehaviour, ConsoleInput.IConsoleActions
    {
        /// <summary>
        /// Instance of the monobehaviour
        /// </summary>
        private static DebugConsole instance;

        private ConsoleInput consoleInput;

        [SerializeField]
        private bool dontDestroyOnLoad = true;

        [SerializeField]
        private GameObject dontDestroyObject;

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
        private const bool LOG_TO_UNITY = false;

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
        private static List<ConsoleCommand> consoleCommands = new List<ConsoleCommand>();

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
            consoleInput = new ConsoleInput();
            consoleInput.Console.SetCallbacks(this);
            // Set the instance and make sure there aren't multiple instances of this thing
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
                Debug.LogError("The Console Text is not assigned at the DebugConsole", this);
            else
            {
                consoleText.text = cachedText;
                cachedText = null;
            }

            if (consolePanel == null)
                Debug.LogError("The Console Panel is not assigned at the DebugConsole", this);
            if (scrollRect == null)
                Debug.LogError("The ScrollRect is not assigned at the DebugConsole", this);

            if (inputField == null)
                Debug.LogError("The InputField is not assigned at the DebugConsole", this);
            /*else
                inputField.onEndEdit.AddListener(text => Submit());*/

            unityLogHandler = new UnityLogHandler();

            defaultLogHandler = Debug.unityLogger.logHandler;
            //Debug.logger.logHandler = unityLogHandler;
            Application.logMessageReceived += Application_logMessageReceived;

            // And finally, start indexing all the commands in this assembly
            StartCommandIndexing(new[] {Assembly.GetExecutingAssembly()});

            ScrollToBottom();
        }

        private void OnEnable()
        {
            consoleInput.Enable();
        }

        private void OnDisable()
        {
            consoleInput.Disable();
        }

        private static void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (LOG_TO_UNITY)
                defaultLogHandler.LogFormat(LogType.Log, null, stackTrace);
            if (type == LogType.Exception)
            {
                Debug.LogError(condition + Environment.NewLine + stackTrace, null);
            }
        }

        public static void DoAutoCompletion()
        {
            if (instance.inputField.text.Length <= 0) return;
            if (string.IsNullOrWhiteSpace(instance.inputField.text)) return;
            if (instance.inputField.text != instance.lastAutoCompleteInput)
            {
                string input = instance.inputField.text.ToLower();
                string[] matchingCommands = consoleCommands
                    .Distinct()
                    .Where(x => x != null && x.IsMatchForAutoComplete(input))
                    .Select(x => x.command)
                    .ToArray();

                if (matchingCommands.Length == 1)
                {
                    instance.inputField.text = matchingCommands[0];
                    instance.inputField.MoveTextEnd(false);
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
                            instance.inputField.text = matchingCommands[0].Substring(0, matchingTillIndex);
                            instance.inputField.MoveTextEnd(false);
                        }

                        if (matchingCommands.Length > 1)
                        {
                            WriteLine("Available commands:");
                            foreach (string item in matchingCommands)
                                WriteLine("- {0}", item);
                        }
                    }
                    else
                    {
                        WriteLine("There are no matches for auto completion");
                    }
                }
            }

            instance.lastAutoCompleteInput = instance.inputField.text;
        }

        #region Commands stuff

        /// <summary>
        /// Go through the assemblies to search for the ConsoleCommandAttribute
        /// </summary>
        /// <param name="assemblies">Assemblies to include in the search</param>
        public static void StartCommandIndexing(IEnumerable<Assembly> assemblies)
        {
            // Start a timer so we know how long the indexing takes
            Stopwatch s = new Stopwatch();
            s.Start();

            // Loop through the assemblies and index them individually
            foreach (Assembly assembly in assemblies)
            {
                StartCommandIndexing(assembly);
            }

            s.Stop();
            // And show how much time has passed
            WriteLine("Indexed all commands in {0} milliseconds", s.ElapsedMilliseconds);
        }

        /// <summary>
        /// Go through a single assembly to search for the ConsoleCommandAttribute
        /// </summary>
        /// <param name="assembly">Assembly to search in</param>
        public static void StartCommandIndexing(Assembly assembly)
        {
            CommandAttributeIndexer attributeIndexer = new CommandAttributeIndexer(assembly);
            attributeIndexer.StartIndexing();

            if (consoleCommands == null)
                consoleCommands = new List<ConsoleCommand>(attributeIndexer.commands.Count);

            foreach (KeyValuePair<ConsoleCommandAttribute, MethodInfo> item in attributeIndexer.commands)
            {
                consoleCommands.Add(new ConsoleCommand(item.Key, item.Value));
            }
        }

        /// <summary>
        /// Easier access for running a command
        /// </summary>
        /// <param name="cmd">Command to run</param>
        public static void RunCommandStatic(string cmd)
        {
            instance.RunCommand(cmd);
        }

        public static void AddCommand([NotNull] ConsoleCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            consoleCommands.Add(command);
        }

        public static void AddCommands([NotNull] params ConsoleCommand[] commands)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            consoleCommands.AddRange(commands);
        }

        /// <summary>
        /// Run a command
        /// </summary>
        /// <param name="cmd">Command to run</param>
        public void RunCommand(string cmd)
        {
            // Do some cleanup
            cmd = TrimCommand(cmd);

            // Make sure there is something left
            if (string.IsNullOrEmpty(cmd)) return;

            WriteLine("> {0}", cmd);
            currentInputHistory = -1;
            if (inputHistory.Count > 0 && inputHistory[0] != cmd || inputHistory.Count == 0)
                inputHistory.Insert(0, cmd);

            // Split the command in to smaller parts
            string[] text = ConsoleCommand.SplitOnSpaces(cmd);

            // Again, make sure there is something
            if (text.Length == 0) return;

            string enteredCommand = text[0].ToLower();
            string[] parameters = new string[text.Length - 1];
            for (int i = 0; i < parameters.Length; i++)
                parameters[i] = text[i + 1];

            // Find matches for the input from the indexed commands
            List<ConsoleCommand> matchingCommands = consoleCommands
                .Where(x => x.MatchesEnteredCommand(enteredCommand))
                .ToList();

            if (matchingCommands.Count > 0)
            {
                string paramsAsString = cmd.Substring(enteredCommand.Length).Trim();
                ConsoleCommand matchingCommand = matchingCommands
                    .FirstOrDefault(x => x.captureAllParamsAsOne
                        ? x.MatchesParameters(paramsAsString)
                        : x.MatchesParameters(parameters));
                if (matchingCommand != null)
                {
                    matchingCommand.Invoke(enteredCommand,
                        matchingCommand.captureAllParamsAsOne ? new[] {paramsAsString} : parameters);
                }
                else
                {
                    // Show the user what went wrong
                    WriteWarningLine("The amount of parameters doesn't match");
                    string help = matchingCommands.Select(x => x.help)
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

        private static string TrimCommand(string cmd)
        {
            cmd = cmd.Trim();
            cmd = cmd.TrimStart('/', '\\', '`');
            cmd = cmd.Trim();
            return cmd;
        }

        // Show all the commands, and if they contain help info, show that too
        [ConsoleCommand("help", "Shows all the registered commands")]
        [ConsoleCommand("?", "Same as help, shows all the registered commands")]
        [ConsoleCommand("commandslist", "Same as help, shows all the registered commands")]
        // ReSharper disable once UnusedMember.Local
        private void ShowCommandsList()
        {
            foreach (ConsoleCommand command in consoleCommands)
            {
                if (command.excludeFromCommandList)
                    continue;
                WriteLine("\r\n{0}", command);
            }
        }

        #endregion Commands stuff

        /// <summary>
        /// Write and error line to the console
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        [StringFormatMethod("format")]
        public static void WriteErrorLine(string format, params object[] args)
        {
            WriteErrorLine(string.Format(format, args));
        }

        /// <summary>
        /// Write an error line to the console
        /// </summary>
        /// <param name="line">The line to show</param>
        public static void WriteErrorLine(string line)
        {
            WriteLine("<color=red>" + line + "</color>", false);

            if (LOG_TO_UNITY && defaultLogHandler != null)
                defaultLogHandler.LogFormat(LogType.Error, null, "{0}", line);

            // If the console should be shown on errors, now is the time to do so
            if (instance == null || !instance.showOnError) return;

            instance.consolePanel.gameObject.SetActive(true);
            instance.inputField.Select();
            instance.inputField.ActivateInputField();
        }

        /// <summary>
        /// Write a warning to the console, lets hope the user wont ignore it
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        [StringFormatMethod("format")]
        public static void WriteWarningLine(string format, params object[] args)
        {
            WriteWarningLine(string.Format(format, args));
        }

        /// <summary>
        /// Write a warning to the console, lets hope the user wont ignore it
        /// </summary>
        /// <param name="line">The line to show</param>
        public static void WriteWarningLine(string line)
        {
            WriteLine("<color=yellow>" + line + "</color>", false);

            if (LOG_TO_UNITY && defaultLogHandler != null)
                defaultLogHandler.LogFormat(LogType.Warning, null, "{0}", line);
        }

        /// <summary>
        /// Write some text to the console
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        [StringFormatMethod("format")]
        public static void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }

        /// <summary>
        /// Write some text to the console
        /// </summary>
        /// <param name="line">The line to show</param>
        public static void WriteLine(string line)
        {
            WriteLine(line, LOG_TO_UNITY);
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
            if (instance.scrollRect == null) return;

            Canvas.ForceUpdateCanvases();
            instance.scrollRect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Show the debug console
        /// </summary>
        public static void Show()
        {
            if (instance == null) return;

            instance.consolePanel.gameObject.SetActive(true);
            instance.inputField.Select();
            instance.inputField.ActivateInputField();
            instance.inputField.text = string.Empty;
        }

        /// <summary>
        /// Hide the debug console
        /// </summary>
        public static void Hide()
        {
            if (instance == null) return;

            instance.consolePanel.gameObject.SetActive(false);
            instance.inputField.text = string.Empty;
        }

        public void Submit(bool resumeTyping = true)
        {
            RunCommand(inputField.text);

            // Clear the inputfield
            inputField.text = string.Empty;
            if (resumeTyping)
            {
                inputField.Select();
                inputField.ActivateInputField();
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
                inputField.OnDeselect(null);
            }
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (string.IsNullOrEmpty(inputField.text)) return;
            Submit();
        }

        public void OnAutoComplete(InputAction.CallbackContext context)
        {
            if (!isOpen) return;

            DoAutoCompletion();
        }

        public void OnShowHide(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

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

        public void OnDeleteWord(InputAction.CallbackContext context)
        {
            if (!isOpen) return;

            while (inputField.text.Length > 0 &&
                   !char.IsWhiteSpace(inputField.text[inputField.text.Length - 1]) &&
                   !char.IsUpper(inputField.text[inputField.text.Length - 1]) &&
                   !char.IsPunctuation(inputField.text[inputField.text.Length - 1]))
            {
                inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
            }
        }

        public void OnHistoryUp(InputAction.CallbackContext context)
        {
            if (!isOpen) return;

            if (inputHistory.Count > 0)
            {
                currentInputHistory = Mathf.Min(currentInputHistory + 1, inputHistory.Count - 1);
                inputField.text = inputHistory[currentInputHistory];
                inputField.caretPosition = inputField.text.Length;
            }
            else
                inputField.text = string.Empty;
        }

        public void OnHistoryDown(InputAction.CallbackContext context)
        {
            if (!isOpen) return;

            currentInputHistory = Mathf.Max(currentInputHistory - 1, -1);
            if (currentInputHistory == -1)
            {
                inputField.text = string.Empty;
            }
            else
            {
                if (inputHistory.Count <= 0) return;
                inputField.text = inputHistory[currentInputHistory];
                inputField.caretPosition = inputField.text.Length;
            }
        }
    }
}