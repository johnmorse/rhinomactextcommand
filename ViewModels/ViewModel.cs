using System;
using System.Reflection;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Diagnostics.CodeAnalysis;

// Extracted and modified from Microsoft.Practices.Prism.ViewModel.NotificationObject
// http://msdn.microsoft.com/en-us/library/gg405484(v=pandp.40).aspx

namespace Rhino.ViewModel
{
  [Serializable]
  abstract class NotificationObject : INotifyPropertyChanged
  {
    /// <summary>
    /// Raised when a property on this object has a new value.
    /// </summary>        
    [field: NonSerialized]
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises this object's <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The property that has a new value.</param>
    [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Method used to raise an event")]
    protected virtual void RaisePropertyChanged(string propertyName)
    {
      var handler = PropertyChanged;
      if (handler != null)
        handler(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises this object's <see cref="PropertyChanged"/> event for each of the properties.
    /// </summary>
    /// <param name="propertyNames">The properties that have a new value.</param>
    [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Method used to raise an event")]
    protected void RaisePropertyChanged(params string[] propertyNames)
    {
      if (propertyNames == null) throw new ArgumentNullException("propertyNames");
      foreach (var name in propertyNames)
        RaisePropertyChanged(name);
    }

    /// <summary>
    /// Raises this object's <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <typeparam name="T">The type of the property that has a new value</typeparam>
    /// <param name="propertyExpression">A Lambda expression representing the property that has a new value.</param>
    [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Method used to raise an event")]
    [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Cannot change the signature")]
    protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
    {
      var propertyName = ExtractPropertyName(propertyExpression);
      RaisePropertyChanged(propertyName);
    }

    /// <summary>
    /// Extracts the property name from a property expression.
    /// </summary>
    /// <typeparam name="T">The object type containing the property specified in the expression.</typeparam>
    /// <param name="propertyExpression">The property expression (e.g. p => p.PropertyName)</param>
    /// <returns>The name of the property.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="propertyExpression"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the expression is:<br/>
    ///     Not a <see cref="MemberExpression"/><br/>
    ///     The <see cref="MemberExpression"/> does not represent a property.<br/>
    ///     Or, the property is static.
    /// </exception>
    [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
    {
      if (propertyExpression == null)
        throw new ArgumentNullException("propertyExpression");

      var memberExpression = propertyExpression.Body as MemberExpression;
      if (memberExpression == null)
        throw new ArgumentException("The expression is not a member access expression.", "propertyExpression");

      var property = memberExpression.Member as PropertyInfo;
      if (property == null)
        throw new ArgumentException("The member access expression does not access a property.", "propertyExpression");

      var getMethod = property.GetGetMethod(true);
      if (getMethod.IsStatic)
        throw new ArgumentException("The referenced property is a static property.", "propertyExpression");

      return memberExpression.Member.Name;
    }
  }
}
