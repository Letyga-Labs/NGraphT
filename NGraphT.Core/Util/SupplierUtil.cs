/*
 * (C) Copyright 2018-2021, by Dimitrios Michail and Contributors.
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

using Graph;

/// <summary>
/// Helper class for suppliers.
///
/// <remarks>Author: Dimitrios Michail.</remarks>
/// </summary>
public class SupplierUtil
{
    /// <summary>
    /// Supplier for <#### cref="DefaultEdge"/>.
    /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static final Supplier<DefaultEdge> DEFAULT_EDGE_SUPPLIER = (Supplier<DefaultEdge> & Serializable) DefaultEdge::new;
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
    public static readonly Func<DefaultEdge>
        DefaultEdgeSupplier = (Func<DefaultEdge> & Serializable) DefaultEdge::new;

    /// <summary>
    /// Supplier for <#### cref="DefaultWeightedEdge"/>.
    /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static final Supplier<DefaultWeightedEdge> DEFAULT_WEIGHTED_EDGE_SUPPLIER = (Supplier<DefaultWeightedEdge> & Serializable) DefaultWeightedEdge::new;
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
    public static readonly Func<DefaultWeightedEdge> DefaultWeightedEdgeSupplier =
        (Func<DefaultWeightedEdge> & Serializable) DefaultWeightedEdge::new;

    /// <summary>
    /// Supplier for <#### cref="object"/>.
    /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static final Supplier<Object> OBJECT_SUPPLIER = (Supplier<Object> & Serializable) Object::new;
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
    public static readonly Func<Object> ObjectSupplier = (Func<Object> & Serializable) Object::new;

    /// <summary>
    /// Create a supplier from a class which calls the default constructor.
    /// </summary>
    /// <param name="clazz"> the class.</param>
    /// <returns>the supplier.</returns>
    /// @param <T> the type of results supplied by this supplier.</param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T> Supplier<T> createSupplier(Class clazz)
    public static Func<T> CreateSupplier<T>(Type clazz)
    {
        // shortcut to use pre-defined constructor method reference based suppliers
        if (clazz == typeof(DefaultEdge))
        {
            return (Func<T>)DefaultEdgeSupplier;
        }
        else if (clazz == typeof(DefaultWeightedEdge))
        {
            return (Func<T>)DefaultWeightedEdgeSupplier;
        }
        else if (clazz == typeof(object))
        {
            return (Func<T>)ObjectSupplier;
        }

        try
        {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Constructor<? extends T> constructor = clazz.getDeclaredConstructor();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
            System.Reflection.ConstructorInfo<T> constructor = clazz.getDeclaredConstructor();
            if ((!Modifier.isPublic(constructor.getModifiers()) ||
                 !Modifier.isPublic(constructor.getDeclaringClass().getModifiers())) && !constructor.canAccess(null))
            {
                constructor.setAccessible(true);
            }

            return new ConstructorSupplier<T>(constructor);
        }
        catch (ReflectiveOperationException edge)
        {
            // Defer throwing an exception to the first time the supplier is called
            return GetThrowingSupplier(edge);
        }
    }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static <T> Supplier<T> getThrowingSupplier(Throwable TEdge)
    private static Func<T> GetThrowingSupplier<T>(Exception edge)
    {
        return (Func<T> & Serializable)() =>
        {
            throw new SupplierException(edge.Message, edge);
        }
        ;
    }

    /// <summary>
    /// Create a default edge supplier.
    /// </summary>
    /// <returns>a default edge supplier.</returns>
    public static Func<DefaultEdge> CreateDefaultEdgeSupplier()
    {
        return DefaultEdgeSupplier;
    }

    /// <summary>
    /// Create a default weighted edge supplier.
    /// </summary>
    /// <returns>a default weighted edge supplier.</returns>
    public static Func<DefaultWeightedEdge> CreateDefaultWeightedEdgeSupplier()
    {
        return DefaultWeightedEdgeSupplier;
    }

    /// <summary>
    /// Create an integer supplier which returns a sequence starting from zero.
    /// </summary>
    /// <returns>an integer supplier.</returns>
    public static Func<int> CreateIntegerSupplier()
    {
        return CreateIntegerSupplier(0);
    }

    /// <summary>
    /// Create an integer supplier which returns a sequence starting from a specific numbers.
    /// </summary>
    /// <param name="start"> where to start the sequence.</param>
    /// <returns>an integer supplier.</returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static Supplier<int> createIntegerSupplier(int start)
    public static Func<int> CreateIntegerSupplier(int start)
    {
        var modifiableInt = new int[] { start }; // like a modifiable int
        return (Func<int> & Serializable)() => modifiableInt[0]++;
    }

    /// <summary>
    /// Create a long supplier which returns a sequence starting from zero.
    /// </summary>
    /// <returns>a long supplier.</returns>
    public static Func<long> CreateLongSupplier()
    {
        return CreateLongSupplier(0);
    }

    /// <summary>
    /// Create a long supplier which returns a sequence starting from a specific numbers.
    /// </summary>
    /// <param name="start"> where to start the sequence.</param>
    /// <returns>a long supplier.</returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static Supplier<long> createLongSupplier(long start)
    public static Func<long> CreateLongSupplier(long start)
    {
        var modifiableLong = new long[] { start }; // like a modifiable long
        return (Func<long> & Serializable)() => modifiableLong[0]++;
    }

    /// <summary>
    /// Create a string supplier which returns unique strings. The returns strings are simply
    /// integers starting from zero.
    /// </summary>
    /// <returns>a string supplier.</returns>
    public static Func<string> CreateStringSupplier()
    {
        return CreateStringSupplier(0);
    }

    /// <summary>
    /// Create a string supplier which returns random UUIDs.
    /// </summary>
    /// <returns>a string supplier.</returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static Supplier<String> createRandomUUIDStringSupplier()
    public static Func<string> CreateRandomUuidStringSupplier()
    {
        return (Func<string> & Serializable)() => UUID.randomUUID().ToString();
    }

    /// <summary>
    /// Create a string supplier which returns unique strings. The returns strings are simply
    /// integers starting from start.
    /// </summary>
    /// <param name="start"> where to start the sequence.</param>
    /// <returns>a string supplier.</returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static Supplier<String> createStringSupplier(int start)
    public static Func<string> CreateStringSupplier(int start)
    {
        var container = new int[] { start };
        return (Func<string> & Serializable)() => (container[0]++).ToString();
    }

        private class ConstructorSupplier<T> : Func<T>
    {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: private final Constructor<? extends T> constructor;
        internal readonly System.Reflection.ConstructorInfo<T> Constructor;

                private class SerializedForm<T>
        {
            internal const long SerialVersionUID = -2385289829144892760L;

            internal readonly Type Type = typeof(T);

            public SerializedForm(Type type)
            {
                Type = type;
            }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object readResolve() throws ObjectStreamException
            internal virtual object ReadResolve()
            {
                try
                {
                    return new ConstructorSupplier<>(Type.getDeclaredConstructor());
                }
                catch (ReflectiveOperationException edge)
                {
                    InvalidObjectException ex =
                        new InvalidObjectException("Failed to get no-args constructor from " + Type);
                    ex.initCause(edge);
                    throw ex;
                }
            }
        }

//JAVA TO C# CONVERTER TODO TASK: Wildcard generics in method parameters are not converted:
//ORIGINAL LINE: public ConstructorSupplier(Constructor<? extends T> constructor)
        public ConstructorSupplier(System.Reflection.ConstructorInfo<T> constructor)
        {
            Constructor = constructor;
        }

        public override T Get()
        {
            try
            {
                return Constructor.Invoke();
            }
            catch (ReflectiveOperationException ex)
            {
                throw new SupplierException("Supplier failed", ex);
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object writeReplace() throws ObjectStreamException
        internal virtual object WriteReplace()
        {
            return new SerializedForm<>(Constructor.getDeclaringClass());
        }
    }
}
