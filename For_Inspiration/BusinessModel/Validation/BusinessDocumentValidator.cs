namespace MetaForge.BusinessModel;

public sealed class BusinessDocumentValidator
{
    public IReadOnlyList<BusinessValidationIssue> Validate(BusinessAuthoringDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var issues = new List<BusinessValidationIssue>();

        if (string.IsNullOrWhiteSpace(document.SchemaVersion))
        {
            issues.Add(new BusinessValidationIssue("schema.missing", "SchemaVersion nesmi byt prazdna.", "Error", "schemaVersion"));
        }

        if (string.IsNullOrWhiteSpace(document.Project.Id))
        {
            issues.Add(new BusinessValidationIssue("project.id.missing", "Project.Id nesmi byt prazdne.", "Error", "project.id"));
        }

        if (string.IsNullOrWhiteSpace(document.Project.Name))
        {
            issues.Add(new BusinessValidationIssue("project.name.missing", "Project.Name nesmi byt prazdne.", "Error", "project.name"));
        }

        AddDuplicateIdIssues(document.Entities.Select(entity => entity.Id), "entity.id.duplicate", "entities", "Entities obsahuji duplicitni id hodnotu {0}.", issues);
        AddDuplicateIdIssues(document.Relations.Select(relation => relation.Id), "relation.id.duplicate", "relations", "Relations obsahuji duplicitni id hodnotu {0}.", issues);
        AddDuplicateIdIssues(document.Workflows.Select(workflow => workflow.Id), "workflow.id.duplicate", "workflows", "Workflows obsahuji duplicitni id hodnotu {0}.", issues);
        AddDuplicateIdIssues(document.Notes.Select(note => note.Id), "note.id.duplicate", "notes", "Notes obsahuji duplicitni id hodnotu {0}.", issues);
        AddDuplicateIdIssues(document.PendingQuestions.Select(question => question.Id), "question.id.duplicate", "pendingQuestions", "PendingQuestions obsahuji duplicitni id hodnotu {0}.", issues);
        AddDuplicateNameIssues(document.Workflows.Select(workflow => workflow.Name), "workflow.name.duplicate", "workflows", "Workflows obsahuji duplicitni name hodnotu {0}.", issues);

        var entityIds = new HashSet<string>(document.Entities.Select(entity => entity.Id), StringComparer.OrdinalIgnoreCase);
        var entitiesById = document.Entities
            .Where(entity => !string.IsNullOrWhiteSpace(entity.Id))
            .GroupBy(entity => entity.Id, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var relationIds = new HashSet<string>(document.Relations.Select(relation => relation.Id), StringComparer.OrdinalIgnoreCase);

        foreach (var entity in document.Entities)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                issues.Add(new BusinessValidationIssue("entity.id.missing", $"Entita {entity.Name} nema Id.", "Error", $"entities[{entity.Name}].id"));
            }

            if (string.IsNullOrWhiteSpace(entity.Name))
            {
                issues.Add(new BusinessValidationIssue("entity.name.missing", $"Entita {entity.Id} nema Name.", "Error", $"entities[{entity.Id}].name"));
            }

            AddDuplicateNameIssues(document.Entities.Select(item => item.Name), "entity.name.duplicate", "entities", "Entities obsahuji duplicitni name hodnotu {0}.", issues);
            AddDuplicateIdIssues(entity.Attributes.Select(attribute => attribute.Id), "attribute.id.duplicate", $"entities[{entity.Id}].attributes", "Atributy v entite {0} obsahuji duplicitni id {1}.", issues, entity.Id);
            AddDuplicateIdIssues(entity.Behaviors.Select(behavior => behavior.Id), "behavior.id.duplicate", $"entities[{entity.Id}].behaviors", "Behaviors v entite {0} obsahuji duplicitni id {1}.", issues, entity.Id);
            AddDuplicateNameIssues(entity.Attributes.Select(attribute => attribute.Name), "attribute.name.duplicate", $"entities[{entity.Id}].attributes", "Atributy v entite {0} obsahuji duplicitni name {1}.", issues, entity.Id);
            AddDuplicateNameIssues(entity.Behaviors.Select(behavior => behavior.Name), "behavior.name.duplicate", $"entities[{entity.Id}].behaviors", "Behaviors v entite {0} obsahuji duplicitni name {1}.", issues, entity.Id);
        }

        foreach (var relation in document.Relations)
        {
            if (!entityIds.Contains(relation.SourceEntityId))
            {
                issues.Add(new BusinessValidationIssue("relation.source.missing", $"Relation {relation.Id} odkazuje na neexistujici SourceEntityId {relation.SourceEntityId}.", "Error", $"relations[{relation.Id}].sourceEntityId"));
            }

            if (!entityIds.Contains(relation.TargetEntityId))
            {
                issues.Add(new BusinessValidationIssue("relation.target.missing", $"Relation {relation.Id} odkazuje na neexistujici TargetEntityId {relation.TargetEntityId}.", "Error", $"relations[{relation.Id}].targetEntityId"));
            }
        }

        foreach (var workflow in document.Workflows)
        {
            if (string.IsNullOrWhiteSpace(workflow.Id))
            {
                issues.Add(new BusinessValidationIssue("workflow.id.missing", $"Workflow {workflow.Name} nema Id.", "Error", $"workflows[{workflow.Name}].id"));
            }

            if (string.IsNullOrWhiteSpace(workflow.Name))
            {
                issues.Add(new BusinessValidationIssue("workflow.name.missing", $"Workflow {workflow.Id} nema Name.", "Error", $"workflows[{workflow.Id}].name"));
            }

            AddDuplicateIdIssues(workflow.Steps.Select(step => step.Id), "workflow.step.id.duplicate", $"workflows[{workflow.Id}].steps", "Workflow {0} obsahuje duplicitni step id {1}.", issues, workflow.Id);
            AddDuplicateNameIssues(workflow.Steps.Select(step => step.Name), "workflow.step.name.duplicate", $"workflows[{workflow.Id}].steps", "Workflow {0} obsahuje duplicitni step name {1}.", issues, workflow.Id);
            AddDuplicateIdIssues(workflow.Transitions.Select(transition => transition.Id), "workflow.transition.id.duplicate", $"workflows[{workflow.Id}].transitions", "Workflow {0} obsahuje duplicitni transition id {1}.", issues, workflow.Id);

            var stepIds = new HashSet<string>(workflow.Steps.Select(step => step.Id), StringComparer.OrdinalIgnoreCase);

            foreach (var step in workflow.Steps)
            {
                if (string.IsNullOrWhiteSpace(step.Id))
                {
                    issues.Add(new BusinessValidationIssue("workflow.step.id.missing", $"Workflow step {step.Name} ve workflow {workflow.Id} nema Id.", "Error", $"workflows[{workflow.Id}].steps[{step.Name}].id"));
                }

                if (string.IsNullOrWhiteSpace(step.Name))
                {
                    issues.Add(new BusinessValidationIssue("workflow.step.name.missing", $"Workflow step {step.Id} ve workflow {workflow.Id} nema Name.", "Error", $"workflows[{workflow.Id}].steps[{step.Id}].name"));
                }

                if (!string.IsNullOrWhiteSpace(step.RelatedEntityId) && !entityIds.Contains(step.RelatedEntityId))
                {
                    issues.Add(new BusinessValidationIssue("workflow.step.entity.missing", $"Workflow step {step.Id} odkazuje na neexistujici entitu {step.RelatedEntityId}.", "Error", $"workflows[{workflow.Id}].steps[{step.Id}].relatedEntityId"));
                }

                if (!string.IsNullOrWhiteSpace(step.RelatedBehaviorId))
                {
                    if (string.IsNullOrWhiteSpace(step.RelatedEntityId))
                    {
                        issues.Add(new BusinessValidationIssue("workflow.step.behavior.entity_required", $"Workflow step {step.Id} odkazuje na behavior {step.RelatedBehaviorId}, ale nema RelatedEntityId.", "Error", $"workflows[{workflow.Id}].steps[{step.Id}].relatedBehaviorId"));
                    }
                    else if (entitiesById.TryGetValue(step.RelatedEntityId, out var entity)
                        && !entity.Behaviors.Any(behavior => string.Equals(behavior.Id, step.RelatedBehaviorId, StringComparison.OrdinalIgnoreCase)))
                    {
                        issues.Add(new BusinessValidationIssue("workflow.step.behavior.missing", $"Workflow step {step.Id} odkazuje na neexistujici behavior {step.RelatedBehaviorId} v entite {step.RelatedEntityId}.", "Error", $"workflows[{workflow.Id}].steps[{step.Id}].relatedBehaviorId"));
                    }
                }

                if (step.BindingDetail is not null)
                {
                    if (string.IsNullOrWhiteSpace(step.BindingDetail.BindingKind))
                    {
                        issues.Add(new BusinessValidationIssue("workflow.step.binding.kind.missing", $"Workflow step {step.Id} ma binding detail, ale BindingKind je prazdne.", "Error", $"workflows[{workflow.Id}].steps[{step.Id}].bindingDetail.bindingKind"));
                    }

                    if (step.BindingDetail.Source != CoreInfoSource.Manual && !step.BindingDetail.LastSyncedAt.HasValue)
                    {
                        issues.Add(new BusinessValidationIssue("workflow.step.binding.lastsynced.missing", $"Workflow step {step.Id} ma binding detail se zdrojem {step.BindingDetail.Source}, ale LastSyncedAt neni nastaveno.", "Error", $"workflows[{workflow.Id}].steps[{step.Id}].bindingDetail.lastSyncedAt"));
                    }
                }
            }

            foreach (var transition in workflow.Transitions)
            {
                if (string.IsNullOrWhiteSpace(transition.Id))
                {
                    issues.Add(new BusinessValidationIssue("workflow.transition.id.missing", $"Workflow transition ve workflow {workflow.Id} nema Id.", "Error", $"workflows[{workflow.Id}].transitions[].id"));
                }

                if (!stepIds.Contains(transition.FromStepId))
                {
                    issues.Add(new BusinessValidationIssue("workflow.transition.from.missing", $"Workflow transition {transition.Id} odkazuje na neexistujici FromStepId {transition.FromStepId}.", "Error", $"workflows[{workflow.Id}].transitions[{transition.Id}].fromStepId"));
                }

                if (!stepIds.Contains(transition.ToStepId))
                {
                    issues.Add(new BusinessValidationIssue("workflow.transition.to.missing", $"Workflow transition {transition.Id} odkazuje na neexistujici ToStepId {transition.ToStepId}.", "Error", $"workflows[{workflow.Id}].transitions[{transition.Id}].toStepId"));
                }

                if (!string.IsNullOrWhiteSpace(transition.FromStepId)
                    && string.Equals(transition.FromStepId, transition.ToStepId, StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(new BusinessValidationIssue("workflow.transition.self_loop", $"Workflow transition {transition.Id} nesmi vest sam na sebe.", "Error", $"workflows[{workflow.Id}].transitions[{transition.Id}]"));
                }
            }
        }

        foreach (var question in document.PendingQuestions)
        {
            if (!string.IsNullOrWhiteSpace(question.RelatedEntityId) && !entityIds.Contains(question.RelatedEntityId))
            {
                issues.Add(new BusinessValidationIssue("question.entity.missing", $"Question {question.Id} odkazuje na neexistujici entitu {question.RelatedEntityId}.", "Error", $"pendingQuestions[{question.Id}].relatedEntityId"));
            }

            if (!string.IsNullOrWhiteSpace(question.RelatedRelationId) && !relationIds.Contains(question.RelatedRelationId))
            {
                issues.Add(new BusinessValidationIssue("question.relation.missing", $"Question {question.Id} odkazuje na neexistujici relation {question.RelatedRelationId}.", "Error", $"pendingQuestions[{question.Id}].relatedRelationId"));
            }

            if (!string.IsNullOrWhiteSpace(question.RelatedAttributeId))
            {
                if (string.IsNullOrWhiteSpace(question.RelatedEntityId))
                {
                    issues.Add(new BusinessValidationIssue("question.attribute.entity_required", $"Question {question.Id} odkazuje na atribut {question.RelatedAttributeId}, ale nema RelatedEntityId.", "Error", $"pendingQuestions[{question.Id}].relatedAttributeId"));
                }
                else if (entitiesById.TryGetValue(question.RelatedEntityId, out var entity)
                    && !entity.Attributes.Any(attribute => string.Equals(attribute.Id, question.RelatedAttributeId, StringComparison.OrdinalIgnoreCase)))
                {
                    issues.Add(new BusinessValidationIssue("question.attribute.missing", $"Question {question.Id} odkazuje na neexistujici atribut {question.RelatedAttributeId} v entite {question.RelatedEntityId}.", "Error", $"pendingQuestions[{question.Id}].relatedAttributeId"));
                }
            }

            if (!string.IsNullOrWhiteSpace(question.RelatedBehaviorId))
            {
                if (string.IsNullOrWhiteSpace(question.RelatedEntityId))
                {
                    issues.Add(new BusinessValidationIssue("question.behavior.entity_required", $"Question {question.Id} odkazuje na behavior {question.RelatedBehaviorId}, ale nema RelatedEntityId.", "Error", $"pendingQuestions[{question.Id}].relatedBehaviorId"));
                }
                else if (entitiesById.TryGetValue(question.RelatedEntityId, out var entity)
                    && !entity.Behaviors.Any(behavior => string.Equals(behavior.Id, question.RelatedBehaviorId, StringComparison.OrdinalIgnoreCase)))
                {
                    issues.Add(new BusinessValidationIssue("question.behavior.missing", $"Question {question.Id} odkazuje na neexistujici behavior {question.RelatedBehaviorId} v entite {question.RelatedEntityId}.", "Error", $"pendingQuestions[{question.Id}].relatedBehaviorId"));
                }
            }
        }

        return issues;
    }

    public void EnsureValid(BusinessAuthoringDocument document)
    {
        var issues = Validate(document)
            .Where(issue => string.Equals(issue.Severity, "Error", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (issues.Count == 0)
            return;

        throw new InvalidOperationException(string.Join(Environment.NewLine, issues.Select(issue => $"{issue.Code}: {issue.Message}")));
    }

    private static void AddDuplicateIdIssues(
        IEnumerable<string> ids,
        string code,
        string path,
        string messageFormat,
        ICollection<BusinessValidationIssue> issues,
        string? entityId = null)
    {
        var duplicateIds = ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .GroupBy(id => id, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);

        foreach (var duplicateId in duplicateIds)
        {
            var message = entityId is null
                ? string.Format(messageFormat, duplicateId)
                : string.Format(messageFormat, entityId, duplicateId);

            issues.Add(new BusinessValidationIssue(code, message, "Error", path));
        }
    }

    private static void AddDuplicateNameIssues(
        IEnumerable<string> names,
        string code,
        string path,
        string messageFormat,
        ICollection<BusinessValidationIssue> issues,
        string? entityId = null)
    {
        var duplicateNames = names
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);

        foreach (var duplicateName in duplicateNames)
        {
            var message = entityId is null
                ? string.Format(messageFormat, duplicateName)
                : string.Format(messageFormat, entityId, duplicateName);

            issues.Add(new BusinessValidationIssue(code, message, "Error", path));
        }
    }
}