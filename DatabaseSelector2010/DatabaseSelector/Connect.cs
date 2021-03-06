using System;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;

namespace DatabaseSelector
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
            Serializer.CreateInstance();
            UpdateThread ut = UpdateThread.CreateInstance();
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            try
            {
                _addInInstance = (AddIn)addInInst;
                _applicationObject = (DTE2)_addInInstance.DTE;

                switch (connectMode)
                {
                    case ext_ConnectMode.ext_cm_UISetup:
                        break;

                    case ext_ConnectMode.ext_cm_Startup:
                        break;

                    case ext_ConnectMode.ext_cm_AfterStartup:
                        AddTemporaryUI();
                        break;
                }
            }
            catch (System.Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            try
            {
                switch (disconnectMode)
                {
                    case ext_DisconnectMode.ext_dm_HostShutdown:
                    case ext_DisconnectMode.ext_dm_UserClosed:

                        if ((myStandardCommandBarButton != null))
                        {
                            myStandardCommandBarButton.Delete(true);
                        }

                        if ((myToolsCommandBarButton != null))
                        {
                            myToolsCommandBarButton.Delete(true);
                        }

                        break;
                }
            }
            catch (System.Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
            AddTemporaryUI();
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }

        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (commandName == "DatabaseSelector.Connect.DatabaseSelector")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
                else
                {
                    status = vsCommandStatus.vsCommandStatusUnsupported;
                }
            }
        }

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName == "DatabaseSelector.Connect.DatabaseSelector")
                {
                    handled = true;
                    ServerInstanceSelector sis = new ServerInstanceSelector();
                    if (_applicationObject.RegistryRoot.Equals("Software\\Microsoft\\Microsoft SQL Server\\100\\Tools\\Shell"))
                        sis.version = 2008;
                    else
                        sis.version = 2005;
                    sis.Show();
                    return;
                }
            }
        }
        private DTE2 _applicationObject;
        private AddIn _addInInstance;

        private const string MY_COMMAND_NAME = "DatabaseSelector";
        private const string MY_COMMAND_CAPTION = "Database Selector";
        private const string MY_COMMAND_TOOLTIP = "This addin is used to select database instance from TEMC";
        private CommandBarButton myStandardCommandBarButton;
        private CommandBarButton myToolsCommandBarButton;

        public void AddTemporaryUI()
        {
            // The only command that will be created. We will create several buttons from it
            Command myCommand = null;

            // The collection of Visual Studio commandbars
            CommandBars commandBars = null;

            // Built-in commandbars of Visual Studio
            CommandBar standardCommandBar = null;
            CommandBar menuCommandBar = null;
            CommandBar toolsCommandBar = null;

            // Constants for names of built-in commandbars of Visual Studio
            const string VS_STANDARD_COMMANDBAR_NAME = "Standard";
            const string VS_MENUBAR_COMMANDBAR_NAME = "MenuBar";
            const string VS_TOOLS_COMMANDBAR_NAME = "Tools";

            object[] contextUIGuids = new object[] { };

            try
            {
                try
                {
                    myCommand = _applicationObject.Commands.Item(
                        _addInInstance.ProgID + "." + MY_COMMAND_NAME,
                        -1);
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString());
                }

                // Add the command if it does not exist
                if (myCommand == null)
                {
                    myCommand = _applicationObject.Commands.AddNamedCommand(
                        _addInInstance,
                       MY_COMMAND_NAME,
                       MY_COMMAND_CAPTION,
                       MY_COMMAND_TOOLTIP,
                       true,
                       59,
                       ref contextUIGuids,
                       (int)(vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled));
                }

                commandBars = (CommandBars)_applicationObject.CommandBars;

                // Retrieve some built-in commandbars
                standardCommandBar = commandBars[VS_STANDARD_COMMANDBAR_NAME];
                menuCommandBar = commandBars[VS_MENUBAR_COMMANDBAR_NAME];
                toolsCommandBar = GetCommandBarPopup(menuCommandBar, VS_TOOLS_COMMANDBAR_NAME);

                // Add a button to the built-in "Standard" toolbar
                myStandardCommandBarButton = (CommandBarButton)myCommand.AddControl(standardCommandBar,
                   standardCommandBar.Controls.Count + 1);
                myStandardCommandBarButton.Caption = MY_COMMAND_CAPTION;
                myStandardCommandBarButton.Style = MsoButtonStyle.msoButtonIcon;
                myStandardCommandBarButton.BeginGroup = true;

                // Add a button to the built-in "Tools" menu
                myToolsCommandBarButton = (CommandBarButton)myCommand.AddControl(toolsCommandBar);
                myToolsCommandBarButton.Caption = MY_COMMAND_CAPTION;
            }
            catch (System.Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
        }

        private CommandBar GetCommandBarPopup(CommandBar parentCommandBar, string commandBarPopupName)
        {
            CommandBar commandBar = null;
            CommandBarPopup commandBarPopup;

            foreach (CommandBarControl commandBarControl in parentCommandBar.Controls)
            {
                if (commandBarControl.Type == MsoControlType.msoControlPopup)
                {
                    commandBarPopup = (CommandBarPopup)commandBarControl;

                    if (commandBarPopup.CommandBar.Name == commandBarPopupName)
                    {
                        commandBar = commandBarPopup.CommandBar;
                        break;
                    }
                }
            }
            return commandBar;
        }


    }
}