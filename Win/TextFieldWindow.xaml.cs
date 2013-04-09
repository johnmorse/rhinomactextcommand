using System;
using System.Collections.Generic;
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

namespace Text.WPF
{
  /// <summary>
  /// Interaction logic for TextFieldWindow.xaml
  /// </summary>
  public partial class TextFieldWindow : Window
  {
    public TextFieldWindow()
    {
      InitializeComponent();
    }

    private void ButtonClickOK(object sender, RoutedEventArgs e)
    {
      var model = DataContext as TextFieldViewModel;
      if (null != model && !model.OkayToClose())
          return;
      DialogResult = true;
      Close();
    }

    private void ButtonClickCancel(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close();
    }
  }
}
