using System.Text;

namespace MetaForge.BusinessModel;

/// <summary>
/// Renders a <see cref="BusinessAuthoringDocument"/> as a Unicode tree with emoji icons.
/// Inspired by ModelTreeBuilder in MetaForge.Builders.
/// </summary>
public static class BusinessTreeRenderer
{
    private static readonly Dictionary<string, string> TypeIcons = new(StringComparer.OrdinalIgnoreCase)
    {
        ["text"] = "📝",
        ["string"] = "📝",
        ["int"] = "🔢",
        ["integer"] = "🔢",
        ["long"] = "🔢",
        ["short"] = "🔢",
        ["decimal"] = "🔢",
        ["double"] = "🔢",
        ["float"] = "🔢",
        ["bool"] = "✅",
        ["boolean"] = "✅",
        ["date"] = "📅",
        ["datetime"] = "📅",
        ["time"] = "🕐",
        ["email"] = "📧",
        ["email-address"] = "📧",
        ["phone"] = "📱",
        ["phone-number"] = "📱",
        ["url"] = "🔗",
        ["money"] = "💰",
        ["vykon"] = "⚙️",
        ["výkon"] = "⚙️",
        ["horsepower"] = "⚙️",
        ["power"] = "⚙️",
        ["guid"] = "🔑",
        ["uuid"] = "🔑",
        ["color"] = "🎨",
        ["colour"] = "🎨",
        ["country"] = "🌍",
        ["postal-code"] = "📮",
        ["postalcode"] = "📮",
        ["ip-address"] = "🌐",
        ["ipaddress"] = "🌐",
        ["vin"] = "🚗",
        ["iban"] = "🏦",
        ["credit-card"] = "💳",
        ["mac-address"] = "🔌",
        ["ssn"] = "🔒",
        ["enum"] = "📋",
        ["computed"] = "⚡",
        ["custom"] = "📦",
    };

    public static string Render(BusinessAuthoringDocument document, BusinessTreeDetailLevel detailLevel = BusinessTreeDetailLevel.Extended)
    {
        return detailLevel switch
        {
            BusinessTreeDetailLevel.Basic => RenderAuthoringBasic(document),
            BusinessTreeDetailLevel.Full => RenderAuthoringDetailed(document, includeDeepDetails: true),
            _ => RenderAuthoringDetailed(document, includeDeepDetails: false),
        };
    }

    private static void AppendMetadataLines(StringBuilder sb, IReadOnlyList<string> lines, string prefix)
    {
        for (int index = 0; index < lines.Count; index++)
        {
            string branch = index == lines.Count - 1 ? "└── " : "├── ";
            sb.AppendLine($"{prefix}{branch}{lines[index]}");
        }
    }

    private static string RenderAuthoringBasic(BusinessAuthoringDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var sb = new StringBuilder();
        string projectIcon = document.Project.Icon ?? "📦";
        sb.AppendLine($"{projectIcon} {document.Project.Name}");

        if (!string.IsNullOrEmpty(document.Project.Description))
            sb.AppendLine($"   {document.Project.Description}");

        sb.AppendLine($"   Přehled: {document.Entities.Count} entit | {document.Relations.Count} vazeb | {document.PendingQuestions.Count} otázek");

        for (int index = 0; index < document.Entities.Count; index++)
        {
            var entity = document.Entities[index];
            bool isLast = index == document.Entities.Count - 1 && document.Relations.Count == 0 && document.PendingQuestions.Count == 0;
            string branch = isLast ? "└── " : "├── ";
            string pipe = isLast ? "    " : "│   ";
            string entityIcon = entity.Icon ?? "📋";

            sb.AppendLine($"{branch}{entityIcon} {entity.Name}");
            sb.AppendLine($"{pipe}   Položky: {entity.Attributes.Count} | Behaviors: {entity.Behaviors.Count}");
        }

        AppendAuthoringSections(sb, document, includeDeepDetails: false);
        return sb.ToString();
    }

    private static string RenderAuthoringDetailed(BusinessAuthoringDocument document, bool includeDeepDetails)
    {
        ArgumentNullException.ThrowIfNull(document);

        var sb = new StringBuilder();
        string projectIcon = document.Project.Icon ?? "📦";
        sb.AppendLine($"{projectIcon} {document.Project.Name}");

        if (!string.IsNullOrEmpty(document.Project.Description))
            sb.AppendLine($"   {document.Project.Description}");

        sb.AppendLine($"   Verze: {document.Project.Version} | Entity: {document.Entities.Count} | Vztahy: {document.Relations.Count} | Otázky: {document.PendingQuestions.Count}");

        foreach (var entity in document.Entities)
        {
            string entityIcon = entity.Icon ?? "📋";
            sb.AppendLine($"├── {entityIcon} {entity.Name}");

            if (!string.IsNullOrWhiteSpace(entity.Summary))
                sb.AppendLine($"│   └── {entity.Summary}");

            if (includeDeepDetails)
            {
                foreach (var note in entity.Notes)
                    sb.AppendLine($"│   ├── 📝 {note.Text}");
            }

            foreach (var attribute in entity.Attributes)
            {
                string typeIcon = TypeIcons.GetValueOrDefault(attribute.Type, "📦");
                string required = attribute.Required ? " *" : string.Empty;
                string syncPrefix = GetSyncStatePrefix(attribute);
                sb.AppendLine($"│   ├── {syncPrefix}{typeIcon} {attribute.Name} : {attribute.Type}{required}");

                if (includeDeepDetails)
                {
                    if (!string.IsNullOrWhiteSpace(attribute.Summary))
                        sb.AppendLine($"│   │   └── summary: {attribute.Summary}");

                    if (attribute.Constraints.Count > 0)
                        sb.AppendLine($"│   │   └── constraints: {string.Join(", ", attribute.Constraints)}");
                }
            }

            foreach (var behavior in entity.Behaviors)
            {
                sb.AppendLine($"│   ├── ⚙ {behavior.Name} ({behavior.Kind})");

                if (includeDeepDetails)
                {
                    if (!string.IsNullOrWhiteSpace(behavior.Summary))
                        sb.AppendLine($"│   │   └── summary: {behavior.Summary}");

                    if (!string.IsNullOrWhiteSpace(behavior.Returns))
                        sb.AppendLine($"│   │   └── returns: {behavior.Returns}");

                    foreach (var note in behavior.Notes)
                        sb.AppendLine($"│   │   └── note: {note.Text}");
                }
            }
        }

        AppendAuthoringSections(sb, document, includeDeepDetails);
        return sb.ToString();
    }

    private static void AppendAuthoringSections(StringBuilder sb, BusinessAuthoringDocument document, bool includeDeepDetails)
    {
        var entityLookup = document.Entities.ToDictionary(entity => entity.Id, entity => entity.Name, StringComparer.OrdinalIgnoreCase);

        if (document.Notes.Count > 0)
        {
            sb.AppendLine("└── 🗒 Notes");
            foreach (var note in document.Notes)
                sb.AppendLine($"    ├── {note.Text}");
        }

        if (document.Relations.Count > 0)
        {
            sb.AppendLine("└── 🔗 Relations");
            foreach (var relation in document.Relations)
            {
                var sourceEntityName = entityLookup.GetValueOrDefault(relation.SourceEntityId, relation.SourceEntityId);
                var targetEntityName = entityLookup.GetValueOrDefault(relation.TargetEntityId, relation.TargetEntityId);
                sb.AppendLine($"    ├── {sourceEntityName} {GetAuthoringRelationIcon(relation.Kind)} {targetEntityName} ({relation.Kind})");

                if (includeDeepDetails)
                {
                    if (!string.IsNullOrWhiteSpace(relation.SourceNavigationName))
                        sb.AppendLine($"    │   └── sourceNavigation: {relation.SourceNavigationName}");

                    if (!string.IsNullOrWhiteSpace(relation.TargetNavigationName))
                        sb.AppendLine($"    │   └── targetNavigation: {relation.TargetNavigationName}");

                    foreach (var note in relation.Notes)
                        sb.AppendLine($"    │   └── note: {note.Text}");
                }
            }
        }

        if (document.PendingQuestions.Count > 0)
        {
            sb.AppendLine("└── ❓ Pending Questions");
            foreach (var question in document.PendingQuestions)
            {
                sb.AppendLine($"    ├── {question.Text} [{question.Status}]");

                if (includeDeepDetails)
                {
                    if (!string.IsNullOrWhiteSpace(question.RelatedEntityId))
                        sb.AppendLine($"    │   └── entity: {question.RelatedEntityId}");

                    if (!string.IsNullOrWhiteSpace(question.RelatedAttributeId))
                        sb.AppendLine($"    │   └── attribute: {question.RelatedAttributeId}");

                    if (!string.IsNullOrWhiteSpace(question.RelatedBehaviorId))
                        sb.AppendLine($"    │   └── behavior: {question.RelatedBehaviorId}");

                    if (!string.IsNullOrWhiteSpace(question.RelatedRelationId))
                        sb.AppendLine($"    │   └── relation: {question.RelatedRelationId}");
                }
            }
        }
    }

    private static string GetSyncStatePrefix(BusinessAttributeNode attribute)
    {
        if (attribute.CoreDetail is null)
            return string.Empty;

        return attribute.CoreDetail.SyncState switch
        {
            AttributeSyncState.Synced => "✔ ",
            AttributeSyncState.BusinessEdited => "✏ ",
            AttributeSyncState.CoreEdited => "⚙ ",
            AttributeSyncState.Conflict => "⚠ ",
            _ => string.Empty,
        };
    }

    private static string GetAuthoringRelationIcon(BusinessRelationKind relationKind)
    {
        return relationKind switch
        {
            BusinessRelationKind.BelongsTo => "→",
            BusinessRelationKind.HasMany => "↠",
            BusinessRelationKind.HasOne => "→",
            BusinessRelationKind.ManyToMany => "⇄",
            _ => "→",
        };
    }
}
