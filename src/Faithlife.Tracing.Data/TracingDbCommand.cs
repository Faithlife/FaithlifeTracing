using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Faithlife.Tracing.Data
{
	internal sealed class TracingDbCommand : DbCommand
	{
		public TracingDbCommand(TracingDbConnection connection, DbCommand command)
		{
			m_connection = connection;
			m_command = command;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
					m_command.Dispose();
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		public override void Cancel() => m_command.Cancel();

		protected override DbParameter CreateDbParameter() => m_command.CreateParameter();

		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			var trace = BeginTrace();
			return new TracingDbDataReader(trace, m_command.ExecuteReader(behavior));
		}

		public override int ExecuteNonQuery()
		{
			using (BeginTrace())
				return m_command.ExecuteNonQuery();
		}

		public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
		{
			using (BeginTrace())
				return await m_command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
		{
			var trace = BeginTrace();
			var reader = await m_command.ExecuteReaderAsync(behavior, cancellationToken).ConfigureAwait(false);
			return new TracingDbDataReader(trace, reader);
		}

		public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
		{
			using (BeginTrace())
				return await m_command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
		}

		public override object ExecuteScalar()
		{
			using (BeginTrace())
				return m_command.ExecuteScalar();
		}

		public override void Prepare() => m_command.Prepare();

		public override string CommandText
		{
			get => m_command.CommandText;
			set => m_command.CommandText = value;
		}

		public override int CommandTimeout { get => m_command.CommandTimeout; set => m_command.CommandTimeout = value; }
		public override CommandType CommandType { get => m_command.CommandType; set => m_command.CommandType = value; }

		protected override DbConnection DbConnection
		{
			get => m_connection;
			set
			{
				m_connection = (TracingDbConnection) value;
				m_command.Connection = m_connection?.WrappedConnection;
			}
		}

		protected override DbParameterCollection DbParameterCollection => m_command.Parameters;
		protected override DbTransaction DbTransaction
		{
			get => m_command.Transaction;
			set => m_command.Transaction = value;
		}

		public override bool DesignTimeVisible { get => m_command.DesignTimeVisible; set => m_command.DesignTimeVisible = value; }

		public override UpdateRowSource UpdatedRowSource
		{
			get => m_command.UpdatedRowSource;
			set => m_command.UpdatedRowSource = value;
		}

		private ITrace BeginTrace()
		{
			var currentTrace = m_connection.TraceProvider?.CurrentTrace;
			if (currentTrace == null)
				return null;

			var sql = CommandText;
			string rpc = null;
			if (CommandType == CommandType.StoredProcedure || CommandType == CommandType.TableDirect)
			{
				rpc = sql;
			}
			else
			{
				for (int index = sql.LastIndexOf("from", StringComparison.OrdinalIgnoreCase); index > 0; index = sql.LastIndexOf("from", index - 1, StringComparison.OrdinalIgnoreCase))
				{
					if (char.IsWhiteSpace(sql, index - 1) && char.IsWhiteSpace(sql, index + 4))
					{
						int start = index + 4;
						while (char.IsWhiteSpace(sql, start))
							start++;
						int end = start;
						while (char.IsLetterOrDigit(sql, end) || sql[end] == '_')
							end++;
						rpc = sql.Substring(start, end - start) + " SELECT"; // TODO
						break;
					}
				}
			}

			return currentTrace.StartChildTrace(TraceKind.Client,
				new[]
				{
					(TraceTagNames.Service, m_connection.WrappedConnection.DataSource),
					(TraceTagNames.Operation, rpc),
					(TraceTagNames.DatabaseInstance, m_connection.WrappedConnection.Database),
					(TraceTagNames.DatabaseStatement, CommandText),
					(TraceTagNames.DatabaseType, "sql"),
				});
		}

		TracingDbConnection m_connection;
		readonly DbCommand m_command;
	}
}
