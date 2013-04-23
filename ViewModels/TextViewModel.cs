using Rhino;
using System.Collections.Generic;

namespace Text
{
  //-------------------------------------------------------------------------
  /// <summary>
  /// Simple view model for creating a Rhino point object
  /// </summary>
  class TextViewModel : Rhino.ViewModel.NotificationObject
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="doc">Document used to create the new point</param>
		public TextViewModel(RhinoDoc doc, Rhino.Geometry.TextEntity textEntity)
    {
      _doc = doc;
      _textEntity = textEntity;
      _text = textEntity.TextFormula;
      if (string.IsNullOrWhiteSpace(_text))
        _text = textEntity.Text;
      // Mask type combo box list
      _maskTypeList.Add(Rhino.UI.LOC.STR("None"));
      _maskTypeList.Add(Rhino.UI.LOC.STR("Background"));
      _maskTypeList.Add(Rhino.UI.LOC.STR("Solid Color"));
#if ON_OS_WINDOWS
      ShowMaskColorDialogCommand = new RhinoWindows.Input.DelegateCommand(ShowMaskColorDialog, null);
      ShowSelectTextFontCommand = new RhinoWindows.Input.DelegateCommand(ShowSelectTextFont, null);
      ShowTextFieldsFormCommand = new RhinoWindows.Input.DelegateCommand(ShowTextFieldsForm, null);
#endif
    }

    #region Mac specific
    #if ON_OS_MAC
    /// <summary>
    /// Used to display the font manager panel and handle
    /// change notifications.
    /// </summary>
    RhinoMac.FontManagerController _fontController;
    /// <summary>
    /// Show the Mac fong manager dialog and set the current
    /// selection equal to the current fontFaceName.
    /// </summary>
    void ShowMacFontDialog()
    {
      if (null == _fontController)
        _fontController = new RhinoMac.FontManagerController(Window, fontFaceName, 12.0f, FontChangedCallback);
      _fontController.ShowFontPanel();
    }
    /// <summary>
    /// Called by the _fontController when the selected font changes.
    /// </summary>
    /// <param name="font">Font.</param>
    void FontChangedCallback(MonoMac.AppKit.NSFont font)
    {
      if (null != font)
        fontFaceName = font.FamilyName;
    }
    /// <summary>
    /// Magic method that gets called when the window is 
    /// about to close.
    /// </summary>
    override public void WindowWillClose()
    {
      if (null != _fontController)
        _fontController.CloseFontPanel();
    }
    #endif
    #endregion Mac Specific

    #region Methods
    /// <summary>
    /// Adds the text entity to document.
    /// </summary>
    /// <returns>The text entity to document.</returns>
    public Rhino.Commands.Result AddTextEntityToDocument()
    {
      if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(_textEntity.TextFormula))
        return Rhino.Commands.Result.Failure;

      _textEntity.Text = string.Empty;
      _textEntity.TextFormula = string.Empty;

      if (_text.IndexOf("%<", System.StringComparison.Ordinal) >= 0)
        _textEntity.TextFormula = _text;
      else
        _textEntity.Text = _text;
      
      Doc.Objects.AddText(_textEntity);
      Doc.Views.Redraw();
      return Rhino.Commands.Result.Success;
    }
    /// <summary>
    /// Shows the select text font dialog.
    /// </summary>
    public void ShowSelectTextFont()
    {
    #if ON_OS_MAC
      ShowMacFontDialog();
    #endif
    #if ON_OS_WINDOWS
      var fontDialog = new System.Windows.Forms.FontDialog();
      var font = new System.Drawing.Font(fontFaceName, 12f);
      fontDialog.Font = font;
      var parent = RhinoWindows.Forms.WindowsInterop.ObjectAsIWin32Window(Window);
      if (null == parent) parent = RhinoApp.MainWindow();
      if (System.Windows.Forms.DialogResult.OK == fontDialog.ShowDialog(parent))
        fontFaceName = fontDialog.Font.FontFamily.Name;
    #endif
    }
    /// <summary>
    /// Show the color dialog and change the mask color if necessary.  This
    /// only works on Windows, Mac has a ColorShade control to handle display
    /// of the color dialog.
    /// </summary>
    public void ShowMaskColorDialog()
    {
    #if ON_OS_WINDOWS
      var color = new Rhino.Display.Color4f(maskColor);
      var parent = RhinoWindows.Forms.WindowsInterop.ObjectAsIWin32Window(Window);
      if (null == parent) parent = RhinoApp.MainWindow();
      if (Rhino.UI.Dialogs.ShowColorDialog(parent, ref color, false))
        maskColor = color.AsSystemColor();
    #endif
    }
    /// <summary>
    /// Shows the text fields form
    /// </summary>
    public void ShowTextFieldsForm()
    {
      // View model to be used as the controller for the text
      // field window.
      var viewModel = new TextFieldViewModel(Doc);
      bool? dialogResult = null;
      var start = -1;
      var length = -1;
    #if ON_OS_MAC
      // Create a NSWindow from a Nib file
      var window = RhinoMac.Window.FromNib("TextFieldWindow", viewModel);
      // Display the window
      window.ShowModal();
      dialogResult = window.DialogResult;
    #endif
    #if ON_OS_WINDOWS
      var textWindow = Window as Win.TextWindow;
      if (null != textWindow)
      {
        // Need to update the text property, it wont get updated when the
        // fields button gets clicked so do it manually
        text = textWindow.textBox.Text;
        // Get the location of the cursor in the text box
        start = textWindow.textBox.SelectionStart;
        // Get the slected text length if something is selected
        length = textWindow.textBox.SelectionLength;
      }
      var window = new Win.TextFieldWindow();
      window.DataContext = viewModel;
      window.Owner = Window;
      viewModel.Window = window;
      window.ShowDialog();
      dialogResult = window.DialogResult;
    #endif
      if (dialogResult != true)
        return;
      var formatString = viewModel.CalculateFinalFormatString();
      if (string.IsNullOrEmpty(formatString))
        return;
      if (!string.IsNullOrEmpty(text) && start >= 0 && length >= 0)
      {
        var before = (start < 1 ? string.Empty : text.Substring(0, start));
        var after = ((start + length) < text.Length) ? text.Substring(start + length) : string.Empty;
        text = before + formatString + after;
      }
      else
        text += formatString;
    }
    #endregion Methods

    #region Public properties
    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    /// <value>The text.</value>
    public string text
    {
      get { return _text; }
      set
      {
        if (value == _text) return;
        _text = value;
        RaisePropertyChanged(() => text);
      }
    }
    /// <summary>
    /// Gets or sets the height of the text.
    /// </summary>
    /// <value>The height of the text.</value>
    public double textHeight
    {
      get { return _textEntity.TextHeight; }
      set
      {
        if (value == textHeight) return;
        if (value > 0.0 && !double.IsNaN(value))
        {
          _textEntity.TextHeight = value;
          RaisePropertyChanged(() => textHeight);
        }
        else
        {
          // Raise the textHeight changed event after
          // this call returns so the controls bound
          // to this value will update to the previous
          // valid value.
          RaiseInvalidPropertyValue(() => textHeight);
        }
      }
    }
    /// <summary>
    /// Gets the text height unit system.
    /// </summary>
    /// <value>The text height unit system.</value>
    public string textHeightUnitSystem
    {
      get
      {
        var units = Rhino.UI.Localization.UnitSystemName(Doc.Views.ModelSpaceIsActive ? Doc.ModelUnitSystem : Doc.PageUnitSystem, false, false, true);
        return units;
      }
    }
    /// <summary>
    /// Gets or sets the index of the font.
    /// </summary>
    /// <value>The index of the font.</value>
    public int fontIndex
    {
      get { return _textEntity.FontIndex; }
      set
      {
        if (value == fontIndex) return;
        _textEntity.FontIndex = value;
        RaisePropertyChanged(() => fontIndex);
        var font = Font;
        if (null == font) return;
        fontFaceName = font.FaceName;
      }
    }
    /// <summary>
    /// Gets or sets the name of the font face.
    /// </summary>
    /// <value>The name of the font face.</value>
    public string fontFaceName
    {
      get
      {
        var font = Font;
        if (null == font) return string.Empty;
        return font.FaceName;
      }
      set
      {
        if (string.IsNullOrWhiteSpace(value))return;
        var font = Font;
        var faceName = null == font ? string.Empty : font.FaceName;
        if (faceName == value) return;
        var newFontIndex = Doc.Fonts.FindOrCreate(value, bold, italic);
        // Failed to create requested font name so bail
        if (fontIndex < 0)
        {
          this.RaiseInvalidPropertyValue(() => fontFaceName);
          return;
        }
        fontIndex = newFontIndex;
        RaisePropertyChanged(() => fontFaceName);
      }
    }
    /// <summary>
    /// Used by WPF to set the bold font property for the preview and input
    /// TextBoxes
    /// </summary>
    public string FontWeight
    {
      get { return (bold ? "Bold" : "Normal"); }
    }
    /// <summary>
    /// Used by WPF to set the italic font property for the preview and input
    /// TextBoxes
    /// </summary>
    public string FontStyle
    {
      get { return (italic ? "Italic" : "Normal"); }
    }
    public string MarginControlsVisibility { get { return (enableMarginControls ? "Visible" : "Hidden"); } }
    public string MarginColorControlsVisibility { get { return (enableMarginColorControls ? "Visible" : "Hidden"); } }
    public bool enableMarginControls { get { return (selectedMaskTypeIndex > 0); } }
    public bool enableMarginColorControls { get { return (selectedMaskTypeIndex == 2); } }
    public bool hideMarginControls { get { return (!enableMarginControls); } }
    public bool hideMarginColorControls { get { return (!enableMarginColorControls); } }
    public List<string>maskTypeList{ get { return _maskTypeList; } }
    public int selectedMaskTypeIndex
    {
      get
      {
        int mode = 0;
        if (_textEntity.MaskEnabled)
          mode = (_textEntity.MaskUsesViewportColor ? 1 : 2);
        return mode;
      }
      set
      {
        if (value == selectedMaskTypeIndex) return;
        _textEntity.MaskEnabled = (value > 0);
        if (value > 0)
          _textEntity.MaskUsesViewportColor = (1 == value);
        RaisePropertyChanged(() => selectedMaskTypeIndex);
        RaisePropertyChanged(() => enableMarginControls);
        RaisePropertyChanged(() => enableMarginColorControls);
        RaisePropertyChanged(() => hideMarginControls);
        RaisePropertyChanged(() => hideMarginColorControls);
        RaisePropertyChanged(() => MarginColorControlsVisibility);
        RaisePropertyChanged(() => MarginControlsVisibility);
      }
    }
    public bool alignLeftChecked
    {
      get { return (justifyHorizontalIndex == 0); }
      set
      {
        if (!value || alignLeftChecked) return;
        justifyHorizontalIndex = 0;
      }
    }
    public bool alignCenterChecked
    {
      get { return (justifyHorizontalIndex == 1); }
      set
      {
        if (!value || alignCenterChecked) return;
        justifyHorizontalIndex = 1;
      }
    }
    public bool alignRightChecked
    {
      get { return (justifyHorizontalIndex == 2); }
      set
      {
        if (!value || alignRightChecked) return;
        justifyHorizontalIndex = 2;
      }
    }
    public string TextAlignment
    {
      get
      {
        var index = justifyHorizontalIndex;
        if (index == 2)
          return "Right";
        return (index == 1 ? "Center" : "Left");
      }
    }
    public int justifyHorizontalIndex
    {
      get
      {
        if ((_textEntity.Justification & Rhino.Geometry.TextJustification.Right) == Rhino.Geometry.TextJustification.Right)
          return 2;
        else if ((_textEntity.Justification & Rhino.Geometry.TextJustification.Center) == Rhino.Geometry.TextJustification.Center)
          return 1;
        return 0;
      }
      set
      {
        if (value == justifyHorizontalIndex) return;
        var justify = _textEntity.Justification;
        justify &= ~(Rhino.Geometry.TextJustification.Left |Rhino.Geometry.TextJustification.Center | Rhino.Geometry.TextJustification.Right);
        if (value == 2)
          justify |= Rhino.Geometry.TextJustification.Right;
        else if (value == 1)
          justify |= Rhino.Geometry.TextJustification.Center;
        else
          justify |= Rhino.Geometry.TextJustification.Left;
        if (0 == (justify & (Rhino.Geometry.TextJustification.Top | Rhino.Geometry.TextJustification.Middle | Rhino.Geometry.TextJustification.Bottom)))
          justify |= Rhino.Geometry.TextJustification.Bottom;
        _textEntity.Justification = justify;
        RaisePropertyChanged(() => justifyHorizontalIndex);
        RaiseInvalidPropertyValue(() => TextAlignment);
      }
    }
    public bool alignTopChecked
    {
      get { return (this.justifyVerticalIndex == 0); }
      set
      {
        if (!value || alignTopChecked) return;
        justifyVerticalIndex = 0;
      }
    }
    public bool alignMidChecked
    {
      get { return (justifyVerticalIndex == 1); }
      set
      {
        if (!value || alignMidChecked) return;
        justifyVerticalIndex = 1;
      }
    }
    public bool alignBottomChecked
    {
      get { return (justifyVerticalIndex == 2); }
      set
      {
        if (!value || alignBottomChecked) return;
        justifyVerticalIndex = 2;
      }
    }
    public int justifyVerticalIndex
    {
      get
      {
        if ((_textEntity.Justification & Rhino.Geometry.TextJustification.Top) == Rhino.Geometry.TextJustification.Top)
          return 0;
        else if ((_textEntity.Justification & Rhino.Geometry.TextJustification.Middle) == Rhino.Geometry.TextJustification.Middle)
          return 1;
        return 2;
      }
      set
      {
        if (value == justifyVerticalIndex) return;
        var justify = _textEntity.Justification;
        justify &= ~(Rhino.Geometry.TextJustification.Top |Rhino.Geometry.TextJustification.Middle | Rhino.Geometry.TextJustification.Bottom);
        if (value == 0)
          justify |= Rhino.Geometry.TextJustification.Top;
        else if (value == 1)
          justify |= Rhino.Geometry.TextJustification.Middle;
        else
          justify |= Rhino.Geometry.TextJustification.Bottom;
        if (0 == (justify & (Rhino.Geometry.TextJustification.Left | Rhino.Geometry.TextJustification.Center | Rhino.Geometry.TextJustification.Right)))
          justify |= Rhino.Geometry.TextJustification.Left;
        _textEntity.Justification = justify;
        RaisePropertyChanged(() => justifyVerticalIndex);
      }
    }
    //justifyVerticalIndex
    /// <summary>
    /// Gets or sets a value indicating whether this text entity mask enabled.
    /// </summary>
    /// <value><c>true</c> if mask enabled; otherwise, <c>false</c>.</value>
    public bool maskEnabled
    {
      get { return _textEntity.MaskEnabled; }
      set
      {
        if (value == maskEnabled) return;
        _textEntity.MaskEnabled = value;
        RaisePropertyChanged(() => maskEnabled);
      }
    }
    /// <summary>
    /// Gets or sets a value indicating whether this text entity mask uses viewport color.
    /// </summary>
    /// <value><c>true</c> if mask uses viewport color; otherwise, <c>false</c>.</value>
    public bool maskUsesViewportColor
    {
      get { return _textEntity.MaskUsesViewportColor; }
      set
      {
        if (value == maskUsesViewportColor) return;
        _textEntity.MaskUsesViewportColor = value;
        RaisePropertyChanged(() => maskUsesViewportColor);
      }
    }
    /// <summary>
    /// Gets or sets the color of the mask.
    /// </summary>
    /// <value>The color of the mask.</value>
    public System.Drawing.Color maskColor
    {
      get { return _textEntity.MaskColor; }
      set
      {
        if (value == maskColor) return;
        _textEntity.MaskColor = value;
        RaisePropertyChanged(() => maskColor);
      }
    }
    
    #region Windows specific command helpers
    #if ON_OS_WINDOWS
    public System.Windows.Window Window { get; set; }
    public System.Windows.Input.ICommand ShowMaskColorDialogCommand { get; private set; }
    public System.Windows.Input.ICommand ShowSelectTextFontCommand { get; private set; }
    public System.Windows.Input.ICommand ShowTextFieldsFormCommand { get; private set; }
    #endif
    #endregion Windows specific command helpers

    /// <summary>
    /// Gets or sets the mask offset.
    /// </summary>
    /// <value>The mask offset.</value>
    public double maskOffset
    {
      get { return _textEntity.MaskOffset; }
      set
      {
        if (value == maskOffset) return;
        if (value >= 0.0 && double.IsNaN(value))
        {
          _textEntity.MaskOffset = value;
          RaisePropertyChanged(() => maskOffset);
        }
        else
          RaiseInvalidPropertyValue(() => maskOffset);
      }
    }
    public bool annotativeScalingEnabled
    {
      get { return _textEntity.AnnotativeScalingEnabled; }
      set
      {
        if (value == annotativeScalingEnabled) return;
        _textEntity.AnnotativeScalingEnabled = value;
        RaisePropertyChanged(() => annotativeScalingEnabled);
      }
    }
    /// <summary>
    /// Gets a value indicating whether the font used by this text entity is bold.
    /// </summary>
    /// <value><c>true</c> if bold; otherwise, <c>false</c>.</value>
    public bool bold
    {
      get
      {
        var font = Font;
        if (null == font) return false;
        return font.Bold;
      }
      set
      {
        var font = Font;
        if (null == font) return;
        if (font.Bold == value) return;
        var index = Doc.Fonts.FindOrCreate(font.FaceName, value, font.Italic);
        fontIndex = index;
        RaisePropertyChanged(() => bold);
        RaisePropertyChanged(() => FontWeight);
      }
    }
    /// <summary>
    /// Gets or sets a value indicating whether the font used by this text entity is italic.
    /// </summary>
    /// <value><c>true</c> if italic; otherwise, <c>false</c>.</value>
    public bool italic
    {
      get
      {
        var font = Font;
        if (null == font) return false;
        return font.Italic;
      }
      set
      {
        var font = Font;
        if (null == font) return;
        if (font.Italic == value) return;
        var index = Doc.Fonts.FindOrCreate(font.FaceName, bold, value);
        fontIndex = index;
        RaisePropertyChanged(() => italic);
        RaisePropertyChanged(() => FontStyle);
      }
    }
    public Rhino.DocObjects.Font Font
    {
      get
      {
        var font = Doc.Fonts[_textEntity.FontIndex];
        if (null == font)
        { //can happen when a new doc is created and the saved font index is invalid
          fontIndex = 0;
          font = Doc.Fonts[_textEntity.FontIndex];
        }
        return font;
      }
    }
    public RhinoDoc Doc { get { return _doc; } }
    #endregion Public properties

    #region Private members
    /// <summary>
    /// The document used to create the new point
    /// </summary>
    private readonly RhinoDoc _doc;
    private Rhino.Geometry.TextEntity _textEntity;
    private string _text = string.Empty;
    private List<string> _maskTypeList = new List<string>();
    #endregion Private members
  }
}
