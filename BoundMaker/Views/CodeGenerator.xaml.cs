﻿using BoundMaker.Models.Triggers;
using BoundMaker.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace BoundMaker.Views
{
    public partial class CodeGenerator : DockPanel
    {
        public CodeGenerator()
        {
            InitializeComponent();
        }

        private void LineBreakTextChangedEventHandler(object sender, TextChangedEventArgs e)
        {
            Line_Break.Text = Regex.Replace(Line_Break.Text, "[^0-9]", "");
            if (Line_Break.Text.Length > 0)
            {
                int i = int.Parse(Line_Break.Text);
                if (i > 999)
                {
                    Line_Break.Text = "999";
                }
            }
        }

        private void GenerateButtonClickedEventHandler(object sender, RoutedEventArgs e)
        {
            if (CodeType_Scmdraft.IsChecked == true)
            {
                GenerateTextTriggers(new ScmdraftTriggerGenerator());
            }
            else if (CodeType_Starforge.IsChecked == true)
            {
                GenerateTextTriggers(new StarforgeTriggerGenerator());
            }
            //else if (CodeType_SomeOtherEditor.IsChecked == true)
            //{
            //    GenerateTextTriggers(new SomeOtherEditorTriggerGenerator());
            //}
        }

        private void GenerateTextTriggers(ITriggerGenerator triggerGenerator)
        {
            var triggers = new StringBuilder();
            int lineBreak;
            if (Line_Break.Text.Length == 0 || int.Parse(Line_Break.Text) == 0)
            {
                lineBreak = 9999999;
            }
            else
            {
                lineBreak = int.Parse(Line_Break.Text);
            }

            var actions = new List<string>();
            foreach (Models.BoundSequence sequence in GlobalState.Sequences)
            {
                foreach (Models.MapLocation location in sequence.States.Keys.Where(x => sequence.States[x] != "default"))
                {
                    actions.Add(triggerGenerator.CreateUnit(Code_Unit_Owner.GetSelectionContent(), sequence.GetExplosionUnitName(location), location.LocationName));
                    actions.Add(triggerGenerator.KillUnit(location.LocationName));
                }
                actions.Add(triggerGenerator.Wait(sequence.WaitTime));
            }

            for (int i = 1; i <= actions.Count; i++)
            {
                triggers.AppendLine(actions[i - 1]);
                if (i % lineBreak == 0)
                {
                    if (Add_Preserve.IsChecked == true)
                    {
                        triggers.AppendLine(triggerGenerator.PreserveTrigger());
                    }

                    triggers.AppendLine();
                }
            }
            if (Add_Preserve.IsChecked == true && (lineBreak > actions.Count || lineBreak % actions.Count != 0))
            {
                triggers.AppendLine(triggerGenerator.PreserveTrigger());
            }

            Code_Output.Text = triggers.ToString();
        }
    }
}
