﻿using SqlScriptRunner.Extensions;
using SqlScriptRunner.ScriptHandler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlScriptRunner.Forms
{
    internal partial class ScriptExecutionMonitorForm : Form
    {
        private Action<ScriptLoader, string> commandAction;        
        private ScriptLoader ScriptLoader;
        private List<Script> ScriptList;
        private delegate void StatusUpdateDelegate(ScriptSection script);

        public ScriptExecutionMonitorForm()
        {
            InitializeComponent();
        }

        public ScriptExecutionMonitorForm(ScriptLoader scriptLoader, Action<ScriptLoader, string> cmdAction): this()
        {
            commandAction = cmdAction;
            ScriptLoader = scriptLoader;
            ScriptList = scriptLoader.SortedFileList().ToList();
        }
        public void StatusUpdate(ScriptSection scriptSection)
        {
            if (this.InvokeRequired)
            {
                var op = new StatusUpdateDelegate(StatusUpdate);
                this.Invoke(op, scriptSection);
                Application.DoEvents(); // give time to UI to make updates
            }
            else
            {
                // update the status on display
                var item = listViewExecution.Items
                    .OfType<ListViewItem>()
                    .FirstOrDefault(ss => ss.Group.Name ==
                                    ScriptList.IndexOf(ScriptList.FirstOrDefault(s => s.FilePath == scriptSection.FilePath)).ToString()
                            && ss.Text == $"GO#{scriptSection.SectionId}");
                if (item != null)
                {
                    // TODO: Use icons instead of colors.
                    switch (scriptSection.Status)
                    {
                        case ExecutionStatusEnum.Running:
                            item.BackColor = Color.Orange;
                            break;
                         case ExecutionStatusEnum.Completed: 
                            item.BackColor = Color.Green;
                            break;
                        case ExecutionStatusEnum.Failed:
                            item.BackColor = Color.Red;
                            break;
                        case ExecutionStatusEnum.Canceled:
                            item.BackColor = Color.WhiteSmoke;
                            break;
                        case ExecutionStatusEnum.Skipped: 
                            item.BackColor = Color.LightGray;
                            break;
                        default:
                            item.BackColor = Color.White;
                            break;
                    }
                    if (scriptSection.MessageLog != null)
                    {
                        item.ToolTipText = string.Join("\r\n", scriptSection.MessageLog);
                    }
                    item.EnsureVisible();
                }
                // if all script has a status than it is completed
                if (ScriptLoader.IsCompleted)
                {
                    SetSettingsAccess(true);
                    labelStatusText.Text = "Completed";
                }
                else
                {
                    labelStatusText.Text = "In Progress";
                }
                Application.DoEvents(); // give time to UI to make updates
            }
        }
        
        private void SetSettingsAccess(bool enabled)
        {
            // collect all settings to enable and disable:
            buttonStart.Enabled = enabled;
            checkBoxAllowTransaction.Enabled = enabled;
            checkBoxSkipIfExists.Enabled = enabled;
        }

        private void ScriptExecutionMonitorForm_Load(object sender, EventArgs e)
        {
            try
            {
                // populate ListView execution
                if (ScriptList?.Any() ?? false) 
                {
                    listViewExecution.BeginUpdate();
                    foreach(Script script in ScriptList)
                    {
                        var group = new ListViewGroup(ScriptList.IndexOf(script).ToString(), script.Name)
                            { HeaderAlignment = HorizontalAlignment.Center };
                        listViewExecution.Groups.Add(group);
                        foreach(ScriptSection scriptSection in script.ScriptSections)
                        {
                            var item = new ListViewItem($"GO#{scriptSection.SectionId}", group) 
                            { 
                                Checked = !scriptSection.ToBeSkipped,
                                Tag = scriptSection,
                            };                            
                            listViewExecution.Items.Add(item);
                            if (!string.IsNullOrEmpty(scriptSection.ObjectName))
                            {
                                item.SubItems.Add($"[{scriptSection.ObjectSchema}].[{scriptSection.ObjectName}] ({scriptSection.ObjectTypeName})");
                            }
                            else
                            {
                                item.SubItems.Add(scriptSection.ObjectTypeName.ToString());
                            }
                        }
                    }
                    listViewExecution.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    listViewExecution.EndUpdate();
                }
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            try
            {
                commandAction?.Invoke(ScriptLoader, ScriptCommandEnum.Cancel.ToString());
                this.Close();
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            try
            {
                commandAction?.Invoke(ScriptLoader, ScriptCommandEnum.Start.ToString());
                // do not change the transaction state when it is running
                SetSettingsAccess(false);                
                
                labelStatusText.Text = "Started";
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void checkBoxAllowTransaction_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                commandAction?.Invoke(ScriptLoader, $"{ScriptCommandEnum.Transaction}:{checkBoxAllowTransaction.Checked}");
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void listViewExecution_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                // Prevent to change checked state if script execution is ongoing
                if (!buttonStart.Enabled)
                {
                    e.NewValue = e.CurrentValue;
                }
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void listViewExecution_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            try
            {
                if (e.Item.Tag is ScriptSection scriptSection)
                {
                    commandAction?.Invoke(ScriptLoader, $"{ScriptCommandEnum.SetSkip}:{scriptSection.Script.Guid}:{scriptSection.SectionId}:{!e.Item.Checked}");
                }
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }

        private void checkBoxSkipIfExists_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                commandAction?.Invoke(ScriptLoader, $"{ScriptCommandEnum.SkipIfExists}:{checkBoxSkipIfExists.Checked}");
            }
            catch (Exception ex)
            {

                this.ShowErrorMessage(ex);
            }
        }
    }
}
