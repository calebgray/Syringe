using System.Collections.Generic;
using System.Reflection;

namespace SyringeInjection {
	public static class Cast {
    /// <summary>Copies the source object to target object.</summary>
    /// <typeparam name="TT">Type of target object.</typeparam>
    /// <typeparam name="TS">Type of source object.</typeparam>
    /// <param name="target">Target object.</param>
    /// <param name="source">Source object.</param>
    public static TT CopyFrom<TT, TS>(this TT target, TS source) where TT : TS {
      foreach (var field in typeof(TS).GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
        var value = field.GetValue(source);
        if (value != null) field.SetValue(target, value);
      }
      return target;
    }

    /// <summary>Copies the source object to target object.</summary>
    /// <typeparam name="TT">Type of target object.</typeparam>
    /// <typeparam name="TS">Type of source object.</typeparam>
    /// <param name="target">Target object.</param>
    /// <param name="source">Source object.</param>
    public static TT CopyTo<TT, TS>(this TS source, TT target) where TT : TS {
      return CopyFrom(target, source);
    }

    /// <summary>Copies the source array to target array.</summary>
    /// <typeparam name="TT">Type of target array.</typeparam>
    /// <typeparam name="TS">Type of source array.</typeparam>
    /// <param name="source">Source array.</param>
    /// <returns>Target array.</returns>
    public static TT[] Copy<TS, TT>(TS[] source) where TT : TS, new() {
      var target = new TT[source.Length];
      for (var i = 0; i < source.Length; ++i) {
        target[i] = new TT();
        source[i].CopyTo(target[i]);
      }
      return target;
    }

    /// <summary>Copies the source list to target list.</summary>
    /// <typeparam name="TT">Type of target list.</typeparam>
    /// <typeparam name="TS">Type of source list.</typeparam>
    /// <param name="source">Source list.</param>
    /// <returns>Target list.</returns>
    public static List<TT> Copy<TT, TS>(List<TS> source) where TT : TS, new() {
      var target = new List<TT>(source.Count);
      for (var i = 0; i < source.Count; ++i) {
        target[i] = new TT();
        source[i].CopyTo(target[i]);
      }
      return target;
    }

    /// <summary>Convert the source object to target type.</summary>
    /// <typeparam name="TT">Type of target object.</typeparam>
    /// <typeparam name="TS">Type of source object.</typeparam>
    /// <param name="source">Source object.</param>
    /// <returns>Target object.</returns>
    public static TT Convert<TS, TT>(this TS source) where TT : TS, new() {
      return CopyFrom(new TT(), source);
    }
	}
}