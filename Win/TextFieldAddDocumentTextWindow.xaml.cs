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
  /// Interaction logic for TextFieldAddDocumentTextWindow.xaml
  /// </summary>
  public partial class TextFieldAddDocumentTextWindow : Window
  {
    public TextFieldAddDocumentTextWindow()
    {
      InitializeComponent();
    }

    private void CancelButtonClicked(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close();
    }

    private void OKButtonClicked(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
      Close();
    }
  }
}
