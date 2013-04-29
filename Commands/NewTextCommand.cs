using Rhino;
using Rhino.Commands;
using Rhino.UI;

namespace Text.Commands
{
  [System.Runtime.InteropServices.Guid("59c5da1d-6b85-4ae2-8a75-d58d35555e9f")]
  public class NewTextCommand : Command
  {
    ///<returns>The command name as it appears on the Rhino command line.</returns>
    public override string EnglishName
    {
      get { return "NewText"; }
    }

    /// <summary>
    /// Gets or sets the default height of the text, gets saved as
    /// a command setting and passed to the ViewModel durring RunCommand.
    /// </summary>
    /// <value>The default height of the text.</value>
    double DefaultTextHeight
    {
      get { return Settings.GetDouble("text_height", 1); }
      set { Settings.SetDouble("text_height", value); }
    }
    /// <summary>
    /// The text entity used to save and restore default values.
    /// </summary>
    Rhino.Geometry.TextEntity m_default_entity;

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      // Get a startpoint for the text
      Rhino.Geometry.Point3d startpoint;
      var rc = Rhino.Input.RhinoGet.GetPoint(Rhino.UI.LOC.STR("Start point"), false, out startpoint);
      if (rc != Rhino.Commands.Result.Success)
        return rc;
      
      if (m_default_entity == null)
      {
        m_default_entity = new Rhino.Geometry.TextEntity();
        double val = DefaultTextHeight;
        if( val>0 )
          m_default_entity.TextHeight = val;
        m_default_entity.FontIndex = doc.Fonts.CurrentIndex;
      }
      
      var plane = doc.Views.ActiveView.ActiveViewport.ConstructionPlane();
      plane.Origin = startpoint;
      m_default_entity.Plane = plane;
      var test_font = doc.Fonts[m_default_entity.FontIndex];
      if (test_font == null) //can happen when a new doc is created and the saved font index is invalid
        m_default_entity.FontIndex = 0;

      // View Model to associate with this instance of RunCommand, this
      // View Model is used by both the scripting and interactive versions
      // of the command.
      var model = new TextViewModel(doc, m_default_entity);
      // Run the scripting or GUI methods to gather data in the View Model.
      var result = (mode == RunMode.Scripted ? RunScript(model) : RunInteractive(model));
      if (result != Result.Success)
        return result;

      result = model.AddTextEntityToDocument();

      return result;
    }
    /// <summary>
    /// Called when the command is in interactive (window) mode
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    static Result RunInteractive(TextViewModel model)
    {
      var result = Result.Cancel;

      #region Mac Specific UI
      #if ON_OS_MAC
      // Create a NSWindow from a Nib file
      using (var window = RhinoMac.Window.FromNib("NewTextWindow", model))
      {
        // Display the window
        window.ShowModal();
        // dialogResult should be null if the window was closed
        // by clicking on the "X", false if the Cancel was called
        // and true if OK was called.
        var dialogResult = window.DialogResult;
        // Success will be true if the window was closed by the
        // OK button otherwise it should be false.
        result = (true == dialogResult ? Result.Success : Result.Cancel);
      }
      #endif
      #endregion

      #if ON_OS_WINDOWS
      var window = new Win.TextWindow();
      window.Loaded += window_Loaded;
      window.DataContext = model;
      // Need to save the window so it can be used as the
      // parent for the color dialog.
      model.Window = window;
      // Need to set the Rhino main frame window as the parent
      // for the new window otherwise the window will go behind
      // the main frame when the Rhino is deactivated then 
      // activated again.
      // http://blogs.msdn.com/b/mhendersblog/archive/2005/10/04/476921.aspx
      var interopHelper = new System.Windows.Interop.WindowInteropHelper(window);
      interopHelper.Owner = Rhino.RhinoApp.MainWindowHandle();
      window.ShowDialog();
      result = (true == window.DialogResult ? Result.Success : Result.Cancel);
      #endif

      return result;
    }

#if ON_OS_WINDOWS
    /// <summary>
    /// Remove the overflow control from the far right side of the tool bar.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    static void window_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
      var window = sender as Win.TextWindow;
      if (null == window) return;
      foreach (System.Windows.FrameworkElement item in window.toolBarControl.Items)
        System.Windows.Controls.ToolBar.SetOverflowMode(item, System.Windows.Controls.OverflowMode.Never);
      var overflowGrid = window.toolBarControl.Template.FindName("OverflowGrid", window.toolBarControl) as System.Windows.FrameworkElement;
      if (overflowGrid == null) return;
      overflowGrid.Visibility = System.Windows.Visibility.Collapsed;
    }
#endif
    /// <summary>
    /// Called when the command is in scripted mode
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    static Result RunScript(TextViewModel model)
    {
      var height = new Rhino.Input.Custom.OptionDouble(model.textHeight);
      var bold = new Rhino.Input.Custom.OptionToggle(model.bold, Localization.LocalizeCommandOptionValue("No", 396), Localization.LocalizeCommandOptionValue("Yes", 397));
      var italic = new Rhino.Input.Custom.OptionToggle(model.italic, Localization.LocalizeCommandOptionValue("No", 398), Localization.LocalizeCommandOptionValue("Yes", 399));
      var mask = new Rhino.Input.Custom.OptionToggle(model.maskEnabled, Localization.LocalizeCommandOptionValue("No", 400), Localization.LocalizeCommandOptionValue("Yes", 401));
      var masksource = new Rhino.Input.Custom.OptionToggle(model.maskUsesViewportColor, Localization.LocalizeCommandOptionValue("No", 402), Localization.LocalizeCommandOptionValue("Yes", 403));
      var maskcolor = new Rhino.Input.Custom.OptionColor(model.maskColor);
      var maskborder = new Rhino.Input.Custom.OptionDouble(model.maskOffset, true, 0);
      string facename = model.Font.FaceName;
      
      string newtext = "";
      var go = new Rhino.Input.Custom.GetString();
      var get_rc = Rhino.Input.GetResult.Cancel;
      var rc = Rhino.Commands.Result.Cancel;
      while (get_rc != Rhino.Input.GetResult.Nothing && get_rc != Rhino.Input.GetResult.String)
      {
        go.ClearCommandOptions();
        go.AcceptNothing(true);
        go.SetCommandPrompt(LOC.STR("Text string"));
        int faceopt = go.AddOption(LOC.CON("Font"));
        go.AddOptionDouble(LOC.CON("Height"), ref height, LOC.STR("Text height"));
        go.AddOptionToggle(LOC.CON("Italic"), ref italic);
        go.AddOptionToggle(LOC.CON("Mask"), ref mask);
        go.AddOptionToggle(LOC.CON("UseBackgroundColor"), ref masksource);
        go.AddOptionColor(LOC.CON("MaskColor"), ref maskcolor);
        go.AddOptionDouble(LOC.CON("MaskMargin"), ref maskborder);
        get_rc = go.Get();
        
        switch (get_rc)
        {
        case Rhino.Input.GetResult.Cancel:
          return Rhino.Commands.Result.Cancel;
        case Rhino.Input.GetResult.String:
          newtext = go.StringResult();
          break;
        case Rhino.Input.GetResult.Option:
          if( go.OptionIndex() == faceopt )
          {
            string oldname = facename;
            rc = Rhino.Input.RhinoGet.GetString(LOC.STR("Font name"), true, ref facename);
            if( rc!=Rhino.Commands.Result.Success)
              return rc;
            if (string.IsNullOrWhiteSpace(facename))
              facename = oldname;
          }
          break;
        }
      }
      
      if (string.IsNullOrWhiteSpace(newtext))
        return Rhino.Commands.Result.Cancel;
      
      // update default settings
      model.text = newtext;
      int fontindex = model.Doc.Fonts.FindOrCreate(facename, bold.CurrentValue, italic.CurrentValue);
      model.fontIndex = fontindex;
      model.textHeight = height.CurrentValue;
      model.maskEnabled = mask.CurrentValue;
      model.maskUsesViewportColor = masksource.CurrentValue;
      model.maskColor = maskcolor.CurrentValue;
      model.maskOffset = maskborder.CurrentValue;

      return Result.Success;
    }
  }
}
