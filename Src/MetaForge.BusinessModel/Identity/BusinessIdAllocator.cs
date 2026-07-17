using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Identity;

/// <summary>
/// Generátor lidsky čitelných ID pro business elementy.
/// Vytváří slugy s prefixem podle typu elementu a detekcí kolizí.
/// 
/// Příklady:
///   "Employee"          → "entity.employee"
///   "Email"             → "attr.employee-email"
///   "CalculateNetSalary" → "behavior.employee-calculate-net-salary"
/// 
/// NENÍ garancí unikátnosti — tu řeší validátor.
/// </summary>
public sealed class BusinessIdAllocator
{
    /// <summary>Vytvoří projektové ID z názvu projektu.</summary>
    public string CreateProjectId(string projectName)
    {
        return $"project.{Slugify(projectName)}";
    }

    /// <summary>Vytvoří ID entity z názvu entity v kontextu dokumentu.</summary>
    public string CreateEntityId(string entityName, BusinessAuthoringDocument document)
    {
        var baseId = $"entity.{Slugify(entityName)}";
        return EnsureUnique(baseId, document.Entities.Select(e => e.Id));
    }

    /// <summary>Vytvoří ID atributu z názvu atributu v kontextu entity.</summary>
    public string CreateAttributeId(string attributeName, BusinessEntityNode entity)
    {
        var baseId = $"attr.{Slugify(entity.Name)}-{Slugify(attributeName)}";
        return EnsureUnique(baseId, entity.Attributes.Select(a => a.Id));
    }

    /// <summary>Vytvoří ID chování z názvu chování v kontextu entity.</summary>
    public string CreateBehaviorId(string behaviorName, BusinessEntityNode entity)
    {
        var baseId = $"behavior.{Slugify(entity.Name)}-{Slugify(behaviorName)}";
        return EnsureUnique(baseId, entity.Behaviors.Select(b => b.Id));
    }

    /// <summary>Vytvoří ID relace mezi dvěma entitami.</summary>
    public string CreateRelationId(
        string sourceEntityId,
        string kind,
        string targetEntityId,
        BusinessAuthoringDocument document)
    {
        var baseId = $"rel.{sourceEntityId}-{Slugify(kind)}-{targetEntityId}";
        return EnsureUnique(baseId, document.Relations.Select(r => r.Id));
    }

    /// <summary>Vytvoří ID otázky v kontextu dokumentu.</summary>
    public string CreateQuestionId(BusinessAuthoringDocument document)
    {
        var baseId = $"question.{document.PendingQuestions.Count + 1}";
        return EnsureUnique(baseId, document.PendingQuestions.Select(q => q.Id));
    }

    /// <summary>Vytvoří ID poznámky (časové razítko + guid suffix).</summary>
    public string CreateNoteId()
    {
        return $"note.{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
    }

    /// <summary>
    /// Převede text na URL-friendly slug:
    /// "CalculateNetSalary" → "calculate-net-salary"
    /// </summary>
    private static string Slugify(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "unnamed";

        // Rozdělí na slova na hranicích velkých písmen
        var sb = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (i > 0 && char.IsUpper(c) && char.IsLower(text[i - 1]))
            {
                sb.Append('-');
            }
            sb.Append(char.ToLowerInvariant(c));
        }

        var slug = sb.ToString();

        // Nahradí vše kromě [a-z0-9-] pomlčkou
        slug = Regex.Replace(slug, @"[^a-z0-9-]", "-");

        // Odstraní duplicitní pomlčky
        slug = Regex.Replace(slug, @"-{2,}", "-");

        // Ořízne počáteční/koncové pomlčky
        slug = slug.Trim('-');

        return slug.Length > 0 ? slug : "unnamed";
    }

    /// <summary>
    /// Zajistí unikátnost ID přidáním suffixu při kolizi.
    /// "entity.employee" → "entity.employee-2" → "entity.employee-3" ...
    /// </summary>
    private static string EnsureUnique(string baseId, IEnumerable<string> existingIds)
    {
        var existingSet = new HashSet<string>(existingIds);
        if (!existingSet.Contains(baseId))
            return baseId;

        int suffix = 2;
        string candidate;
        do
        {
            candidate = $"{baseId}-{suffix}";
            suffix++;
        } while (existingSet.Contains(candidate));

        return candidate;
    }
}
