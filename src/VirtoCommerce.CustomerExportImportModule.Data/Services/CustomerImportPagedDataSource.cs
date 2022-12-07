using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.AssetsModule.Core.Assets;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CustomerImportPagedDataSource<T> : ICustomerImportPagedDataSource<T> where T : CsvMember
    {
        private readonly Stream _stream;
        private readonly CsvConfiguration _configuration;
        private readonly StreamReader _streamReader;
        private readonly CsvReader _csvReader;
        private int? _totalCount;

        public CustomerImportPagedDataSource(string filePath, IBlobStorageProvider blobStorageProvider, int pageSize, CsvConfiguration configuration, ClassMap map)
        {
            var stream = blobStorageProvider.OpenRead(filePath);

            _stream = stream;
            _streamReader = new StreamReader(stream);

            _configuration = configuration;
            _configuration.LeaveOpen = true;
            _csvReader = new CsvReader(_streamReader, configuration);
            _csvReader.Context.RegisterClassMap(map);

            PageSize = pageSize;
        }

        public int CurrentPageNumber { get; private set; }

        public int PageSize { get; }

        public int GetTotalCount()
        {
            if (_totalCount != null)
            {
                return _totalCount.Value;
            }

            _totalCount = 0;

            var streamPosition = _stream.Position;
            _stream.Seek(0, SeekOrigin.Begin);

            using var streamReader = new StreamReader(_stream, leaveOpen: true);
            using var csvReader = new CsvReader(streamReader, _configuration);
            try
            {
                csvReader.Read();
                csvReader.ReadHeader();
                csvReader.ValidateHeader<T>();
            }
            catch (ValidationException)
            {
                _totalCount++;
            }

            while (csvReader.Read())
            {
                _totalCount++;
            }

            _stream.Seek(streamPosition, SeekOrigin.Begin);

            return _totalCount.Value;
        }

        public string GetHeaderRaw()
        {
            string result;

            var streamPosition = _stream.Position;
            _stream.Seek(0, SeekOrigin.Begin);

            using var streamReader = new StreamReader(_stream, leaveOpen: true);
            using var csvReader = new CsvReader(streamReader, _configuration);

            try
            {
                csvReader.Read();
                csvReader.ReadHeader();
                csvReader.ValidateHeader<T>();

                result = string.Join(csvReader.Configuration.Delimiter, csvReader.HeaderRecord);

            }
            finally
            {
                _stream.Seek(streamPosition, SeekOrigin.Begin);
            }

            return result;
        }

        public async Task<bool> FetchAsync()
        {
            if (CurrentPageNumber * PageSize >= GetTotalCount())
            {
                Items = Array.Empty<ImportRecord<T>>();
                return false;
            }

            var items = new List<ImportRecord<T>>();

            for (var i = 0; i < PageSize && await _csvReader.ReadAsync(); i++)
            {
                var record = _csvReader.GetRecord<T>();

                if (record != null)
                {
                    var rawRecord = _csvReader.Parser.RawRecord;
                    var row = _csvReader.Parser.Row;

                    items.Add(new ImportRecord<T> { Row = row, RawRecord = rawRecord, Record = record });
                }
            }

            Items = items.ToArray();

            CurrentPageNumber++;

            return true;
        }

        public ImportRecord<T>[] Items { get; private set; }

        public void Dispose()
        {
            _csvReader.Dispose();
            _streamReader.Dispose();
            _stream.Dispose();
        }
    }
}
