using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Rhino;

namespace Text
{
  //////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Text field Window view model
  /// </summary>
  class TextFieldViewModel : Rhino.ViewModel.NotificationObject
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="doc"></param>
    public TextFieldViewModel(RhinoDoc doc)
    {
      _doc = doc;

      #region Define all of the fields
      // area
      string name = Rhino.UI.LOC.STR("Area");
      string description = Rhino.UI.LOC.STR("Calculate the area of a closed curve, hatch, surface, or mesh");
      _fields.Add("Area", new TextFieldData(TextFieldType.area, name, description));
      // block instance count
      name = Rhino.UI.LOC.STR("BlockInstanceCount");
      description = Rhino.UI.LOC.STR("Number of instances of a given block");
      _fields.Add("BlockInstanceCount", new TextFieldData(TextFieldType.blockinstancecount, name, description));
      //curvelength
      name = Rhino.UI.LOC.STR("CurveLength");
      description = Rhino.UI.LOC.STR("Calculate the length of a curve");
      _fields.Add("CurveLength", new TextFieldData(TextFieldType.curvelength, name, description));
      //date
      name = Rhino.UI.LOC.STR("Date");
      description = Rhino.UI.LOC.STR("The current date");
      _fields.Add("Date", new TextFieldData(TextFieldType.date, name, description));
      //datemodified
      name = Rhino.UI.LOC.STR("DateModified");
      description = Rhino.UI.LOC.STR("Date this document was last saved");
      _fields.Add("DateModified", new TextFieldData(TextFieldType.datemodified, name, description));
      //documenttext
      name = Rhino.UI.LOC.STR("DocumentText");
      description = Rhino.UI.LOC.STR("DocumentText value for a defined key");
      _fields.Add("DocumentText", new TextFieldData(TextFieldType.documenttext, name, description));
      //filename
      name = Rhino.UI.LOC.STR("FileName");
      description = Rhino.UI.LOC.STR("The document's name and location");
      _fields.Add("FileName", new TextFieldData(TextFieldType.filename, name, description));
      //modelunits
      name = Rhino.UI.LOC.STR("ModelUnits");
      description = Rhino.UI.LOC.STR("Current working units for the document");
      _fields.Add("ModelUnits", new TextFieldData(TextFieldType.modelunits, name, description));
      //notes
      name = Rhino.UI.LOC.STR("Notes");
      description = Rhino.UI.LOC.STR("The notes for the document");
      _fields.Add("Notes", new TextFieldData(TextFieldType.notes, name, description));
      //numpages
      name = Rhino.UI.LOC.STR("NumPages");
      description = Rhino.UI.LOC.STR("Total number of layout pages in the document");
      _fields.Add("NumPages", new TextFieldData(TextFieldType.numpages, name, description));
      //objectname
      name = Rhino.UI.LOC.STR("ObjectName");
      description = Rhino.UI.LOC.STR("Name for a given object", 383);
      _fields.Add("ObjectName", new TextFieldData(TextFieldType.objectname, name, description));
      //pagename
      name = Rhino.UI.LOC.STR("PageName");
      description = Rhino.UI.LOC.STR("Name of the layout page that this text field exists on");
      _fields.Add(name, new TextFieldData(TextFieldType.pagename, name, description));
      //pagenumber
      name = Rhino.UI.LOC.STR("PageNumber");
      description = Rhino.UI.LOC.STR("Number of the layout page that this text field exists on");
      _fields.Add("PageNumber", new TextFieldData(TextFieldType.pagenumber, name, description));
      //usertext
      name = Rhino.UI.LOC.STR("UserText");
      description = Rhino.UI.LOC.STR("UserText value for a given object/key combination");
      _fields.Add("UserText", new TextFieldData(TextFieldType.usertext, name, description));
      #endregion

      // Map localized field name to English field name
      foreach (var item in _fields)
        _localizdeFieldNameDictionary.Add(item.Value.LocalName, item.Key);

      // Sort the localized field name list
      string[] keys = new string[_localizdeFieldNameDictionary.Count];
      _localizdeFieldNameDictionary.Keys.CopyTo(keys, 0);
      List<string> sortedList = new List<string>(keys);
      sortedList.Sort((string0, string1) => { return string0.CompareTo(string1); });
      _sortedLocalizedFieldNameList = sortedList.ToArray();

      // Select the first key in the sorted list
      var key = KeyFromLocalizedKey(_sortedLocalizedFieldNameList[0]);
      selectedFieldKey = key;

    #if ON_OS_WINDOWS
      // Command delegates for WPF
      SelectObjectButtonClickedCommand = new RhinoWindows.Input.DelegateCommand(SelectObjectButtonClicked, null);
      AddNameValuePairButtonClickedCommand = new RhinoWindows.Input.DelegateCommand(AddNameValuePairButtonClicked, null);
    #endif
    }

    #region Windows specific
    #if ON_OS_WINDOWS
    /// <summary>
    /// The main window assoicated with this view model, this is
    /// used as the owner window when displaying common dialogs
    /// and child windows.
    /// </summary>
    public System.Windows.Window Window { get; set; }
    #region WPF Command delegates
    /// <summary>
    /// Command deleate for the "Select Objects" button.
    /// </summary>
    public System.Windows.Input.ICommand SelectObjectButtonClickedCommand { get; private set; }
    /// <summary>
    /// Command delegate for the "+" button located at the bottom
    /// of the name/value pair lists.
    /// </summary>
    public System.Windows.Input.ICommand AddNameValuePairButtonClickedCommand { get; private set; }
    #endregion WPF Command delegates
    /// <summary>
    /// Called when the "Select Objects" button is clicked when
    /// running in Windows.  Will hid the current Window and 
    /// prompt the user to select the appropriate object type.
    /// </summary>
    private void SelectObjectButtonClicked()
    {
      var field = SelectedField;
      if (null == field)
        return;
      var fieldType = field.Style;
      if (fieldType == TextFieldType.area)
        RhinoWindows.RhinoWindow.PushPickButton(Window, GetObjectForArea);
      else if (fieldType == TextFieldType.curvelength)
        RhinoWindows.RhinoWindow.PushPickButton(Window, GetObjectForCurveLength);
      else if (fieldType == TextFieldType.usertext)
        RhinoWindows.RhinoWindow.PushPickButton(Window, GetObjectForUserText);
      else if (fieldType == TextFieldType.objectname)
        RhinoWindows.RhinoWindow.PushPickButton(Window, GetObjectForName);

      if (Doc != null)
      {
        Doc.Objects.UnselectAll();
        Doc.Views.Redraw();
      }
    }
    #endif
    #endregion Windows specific

    #region Select object methods
    /// <summary>
    /// Select object for which an area value can be calculated
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GetObjectForArea(object sender, EventArgs e)
    {
      using (Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject())
      {
        go.SetCommandPrompt(Rhino.UI.Localization.LocalizeString("Select closed curve, hatch, surface, or mesh", 359));
        go.AcceptNothing(true);
        go.SubObjectSelect = false;
        go.GeometryFilter = Rhino.DocObjects.ObjectType.Curve | Rhino.DocObjects.ObjectType.Hatch | Rhino.DocObjects.ObjectType.Surface | Rhino.DocObjects.ObjectType.Brep | Rhino.DocObjects.ObjectType.Mesh;
        go.GeometryAttributeFilter = Rhino.Input.Custom.GeometryAttributeFilter.ClosedCurve;
        go.DisablePreSelect();
        go.Get();
        if (go.CommandResult() == Rhino.Commands.Result.Success)
        {
          Rhino.DocObjects.ObjRef objref = go.Object(0);
          if (objref != null)
          {
            SelectedObjectId = objref.ObjectId;
            objref.Dispose();
          }
        }
      }
    }
    /// <summary>
    /// Select a cureve for length calculation
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GetObjectForCurveLength(object sender, EventArgs e)
    {
      using (Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject())
      {
        go.SetCommandPrompt(Rhino.UI.Localization.LocalizeString("Select curve", 360));
        go.AcceptNothing(true);
        go.SubObjectSelect = false;
        go.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
        go.DisablePreSelect();
        go.Get();
        if (go.CommandResult() == Rhino.Commands.Result.Success)
        {
          Rhino.DocObjects.ObjRef objref = go.Object(0);
          if (objref != null)
          {
            SelectedObjectId = objref.ObjectId;
            objref.Dispose();
          }
        }
      }
    }
    /// <summary>
    /// Select an object for object name display
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GetObjectForName(object sender, EventArgs e)
    {
      using (Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject())
      {
        go.SetCommandPrompt(Rhino.UI.Localization.LocalizeString("Select object", 384));
        go.AcceptNothing(true);
        go.SubObjectSelect = false;
        go.DisablePreSelect();
        go.Get();
        if (go.CommandResult() == Rhino.Commands.Result.Success)
        {
          Rhino.DocObjects.ObjRef objref = go.Object(0);
          if (objref != null)
          {
            SelectedObjectId = objref.ObjectId;
            objref.Dispose();
          }
        }
      }
    }
    /// <summary>
    /// Select an object which will be used for the user text list
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GetObjectForUserText(object sender, EventArgs e)
    {
      using (Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject())
      {
        go.SetCommandPrompt(Rhino.UI.Localization.LocalizeString("Select object", 361));
        go.AcceptNothing(true);
        go.DisablePreSelect();
        go.Get();
        if (go.CommandResult() == Rhino.Commands.Result.Success)
        {
          Rhino.DocObjects.ObjRef objref = go.Object(0);
          if (objref != null)
          {
            SelectedObjectId = objref.ObjectId;
            SetupSelectObjectPanelHelper(TextFieldType.usertext, false);
            objref.Dispose();
          }
        }
      }
    }
    #endregion Select object methods

    #region Local methods
    /// <summary>
    /// Get internal key name from localized key name
    /// </summary>
    /// <returns>The from localized key.</returns>
    /// <param name="key">Key.</param>
    private string KeyFromLocalizedKey(string key)
    {
      string result;
      _localizdeFieldNameDictionary.TryGetValue(key, out result);
      return result;
    }
    /// <summary>
    /// Get the field index for the specified English
    /// field key name.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>
    /// Returns the dictionary index if the key is found
    /// otherwise returns -1;
    /// </returns>
    private int FieldIndexFromKey(string key)
    {
      int i = 0;
      foreach (var field in _fields)
      {
        if (field.Key == key)
          return i;
        i++;
      }
      return -1;
    }
    #endregion Local methods

    #region Overrides for Mac
    #if ON_OS_MAC
    /// <summary>
    /// Check to see if the window should close.
    /// </summary>
    /// <returns>
    /// Return true if it is okay to close the window otherwise
    /// return false.
    /// </returns>
    public override bool WindowShouldClose()
    {
      if (null != Window && Window.DialogResult != true || OkayToClose())
        return true;
      // Clear the dialog result flag so it will be set
      // properly if the window is closed by clicking
      // the "X"
      if (null != Window)
        Window.DialogResult = null;
      return false;
    }
    #endif
    #endregion Overrides for Mac

    #region Panel setup helper functions
    /// <summary>
    /// Called when the current filed changes, will inialize the
    /// new current page.
    /// </summary>
    /// <param name="fieldData">The current field defintion</param>
    private void OnSelectedFieldChanged(TextFieldData fieldData)
    {
      // update description
      var fieldType = TextFieldType.None;
      if (fieldData != null)
      {
        // Set the field decription display text
        selectedFieldDescription = fieldData.Description;
        // Extract the field type
        fieldType = fieldData.Style;
      }
      else
      {
        // Nothing is selected so clear the description text
        selectedFieldDescription = string.Empty;
      }
      // Set up panels based on the text field type
      formatString = string.Empty;
      SetupSelectObjectPanelHelper(fieldType, true);
      SetupSimplePanelHelper(fieldType);
      SetupFilenamePanelHelper(fieldType);
      SetupDatePanelHelper(fieldType);
      SetupDocTextPanelHelper(fieldType);
      SetupCountPanelHelper(fieldType);
    }
    private void SetupSelectObjectPanelHelper(TextFieldType fieldType, bool resetId)
    {
      if( resetId )
        SelectedObjectId = Guid.Empty;
      if (fieldType != TextFieldType.usertext) return;
      showUserTextFieldCollection = (SelectedObjectId != Guid.Empty && Doc != null);
      _nameValuePairCollection.Clear();
      selectedNameValuePairIndex = -1;
      if (showUserTextFieldCollection)
      {
        var rhobj = SelectedObject;
        if (null != rhobj)
        {
          var userstrings = rhobj.Attributes.GetUserStrings();
          for (int i = 0; i < userstrings.Count; i++)
          {
            var namevalue = new NameValuePair(userstrings.Keys[i], userstrings[i]);
            _nameValuePairCollection.Add(namevalue);
          }
        }
      }
    }
    private void SetupSimplePanelHelper(TextFieldType fieldType)
    {
      switch(fieldType)
      {
        case TextFieldType.modelunits:
          if (Doc != null)
            formatString = Doc.GetUnitSystemName(true, false, false, false);
          break;
        case TextFieldType.notes:
          if (Doc != null)
          {
            string notes = Doc.Notes;
            formatString = string.IsNullOrEmpty(notes) ? "####" : notes;
          }
          break;
        case TextFieldType.numpages:
          if (Doc != null)
          {
            var pageviews = Doc.Views.GetPageViews();
            int count = pageviews == null ? 0 : pageviews.Length;
            formatString = count.ToString();
          }
          break;
        case TextFieldType.pagename:
          if (Doc != null)
          {
            var page = Doc.Views.ActiveView as Rhino.Display.RhinoPageView;
            formatString = page == null ? "####" : page.MainViewport.Name;
          }
          break;
        case TextFieldType.pagenumber:
          if (Doc != null)
          {
            var page = Doc.Views.ActiveView as Rhino.Display.RhinoPageView;
            formatString = page == null ? "####" : page.PageNumber.ToString();
          }
          break;
      }
    }
    private void SetupFilenamePanelHelper(TextFieldType fieldType)
    {
      if (null == Doc || fieldType != TextFieldType.filename)
        return;
      string path = includeFullPath ? Doc.Path : Doc.Name;
      if (!includeFileExtension && path.EndsWith(".3dm", StringComparison.OrdinalIgnoreCase))
        path = path.Substring(0, path.Length - ".3dm".Length);
      formatString = string.IsNullOrEmpty(path) ? "####" : path;
    }
    private void SetupDatePanelHelper(TextFieldType fieldType)
    {
      if (fieldType != TextFieldType.date && fieldType != TextFieldType.datemodified)
        return;
      if (_dateFormatList.Count < 1)
      {
        var dt = DateTime.Now;
        if (fieldType == TextFieldType.datemodified && Doc != null)
        {
          var doc_dt = Doc.DateLastEdited;
          if (doc_dt != DateTime.MinValue)
            dt = doc_dt;
        }
        // Use the same format codes as Word
        _dateFormatList.Add(new DateFormat("M/d/yyyy", dt));
        _dateFormatList.Add(new DateFormat("dddd, MMMM dd, yyyy", dt));
        _dateFormatList.Add(new DateFormat("MMMM d, yyyy", dt));
        _dateFormatList.Add(new DateFormat("M/d/yy", dt));
        _dateFormatList.Add(new DateFormat("yyyy-MM-dd", dt));
        _dateFormatList.Add(new DateFormat("d-MMM-yy", dt));
        _dateFormatList.Add(new DateFormat("M.d.yyyy", dt));
        _dateFormatList.Add(new DateFormat("MMM. d, yy", dt));
        _dateFormatList.Add(new DateFormat("d MMMM yyyy", dt));
        _dateFormatList.Add(new DateFormat("MMMM yy", dt));
        _dateFormatList.Add(new DateFormat("MMM-yy", dt));
        _dateFormatList.Add(new DateFormat("M/d/yyyy h:mm", dt));
        _dateFormatList.Add(new DateFormat("M/d/yyyy h:mm:ss", dt));
        _dateFormatList.Add(new DateFormat("h:mm", dt));
        _dateFormatList.Add(new DateFormat("h:mm:ss", dt));
        _dateFormatList.Add(new DateFormat("HH:mm", dt));
        _dateFormatList.Add(new DateFormat("HH:mm:ss", dt));
        RaisePropertyChanged(() => dateFormatList);
      }
      if (selectedDateFormat >= 0 && selectedDateFormat < _dateFormatList.Count)
        formatString = _dateFormatList[selectedDateFormat].Format;
      else
        formatString = string.Empty;
    }
    private void SetupDocTextPanelHelper(TextFieldType fieldType)
    {
      if (fieldType != TextFieldType.documenttext)
        return;
      _nameValuePairCollection.Clear();
      selectedNameValuePairIndex = -1;
      if (Doc != null && Doc.Strings.Count > 0)
      {
        int count = Doc.Strings.Count;
        for (int i = 0; i < count; i++)
        {
          var key = Doc.Strings.GetKey(i);
          var value = Doc.Strings.GetValue(i);
          var item = new NameValuePair(key, value);
          _nameValuePairCollection.Add(item);
        }
      }
    }
    private void SetupCountPanelHelper(TextFieldType fieldType)
    {
      if (fieldType != TextFieldType.blockinstancecount)
        return;
      if (Doc != null && _blockNameList.Count == 0)
      {
        Rhino.DocObjects.InstanceDefinition[] idefs = Doc.InstanceDefinitions.GetList(true);
        for (int i = 0; i < idefs.Length; i++)
        {
          if (idefs[i].InUse(0))
          {
            string name = idefs[i].Name;
            if (!string.IsNullOrEmpty(name))
              _blockNameList.Add(name);
          }
        }
        RaisePropertyChanged(() => blockNameList);
      }
      if (selectedBlockNameIndex < 0)
        formatString = Rhino.UI.LOC.STR("Selct block to count");
      else if (null == Doc)
        formatString = string.Empty;
      else if (blockNameList.Count < 1)
        formatString = Rhino.UI.LOC.STR("No blocks to count");
      else
      {
        var blockName = _blockNameList[selectedBlockNameIndex];
        var idef = Doc.InstanceDefinitions.Find(blockName, true);
        if (null == idef)
          formatString = string.Empty;
        else
        {
          var instanceobjs = idef.GetReferences(0);
          int count = instanceobjs.Length;
          formatString = count.ToString();
        }
      }
    }
    #endregion Panel setup helper functions

    #region Button click methods
    /// <summary>
    /// The "+" at the bottom of a name/value pair list was
    /// clicked so show the add field Window and add the user
    /// text to the appropriate place.
    /// </summary>
    public void AddNameValuePairButtonClicked()
    {
      // If there is no document or field selected bail
      if (Doc == null || SelectedField == null)
        return;
      // If the current field is not Document Text or 
      // object User Text then bail
      var field = SelectedField;
      if (field.Style != TextFieldType.documenttext && field.Style != TextFieldType.usertext)
        return;
      // If filed type is user string then make sure there is an object
      // to get/set user strings for
      Rhino.DocObjects.RhinoObject rhobj = null;
      if (field.Style == TextFieldType.usertext)
      {
        rhobj = SelectedObject;
        // Can't add a field if we can't find the object
        if (null == rhobj)
          return;
      }
      // Place holders for new text values
      var keyText = string.Empty;
      var valueText = string.Empty;
      // Display the OS appropriate Window
#if ON_OS_WINDOWS
      var window = new WPF.TextFieldAddDocumentTextWindow();
      window.Owner = Window;
      window.ShowDialog();
      if (window.DialogResult == true)
      {
        keyText = window.keyText.Text;
        valueText = window.valueText.Text;
      }
#endif
#if ON_OS_MAC
#endif
      // If no new users strings were specified then bail
      if (string.IsNullOrEmpty(keyText) || string.IsNullOrWhiteSpace(valueText))
        return;
      if (field.Style == TextFieldType.documenttext)
      {
        // Add or update document user string
        Doc.Strings.SetString(keyText, valueText);
        SetupDocTextPanelHelper(TextFieldType.documenttext);
      }
      else if (field.Style == TextFieldType.usertext && null != rhobj)
      {
        // Add or update object user string
        rhobj.Attributes.SetUserString(keyText, valueText);
        rhobj.CommitChanges();
        SetupSelectObjectPanelHelper(TextFieldType.usertext, false);
      }
    }
    #endregion Button click methods

    #region Public methods
    /// <summary>
    /// Call just prior to closing this Window to determine if
    /// all appropriate data has been provided.
    /// </summary>
    /// <returns>
    /// If everything needed for the field defintion has been
    /// provided the return true otherwiwse; return false.
    /// </returns>
    public bool OkayToClose()
    {
      TextFieldData fieldData = SelectedField;
      if (null == fieldData)
        return false;
      bool result = true;
      switch (fieldData.Style)
      {
        case TextFieldType.area:
        case TextFieldType.curvelength:
        case TextFieldType.objectname:
        case TextFieldType.usertext:
          if (SelectedObjectId == Guid.Empty)
          {
            result = false;
            selectedFieldDescription = formatString = Rhino.UI.LOC.STR("Must select an object first");
          }
          break;
        case TextFieldType.blockinstancecount:
          if (selectedBlockNameIndex < 0 || selectedBlockNameIndex >= blockNameList.Count)
          {
            selectedFieldDescription = formatString = Rhino.UI.LOC.STR("Must specify a block name first");
          }
          break;
        }
      return result;
    }
    /// <summary>
    /// Calculate the format string to add to the text window.
    /// </summary>
    /// <returns>
    /// Returns a properly formatted text field string which
    /// may be added to the new text dialog or null if the
    /// format string could not be calculated.
    /// </returns>
    public string CalculateFinalFormatString()
    {
      TextFieldData fieldData = SelectedField;
      if (null == fieldData)
        return string.Empty;
      string result = string.Empty;
      // set up the format string
      switch (fieldData.Style)
      {
        case TextFieldType.area:
        case TextFieldType.curvelength:
        case TextFieldType.objectname:
          if (SelectedObjectId != Guid.Empty)
            result = "%<" + fieldData.Style.ToString() + "(\"" + SelectedObjectId.ToString() + "\")>%";
          break;
        case TextFieldType.blockinstancecount:
          if (selectedBlockNameIndex >= 0 && selectedBlockNameIndex < blockNameList.Count)
            result = "%<" + fieldData.Style.ToString() + "(\"" + blockNameList[selectedBlockNameIndex] + "\")>%";
          break;
        case TextFieldType.date:
        case TextFieldType.datemodified:
          {
            string format = formatString;
            if (string.IsNullOrEmpty(format))
              result = "%<" + fieldData.Style.ToString() + ">%";
            else
              result = "%<" + fieldData.Style.ToString() + "(\"" + format + "\")>%";
          }
          break;
        case TextFieldType.documenttext:
          if (null != SelectedNameValuePair)
          {
            var item = SelectedNameValuePair;
            string format = item.Name;
            if (!string.IsNullOrEmpty(format))
              result = "%<" + fieldData.Style.ToString() + "(\"" + format + "\")>%";
          }
          break;
        case TextFieldType.filename:
          if (includeFileExtension && includeFullPath)
            result = "%<" + fieldData.Style.ToString() + ">%";
          else
          {
            int option = 0;
            if (!includeFullPath)
              option = 1;
            if (!includeFileExtension)
              option += 2;
            result = string.Format("%<{0}(\"{1}\")>%", fieldData.Style.ToString(), option);
          }
          break;
        case TextFieldType.modelunits:
        case TextFieldType.notes:
        case TextFieldType.numpages:
        case TextFieldType.pagename:
        case TextFieldType.pagenumber:
          result = "%<" + fieldData.Style.ToString() + ">%";
          break;
        case TextFieldType.usertext:
          if (SelectedObjectId != Guid.Empty && null != SelectedNameValuePair)
          {
            string format = SelectedNameValuePair.Name;
            if (!string.IsNullOrEmpty(format))
              result = "%<" + fieldData.Style.ToString() + "(\"" + SelectedObjectId.ToString() + "\",\"" + format + "\")>%";
          }
          break;
      }
      return result;
    }
    #endregion

    #region GetPoint helpers
    public void GetObjectForArea()
    {
      using(Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject())
      {
        go.SetCommandPrompt(Rhino.UI.LOC.STR("Select closed curve, hatch, surface, or mesh"));
        go.AcceptNothing(true);
        go.SubObjectSelect = false;
        go.GeometryFilter = Rhino.DocObjects.ObjectType.Curve | Rhino.DocObjects.ObjectType.Hatch | Rhino.DocObjects.ObjectType.Surface | Rhino.DocObjects.ObjectType.Brep | Rhino.DocObjects.ObjectType.Mesh;
        go.GeometryAttributeFilter = Rhino.Input.Custom.GeometryAttributeFilter.ClosedCurve;
        go.DisablePreSelect();
        go.Get();
        if( go.CommandResult()== Rhino.Commands.Result.Success )
        {
          Rhino.DocObjects.ObjRef objref = go.Object(0);
          if( objref!=null )
          {
            SelectedObjectId = objref.ObjectId;
            objref.Dispose();
          }
        }
      }
    }
    #endregion GetPoint helpers

    #region Public properties
    /// <summary>
    /// Gets the field name list.
    /// </summary>
    /// <value>The field name list.</value>
    public string[] fieldNameList
    {
      get { return _sortedLocalizedFieldNameList; }
    }
    /// <summary>
    /// Currently selected English field key name
    /// </summary>
    public string selectedFieldKey
    {
      get { return _selectedFieldKey; }
      set
      {
        if (value == _selectedFieldKey) return;
        _selectedFieldKey = value;
        RaisePropertyChanged(() => selectedFieldKey);
        if (_fields.ContainsKey(_selectedFieldKey))
          selectedFieldDescription = _fields[_selectedFieldKey].Description;
        else
          selectedFieldDescription = string.Empty;
        TextFieldData data = SelectedField;
        OnSelectedFieldChanged(data);
      }
    }
    /// <summary>
    /// Currenly selected field index
    /// </summary>
    public int selectedFieldIndex
    {
      get { return FieldIndexFromKey(_selectedFieldKey); }
      set
      {
        if (value == selectedBlockNameIndex) return;        var keys = _fields.Keys;
        if (value >= 0 && value < _fields.Count)
        {
          int i = 0;
          foreach (var field in _fields)
          {
            if (i == value)
            {
              selectedFieldKey = field.Key;
              break;
            }
            i++;
          }
        }
        else
          selectedFieldKey = string.Empty;
        RaisePropertyChanged(() => selectedFieldIndex);
      }
    }
    /// <summary>
    /// Currently seelcted field record
    /// </summary>
    public TextFieldData SelectedField
    {
      get
      {
        TextFieldData result;
        _fields.TryGetValue(selectedFieldKey, out result);
        return result;
      }
    }
    /// <summary>
    /// Currently selected field description display string
    /// </summary>
    public string selectedFieldDescription
    {
      get { return _selectedFieldDescription; }
      private set
      {
        if (value == _selectedFieldDescription) return;
        _selectedFieldDescription = value;
        RaisePropertyChanged(() => selectedFieldDescription);
      }
    }
    /// <summary>
    /// Evaluated expression text
    /// </summary>
    public string formatString
    {
      get
      {
        return _formatString;
      }
      private set
      {
        if (value == _formatString) return;
        _formatString = value;
        RaisePropertyChanged(() => formatString);
      }
    }
    /// <summary>
    /// Currently selected date format option
    /// </summary>
    public int selectedDateFormat
    {
      get { return _selectedDateFormat; }
      set
      {
        if (value == selectedDateFormat) return;
        _selectedDateFormat = value;
        RaisePropertyChanged(() => selectedDateFormat);
        SetupDatePanelHelper(TextFieldType.date);
      }
    }
    /// <summary>
    /// Date format list to be used as list control
    /// contents.
    /// </summary>
    public ObservableCollection<string> dateFormatList
    {
      get
      {
        var result = new ObservableCollection<string>();
        foreach (var item in _dateFormatList)
          result.Add(item.ToString());
        return result;
      }
    }
    /// <summary>
    /// File name option to include file path in output
    /// </summary>
    public bool includeFullPath
    {
      get
      {
        return _includeFullPath; }
      set
      {
        if (value == _includeFullPath) return;
        _includeFullPath = value;
        RaisePropertyChanged(() => includeFullPath);
        SetupFilenamePanelHelper(TextFieldType.filename);
      }
    }
    /// <summary>
    /// File name option to include file extension in output
    /// </summary>
    public bool includeFileExtension
    {
      get {
        return _includeFileExtension; }
      set
      {
        if (value == _includeFileExtension) return;
        _includeFileExtension = value;
        RaisePropertyChanged(() => includeFileExtension);
        SetupFilenamePanelHelper(TextFieldType.filename);
      }
    }
    /// <summary>
    /// Documnets block name list for List Controls
    /// </summary>
    public ObservableCollection<string> blockNameList
    {
      get { return _blockNameList; }
    }
    /// <summary>
    /// Currently selected block name
    /// </summary>
    public int selectedBlockNameIndex
    {
      get { return _selectedBlockNameIndex; }
      set
      {
        if (value == _selectedBlockNameIndex) return;
        _selectedBlockNameIndex = value;
        RaisePropertyChanged(() => selectedBlockNameIndex);
        SetupCountPanelHelper(TextFieldType.blockinstancecount);
      }
    }
    /// <summary>
    /// Selected object Guid
    /// </summary>
    public Guid SelectedObjectId
    {
      get { return _objectId; }
      set
      {
        _objectId = value;
        if (Guid.Empty == _objectId)
          formatString = Rhino.UI.LOC.STR("No object selected");
        else
          formatString = Rhino.UI.LOC.STR("Id = ") + _objectId.ToString();
        showUserTextFieldCollection = (null != SelectedObject);
      }
    }
    /// <summary>
    /// The selected object from the current document
    /// </summary>
    public Rhino.DocObjects.RhinoObject SelectedObject
    {
      get
      {
        if (null == Doc || SelectedObjectId == Guid.Empty)
          return null;
        return Doc.Objects.Find(SelectedObjectId);
      }
    }
    /// <summary>
    /// Name, value collection used for document and 
    /// object user strings.
    /// </summary>
    public ObservableCollection<NameValuePair> nameValuePairCollection
    {
      get { return _nameValuePairCollection; }
      set { }
    }
    /// <summary>
    /// Currently selected user string index
    /// </summary>
    public int selectedNameValuePairIndex
    {
      get { return _selectedNameValuePair; }
      set
      {
        if (value == _selectedNameValuePair) return;
        _selectedNameValuePair = value;
        RaisePropertyChanged(() => selectedNameValuePairIndex);
      }
    }
    /// <summary>
    /// Currently selected user string record
    /// </summary>
    public NameValuePair SelectedNameValuePair
    {
      get
      {
        if (selectedNameValuePairIndex >= 0 && selectedNameValuePairIndex < _nameValuePairCollection.Count)
          return _nameValuePairCollection[selectedNameValuePairIndex];
        return null;
      }
    }
    /// <summary>
    /// Show user text list control flag
    /// </summary>
    public bool showUserTextFieldCollection
    {
      get { return _showUserTextFieldCollection; }
      set
      {
        if (_showUserTextFieldCollection == value) return;
        _showUserTextFieldCollection = value;
        RaisePropertyChanged(() => showUserTextFieldCollection);
        RaisePropertyChanged(() => userTextFieldCollectionVisibility);
      }
    }
    public string userTextFieldCollectionVisibility
    {
      get { return (showUserTextFieldCollection ? "Visible" : "Hidden"); }
    }
    /// <summary>
    /// Rhino document associated with this view model instance.
    /// </summary>
    public RhinoDoc Doc { get { return _doc; } }
    #endregion Public properties

    #region Private members
    /// <summary>
    /// The document used by this view model instance
    /// </summary>
    private readonly RhinoDoc _doc;
    /// <summary>
    /// Dictionary of filed defintions
    /// </summary>
    private readonly Dictionary<string, TextFieldData> _fields = new Dictionary<string, TextFieldData>();
    /// <summary>
    /// Map localized field names to English field names
    /// </summary>
    private readonly Dictionary<string, string> _localizdeFieldNameDictionary = new Dictionary<string, string>();
    /// <summary>
    /// Sorted, localized field name list
    /// </summary>
    private readonly string[] _sortedLocalizedFieldNameList;
    /// <summary>
    /// Currently selected field
    /// </summary>
    private string _selectedFieldKey;
    /// <summary>
    /// Currently selected field description
    /// </summary>
    private string _selectedFieldDescription = string.Empty;
    /// <summary>
    /// Evalued expression string
    /// </summary>
    private string _formatString = string.Empty;
    /// <summary>
    /// Selected object Id
    /// </summary>
    private Guid _objectId = Guid.Empty;
    /// <summary>
    /// File name option to display full path
    /// </summary>
    private bool _includeFullPath = true;
    /// <summary>
    /// File name option to display file extension
    /// </summary>
    private bool _includeFileExtension = true;
    /// <summary>
    /// Date format list used by current or last save date options
    /// </summary>
    private ObservableCollection<DateFormat> _dateFormatList = new ObservableCollection<DateFormat>();
    /// <summary>
    /// Currently selected date format
    /// </summary>
    private int _selectedDateFormat = -1;
    /// <summary>
    /// Block name list used when diplaying block counts
    /// </summary>
    private ObservableCollection<string> _blockNameList = new ObservableCollection<string>();
    /// <summary>
    /// Selected block to count
    /// </summary>
    private int _selectedBlockNameIndex = -1;
    /// <summary>
    /// Colleciton of name value pairs used by document and object user text
    /// options
    /// </summary>
    private ObservableCollection<NameValuePair> _nameValuePairCollection = new ObservableCollection<NameValuePair>();
    /// <summary>
    /// Currently selected name value pair
    /// </summary>
    private int _selectedNameValuePair = -1;
    /// <summary>
    /// If there is an object selected and it can be found then display the
    /// user text list
    /// </summary>
    private bool _showUserTextFieldCollection;
    #endregion Private members
  }
  //////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Document or object user string
  /// </summary>
  class NameValuePair
  {
    public string Name { get; set; }
    public string Value { get; set; }
    public NameValuePair(string name, string value)
    {
      Name = name;
      Value = value;
    }
  }
  //////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Date format used by current or last saved date
  /// </summary>
  class DateFormat
  {
    DateTime m_date;
    readonly string m_format;
    public DateFormat(string format, DateTime date)
    {
      m_format = format;
      m_date = date;
    }
    public override string ToString()
    {
      return m_date.ToString(m_format);
    }
    public string Format
    {
      get { return m_format; }
    }
  }
  //////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Text field type.
  /// </summary>
  enum TextFieldType
  {
    None,
    area,
    blockinstancecount,
    curvelength,
    date,
    datemodified,
    documenttext,
    filename,
    modelunits,
    notes,
    numpages,
    objectname,
    pagename,
    pagenumber,
    usertext
  }
  //////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Text field data.
  /// </summary>
  class TextFieldData
  {
    readonly TextFieldType m_style;
    readonly string m_local_name;
    readonly string m_description;
    public TextFieldData(TextFieldType style, string local_name, string description)
    {
      m_style = style;
      m_local_name = local_name;
      m_description = description;
    }
    public TextFieldType Style { get { return m_style; } }
    public string EnglishName { get { return m_style.ToString(); } }
    public string LocalName { get { return m_local_name; } }
    public string Description { get { return m_description; } }
    public override string ToString()
    {
      return LocalName;
    }
  }
}

