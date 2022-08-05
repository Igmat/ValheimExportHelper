﻿using AssetRipper.Core.Logging;
using AssetRipper.Library;
using AssetRipper.Library.Exporters;
using YamlDotNet.Serialization;

namespace ValheimExportHelper
{
  internal abstract class PostExporterEx : IPostExporter
  {
    public Ripper CurrentRipper { get; set; }

    public void Init(Ripper ripper)
    {
      CurrentRipper = ripper;
    }

    void IPostExporter.DoPostExport(Ripper ripper)
    {
      LogInfo($"Running PostExporter module {GetType().Name}");
      Init(ripper);
      Export();
    }

    public abstract void Export();

    public void LogInfo(string text)
    {
      Logger.Info(LogCategory.Plugin, $"[{GetType().FullName}] {text}");
    }

    public void LogWarn(string text)
    {
      Logger.Warning(LogCategory.Plugin, $"[{GetType().FullName}] {text}");
    }

    public void LogError(string text)
    {
      Logger.Error(LogCategory.Plugin, $"[{GetType().FullName}] {text}");
    }

    private bool IsValidYamlLine(string line)
    {
      return !line.StartsWith('%') && !line.StartsWith("---");
    }

    private string ReadYamlFileAsText(string fileName)
    {
      try
      {
        string[] yamlLines = File.ReadAllLines(fileName)
          .Where(IsValidYamlLine)
          .ToArray();
        return String.Join('\n', yamlLines);
      }
      catch
      {
        LogError($"Failed to open {fileName}");
        throw;
      }
    }

    private dynamic DeserializeYamlFromText(string yamlText)
    {
      try
      {
        var deserializer = new DeserializerBuilder().Build();
        return deserializer.Deserialize(new StringReader(yamlText));
      }
      catch
      {
        LogError($"Failed to deserialize yaml content:\n{yamlText}");
        throw;
      }
    }

    public dynamic ReadYamlFile(string filename)
    {
      string yamlText = ReadYamlFileAsText(filename);
      return DeserializeYamlFromText(yamlText);
    }

    public void WriteYamlFile(string filename, dynamic yaml)
    {
      var serializer = new SerializerBuilder().Build();
      string result = serializer.Serialize(yaml);

      File.WriteAllText(filename, result);
    }
  }
}
