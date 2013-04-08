using System;
using System.Collections.Generic;
using Rhino;

namespace Text
{
  class TextFieldViewModel : Rhino.ViewModel.NotificationObject
  {
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

      // Translate from localized name field name key to key name
      foreach (var item in _fields)
        _localizdeFieldNameDictionary.Add(item.Value.LocalName, item.Key);

      string[] keys = new string[_localizdeFieldNameDictionary.Count];
      _localizdeFieldNameDictionary.Keys.CopyTo(keys, 0);
      List<string> sortedList = new List<string>(keys);
      sortedList.Sort((string0, string1) => { return string0.CompareTo(string1); });
      _sortedLocalizedFieldNameList = sortedList.ToArray();

      var key = KeyFromLocalizedKey(_sortedLocalizedFieldNameList[0]);
      selectedFieldKey = key;
    #if ON_OS_WINDOWS
      SelectObjectButtonClickedCommand = new Rhino.Windows.Input.DelegateCommand(SelectObjectButtonClicked, null);
    #endif
    }

    #region Windows specific
    #if ON_OS_WINDOWS
    public System.Windows.Window Window { get; set; }
    public System.Windows.Input.ICommand SelectObjectButtonClickedCommand { get; private set; }
    private void SelectObjectButtonClicked()
    {
      TextFieldData field;
      _fields.TryGetValue(selectedFieldKey, out field);
      if (null == field)
        return;
      //var fieldType = field.Style;
      //var parent = Rhino.Windows.Forms.WindowsInterop.ObjectAsIWin32Window(Window);
      //if (fieldType == TextFieldType.area)
      //  Rhino.UI.Dialogs.PushPickButton(parent, GetObjectForArea);
    }
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
    #endif
    #endregion Windows specific

    #region Local methods
    /// <summary>
    /// Get internal key name from localized key name
    /// </summary>
    /// <returns>The from localized key.</returns>
    /// <param name="key">Key.</param>
    string KeyFromLocalizedKey(string key)
    {
      string result;
      _localizdeFieldNameDictionary.TryGetValue(key, out result);
      return result;
    }
    #endregion Local methods

    #region Overrides for Mac
    #if ON_OS_MAC
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

    #region panel setup helper functions
    private void OnSelectedFieldChanged(TextFieldData fieldData)
    {
      // update description
      var fieldType = TextFieldType.None;
      if (fieldData != null)
      {
        selectedFieldDescription = fieldData.Description;
        fieldType = fieldData.Style;
      }
      else
      {
        selectedFieldDescription = string.Empty;
      }
      // set up panels based on the text field type
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
      bool showUserText = (fieldType == TextFieldType.usertext && SelectedObjectId != Guid.Empty && Doc != null);
      if (showUserText)
      {
        Rhino.DocObjects.RhinoObject rhobj = Doc.Objects.Find(SelectedObjectId);
        if( rhobj!=null )
        {
          _userStringList.Clear();
          System.Collections.Specialized.NameValueCollection userstrings = rhobj.Attributes.GetUserStrings();
          for (int i = 0; i < userstrings.Count; i++)
          {
            var namevalue = new KeyValuePair<string,string>(userstrings.Keys[i], userstrings[i]);
            _userStringList.Add(namevalue);
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
      _docTextList.Clear();
      if (Doc != null && Doc.Strings.Count > 0)
      {
        int count = Doc.Strings.Count;
        for (int i = 0; i < count; i++)
        {
          var key = Doc.Strings.GetKey(i);
          var value = Doc.Strings.GetValue(i);
          var item = new DocumentString(key, value);
          _docTextList.Add(item);
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
      else if (blockNameList.Length < 1)
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
    #endregion panel setup helper functions

    #region Public methods
    void SelectAreaObjectButtonClick()
    {
      // TODO:
      // Rhino.UI.Dialogs.PushPickButton(this, GetObjectForArea);
    }
    public bool OkayToClose()
    {
      TextFieldData fieldData;
      _fields.TryGetValue(selectedFieldKey, out fieldData);
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
          if (selectedBlockNameIndex < 0 || selectedBlockNameIndex >= blockNameList.Length)
          {
            selectedFieldDescription = formatString = Rhino.UI.LOC.STR("Must specify a block name first");
          }
          break;
        }
      return result;
    }
    public string CalculateFinalFormatString()
    {
      TextFieldData fieldData;
      _fields.TryGetValue(selectedFieldKey, out fieldData);
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
          if (selectedBlockNameIndex >= 0 && selectedBlockNameIndex < blockNameList.Length)
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
/* TODO:
          {
            ListView.SelectedListViewItemCollection items = m_list_doctext.SelectedItems;
            if (items.Count == 1)
            {
              string format = items[0].Text;
              if( !string.IsNullOrEmpty(format) )
                m_textfield_string = "%<" + fieldData.Style.ToString() + "(\"" + format + "\")>%";
            }
          }
*/
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
/* TODO:
          if (SelectedObjectId != Guid.Empty)
          {
            ListView.SelectedListViewItemCollection items = m_list_usertext.SelectedItems;
            if (items.Count == 1)
            {
              string format = items[0].Text;
              if (!string.IsNullOrEmpty(format))
                m_textfield_string = "%<" + fieldData.Style.ToString() + "(\"" + m_object_id.ToString() + "\",\"" + format + "\")>%";
            }
          }
*/
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
        TextFieldData data;
        _fields.TryGetValue(selectedFieldKey, out data);
        OnSelectedFieldChanged(data);
      }
    }
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
    int FieldIndexFromKey(string key)
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
    public string[] dateFormatList
    {
      get
      {
        var result = new string[_dateFormatList.Count];
        var i = 0;
        foreach (var item in _dateFormatList)
          result[i++] = item.ToString();
        return result;
      }
    }
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
    public string[] blockNameList
    {
      get { return _blockNameList.ToArray(); }
    }
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
      }
    }
    public List<DocumentString> documentTextList
    {
      get
      {
        if (_docTextList.Count < 1)
        {
          _docTextList.Add(new DocumentString("Key1", "Value1"));
          _docTextList.Add(new DocumentString("Key2", "Value2"));
          _docTextList.Add(new DocumentString("Key3", "Value3"));
        }
        return _docTextList;
      }
    }

    RhinoDoc Doc { get { return _doc; } }
    #endregion Public properties

    #region Private members
    /// <summary>
    /// The document used to create the new point
    /// </summary>
    private readonly RhinoDoc _doc;
    private string _selectedFieldKey;
    private string _selectedFieldDescription = string.Empty;
    private string _formatString = string.Empty;
    private readonly Dictionary<string,TextFieldData> _fields = new Dictionary<string,TextFieldData>();
    private readonly Dictionary<string,string>_localizdeFieldNameDictionary = new Dictionary<string, string>();
    private readonly string[] _sortedLocalizedFieldNameList;
    private Guid _objectId = Guid.Empty;
    private List<KeyValuePair<string,string>> _userStringList = new List<KeyValuePair<string,string>>();
    private bool _includeFullPath = true;
    private bool _includeFileExtension = true;
    private List<DateFormat> _dateFormatList = new List<DateFormat>();
    private int _selectedDateFormat = -1;
    private List<DocumentString> _docTextList = new List<DocumentString>();
    private int _selectedDockTextIndex = -1;
    private List<string> _blockNameList = new List<string>();
    private int _selectedBlockNameIndex = -1;
    #endregion Private members
  }
  class DocumentString
  {
    public string Name { get; set; }
    public string Value { get; set; }
    public DocumentString(string name, string value)
    {
      Name = name;
      Value = value;
    }
  }
  //////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Date format.
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

