﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dapplo.Utils.Tasks
{
	/// <summary>
	/// A few simple task helpers, these could be simplified.
	/// </summary>
    public static class AsyncHelper
	{
		/// <summary>
		/// Run the specified action as a task with a timeout
		/// </summary>
		/// <param name="action">Action to run</param>
		/// <param name="timeout">TimeSpan</param>
		/// <param name="cancellationToken">optional CancellationToken</param>
		/// <returns>Task</returns>
		public static Task RunWithTimeout(Action action, TimeSpan timeout, CancellationToken? cancellationToken = null)
		{
			var taskCompletionSource = new TaskCompletionSource<bool>();
			var cancellationTokenSource = new CancellationTokenSource(timeout);

			var registrationTcs = cancellationTokenSource.Token.Register(() =>
			{
				taskCompletionSource.TrySetException(new TimeoutException($"The timeout of {timeout} has expired."));
			});
			var registrationCt = cancellationToken?.Register(() => taskCompletionSource.TrySetCanceled());

			Task.Run(async () =>
			{
				try
				{
					await Task.Run(action, cancellationTokenSource.Token).ConfigureAwait(false);
					taskCompletionSource.TrySetResult(true);
				}
				catch (Exception ex)
				{
					taskCompletionSource.TrySetException(ex);
				}
				finally
				{
					registrationTcs.Dispose();
					registrationCt?.Dispose();
				}
			});
			return taskCompletionSource.Task;
		}

		/// <summary>
		/// Run the specified function as a task with a timeout
		/// </summary>
		/// <param name="function">Func which is run, and returns a result</param>
		/// <param name="timeout">TimeSpan</param>
		/// <param name="cancellationToken">CancellationToken optional</param>
		/// <typeparam name="TResult">Type for the result</typeparam>
		/// <returns>Task with result</returns>
		public static Task<TResult> RunWithTimeout<TResult>(Func<TResult> function, TimeSpan timeout, CancellationToken? cancellationToken = null)
		{
			var taskCompletionSource = new TaskCompletionSource<TResult>();
			var cancellationTokenSource = new CancellationTokenSource(timeout);

			var registrationTcs = cancellationTokenSource.Token.Register(() =>
			{
				taskCompletionSource.TrySetException(new TimeoutException($"The timeout of {timeout} has expired."));
			});
			var registrationCt = cancellationToken?.Register(() => taskCompletionSource.TrySetCanceled());

			Task.Run(async () =>
			{
				try
				{
					var result = await Task.Run(function, cancellationTokenSource.Token).ConfigureAwait(false);
					taskCompletionSource.TrySetResult(result);
				}
				catch (Exception ex)
				{
					taskCompletionSource.TrySetException(ex);
				}
				finally
				{
					registrationTcs.Dispose();
					registrationCt?.Dispose();
				}
			});
			return taskCompletionSource.Task;
		}

		/// <summary>
		/// Run the specified function as a task with a timeout
		/// </summary>
		/// <param name="function">Func which is run, and returns a result</param>
		/// <param name="timeout">TimeSpan</param>
		/// <param name="cancellationToken">CancellationToken optional</param>
		/// <typeparam name="TResult">Type for the result</typeparam>
		/// <returns>Task with result</returns>
		public static Task<TResult> RunWithTimeout<TResult>(Func<Task<TResult>> function, TimeSpan timeout, CancellationToken? cancellationToken)
		{
			var taskCompletionSource = new TaskCompletionSource<TResult>();
			var cancellationTokenSource = new CancellationTokenSource(timeout);

			var registrationTcs = cancellationTokenSource.Token.Register(() =>
			{
				taskCompletionSource.TrySetException(new TimeoutException($"The timeout of {timeout} has expired."));
			});

			var registrationCt = cancellationToken?.Register(() => taskCompletionSource.TrySetCanceled());

			Task.Run(async () =>
			{
				try
				{
					var result = await Task.Run(function, cancellationTokenSource.Token).ConfigureAwait(false);
					taskCompletionSource.TrySetResult(result);
				}
				catch (Exception ex)
				{
					taskCompletionSource.TrySetException(ex);
				}
				finally
				{
					registrationTcs.Dispose();
					registrationCt?.Dispose();
				}
			});
			return taskCompletionSource.Task;
		}
	}
}