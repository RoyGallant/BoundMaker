using BoundMaker.Models.Triggers;
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
            LineBreakCount.Text = Regex.Replace(LineBreakCount.Text, "[^0-9]", "");
            if (LineBreakCount.Text.Length > 0)
            {
                int i = int.Parse(LineBreakCount.Text);
                if (i > 999)
                {
                    LineBreakCount.Text = "999";
                }
            }
        }

        private void GenerateButtonClickedEventHandler(object sender, RoutedEventArgs e)
        {
            if (Scmdraft.IsChecked == true)
            {
                GenerateTextTriggers(new ScmdraftTriggerGenerator());
            }
            else if (Starforge.IsChecked == true)
            {
                GenerateTextTriggers(new StarforgeTriggerGenerator());
            }
            else if (TrigEdit.IsChecked == true)
            {
                GenerateTextTriggers(new TrigEditTriggerGenerator());
            }
            //else if (SomeOtherEditor.IsChecked == true)
            //{
            //    GenerateTextTriggers(new SomeOtherEditorTriggerGenerator());
            //}
        }

        private void GenerateTextTriggers(ITriggerGenerator triggerGenerator)
        {
            var triggers = new StringBuilder();
            int lineBreak;
            if (LineBreakCount.Text.Length == 0 || int.Parse(LineBreakCount.Text) == 0)
            {
                lineBreak = 9999999;
            }
            else
            {
                lineBreak = int.Parse(LineBreakCount.Text);
            }

            var actions = new List<string>();
            foreach (Models.BoundSequence sequence in GlobalState.Sequences)
            {
                foreach (Models.MapLocation location in sequence.States.Keys.Where(x => sequence.States[x] != "default"))
                {
                    actions.Add(triggerGenerator.CreateUnit(Player.GetSelectionContent(), sequence.GetExplosionUnitName(location), location.LocationName));
                    actions.Add(triggerGenerator.KillUnit(location.LocationName));
                }
                actions.Add(triggerGenerator.Wait(sequence.WaitTime));
            }

            for (int i = 1; i <= actions.Count; i++)
            {
                triggers.AppendLine(actions[i - 1]);
                if (i % lineBreak == 0)
                {
                    if (AddPreserveTriggerOnLineBreaks.IsChecked == true)
                    {
                        triggers.AppendLine(triggerGenerator.PreserveTrigger());
                    }

                    triggers.AppendLine();
                }
            }
            if (AddPreserveTriggerOnLineBreaks.IsChecked == true && (lineBreak > actions.Count || lineBreak % actions.Count != 0))
            {
                triggers.AppendLine(triggerGenerator.PreserveTrigger());
            }

            TriggerOutput.Text = triggers.ToString();
        }
    }
}
