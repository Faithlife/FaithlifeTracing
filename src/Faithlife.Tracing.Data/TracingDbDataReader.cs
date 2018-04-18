using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Faithlife.Tracing.Data
{
	internal sealed class TracingDbDataReader : DbDataReader
#if !NET461 && !NET47
		, IDbColumnSchemaGenerator
#endif
	{
		public TracingDbDataReader(ITrace trace, DbDataReader reader)
		{
			m_trace = trace;
			m_reader = reader;
		}

#if !NETSTANDARD1_4 && !NETSTANDARD2_0
		public override void Close()
		{
			m_reader.Close();
			if (m_trace != null)
			{
				m_trace.Dispose();
				m_trace = null;
			}
		}
#endif

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					m_reader.Dispose();
					if (m_trace != null)
					{
						m_trace.Dispose();
						m_trace = null;
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		public override string GetDataTypeName(int ordinal) => m_reader.GetDataTypeName(ordinal);
		public override IEnumerator GetEnumerator() => m_reader.GetEnumerator();
		public override Type GetFieldType(int ordinal) => m_reader.GetFieldType(ordinal);
		public override string GetName(int ordinal) => m_reader.GetName(ordinal);
		public override int GetOrdinal(string name) => m_reader.GetOrdinal(name);
#if !NETSTANDARD1_4
		public override DataTable GetSchemaTable() => m_reader.GetSchemaTable();
#endif
		public override bool GetBoolean(int ordinal) => m_reader.GetBoolean(ordinal);
		public override byte GetByte(int ordinal) => m_reader.GetByte(ordinal);
		public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) => m_reader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
		public override char GetChar(int ordinal) => m_reader.GetChar(ordinal);
		public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) => m_reader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
		protected override DbDataReader GetDbDataReader(int ordinal) => m_reader.GetData(ordinal);
		public override DateTime GetDateTime(int ordinal) => m_reader.GetDateTime(ordinal);
		public override decimal GetDecimal(int ordinal) => m_reader.GetDecimal(ordinal);
		public override double GetDouble(int ordinal) => m_reader.GetDouble(ordinal);
		public override float GetFloat(int ordinal) => m_reader.GetFloat(ordinal);
		public override Guid GetGuid(int ordinal) => m_reader.GetGuid(ordinal);
		public override short GetInt16(int ordinal) => m_reader.GetInt16(ordinal);
		public override int GetInt32(int ordinal) => m_reader.GetInt32(ordinal);
		public override long GetInt64(int ordinal) => m_reader.GetInt64(ordinal);
		public override Type GetProviderSpecificFieldType(int ordinal) => m_reader.GetProviderSpecificFieldType(ordinal);
		public override object GetProviderSpecificValue(int ordinal) => m_reader.GetProviderSpecificValue(ordinal);
		public override int GetProviderSpecificValues(object[] values) => m_reader.GetProviderSpecificValues(values);
		public override string GetString(int ordinal) => m_reader.GetString(ordinal);
		public override Stream GetStream(int ordinal) => m_reader.GetStream(ordinal);
		public override TextReader GetTextReader(int ordinal) => m_reader.GetTextReader(ordinal);
		public override object GetValue(int ordinal) => m_reader.GetValue(ordinal);
		public override T GetFieldValue<T>(int ordinal) => m_reader.GetFieldValue<T>(ordinal);
		public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken) => m_reader.GetFieldValueAsync<T>(ordinal, cancellationToken);
		public override int GetValues(object[] values) => m_reader.GetValues(values);
		public override bool IsDBNull(int ordinal) => m_reader.IsDBNull(ordinal);
		public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken) => m_reader.IsDBNullAsync(ordinal, cancellationToken);
		public override bool NextResult() => m_reader.NextResult();
		public override bool Read() => m_reader.Read();
		public override Task<bool> ReadAsync(CancellationToken cancellationToken) => m_reader.ReadAsync(cancellationToken);
		public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => m_reader.NextResultAsync(cancellationToken);
		public override int Depth => m_reader.Depth;
		public override int FieldCount => m_reader.FieldCount;
		public override bool HasRows => m_reader.HasRows;
		public override bool IsClosed => m_reader.IsClosed;
		public override int RecordsAffected => m_reader.RecordsAffected;
		public override int VisibleFieldCount => m_reader.VisibleFieldCount;
		public override object this[int ordinal] => m_reader[ordinal];
		public override object this[string name] => m_reader[name];
#if !NET461 && !NET47
		public ReadOnlyCollection<DbColumn> GetColumnSchema() => ((IDbColumnSchemaGenerator) m_reader).GetColumnSchema();
#endif

		ITrace m_trace;
		readonly DbDataReader m_reader;
	}
}
