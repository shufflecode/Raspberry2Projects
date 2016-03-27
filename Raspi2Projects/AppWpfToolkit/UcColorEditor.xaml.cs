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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppWpfToolkit
{
    /// <summary>
    /// Interaktionslogik für UcColorEditor.xaml
    /// </summary>
    public partial class UcColorEditor : UserControl, Xceed.Wpf.Toolkit.PropertyGrid.Editors.ITypeEditor
    {
        public UcColorEditor()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(libShared.SharedColor), typeof(UcColorEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public libShared.SharedColor Value
        {
            get
            {
                return (libShared.SharedColor)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
               
            }
        }

        public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
        {
            Binding binding = new Binding("Value");
            binding.Source = propertyItem;
            binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            BindingOperations.SetBinding(this, UcColorEditor.ValueProperty, binding);

            System.Windows.Media.Color col = System.Windows.Media.Color.FromArgb(this.Value.Intensity, this.Value.Red, this.Value.Green, this.Value.Blue);
            this.colorEditor.SelectedColor = col;

            return this;
        }

        private void colorEditor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            libShared.SharedColor temp = new libShared.SharedColor();

            temp.Intensity = this.colorEditor.SelectedColor.Value.A;
            temp.Green = this.colorEditor.SelectedColor.Value.G;
            temp.Red = this.colorEditor.SelectedColor.Value.R;
            temp.Blue = this.colorEditor.SelectedColor.Value.B;

            //Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid pg = (Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid)this.propertyItem.ParentElement;
            //OnPropertyChanged(propertyItem.DisplayName);
            this.Value = temp;    
        }       
    }
}
