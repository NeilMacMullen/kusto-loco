using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core.Raw;

namespace lokqlDx
{
    /// <summary>
    ///     Simple text editor for running queries
    /// </summary>
    /// <remarks>
    ///     The key thing this provides is an event based on CTRL-ENTER that
    ///     selects text around the cursor and sends it to the RunEvent
    ///     In future it would be nice to replace this with a more capable editor
    ///     that supports syntax highlighting and other features
    /// </remarks>
    public partial class QueryEditor : UserControl
    {
        public QueryEditor()
        {
            InitializeComponent();
        }

        #region public interface

        public event EventHandler<QueryEditorRunEventArgs>? RunEvent;

        #endregion

        private void Query_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    e.Handled = true;
                    var query = GetTextAroundCursor();
                    if (query.Length > 0)
                    {
                        RunEvent?.Invoke(this, new QueryEditorRunEventArgs(query));
                    }
                }
            }
        }

        /// <summary>
        ///     searches for lines around the cursor that contain text
        /// </summary>
        /// <remarks>
        ///     This allows us to easily run multi-line queries
        /// </remarks>
        public string GetTextAroundCursor()
        {
            var i = Query.GetLineIndexFromCharacterIndex(Query.CaretIndex);
            var sb = new StringBuilder();
            while (i >= 1 && Query.GetLineText(i - 1).Trim().Length > 0)
                i--;
            while (i < Query.LineCount && Query.GetLineText(i).Trim().Length > 0)
            {
                sb.AppendLine(Query.GetLineText(i));
                i++;
            }
           
            return sb.ToString().Trim();
        }

        public void SetFontSize(double newSize)
        {
            Query.FontSize = newSize;
        }
        public void SetText(string text)
        {
            Query.Text = text;
        }

        public string GetText()
        {
           return Query.Text;
        }

        public void SetBusy(bool isBusy)
        {
            BusyStatus.Content = isBusy ? "Busy" : "Ready";
        }
    }

    public class QueryEditorRunEventArgs(string query) : EventArgs
    {
        public string Query { get; private set; } = query;
    }

}
