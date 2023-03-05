/*
 * (C) Copyright 2020-2021, by Semen Chudakov and Contributors.
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
/// Utility class to manage creation and shutting down instance of the <see cref="ThreadPoolExecutor"/>.
/// </summary>
public class ConcurrencyUtil
{
    /// <summary>
    /// Creates a <see cref="ThreadPoolExecutor"/> with fixed number of threads which is equal to
    /// {@code parallelism}.
    /// </summary>
    /// <param name="parallelism"> number of threads for the executor.</param>
    /// <returns>created executor.</returns>
    public static ThreadPoolExecutor CreateThreadPoolExecutor(int parallelism)
    {
        return (ThreadPoolExecutor)Executors.newFixedThreadPool(parallelism);
    }

    /// <summary>
    /// Shuts down the {@code executor}. This operation puts the {@code service} into a state where
    /// every subsequent task submitted to the {@code service} will be rejected. This method calls
    /// <see cref="shutdownExecutionService(ExecutorService, long, TimeUnit)"/> with $time =
    /// Long.MAX_VALUE$ and $timeUnit = TimeUnit.MILLISECONDS$.
    /// </summary>
    /// <param name="service"> service to be shut down.</param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void shutdownExecutionService(java.util.concurrent.ExecutorService service) throws InterruptedException
    public static void ShutdownExecutionService(ExecutorService service)
    {
        ShutdownExecutionService(service, long.MaxValue, TimeUnit.MILLISECONDS);
    }

    /// <summary>
    /// Shuts down the {@code executor}. This operation puts the {@code service} into a state where
    /// every subsequent task submitted to the {@code service} will be rejected.
    /// </summary>
    /// <param name="service"> service to be shut down.</param>
    /// <param name="time"> period of time to wait for the completion of the termination.</param>
    /// <param name="timeUnit"> time duration granularity for the provided {@code time} </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void shutdownExecutionService(java.util.concurrent.ExecutorService service, long time, java.util.concurrent.TimeUnit timeUnit) throws InterruptedException
    public static void ShutdownExecutionService(ExecutorService service, long time, TimeUnit timeUnit)
    {
        service.shutdown();
        service.awaitTermination(time, timeUnit);
    }
}
