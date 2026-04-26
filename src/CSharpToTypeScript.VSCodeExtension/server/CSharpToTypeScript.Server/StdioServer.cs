using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpToTypeScript.Core.Services;
using CSharpToTypeScript.Server.DTOs;
using Server.Services;

namespace CSharpToTypeScript.Server
{
    public class StdioServer
    {
        private readonly ICodeConverter _codeConverter;
        private readonly IFileNameConverter _fileNameConverter;
        private readonly IStdio _stdio;

        public StdioServer(ICodeConverter codeConverter, IStdio stdio, IFileNameConverter fileNameConverter)
        {
            _codeConverter = codeConverter;
            _stdio = stdio;
            _fileNameConverter = fileNameConverter;
        }

        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public void Handle()
        {
            while (_stdio.ReadLine() is var inputLine && inputLine != "EXIT")
            {
                Output output;

                try
                {
                    var input = JsonSerializer.Deserialize<Input>(inputLine, _serializerOptions)
                        ?? throw new InvalidOperationException("Input payload cannot be null.");

                    var codeConversionOptions = input.MapToCodeConversionOptions();

                    var fileNodes = _codeConverter.ConvertToTypeScript(input.Code, codeConversionOptions).ToList();

                    if (codeConversionOptions.ExportOneFilePerType)
                    {
                        var convertedFiles = fileNodes.Select(f => new ConvertedFile
                        {
                            ConvertedCode = f.Code,
                            ConvertedFileName = _fileNameConverter.ConvertToTypeScript(f.Name, codeConversionOptions)
                        }).ToList();

                        output = new Output { Succeeded = true, ConvertedFiles = convertedFiles };
                    }
                    else
                    {
                        var convertedCode = fileNodes.Single().Code;
                        var convertedFileName = string.IsNullOrWhiteSpace(input.FileName)
                            ? null
                            : _fileNameConverter.ConvertToTypeScript(input.FileName, codeConversionOptions);

                        output = new Output { Succeeded = true, ConvertedCode = convertedCode, ConvertedFileName = convertedFileName };
                    }
                }
                catch (Exception ex)
                {
                    output = new Output { Succeeded = false, ErrorMessage = ex.Message };
                }

                var outputLine = JsonSerializer.Serialize(output, _serializerOptions);

                _stdio.WriteLine(outputLine);
            }
        }
    }
}