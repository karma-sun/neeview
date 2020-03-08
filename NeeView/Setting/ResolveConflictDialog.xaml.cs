using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView.Setting
{
    /// <summary>
    /// ResolveConflictDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ResolveConflictDialog : Window
    {
        //
        public ResolveConflictDialog(ResolveConflictDialogContext context)
        {
            InitializeComponent();

            this.DataContext = new ResolveConflictDialogVM(context);

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));
        }

        //
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        //
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    /// <summary>
    /// 競合しているコマンド情報
    /// </summary>
    public class ConflictItem
    {
        public string CommandName { get; set; }
        public bool IsChecked { get; set; }

        public string Text => CommandName.ToCommand().Text;

        public ConflictItem(string cmmandName, bool isChecked)
        {
            CommandName = cmmandName;
            IsChecked = isChecked;
        }
    }

    /// <summary>
    /// ConflictDialog ViewModel
    /// </summary>
    public class ResolveConflictDialogVM : BindableBase
    {
        private ResolveConflictDialogContext _context;

        // window title
        public string Title => $"{_context.Command.ToCommand().Text} - {Properties.Resources.ControlResolveConflictTitle}";

        ////public string Gesture => _context.Gesture;
        public string Note => string.Format(Properties.Resources.ControlResolveConflictNote, _context.Gesture);

        public List<ConflictItem> Conflicts => _context.Conflicts;

        //
        public ResolveConflictDialogVM(ResolveConflictDialogContext context)
        {
            _context = context;
        }
    }

    /// <summary>
    /// ConflictDialog Model
    /// </summary>
    public class ResolveConflictDialogContext
    {
        public string Command { get; set; }
        public string Gesture { get; set; }
        public List<ConflictItem> Conflicts { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="command">currrent command</param>
        /// <param name="gesture">conflict gesture</param>
        /// <param name="commands">conflict commands</param>
        public ResolveConflictDialogContext(string gesture, List<string> commands, string command)
        {
            Command = command;
            Gesture = gesture;

            Conflicts = commands
                .Select(e => new ConflictItem(e, e == command))
                .ToList();
        }
    }
}
