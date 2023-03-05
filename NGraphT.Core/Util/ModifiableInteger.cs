/*
 * (C) Copyright 2002-2021, by Barak Naveh and Contributors.
 *
 * JGraphT : a free Java graph-theory library
 *
 * See the CONTRIBUTORS.md file distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Eclipse Public License 2.0 which is available at
 * http://www.eclipse.org/legal/epl-2.0, or the
 * GNU Lesser General Public License v2.1 or later
 * which is available at
 * http://www.gnu.org/licenses/old-licenses/lgpl-2.1-standalone.html.
 *
 * SPDX-License-Identifier: EPL-2.0 OR LGPL-2.1-or-later
 */

namespace NGraphT.Core.Util;

/// <summary>
/// The <c>ModifiableInteger</c> class wraps a value of the primitive type <c>int</c> in
/// an object, similarly to <see cref="java.lang.Integer"/>. An object of type
/// <c>ModifiableInteger</c> contains a single field whose type is <c>int</c>.
///
/// <para>
/// Unlike <c>java.lang.Integer</c>, the int value which the ModifiableInteger represents can
/// be modified. It becomes useful when used together with the collection framework. For example, if
/// you want to have a <see cref="System.Collections.IList"/> of counters. You could use <c>Integer</c> but
/// that would have became wasteful and inefficient if you frequently had to update the counters.
/// </para>
///
/// <para>
/// WARNING: Because instances of this class are mutable, great care must be exercised if used as
/// keys of a <see cref="System.Collections.IDictionary"/> or as values in a <see cref="System.Collections.Generic.ISet<object>"/> in a manner that affects
/// equals comparisons while the instances are keys in the map (or values in the set). For more see
/// documentation of <c>Map</c> and <c>Set</c>.
/// </para>
///
/// <remarks>Author: Barak Naveh.</remarks>
/// </summary>
public class ModifiableInteger : Number, IComparable<ModifiableInteger>
{
    /// <summary>
    /// The int value represented by this <c>ModifiableInteger</c>.
    /// </summary>
    public int Value;

    /// <summary>
    /// <b>!!! DON'T USE - Use the <#### cref="ModifiableInteger(int)"/> constructor instead !!!</b>
    ///
    /// <para>
    /// This constructor is for the use of java.beans.XMLDecoder deserialization. The constructor is
    /// marked as 'deprecated' to indicate to the programmer against using it by mistake.
    /// </para>
    /// </summary>
    /// @deprecated not really deprecated, just marked so to avoid mistaken use. 
    [Obsolete("not really deprecated, just marked so to avoid mistaken use.")]
    public ModifiableInteger()
    {
    }

    /// <summary>
    /// Constructs a newly allocated <c>ModifiableInteger</c> object that represents the
    /// specified <c>int</c> value.
    /// </summary>
    /// <param name="value"> the value to be represented by the <c>
    /// ModifiableInteger</c> object.</param>
    public ModifiableInteger(int value)
    {
        this.value = value;
    }

    /// <summary>
    /// Sets a new value for this modifiable integer.
    /// </summary>
    /// <param name="value"> the new value to set.</param>
    public virtual int Value
    {
        set
        {
            this.value = value;
        }
        get
        {
            return this.value;
        }
    }


    /// <summary>
    /// Adds one to the value of this modifiable integer.
    /// </summary>
    public virtual void Increment()
    {
        this.value++;
    }

    /// <summary>
    /// Subtracts one from the value of this modifiable integer.
    /// </summary>
    public virtual void Decrement()
    {
        this.value--;
    }

    /// <summary>
    /// Compares two <c>ModifiableInteger</c> objects numerically.
    /// </summary>
    /// <param name="anotherInteger"> the <c>ModifiableInteger</c> to be compared.</param>
    /// <returns>the value <c>0</c> if this <c>ModifiableInteger</c> is equal to the
    ///         argument <c>ModifiableInteger</c>; a value less than <c>0</c> if this
    ///         <c>ModifiableInteger</c> is numerically less than the argument
    ///         <c>ModifiableInteger</c>; and a value greater than <c>0</c> if this
    ///         <c>ModifiableInteger</c> is numerically greater than the argument
    ///         <c>ModifiableInteger</c> (signed comparison).</returns>
    public virtual int CompareTo(ModifiableInteger anotherInteger)
    {
        int thisVal    = this.value;
        int anotherVal = anotherInteger.value;

        return Integer.compare(thisVal, anotherVal);
    }

    /// <#### cref="Number.doubleValue()"/>
    public override double DoubleValue()
    {
        return this.value;
    }

    /// <summary>
    /// Compares this object to the specified object. The result is <c>
    /// true</c> if and only if the argument is not <c>null</c> and is an
    /// <c>ModifiableInteger</c> object that contains the same <c>
    /// int</c> value as this object.
    /// </summary>
    /// <param name="o"> the object to compare with.</param>
    /// <returns><c>true</c> if the objects are the same; <c>false</c> otherwise.</returns>
    public override bool Equals(object o)
    {
        if (o is ModifiableInteger)
        {
            return this.value == ((ModifiableInteger)o).value;
        }

        return false;
    }

    /// <#### cref="Number.floatValue()"/>
    public override float FloatValue()
    {
        return this.value;
    }

    /// <summary>
    /// Returns a hash code for this <c>ModifiableInteger</c>.
    /// </summary>
    /// <returns>a hash code value for this object, equal to the primitive <c>
    /// int</c> value represented by this <c>ModifiableInteger</c> object.</returns>
    public override int GetHashCode()
    {
        return this.value;
    }

    /// <#### cref="Number.intValue()"/>
    public override int IntValue()
    {
        return this.value;
    }

    /// <#### cref="Number.longValue()"/>
    public override long LongValue()
    {
        return this.value;
    }

    /// <summary>
    /// Returns an <c>Integer</c> object representing this <c>
    /// ModifiableInteger</c>'s value.
    /// </summary>
    /// <returns>an <c>Integer</c> representation of the value of this object.</returns>
    public virtual int? ToInteger()
    {
        return this.value;
    }

    /// <summary>
    /// Returns a <c>String</c> object representing this <c>
    /// ModifiableInteger</c>'s value. The value is converted to signed decimal representation and
    /// returned as a string, exactly as if the integer value were given as an argument to the
    /// <#### cref="java.lang.Integer.toString(int)"/> method.
    /// </summary>
    /// <returns>a string representation of the value of this object in base&nbsp;10.</returns>
    public override string ToString()
    {
        return this.value.ToString();
    }
}
