// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeLaboratory.Property
{
    /// <summary>
    /// Inspector.xaml の相互作用ロジック
    /// </summary>
    public partial class PropertyInspector : UserControl
    {


        public PropertyDocument Document
        {
            get { return (PropertyDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Document.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(PropertyDocument), typeof(PropertyInspector), new PropertyMetadata(null));

        //
        //public List<PropertyDrawElement> ItemsSource => Document.Elements;


        public PropertyInspector()
        {
            InitializeComponent();

            this.Root.DataContext = this;
        }

        //
        private void Reset(object sender, RoutedEventArgs e)
        {
            foreach (var item in Document.Elements.OfType<PropertyMemberElement>())
            {
                item.ResetValue();
            }

            this.properties.Items.Refresh();
        }
    }

}
