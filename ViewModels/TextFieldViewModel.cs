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

      var key = KeyFromLocalizedKey(_sortedLocalizedFieldNameList[1]);
      selectedFieldKey = key;
    }

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
    #endregion

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
          selectedFiledDescription = _fields[_selectedFieldKey].Description;
        else
          selectedFiledDescription = string.Empty;
      }
    }
    public string selectedFiledDescription
    {
      get { return _selectedFieldDescription; }
      private set
      {
        if (value == _selectedFieldDescription) return;
        _selectedFieldDescription = value;
        RaisePropertyChanged(() => selectedFiledDescription);
      }
    }
    #endregion

    #region Private members
    /// <summary>
    /// The document used to create the new point
    /// </summary>
    private readonly RhinoDoc _doc;
    private string _selectedFieldKey;
    private string _selectedFieldDescription = string.Empty;
    private readonly Dictionary<string,TextFieldData> _fields = new Dictionary<string,TextFieldData>();
    private readonly Dictionary<string,string>_localizdeFieldNameDictionary = new Dictionary<string, string>();
    private readonly string[] _sortedLocalizedFieldNameList;

    #endregion Private members
  }
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

