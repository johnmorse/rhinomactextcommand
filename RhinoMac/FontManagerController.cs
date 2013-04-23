using System;
#if ON_OS_MAC
using MonoMac.AppKit;

namespace RhinoMac
{
  /// <summary>
  /// Class used to display the font manager panel and get updates
  /// when the selected font changes.
  /// </summary>
  class FontManagerController : MonoMac.AppKit.NSResponder, IDisposable
  {
    new public void Dispose()
    {
      ResetResponderChain();
      if (null != _fontManager)
        _fontManager.Dispose();
      _fontManager = null;
      base.Dispose();
    }
    /// <summary>
    /// Method to be called when the selected font changes
    /// </summary>
    public delegate void FontChangedEvent(MonoMac.AppKit.NSFont font);
    /// <summary>
    /// Initializes a new instance of the <see cref="Text.TextViewModel+FontManagerController"/> class.
    /// </summary>
    /// <param name="window">Window.</param>
    /// <param name="fontFaceName">Font face name.</param>
    /// <param name="fontSize">Font size.</param>
    /// <param name="fontChangedCallback">Font changed callback.</param>
    public FontManagerController(RhinoMac.Window window, string fontFaceName, float fontSize, FontChangedEvent fontChangedCallback)
    {
      // Need the window to get the responder chain working
      _window = window;
      // Call this function when the selected font changes
      _fontChangedCallback = fontChangedCallback;
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
      _font = MonoMac.AppKit.NSFont.FromFontName(fontFaceName, fontSize);
      // Set the font manager panel target to this object so the
      // ChangeFont() method will get called when the current font
      // selection changes
      //_fontManager.Target = this;
      //
      // Save the responder chain
      //
      _resetResponderChain = true;
      _thisNextResponder = NextResponder;
      _windowNextResponder = _window.NextResponder;
      _action = _fontManager.Action;
      //
      // Redirect the responder chain
      this.NextResponder = _window.NextResponder;
      _window.NextResponder = this;
      _fontManager.Action = new MonoMac.ObjCRuntime.Selector("changeFontAction:");
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
    protected void ResetResponderChain()
    {
      if (!_resetResponderChain)
        return;
      _resetResponderChain = false;
      NextResponder = _thisNextResponder;
      _window.NextResponder = _windowNextResponder;
      _fontManager.Action = _action;
    }
    /// <summary>
    /// Call when the window is closing to close font manager
    /// panel and clean up.
    /// </summary>
    public void CloseFontPanel()
    {
      // Restore the responder chain
      ResetResponderChain();
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
    [MonoMac.Foundation.Export ("changeFontAction:")]
    public void changeFont(MonoMac.AppKit.NSFontManager sender)
    {
      // Convert the font value and save it
      var newFont = sender.ConvertFont(_font);
      _font = newFont;
      // Update the view model
      _fontChangedCallback(newFont);
    }
    /// <summary>
    /// Instance of the font manager associated with this
    /// object.
    /// </summary>
    private MonoMac.AppKit.NSFontManager _fontManager;
    /// <summary>
    /// Font used to get/set current state of the font
    /// manager panel.
    /// </summary>
    private MonoMac.AppKit.NSFont _font;
    /// <summary>
    /// Flag used to determine if the font manager panel
    /// should be closed when done.
    /// </summary>
    private bool _closeFontManager = true;
    /// <summary>
    /// The window responder chain to modify
    /// </summary>
    private Window _window;
    /// <summary>
    /// The previous changeFont action
    /// </summary>
    private MonoMac.ObjCRuntime.Selector _action;
    /// <summary>
    /// This objects original NextResponder
    /// </summary>
    private MonoMac.AppKit.NSResponder _thisNextResponder;
    /// <summary>
    /// The original _window.NextResponder
    /// </summary>
    private MonoMac.AppKit.NSResponder _windowNextResponder;
    /// <summary>
    /// Call this when the selected font changes
    /// </summary>
    private FontChangedEvent _fontChangedCallback;
    /// <summary>
    /// If true then the responder chanin needs to be reset
    /// </summary>
    private bool _resetResponderChain;
  }
}
#endif
