using System.Data;
using System.Data.Common;

namespace Faithlife.Tracing.Data
{
	internal sealed class TracingDbTransaction : DbTransaction
	{
		public TracingDbTransaction(TracingDbConnection connection, DbTransaction transaction)
		{
			m_connection = connection;
			m_wrappedTransaction = transaction;
		}

		public override void Commit() => m_wrappedTransaction.Commit();
		public override void Rollback() => m_wrappedTransaction.Rollback();
		public override IsolationLevel IsolationLevel => m_wrappedTransaction.IsolationLevel;

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
					m_wrappedTransaction.Dispose();
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		protected override DbConnection DbConnection => m_connection;

		readonly TracingDbConnection m_connection;
		readonly DbTransaction m_wrappedTransaction;
	}
}
