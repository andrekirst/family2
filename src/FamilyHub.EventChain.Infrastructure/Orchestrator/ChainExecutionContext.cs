using System.Text.Json.Nodes;

namespace FamilyHub.EventChain.Infrastructure.Orchestrator;

public sealed class ChainExecutionContext
{
    private readonly JsonObject _root;

    private const string PropertyTrigger = "trigger";
    private const string PropertySteps = "steps";

    public ChainExecutionContext()
    {
        _root = new JsonObject
        {
            [PropertyTrigger] = new JsonObject(),
            [PropertySteps] = new JsonObject()
        };
    }

    public ChainExecutionContext(string json)
    {
        _root = JsonNode.Parse(json)?.AsObject() ?? new JsonObject
        {
            [PropertyTrigger] = new JsonObject(),
            [PropertySteps] = new JsonObject()
        };
    }

    public void SetTriggerData(string triggerPayloadJson)
    {
        var triggerData = JsonNode.Parse(triggerPayloadJson);
        _root[PropertyTrigger] = triggerData;
    }

    public void SetStepOutput(string stepAlias, string outputJson)
    {
        var stepsNode = _root[PropertySteps]?.AsObject() ?? new JsonObject();
        var outputData = JsonNode.Parse(outputJson);
        stepsNode[stepAlias] = outputData;
        _root[PropertySteps] = stepsNode;
    }

    public string? GetValue(string path)
    {
        // Path format: "trigger.fieldName" or "steps.stepAlias.fieldName"
        var parts = path.Split('.');
        JsonNode? current = _root;

        foreach (var part in parts)
        {
            current = current[part];
            if (current is null)
            {
                return null;
            }
        }

        return current.ToJsonString();
    }

    public bool EvaluateCondition(string? conditionExpression)
    {
        if (string.IsNullOrWhiteSpace(conditionExpression))
        {
            return true;
        }

        // Simple condition evaluation: {{path}} == value
        var expression = conditionExpression.Trim();

        // Extract template references: {{steps.create_calendar.calendarEventId}}
        var parts = expression.Split("==", 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return true; // Invalid expression defaults to true
        }

        var left = ResolveValue(parts[0]);
        var right = parts[1].Trim().Trim('"', '\'');

        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }

    public string ResolveInputMappings(string inputMappingsJson)
    {
        // Replace {{path}} templates with actual values from context
        var result = inputMappingsJson;

        while (result.Contains("{{"))
        {
            var start = result.IndexOf("{{", StringComparison.Ordinal);
            var end = result.IndexOf("}}", start, StringComparison.Ordinal);
            if (end < 0)
            {
                break;
            }

            var path = result.Substring(start + 2, end - start - 2).Trim();
            var value = ResolveValue(path) ?? "null";

            result = result[..start] + value + result[(end + 2)..];
        }

        return result;
    }

    private string? ResolveValue(string pathOrLiteral)
    {
        var path = pathOrLiteral.Trim().Trim('{', '}').Trim();

        var parts = path.Split('.');
        JsonNode? current = _root;

        foreach (var part in parts)
        {
            current = current[part];
            if (current is null)
            {
                return null;
            }
        }

        return current switch
        {
            JsonValue val => val.ToString(),
            _ => current.ToJsonString()
        };
    }

    public string ToJson() => _root.ToJsonString();
}
