using System;
using System.IO;
using System.Text;
using MindMapCore;

string inputPath = args.Length > 0 ? args[0] : "sample-bullets.txt";
string outputPath = args.Length > 1 ? args[1] : "docs/mindmap.json";
string rootTitle = args.Length > 2 ? args[2] : "خريطة ذهنية";

if (!File.Exists(inputPath))
{
    Console.Error.WriteLine($"لم يتم العثور على ملف الإدخال: {inputPath}");
    Environment.Exit(1);
}

string rawText = await File.ReadAllTextAsync(inputPath, Encoding.UTF8);
MindMapNode tree = MindMapParser.ParseFromBulletText(rawText, rootTitle);
string json = MindMapParser.ToJson(tree);

string? outDir = Path.GetDirectoryName(outputPath);
if (!string.IsNullOrEmpty(outDir))
{
    Directory.CreateDirectory(outDir);
}

await File.WriteAllTextAsync(outputPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
Console.WriteLine($"تم توليد ملف الخريطة الذهنية بنجاح: {outputPath}");
