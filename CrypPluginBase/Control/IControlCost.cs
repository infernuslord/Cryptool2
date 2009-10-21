using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Control
{

  /// <summary>
  /// Which operator has to be used to relate two values of the cost function
  /// </summary>
  public enum RelationOperator
  {
      LessThen, LargerThen
  }

  public interface IControlCost : IControl
  {   
      /// <summary>
      /// Returns the relation operator which has to be used to relate two values of the cost function
      /// </summary>
      /// <returns>RelationOperator</returns>
      RelationOperator getRelationOperator();
      
      /// <summary>
      /// Calculate a value for the given text
      /// </summary>
      /// <param name="text">Text to calculate</param>
      /// <returns>cost</returns>
      double calculateCost(byte[] text);

      int getBytesToUse();
  }
}
