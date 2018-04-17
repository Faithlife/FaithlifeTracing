using System.Data.Common;
#if !NETSTANDARD1_4 && !NETSTANDARD2_0
using System.Security;
using System.Security.Permissions;
#endif

namespace Faithlife.Tracing.Data
{
	public sealed class TracingDbProviderFactory : DbProviderFactory
	{
		internal TracingDbProviderFactory(DbProviderFactory factory) => m_wrappedFactory = factory;

		public override DbCommand CreateCommand() => new TracingDbCommand(null, m_wrappedFactory.CreateCommand());
		public override DbConnection CreateConnection() => new TracingDbConnection(m_wrappedFactory.CreateConnection(), null);
		public override DbConnectionStringBuilder CreateConnectionStringBuilder() => m_wrappedFactory.CreateConnectionStringBuilder();
		public override DbParameter CreateParameter() => m_wrappedFactory.CreateParameter();

#if !NETSTANDARD1_4
		public override DbCommandBuilder CreateCommandBuilder() => m_wrappedFactory.CreateCommandBuilder();
		public override DbDataAdapter CreateDataAdapter() => m_wrappedFactory.CreateDataAdapter();
		public override DbDataSourceEnumerator CreateDataSourceEnumerator() => m_wrappedFactory.CreateDataSourceEnumerator();
		public override bool CanCreateDataSourceEnumerator => m_wrappedFactory.CanCreateDataSourceEnumerator;
#endif

#if !NETSTANDARD1_4 && !NETSTANDARD2_0
		public override CodeAccessPermission CreatePermission(PermissionState state) => m_wrappedFactory.CreatePermission(state);
#endif

		readonly DbProviderFactory m_wrappedFactory;
	}
}
