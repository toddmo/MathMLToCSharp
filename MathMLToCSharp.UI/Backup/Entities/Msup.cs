using System.Text;
using Wintellect.PowerCollections;

namespace MathMLToCSharp.Entities
{
  /// <summary>
  /// Superscript. Taken to mean 'power of' unless the superscript parses into a *.
  /// </summary>
  public class Msup : WithBinaryContent
  {
    public Msup(IBuildable first, IBuildable second) : base(first, second) { }

    public Pair<IBuildable, IBuildable> Values
    {
      get
      {
        return new Pair<IBuildable, IBuildable>(first, second);
      }
    }

    public override void Visit(StringBuilder sb, BuildContext bc)
    {
      if (bc.Options.SubscriptMode)
      { 
        // separate by double underscore to differentiate from Msub and prevent clashes
        first.Visit(sb, bc);
        sb.Append("__");
        second.Visit(sb, bc);
      }
      else if (second is Mo && (second as Mo).IsTimesOrStar && first is Mi)
      {
        // this isn't really a superscript - it's part of the variable
        Mi mi = (Mi)first;
        Mi newMi = new Mi(mi.Content + Semantics.starPrefix);
        newMi.Visit(sb, bc);
      }
      else
      {
        if (bc.LastTokenRequiresTimes)
          sb.Append("*");

        // determine whether power must be inlined
        bool firstIsMrowMi = (first is Mrow) && ((first as Mrow).ContainsSingleMi);
        bool secondIsIntegralPower = (second is Mrow) && ((second as Mrow).ContainsSingleMn) && 
                                     ((second as Mrow).LastElement as Mn).IsIntegerGreaterThan1;
        bool mustInlinePower = false;
        int power = 0;
        if (secondIsIntegralPower)
        {
          power = ((second as Mrow).LastElement as Mn).IntegerValue;
          mustInlinePower = power <= bc.Options.MaxInlinePower;
        }
        if (mustInlinePower)
        {
          for (int i = 0; i < power; ++i)
          {
            first.Visit(sb, bc); // * sign appended automatically
          }
        }
        else
        {
          sb.Append("Math.Pow(");
          first.Visit(sb, bc);
          sb.Append(", ");
          second.Visit(sb, bc);
          sb.Append(")");
        }
      }

      bc.Tokens.Add(this);
    }
  }
}