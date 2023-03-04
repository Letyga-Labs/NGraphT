/*
 * (C) Copyright 2005-2021, by Assaf Lehr and Contributors.
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
/// Utility class to help implement an iterator/enumerator in which the hasNext() method needs to
/// calculate the next elements ahead of time.
///
/// <para>
/// Many classes which implement an iterator face a common problem: if there is no easy way to
/// calculate hasNext() other than to call getNext(), then they save the result for fetching in the
/// next call to getNext(). This utility helps in doing just that.
///
/// </para>
/// <para>
/// <b>Usage:</b> The new iterator class will hold this class as a member variable and forward the
/// hasNext() and next() to it. When creating an instance of this class, you supply it with a functor
/// that is doing the real job of calculating the next element.
/// 
/// <pre>
/// <c>
///  //This class supplies enumeration of integer till 100.
///  public class IteratorExample implements Enumeration{
///  private int counter=0;
///  private PrefetchIterator nextSupplier;
/// 
///      IteratorExample()
///      {
///          nextSupplier = new PrefetchIterator(new PrefetchIterator.NextElementFunctor(){
/// 
///              public Object nextElement() throws NoSuchElementException {
///                  counter++;
///                  if (counter &lt;= 100)
///                      throw new NoSuchElementException();
///                  else
///                      return new Integer(counter);
///              }
/// 
///          });
///      }
/// 
///      // forwarding to nextSupplier and return its returned value
///      public boolean hasMoreElements() {
///          return this.nextSupplier.hasMoreElements();
///      }
/// 
///      // forwarding to nextSupplier and return its returned value
///      public Object nextElement() {
///          return this.nextSupplier.nextElement();
///      }
///  }</c>
/// </pre>
///
/// </para>
/// </summary>
/// @param <TEdge> the element type
///
/// <remarks>Author: Assaf Lehr.</remarks>
public class PrefetchIterator<TEdge> : IEnumerator<TEdge>, IEnumerator<TEdge>
{
    private INextElementFunctor<TEdge> _innerEnum;
    private TEdge                      _getNextLastResult;
    private bool                       _isGetNextLastResultUpToDate   = false;
    private bool                       _endOfEnumerationReached       = false;
    private bool                       _flagIsEnumerationStartedEmpty = true;
    private int                        _innerFunctorUsageCounter      = 0;

    /// <summary>
    /// Construct a new prefetch iterator.
    /// </summary>
    /// <param name="aEnum"> the next element functor.</param>
    public PrefetchIterator(INextElementFunctor<TEdge> aEnum)
    {
        _innerEnum = aEnum;
    }

    /// <summary>
    /// Serves as one contact place to the functor; all must use it and not directly the
    /// NextElementFunctor.
    /// </summary>
    private TEdge NextElementFromInnerFunctor
    {
        get
        {
            _innerFunctorUsageCounter++;
            var result = _innerEnum.NextElement();

            // if we got here , an exception was not thrown, so at least
            // one time a good value returned
            _flagIsEnumerationStartedEmpty = false;
            return result;
        }
    }

    /// <inheritdoc/>
    public override TEdge NextElement()
    {
        /*
         * 1. Retrieves the saved value or calculates it if it does not exist 2. Changes
         * isGetNextLastResultUpToDate to false. (Because it does not save the NEXT element now; it
         * saves the current one!)
         */
        TEdge result;
        if (_isGetNextLastResultUpToDate)
        {
            result = _getNextLastResult;
        }
        else
        {
            result = NextElementFromInnerFunctor;
        }

        _isGetNextLastResultUpToDate = false;
        return result;
    }

    /// <inheritdoc/>
    public override bool HasMoreElements()
    {
        /*
         * If (isGetNextLastResultUpToDate==true) returns true else 1. calculates getNext() and
         * saves it 2. sets isGetNextLastResultUpToDate to true.
         */
        if (_endOfEnumerationReached)
        {
            return false;
        }

        if (_isGetNextLastResultUpToDate)
        {
            return true;
        }
        else
        {
            try
            {
                _getNextLastResult           = NextElementFromInnerFunctor;
                _isGetNextLastResultUpToDate = true;
                return true;
            }
            catch (NoSuchElementException)
            {
                _endOfEnumerationReached = true;
                return false;
            }
        } // else
    }     // method

    /// <summary>
    /// Tests whether the enumeration started as an empty one. It does not matter if it
    /// hasMoreElements() now, only at initialization time. Efficiency: if nextElements(),
    /// hasMoreElements() were never used, it activates the hasMoreElements() once. Else it is
    /// immediately(O(1))
    /// </summary>
    /// <returns>true if the enumeration started as an empty one, false otherwise.</returns>
    public virtual bool EnumerationStartedEmpty
    {
        get
        {
            if (_innerFunctorUsageCounter == 0)
            {
                return !HasMoreElements();
            }
            else // it is not the first time , so use the saved value
                // which was initilaizeed during a call to
                // getNextElementFromInnerFunctor
            {
                return _flagIsEnumerationStartedEmpty;
            }
        }
    }

    /// <inheritdoc/>
    public override bool HasNext()
    {
        return HasMoreElements();
    }

    /// <inheritdoc/>
    public override TEdge Next()
    {
        return NextElement();
    }

    /// <inheritdoc/>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void remove() throws UnsupportedOperationException
    public override void Remove()
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// A functor for the calculation of the next element.
    /// </summary>
    /// @param <EE> the element type.</param>
    public interface INextElementFunctor<TEe>
    {
        /// <summary>
        /// Return the next element or throw a <seealso cref="NoSuchElementException"/> if there are no more
        /// elements.
        /// </summary>
        /// <returns>the next element.</returns>
        /// <exception cref="NoSuchElementException"> in case there is no next element.</exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: EE nextElement() throws NoSuchElementException;
        TEe NextElement();
    }
}
