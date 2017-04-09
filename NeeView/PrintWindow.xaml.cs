// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Printing;
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

namespace NeeView
{
    /// <summary>
    /// PrintWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PrintWindow : Window
    {
        private PrintWindowViewModel _vm;

        public PrintWindow()
        {
            InitializeComponent();
        }

        public PrintWindow(PrintContext context) : this()
        {
            _vm = new PrintWindowViewModel(context);
            this.DataContext = _vm;

            _vm.Close += ViewModel_Close;
        }

        private void ViewModel_Close(object sender, PrintWindowCloseEventArgs e)
        {
            this.DialogResult = e.Result;
            this.Close();
        }
    }

    public class PrintWindowCloseEventArgs : EventArgs
    {
        public bool? Result { get; set; }
    }

    public class PrintWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Model property.
        /// </summary>
        private PrintModel _model;
        public PrintModel Model => _model;

        /// <summary>
        /// MainContent property.
        /// </summary>
        private FrameworkElement _MainContent;
        public FrameworkElement MainContent
        {
            get { return _MainContent; }
            set { if (_MainContent != value) { _MainContent = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// PageCollection property.
        /// </summary>
        private List<FixedPage> _PageCollection;
        public List<FixedPage> PageCollection
        {
            get { return _PageCollection; }
            set { if (_PageCollection != value) { _PageCollection = value; RaisePropertyChanged(); } }
        }




        //
        public PrintWindowViewModel(PrintContext context)
        {
            _model = new PrintModel(context);
            _model.PropertyChanged += PrintService_PropertyChanged;
            _model.Margin.PropertyChanged += PrintService_PropertyChanged;

            UpdatePreview();
        }

        //
        private void PrintService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdatePreview();
        }

        //
        private void UpdatePreview()
        {
            var sw = new Stopwatch();
            sw.Start();
            //MainContent = _model.CreateVisual();
            //MainContent = _model.CreatePage();

            //MainContent = _model.CreatePageCollection().First();

            PageCollection = _model.CreatePageCollection();

            sw.Stop();
            Debug.WriteLine($"Gene: {sw.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// PrintCommand command.
        /// </summary>
        private RelayCommand _PrintCommand;
        public RelayCommand PrintCommand
        {
            get { return _PrintCommand = _PrintCommand ?? new RelayCommand(PrintCommand_Executed); }
        }

        private void PrintCommand_Executed()
        {
            _model.Print();
            Close?.Invoke(this, new PrintWindowCloseEventArgs() { Result = true });
        }


        /// <summary>
        /// CancelCommand command.
        /// </summary>
        private RelayCommand _CancelCommand;
        public RelayCommand CancelCommand
        {
            get { return _CancelCommand = _CancelCommand ?? new RelayCommand(CancelCommand_Executed); }
        }

        public event EventHandler<PrintWindowCloseEventArgs> Close;

        private void CancelCommand_Executed()
        {
            Close?.Invoke(this, new PrintWindowCloseEventArgs() { Result = false });
        }

        /// <summary>
        /// PrintDialogCommand command.
        /// </summary>
        private RelayCommand _PrintDialogCommand;
        public RelayCommand PrintDialogCommand
        {
            get { return _PrintDialogCommand = _PrintDialogCommand ?? new RelayCommand(PrintDialogCommand_Executed); }
        }

        private void PrintDialogCommand_Executed()
        {
            _model.ShowPrintDialog();
            UpdatePreview();
        }

    }

    //
    public class PrintWindowContext
    {
        public FrameworkElement ViewContent { get; set; }

        public System.Printing.PageImageableArea Area { get; set; }
    }

    //
    public class ItemsUniformGrid : System.Windows.Controls.Primitives.UniformGrid
    {
        public IEnumerable<FrameworkElement> ItemsSource
        {
            get { return (IEnumerable<FrameworkElement>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<FrameworkElement>), typeof(ItemsUniformGrid), new PropertyMetadata(null, ItemsSource_Changed));

        private static void ItemsSource_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ItemsUniformGrid;
            if (control != null)
            {
                control.Reflesh();
            }
        }

        //
        public void Reflesh()
        {
            this.Children.Clear();

            if (ItemsSource == null) return;

            foreach(var child in ItemsSource)
            {
                var grid = new Grid();
                grid.Background = Brushes.White;
                grid.Margin = new Thickness(10);
                grid.Children.Add(child);

                this.Children.Add(grid);
            }
        }
    }
}
