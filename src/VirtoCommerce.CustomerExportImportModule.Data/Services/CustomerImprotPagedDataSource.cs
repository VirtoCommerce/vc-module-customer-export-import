using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CustomerImportPagedDataSource : ICustomerImportPagedDataSource
    {
        private readonly Stream _stream;
        private readonly IMemberService _memberService;
        private readonly Configuration _configuration;
        private readonly StreamReader _streamReader;
        private readonly CsvReader _csvReader;
        private int? _totalCount;

        public CustomerImportPagedDataSource(string filePath, IBlobStorageProvider blobStorageProvider, IMemberService memberService, int pageSize, Configuration configuration)
        {
            _memberService = memberService;

            var stream = blobStorageProvider.OpenRead(filePath);

            _stream = stream;
            _streamReader = new StreamReader(stream);

            _memberService = memberService;
            _configuration = configuration;
            _csvReader = new CsvReader(_streamReader, configuration);

            PageSize = pageSize;
        }

        public int CurrentPageNumber { get; private set; }

        public int PageSize { get; }

        public string GetHeaderRaw()
        {
            var result = string.Empty;

            var streamPosition = _stream.Position;
            _stream.Seek(0, SeekOrigin.Begin);

            using var streamReader = new StreamReader(_stream, leaveOpen: true);
            using var csvReader = new CsvReader(streamReader, _configuration, true);

            try
            {
                csvReader.Read();
                csvReader.ReadHeader();
                csvReader.ValidateHeader<CsvContact>();

                result = string.Join(csvReader.Configuration.Delimiter, csvReader.Context.HeaderRecord);

            }
            finally
            {
                _stream.Seek(streamPosition, SeekOrigin.Begin);
            }

            return result;
        }

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
            using var csvReader = new CsvReader(streamReader, _configuration, true);
            try
            {
                csvReader.Read();
                csvReader.ReadHeader();
                csvReader.ValidateHeader<CsvContact>();
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

        public async Task<bool> FetchAsync()
        {
            if (CurrentPageNumber * PageSize >= GetTotalCount())
            {
                Contacts = Array.Empty<CsvContact>();
                return false;
            }

            var recordTuples = new List<(CsvContact, string, int)>();

            for (var i = 0; i < PageSize && await _csvReader.ReadAsync(); i++)
            {
                var csvRecord = _csvReader.GetRecord<CsvContact>();

                var rawRecord = _csvReader.Context.RawRecord;
                var row = _csvReader.Context.Row;

                var recordTuple = (csvRecord, rawRecord, row);
                recordTuples.Add(recordTuple);

            }

            Contacts = recordTuples.Where(x => x.Item1 != null).Select(record =>
              {
                  var (importableContact, _, _) = record;
                  return importableContact;
              }).ToArray();

            CurrentPageNumber++;

            return true;
        }

        public CsvContact[] Contacts { get; private set; }

        public void Dispose()
        {
            _csvReader.Dispose();
            _streamReader.Dispose();
            _stream.Dispose();
        }
    }
}
