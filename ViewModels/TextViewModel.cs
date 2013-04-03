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
      // Mask type combo box list
      _maskTypeList.Add(Rhino.UI.LOC.STR("None"));
      _maskTypeList.Add(Rhino.UI.LOC.STR("Background"));
      _maskTypeList.Add(Rhino.UI.LOC.STR("Solid Color"));
      // Timer used to raise property changed event when attempting
      // to set a property to in an invalid value, when the timer
      // is fired then a RasiePropertyChanged notification is sent
      // telling the bound control to reset its contents to the
      // previous value.
      _invalidValueTimer.Enabled = false;
      _invalidValueTimer.Interval = 1;
      _invalidValueTimer.Elapsed += InvalidValueTimerElapsed;
    }

    #region Mac specific
    #if ON_OS_MAC
    /// <summary>
    /// Class used to display the font manager panel and get updates
    /// when the selected font changes.
    /// </summary>
    class FontManagerController : MonoMac.AppKit.NSWindowController
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="Text.TextViewModel+FontManagerController"/> class.
      /// </summary>
      /// <param name="viewModel">View model.</param>
      public FontManagerController(TextViewModel viewModel)
      {
        // Save the view model, need to notify it when the font selection
        // changes
        _viewModel = viewModel;
        // The close flag defaults to ture, set it to false if the
        // shared font manager panel is currently open so it will
        // be left open when this window closes otherwise the
        // font panel will close when this form does.
        if (null != MonoMac.AppKit.NSFontPanel.SharedFontPanel && MonoMac.AppKit.NSFontPanel.SharedFontPanel.IsVisible)
          _closeFontManager = false;
        // Get an instance of the font manager panel
        _fontManager = MonoMac.AppKit.NSFontManager.SharedFontManager;
        // Create an instance of the font we want to change when
        // the font manger selection changes, the font face name
        // will be extracted from this font and passed to the
        // associated view model.
        _font = MonoMac.AppKit.NSFont.FromFontName(_viewModel.fontFaceName, 12.0);
        // Set the font manager panel target to this object so the
        // ChangeFont() method will get called when the current font
        // selection changes
        _fontManager.Target = this;
        // Set the currently selected font in the font manger panel
        _fontManager.SetSelectedFont(_font, false);
      }
      /// <summary>
      /// Shows the font panel.
      /// </summary>
      public void ShowFontPanel()
      {
        // Display the font manager panel
        _fontManager.OrderFrontFontPanel(this);
      }
      public void CloseFontPanel()
      {
        // Get the font manager panel
        var panel = null == _fontManager ? null : _fontManager.FontPanel(false);
        // If the panel is currently visible and it was not when this
        // object was created then close it now.
        if (null != panel && panel.IsVisible && _closeFontManager)
          panel.IsVisible = false;
      }
      // - (void) changeFont: (id) sender
      // http://docs.go-mono.com/?link=T%3aMonoMac.Foundation.ExportAttribute%2f*
      // This attribute is supose to:
      //   "Exports a method or property to the Objective-C world."
      /// <summary>
      /// Called by the font manager panel when the current font
      /// value changes.
      /// </summary>
      /// <param name="sender">Sender.</param>
      [MonoMac.Foundation.Export ("changeFont:")]
      public void changeFont(MonoMac.AppKit.NSFontManager sender)
      {
        // Convert the font value and save it
        var newFont = sender.ConvertFont(_font);
        _font = newFont;
        // Update the view model
        _viewModel.fontFaceName = newFont.FamilyName;
      }
      /// <summary>
      /// The view model to update.
      /// </summary>
      TextViewModel _viewModel;
      /// <summary>
      /// Instance of the font manager associated with this
      /// object.
      /// </summary>
      MonoMac.AppKit.NSFontManager _fontManager;
      /// <summary>
      /// Font used to get/set current state of the font
      /// manager panel.
      /// </summary>
      MonoMac.AppKit.NSFont _font;
      /// <summary>
      /// Flag used to determine if the font manager panel
      /// should be closed when done.
      /// </summary>
      bool _closeFontManager = true;
    }
    /// <summary>
    /// Used to display the font manager panel and handle
    /// change notifications.
    /// </summary>
    FontManagerController _fontController;
    /// <summary>
    /// Show the Mac fong manager dialog and set the current
    /// selection equal to the current fontFaceName.
    /// </summary>
    void ShowMacFontDialog()
    {
      if (null == _fontController)
        _fontController = new FontManagerController(this);
      _fontController.ShowFontPanel();
    }
    /// <summary>
    /// Magic method that gets called when the window is 
    /// about to close.
    /// </summary>
    public void WindowWillClose()
    {
      if (null != _fontController)
        _fontController.CloseFontPanel();
    }
    #endif
    #endregion Mac Specific

    #region Invalid value timer methods
    /// <summary>
    /// Called to fire property changed notifications from
    /// within a set method when attempting to set the propert
    /// to an invalid value.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Arguments.</param>
    void InvalidValueTimerElapsed(object sender, System.Timers.ElapsedEventArgs args)
    {
      _invalidValueTimer.Stop();
      _invalidValueTimer.Enabled = false;
      var property = _invalidValueProperty;
      _invalidValueProperty = null;
      if (!string.IsNullOrWhiteSpace(property))
        RaisePropertyChanged(property);
    }
    /// <summary>
    /// Start the invalid value timer which will raise
    /// the appropriate change notification once the
    /// calling function has a chance to return.
    /// </summary>
    /// <param name="propertyExpression">Property expression.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    void RaiseInvalidPropertyValue<T>(System.Linq.Expressions.Expression<System.Func<T>> propertyExpression)
    {
      _invalidValueProperty = Rhino.ViewModel.NotificationObject.ExtractPropertyName(propertyExpression);
      _invalidValueTimer.Enabled = true;
      _invalidValueTimer.Start();
    }
    #endregion Invalid value timer methods

    #region Methods
    /// <summary>
    /// Adds the text entity to document.
    /// </summary>
    /// <returns>The text entity to document.</returns>
    public Rhino.Commands.Result AddTextEntityToDocument()
    {
      if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(_textEntity.TextFormula))
        return Rhino.Commands.Result.Failure;
      
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
    #endif
    }
    /// <summary>
    /// Shows the text fields form.
    /// </summary>
    public void ShowTextFieldsForm()
    {
      // View model to be used as the controller for the text
      // field window.
      var viewModel = new TextFieldViewModel(Doc);
    #if ON_OS_MAC
      // Create a NSWindow from a Nib file
      var window = RhinoMac.Window.FromNib("TextFieldWindow", viewModel);
      // Display the window
      window.ShowModal();
    #endif
    }
    #endregion Methods

    #region Public properties
    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    /// <value>The text.</value>
    public string text
    {
      get { return _textEntity.Text; }
      set
      {
        if (value == _textEntity.Text) return;
        _textEntity.Text = value;
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
        if (value > 0.0)
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
        if (newFontIndex < 0 || newFontIndex == fontIndex) return;
        RaisePropertyChanged(() => fontFaceName);
        fontIndex = newFontIndex;
      }
    }
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
        _textEntity.MaskOffset = value;
        RaisePropertyChanged(() => maskOffset);
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
    private System.Timers.Timer _invalidValueTimer = new System.Timers.Timer();
    private string _invalidValueProperty = string.Empty;
    private List<string> _maskTypeList = new List<string>();
    #endregion Private members
  }
}
