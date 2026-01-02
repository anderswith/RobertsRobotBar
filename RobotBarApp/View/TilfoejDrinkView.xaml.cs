using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RobotBarApp.View
{
    public partial class TilfoejDrinkView : UserControl
    {
        private static readonly char[] _disallowedScriptChars = { 'æ', 'ø', 'å', 'Æ', 'Ø', 'Å' };

        public TilfoejDrinkView()
        {
            InitializeComponent();
        }

        private static bool ContainsDisallowedScriptChars(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return text.IndexOfAny(_disallowedScriptChars) >= 0;
        }

        private void ScriptTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Blocks typing disallowed characters. MaxLength is handled by the TextBox itself.
            if (ContainsDisallowedScriptChars(e.Text))
                e.Handled = true;
        }

        private void ScriptTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
                return;

            var pastedText = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            if (ContainsDisallowedScriptChars(pastedText))
                e.CancelCommand();
        }
    }
}
