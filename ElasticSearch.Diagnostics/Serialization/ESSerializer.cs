using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Newtonsoft.Json;

namespace ElasticSearch.Diagnostics.Serialization
{
	public class ElasticsearchJsonNetSerializer : IElasticsearchSerializer
	{
		private static readonly Encoding ExpectedEncoding = new UTF8Encoding(false);
		/// <summary>
		/// The size of the buffer to use when writing the serialized request
		/// to the request stream
		/// Performance tests as part of https://github.com/elastic/elasticsearch-net/issues/1899 indicate this 
		/// to be a good compromise buffer size for performance throughput and bytes allocated.
		/// </summary>
		protected virtual int BufferSize => 1024;

		private readonly JsonSerializerSettings _settings;
		private JsonSerializer _defaultSerializer;

		public ElasticsearchJsonNetSerializer(JsonSerializerSettings settings = null)
		{
			_settings = settings ?? CreateSettings();
			this._defaultSerializer = JsonSerializer.Create(_settings);
		}

		private JsonSerializerSettings CreateSettings()
		{
			var settings = new JsonSerializerSettings()
			{
				DefaultValueHandling = DefaultValueHandling.Include,
				NullValueHandling = NullValueHandling.Ignore,
			};
			return settings;
		}


		public T Deserialize<T>(Stream stream)
		{
			var settings = this._settings;
			return _Deserialize<T>(stream, settings);
		}

		public async Task<T> DeserializeAsync<T>(Stream responseStream, CancellationToken cancellationToken = new CancellationToken())
		{
			return this.Deserialize<T>(responseStream);
		}

		protected internal T _Deserialize<T>(Stream stream, JsonSerializerSettings settings = null)
		{
			settings = settings ?? this._settings;
			var serializer = JsonSerializer.Create(settings);
			var jsonTextReader = new JsonTextReader(new StreamReader(stream));
			var t = (T)serializer.Deserialize(jsonTextReader, typeof(T));
			return t;
		}


		public void Serialize(object data, Stream writableStream, SerializationFormatting formatting = SerializationFormatting.Indented)
		{
			using (var writer = new StreamWriter(writableStream, ExpectedEncoding, BufferSize, leaveOpen: true))
			using (var jsonWriter = new JsonTextWriter(writer))
			{
				_defaultSerializer.Serialize(jsonWriter, data);
				writer.Flush();
				jsonWriter.Flush();
			}
		}

		public IPropertyMapping CreatePropertyMapping(MemberInfo memberInfo) => null;
	}





	//public class ElasticsearchJsonNetSerializerX : IElasticsearchSerializer
	//{
	//	private readonly JsonSerializerSettings _settings;

	//	public ElasticsearchJsonNetSerializerX(JsonSerializerSettings settings = null)
	//	{
	//		_settings = settings ?? CreateSettings();
	//	}

	//	/// <summary>
	//	/// Deserialize an object 
	//	/// </summary>
	//	public virtual T Deserialize<T>(Stream stream)
	//	{
	//		var settings = this._settings;


	//		return _Deserialize<T>(stream, settings);
	//	}
	//	public virtual Task<T> DeserializeAsync<T>(Stream stream)
	//	{
	//		//TODO sadly json .net async does not read the stream async so 
	//		//figure out wheter reading the stream async on our own might be beneficial 
	//		//over memory possible memory usage
	//		var tcs = new TaskCompletionSource<T>();
	//		var r = this.Deserialize<T>(stream);
	//		tcs.SetResult(r);
	//		return tcs.Task;
	//	}
	//	private JsonSerializerSettings CreateSettings()
	//	{
	//		var settings = new JsonSerializerSettings()
	//		{
	//			DefaultValueHandling = DefaultValueHandling.Include,
	//			NullValueHandling = NullValueHandling.Ignore,
	//		};
	//		return settings;
	//	}

	//	protected internal T _Deserialize<T>(Stream stream, JsonSerializerSettings settings = null)
	//	{
	//		settings = settings ?? this._settings;
	//		var serializer = JsonSerializer.Create(settings);
	//		var jsonTextReader = new JsonTextReader(new StreamReader(stream));
	//		var t = (T)serializer.Deserialize(jsonTextReader, typeof(T));
	//		return t;
	//	}

	//	public byte[] Serialize(object data, SerializationFormatting formatting = SerializationFormatting.Indented)
	//	{
	//		var format = formatting == SerializationFormatting.Indented
	//			? Formatting.Indented
	//			: Formatting.None;
	//		var json = JsonConvert.SerializeObject(data, format, this._settings);

	//		return Encoding.UTF8.GetBytes(json);
	//	}

	//	public string Stringify(object valueType)
	//	{
	//		return ElasticsearchDefaultSerializer.DefaultStringify(valueType);
	//	}
	//}
}