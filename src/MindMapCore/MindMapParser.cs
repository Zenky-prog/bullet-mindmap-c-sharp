using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace MindMapCore;

public sealed class MindMapNode
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Title { get; set; } = string.Empty;
    public List<MindMapNode> Children { get; set; } = new();
}

public static class MindMapParser
{
    public static MindMapNode ParseFromBulletText(string rawText, string rootTitle = "الخريطة الذهنية")
    {
        var root = new MindMapNode { Title = rootTitle };
        var stack = new Stack<(int Indent, MindMapNode Node)>();
        stack.Push((-1, root));

        var lines = rawText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var rawLine in lines)
        {
            if (string.IsNullOrWhiteSpace(rawLine)) continue;

            // تحويل التبويب (Tab) إلى مسافتين لتوحيد المستويات
            string normalized = rawLine.Replace("\t", "  ");
            int indent = normalized.TakeWhile(c => c == ' ').Count();
            string cleaned = normalized.Trim().TrimStart('-', '*', '•').Trim();

            if (string.IsNullOrEmpty(cleaned)) continue;

            while (stack.Count > 1 && stack.Peek().Indent >= indent)
            {
                stack.Pop();
            }

            var newNode = new MindMapNode { Title = cleaned };
            stack.Peek().Node.Children.Add(newNode);
            stack.Push((indent, newNode));
        }

        return root;
    }

    public static string ToJson(MindMapNode root)
    {
        return JsonSerializer.Serialize(root, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
