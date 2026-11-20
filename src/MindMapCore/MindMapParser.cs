using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace MindMapCore;

public sealed class MindMapNode
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; }
    public List<MindMapNode> Children { get; set; } = new();
}

public static class MindMapParser
{
    public static MindMapNode ParseFromBulletText(string rawText, string rootTitle = "الخريطة الذهنية")
    {
        var root = new MindMapNode
        {
            Id = ComputeStableId($"root:{rootTitle}"),
            Title = rootTitle,
            Note = null
        };

        var stack = new Stack<(int Indent, MindMapNode Node, string Path)>();
        stack.Push((-1, root, rootTitle));

        var lines = rawText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var rawLine in lines)
        {
            if (string.IsNullOrWhiteSpace(rawLine)) continue;

            string normalized = rawLine.Replace("\t", "  ");
            int indent = normalized.TakeWhile(c => c == ' ').Count();
            string cleaned = normalized.Trim().TrimStart('-', '*', '•').Trim();
            if (string.IsNullOrEmpty(cleaned)) continue;

            string title = cleaned;
            string? note = null;

            int pipeIndex = cleaned.IndexOf('|');
            if (pipeIndex >= 0)
            {
                title = cleaned[..pipeIndex].Trim();
                string rawNote = cleaned[(pipeIndex + 1)..].Trim();
                if (!string.IsNullOrEmpty(rawNote))
                {
                    note = rawNote;
                }
            }

            if (string.IsNullOrEmpty(title)) continue;

            while (stack.Count > 1 && stack.Peek().Indent >= indent)
            {
                stack.Pop();
            }

            string nodePath = $"{stack.Peek().Path} > {title}";
            var newNode = new MindMapNode
            {
                Id = ComputeStableId(nodePath),
                Title = title,
                Note = note
            };

            stack.Peek().Node.Children.Add(newNode);
            stack.Push((indent, newNode, nodePath));
        }

        return root;
    }

    public static string ToJson(MindMapNode root)
    {
        return JsonSerializer.Serialize(root, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    private static string ComputeStableId(string input)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..8].ToLowerInvariant();
    }
}
