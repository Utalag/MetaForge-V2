using MetaForge.BusinessModel.Models;

namespace MetaForge.BusinessModel.Validation;

/// <summary>
/// Validátor BusinessAuthoringDocument.
/// Kontroluje strukturální integritu, unikátnost ID, povinná pole.
/// </summary>
public sealed class BusinessDocumentValidator
{
    /// <summary>
    /// Validuje celý dokument a vrátí seznam nalezených problémů.
    /// Prázdný seznam = dokument je validní.
    /// </summary>
    public IReadOnlyList<BusinessValidationIssue> Validate(BusinessAuthoringDocument document)
    {
        var issues = new List<BusinessValidationIssue>();

        ValidateProject(document, issues);
        ValidateEntities(document, issues);
        ValidateRelations(document, issues);

        return issues;
    }

    /// <summary>Validuje projektovou sekci dokumentu.</summary>
    private static void ValidateProject(BusinessAuthoringDocument document, List<BusinessValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(document.Project.Name))
        {
            issues.Add(new BusinessValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "MISSING_PROJECT_NAME",
                Message = "Projekt musí mít název.",
                Path = "Project.Name",
                Suggestion = "Zadejte název projektu.",
            });
        }

        if (string.IsNullOrWhiteSpace(document.SchemaVersion))
        {
            issues.Add(new BusinessValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "MISSING_SCHEMA_VERSION",
                Message = "Dokument musí mít verzi schématu.",
                Path = "SchemaVersion",
            });
        }
    }

    /// <summary>Validuje všechny entity v dokumentu.</summary>
    private static void ValidateEntities(BusinessAuthoringDocument document, List<BusinessValidationIssue> issues)
    {
        var entityIds = new HashSet<string>();
        var entityNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < document.Entities.Count; i++)
        {
            var entity = document.Entities[i];
            var path = $"Entities[{i}]";

            // Prázdné jméno
            if (string.IsNullOrWhiteSpace(entity.Name))
            {
                issues.Add(new BusinessValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Code = "MISSING_ENTITY_NAME",
                    Message = "Entita musí mít název.",
                    Path = path,
                    ElementId = entity.Id,
                });
            }

            // Duplicitní ID
            if (!entityIds.Add(entity.Id))
            {
                issues.Add(new BusinessValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Code = "DUPLICATE_ENTITY_ID",
                    Message = $"Duplicitní ID entity: '{entity.Id}'.",
                    Path = path,
                    ElementId = entity.Id,
                });
            }

            // Duplicitní název (case-insensitive)
            if (!string.IsNullOrWhiteSpace(entity.Name) && !entityNames.Add(entity.Name))
            {
                issues.Add(new BusinessValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Code = "DUPLICATE_ENTITY_NAME",
                    Message = $"Duplicitní název entity: '{entity.Name}'.",
                    Path = path,
                    ElementId = entity.Id,
                    Suggestion = "Zvažte přejmenování entity pro jednoznačnost.",
                });
            }

            // Validace atributů entity
            ValidateAttributes(entity, path, issues);
        }
    }

    /// <summary>Validuje atributy jedné entity.</summary>
    private static void ValidateAttributes(BusinessEntityNode entity, string parentPath, List<BusinessValidationIssue> issues)
    {
        var attrIds = new HashSet<string>();

        for (int j = 0; j < entity.Attributes.Count; j++)
        {
            var attr = entity.Attributes[j];
            var path = $"{parentPath}.Attributes[{j}]";

            // Prázdné jméno
            if (string.IsNullOrWhiteSpace(attr.Name))
            {
                issues.Add(new BusinessValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Code = "MISSING_ATTRIBUTE_NAME",
                    Message = "Atribut musí mít název.",
                    Path = path,
                    ElementId = attr.Id,
                });
            }

            // Duplicitní ID atributu
            if (!attrIds.Add(attr.Id))
            {
                issues.Add(new BusinessValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Code = "DUPLICATE_ATTRIBUTE_ID",
                    Message = $"Duplicitní ID atributu: '{attr.Id}'.",
                    Path = path,
                    ElementId = attr.Id,
                });
            }

            // CoreDetail SyncState validace
            if (attr.CoreDetail is not null && attr.CoreDetail.SyncState == AttributeSyncState.Conflict)
            {
                issues.Add(new BusinessValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Code = "SYNC_CONFLICT",
                    Message = $"Atribut '{attr.Name}' je v konfliktním stavu synchronizace.",
                    Path = path,
                    ElementId = attr.Id,
                    Suggestion = "Vyřešte konflikt mezi business a core hodnotami.",
                });
            }
        }
    }

    /// <summary>Validuje relace mezi entitami.</summary>
    private static void ValidateRelations(BusinessAuthoringDocument document, List<BusinessValidationIssue> issues)
    {
        var entityIds = new HashSet<string>(document.Entities.Select(e => e.Id));

        for (int i = 0; i < document.Relations.Count; i++)
        {
            var rel = document.Relations[i];
            var path = $"Relations[{i}]";

            // Zdrojová entita neexistuje
            if (!entityIds.Contains(rel.FromEntityId))
            {
                issues.Add(new BusinessValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Code = "ORPHANED_RELATION_SOURCE",
                    Message = $"Relace odkazuje na neexistující zdrojovou entitu: '{rel.FromEntityId}'.",
                    Path = path,
                    ElementId = rel.Id,
                    Suggestion = "Odstraňte relaci nebo vytvořte zdrojovou entitu.",
                });
            }

            // Cílová entita neexistuje
            if (!entityIds.Contains(rel.ToEntityId))
            {
                issues.Add(new BusinessValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Code = "ORPHANED_RELATION_TARGET",
                    Message = $"Relace odkazuje na neexistující cílovou entitu: '{rel.ToEntityId}'.",
                    Path = path,
                    ElementId = rel.Id,
                    Suggestion = "Odstraňte relaci nebo vytvořte cílovou entitu.",
                });
            }
        }
    }
}
