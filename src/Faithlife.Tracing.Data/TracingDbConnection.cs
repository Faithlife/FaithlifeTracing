using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
#if !NETSTANDARD1_4
using System.Transactions;
#endif
using IsolationLevel = System.Data.IsolationLevel;

namespace Faithlife.Tracing.Data
{
	public class TracingDbConnection : DbConnection
	{
		public static DbConnection Create(DbConnection connection, ITraceSpanProvider traceSpanProvider) => new TracingDbConnection(connection ?? throw new ArgumentNullException(nameof(connection)), traceSpanProvider);

		internal TracingDbConnection(DbConnection connection, ITraceSpanProvider traceSpanProvider)
		{
			WrappedConnection = connection;
			TraceSpanProvider = traceSpanProvider;
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => WrappedConnection.BeginTransaction(isolationLevel);

		public override void Close() => WrappedConnection.Close();

		public override void ChangeDatabase(string databaseName) => WrappedConnection.ChangeDatabase(databaseName);

		protected override DbCommand CreateDbCommand() => new TracingDbCommand(this, WrappedConnection.CreateCommand());

#if !NETSTANDARD1_4
		public override void EnlistTransaction(Transaction transaction)
		{
			WrappedConnection.EnlistTransaction(transaction);
		}

		public override DataTable GetSchema()
		{
			return WrappedConnection.GetSchema();
		}

		public override DataTable GetSchema(string collectionName)
		{
			return WrappedConnection.GetSchema(collectionName);
		}

		public override DataTable GetSchema(string collectionName, string[] restrictionValues)
		{
			return WrappedConnection.GetSchema(collectionName, restrictionValues);
		}
#endif

		protected override void OnStateChange(StateChangeEventArgs stateChange)
		{
			// TODO
			base.OnStateChange(stateChange);
		}

		public override void Open()
		{
			using (BeginSpan())
				WrappedConnection.Open();
		}

		public override async Task OpenAsync(CancellationToken cancellationToken)
		{
			using (BeginSpan())
				await WrappedConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
		}

		public override string ConnectionString
		{
			get => WrappedConnection.ConnectionString;
			set => WrappedConnection.ConnectionString = value;
		}

		public override int ConnectionTimeout
		{
			get => WrappedConnection.ConnectionTimeout;
		}

		public override string Database => WrappedConnection.Database;
		public override string DataSource => WrappedConnection.DataSource;
		public override string ServerVersion => WrappedConnection.ServerVersion;
		public override ConnectionState State => WrappedConnection.State;

#if !NETSTANDARD1_4
		protected override DbProviderFactory DbProviderFactory { get; }
#endif

		public override event StateChangeEventHandler StateChange // TODO
		{
			add { base.StateChange += value; }
			remove { base.StateChange -= value; }
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
					WrappedConnection.Dispose();
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		internal DbConnection WrappedConnection { get; }
		internal ITraceSpanProvider TraceSpanProvider { get; }

		private ITraceSpan BeginSpan()
		{
			return TraceSpanProvider?.CurrentSpan.StartChildSpan(TraceSpanKind.Client,
				new[]
				{
					(SpanTagNames.Service, WrappedConnection.DataSource),
					(SpanTagNames.Operation, "OpenConnection"),
					(SpanTagNames.DatabaseInstance, WrappedConnection.Database),
					(SpanTagNames.DatabaseType, "sql"),
				});
		}
	}
}
