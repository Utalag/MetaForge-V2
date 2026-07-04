namespace MetaForge.BusinessModel.Samples;

public static class PayrollSampleDocument
{
    public static BusinessAuthoringDocument Create()
    {
        var employeeId = "entity.employee";
        var employerId = "entity.employer";
        var payrollRecordId = "entity.payroll-record";
        var taxBracketId = "entity.tax-bracket";
        var deductionId = "entity.deduction";

        return new BusinessAuthoringDocument
        {
            SchemaVersion = "1.0",
            Project = new BusinessProjectInfo
            {
                Id = "payroll-calculation",
                Name = "PayrollCalculation",
                Description = "Výpočet čisté mzdy — testovací workspace pro MetaForge front-end.",
                Version = 1
            },
            Entities =
            [
                new BusinessEntityNode
                {
                    Id = employerId,
                    Name = "Employer",
                    Summary = "Zaměstnavatel odpovědný za výpočet mezd a odvody.",
                    Attributes =
                    [
                        new BusinessAttributeNode { Id = "attr.employer-id", Name = "Id", Type = "uuid", Required = true, Summary = "Primární klíč zaměstnavatele." },
                        new BusinessAttributeNode { Id = "attr.employer-name", Name = "Name", Type = "text", Required = true, Constraints = ["not-empty", "max-length:255"], Summary = "Obchodní název zaměstnavatele." },
                        new BusinessAttributeNode { Id = "attr.employer-reg", Name = "RegistrationNumber", Type = "text", Required = true, Constraints = ["exact-length:8"], Summary = "IČO zaměstnavatele." },
                        new BusinessAttributeNode { Id = "attr.employer-rate", Name = "ContributionRate", Type = "decimal", Required = true, DefaultValue = "0.338", Constraints = ["range:0;1"], Summary = "Celková míra odvodů zaměstnavatele (0–1)." }
                    ],
                    Behaviors =
                    [
                        new BusinessBehaviorNode { Id = "bh.employer-total-cost", Name = "CalculateTotalLaborCost", Kind = BusinessBehaviorKind.Query, Returns = "Money", Summary = "Vypočítá celkové personální náklady včetně odvodů zaměstnavatele." }
                    ]
                },
                new BusinessEntityNode
                {
                    Id = employeeId,
                    Name = "Employee",
                    Summary = "Zaměstnanec s osobními a mzdovými údaji.",
                    Attributes =
                    [
                        new BusinessAttributeNode { Id = "attr.emp-id", Name = "Id", Type = "uuid", Required = true, Summary = "Primární klíč zaměstnance." },
                        new BusinessAttributeNode { Id = "attr.emp-first", Name = "FirstName", Type = "text", Required = true, Constraints = ["not-empty", "max-length:100"], Summary = "Jméno zaměstnance." },
                        new BusinessAttributeNode { Id = "attr.emp-last", Name = "LastName", Type = "text", Required = true, Constraints = ["not-empty", "max-length:100"], Summary = "Příjmení zaměstnance." },
                        new BusinessAttributeNode { Id = "attr.emp-email", Name = "Email", Type = "email", Required = true, Constraints = ["email-format"], Summary = "Kontaktní email." },
                        new BusinessAttributeNode { Id = "attr.emp-tax-id", Name = "TaxId", Type = "text", Required = true, Constraints = ["exact-length:10"], Summary = "Daňové identifikační číslo (TIN)." },
                        new BusinessAttributeNode { Id = "attr.emp-gross", Name = "GrossSalary", Type = "money", Required = true, Constraints = ["greater-than:0"], Summary = "Hrubá měsíční mzda." },
                        new BusinessAttributeNode { Id = "attr.emp-hired", Name = "HiredAt", Type = "date", Required = true, Summary = "Datum nástupu." },
                        new BusinessAttributeNode { Id = "attr.emp-dept", Name = "Department", Type = "text", Required = false, DefaultValue = "General", Summary = "Oddělení / útvar." }
                    ],
                    Behaviors =
                    [
                        new BusinessBehaviorNode { Id = "bh.emp-net", Name = "CalculateNetSalary", Kind = BusinessBehaviorKind.Query, Returns = "Money", Summary = "Vypočítá čistou mzdu na základě hrubé mzdy, odvodů a daní." },
                        new BusinessBehaviorNode { Id = "bh.emp-validate", Name = "ValidateTaxId", Kind = BusinessBehaviorKind.Rule, Returns = "bool", Summary = "Validuje formát daňového identifikačního čísla." }
                    ]
                },
                new BusinessEntityNode
                {
                    Id = taxBracketId,
                    Name = "TaxBracket",
                    Summary = "Daňové pásmo pro progresivní zdanění příjmu.",
                    Attributes =
                    [
                        new BusinessAttributeNode { Id = "attr.tax-id", Name = "Id", Type = "uuid", Required = true, Summary = "Primární klíč daňového pásma." },
                        new BusinessAttributeNode { Id = "attr.tax-min", Name = "MinIncome", Type = "money", Required = true, Constraints = ["greater-than-or-equal:0"], Summary = "Spodní hranice pásma (včetně)." },
                        new BusinessAttributeNode { Id = "attr.tax-max", Name = "MaxIncome", Type = "money", Required = false, Summary = "Horní hranice pásma (null = bez omezení)." },
                        new BusinessAttributeNode { Id = "attr.tax-rate", Name = "Rate", Type = "decimal", Required = true, Constraints = ["range:0;1"], Summary = "Sazba daně pro toto pásmo (0–1)." },
                        new BusinessAttributeNode { Id = "attr.tax-desc", Name = "Description", Type = "text", Required = false, Summary = "Popis daňového pásma." }
                    ]
                },
                new BusinessEntityNode
                {
                    Id = deductionId,
                    Name = "Deduction",
                    Summary = "Sociální, zdravotní a jiné povinné srážky.",
                    Attributes =
                    [
                        new BusinessAttributeNode { Id = "attr.ded-id", Name = "Id", Type = "uuid", Required = true, Summary = "Primární klíč srážky." },
                        new BusinessAttributeNode { Id = "attr.ded-name", Name = "Name", Type = "text", Required = true, Constraints = ["not-empty"], Summary = "Název srážky (např. Sociální pojištění)." },
                        new BusinessAttributeNode { Id = "attr.ded-type", Name = "Type", Type = "enum", CustomType = "DeductionKind", Required = true, Summary = "Typ srážky: Social, Health, TaxAdvance, Other." },
                        new BusinessAttributeNode { Id = "attr.ded-rate", Name = "Rate", Type = "decimal", Required = true, Constraints = ["range:0;1"], Summary = "Míra srážky (0–1)." },
                        new BusinessAttributeNode { Id = "attr.ded-mandatory", Name = "IsMandatory", Type = "boolean", Required = true, DefaultValue = "true", Summary = "Určuje, zda je srážka povinná." }
                    ]
                },
                new BusinessEntityNode
                {
                    Id = payrollRecordId,
                    Name = "PayrollRecord",
                    Summary = "Mzdový záznam za konkrétní období.",
                    Attributes =
                    [
                        new BusinessAttributeNode { Id = "attr.pr-id", Name = "Id", Type = "uuid", Required = true, Summary = "Primární klíč mzdového záznamu." },
                        new BusinessAttributeNode { Id = "attr.pr-period", Name = "Period", Type = "text", Required = true, Constraints = ["regex:^\\d{4}-\\d{2}$"], Summary = "Mzdové období ve formátu RRRR-MM." },
                        new BusinessAttributeNode { Id = "attr.pr-gross", Name = "GrossAmount", Type = "money", Required = true, Constraints = ["greater-than:0"], Summary = "Hrubá mzda za období." },
                        new BusinessAttributeNode { Id = "attr.pr-deductions", Name = "TotalDeductions", Type = "money", Required = true, DefaultValue = "0", Computed = "sum(SocialInsurance + HealthInsurance)", Summary = "Součet povinných odvodů." },
                        new BusinessAttributeNode { Id = "attr.pr-tax", Name = "TaxAmount", Type = "money", Required = true, DefaultValue = "0", Computed = "applyTaxBracket(GrossAmount - TotalDeductions)", Summary = "Vypočtená daň po odpočtu odvodů." },
                        new BusinessAttributeNode { Id = "attr.pr-net", Name = "NetAmount", Type = "money", Required = true, Computed = "GrossAmount - TotalDeductions - TaxAmount", Summary = "Čistá mzda k výplatě." },
                        new BusinessAttributeNode { Id = "attr.pr-calc", Name = "CalculatedAt", Type = "datetime", Required = true, Summary = "Čas výpočtu záznamu." }
                    ],
                    Behaviors =
                    [
                        new BusinessBehaviorNode
                        {
                            Id = "bh.pr-generate",
                            Name = "GeneratePayroll",
                            Kind = BusinessBehaviorKind.Command,
                            Summary = "Vygeneruje PayrollRecord pro zaměstnance a období.",
                            Inputs =
                            [
                                new BusinessBehaviorInputNode { Id = "input.emp", Name = "EmployeeId", Type = "uuid", Required = true },
                                new BusinessBehaviorInputNode { Id = "input.period", Name = "Period", Type = "text", Required = true }
                            ]
                        }
                    ]
                }
            ],
            Relations =
            [
                new BusinessRelationNode { Id = "rel.employer-employees", SourceEntityId = employerId, TargetEntityId = employeeId, Kind = BusinessRelationKind.HasMany, SourceNavigationName = "Employees", TargetNavigationName = "Employer", Notes = [new BusinessNoteNode { Id = "note.1", Text = "Jeden zaměstnavatel má více zaměstnanců." }] },
                new BusinessRelationNode { Id = "rel.employee-records", SourceEntityId = employeeId, TargetEntityId = payrollRecordId, Kind = BusinessRelationKind.HasMany, SourceNavigationName = "PayrollRecords", TargetNavigationName = "Employee", Notes = [new BusinessNoteNode { Id = "note.2", Text = "Jeden zaměstnanec má více mzdových záznamů (za různá období)." }] },
                new BusinessRelationNode { Id = "rel.bracket-records", SourceEntityId = taxBracketId, TargetEntityId = payrollRecordId, Kind = BusinessRelationKind.HasMany, SourceNavigationName = "PayrollRecords", TargetNavigationName = "AppliedTaxBracket", Notes = [new BusinessNoteNode { Id = "note.3", Text = "Daňové pásmo se aplikuje na více záznamů." }] },
                new BusinessRelationNode { Id = "rel.deduction-records", SourceEntityId = deductionId, TargetEntityId = payrollRecordId, Kind = BusinessRelationKind.HasMany, SourceNavigationName = "PayrollRecords", TargetNavigationName = "AppliedDeductions", Notes = [new BusinessNoteNode { Id = "note.4", Text = "Srážka se může aplikovat na více záznamů." }] }
            ],
            Workflows =
            [
                new BusinessWorkflowNode
                {
                    Id = "wf.payroll-calc",
                    Name = "PayrollCalculation",
                    Summary = "Workflow pro výpočet čisté mzdy od hrubé mzdy po výplatu.",
                    Trigger = "monthly-schedule",
                    Steps =
                    [
                        new BusinessWorkflowStepNode { Id = "step.gross", Name = "LoadGrossSalary", Kind = BusinessWorkflowStepKind.Task, Summary = "Načtení hrubé mzdy zaměstnance." },
                        new BusinessWorkflowStepNode { Id = "step.deductions", Name = "ApplyDeductions", Kind = BusinessWorkflowStepKind.Task, Summary = "Výpočet sociálního a zdravotního pojištění." },
                        new BusinessWorkflowStepNode { Id = "step.tax-base", Name = "CalculateTaxBase", Kind = BusinessWorkflowStepKind.Task, Summary = "Základ daně = hrubá mzda − odvody." },
                        new BusinessWorkflowStepNode { Id = "step.tax", Name = "ApplyTaxBracket", Kind = BusinessWorkflowStepKind.Decision, Summary = "Aplikace progresivního daňového pásma." },
                        new BusinessWorkflowStepNode { Id = "step.net", Name = "ComputeNetSalary", Kind = BusinessWorkflowStepKind.Task, Summary = "Výsledná čistá mzda k výplatě." }
                    ],
                    Transitions =
                    [
                        new BusinessWorkflowTransitionNode { Id = "tr.1", FromStepId = "step.gross", ToStepId = "step.deductions", Condition = "always" },
                        new BusinessWorkflowTransitionNode { Id = "tr.2", FromStepId = "step.deductions", ToStepId = "step.tax-base", Condition = "always" },
                        new BusinessWorkflowTransitionNode { Id = "tr.3", FromStepId = "step.tax-base", ToStepId = "step.tax", Condition = "always" },
                        new BusinessWorkflowTransitionNode { Id = "tr.4", FromStepId = "step.tax", ToStepId = "step.net", Condition = "always" }
                    ]
                }
            ],
            Notes =
            [
                new BusinessNoteNode { Id = "note.global.1", Text = "Všechny částky jsou v CZK." },
                new BusinessNoteNode { Id = "note.global.2", Text = "Daňové sazby odpovídají roku 2026." }
            ],
            PendingQuestions =
            [
                new PendingQuestionNode
                {
                    Id = "q.1",
                    Text = "Má se započítávat daňové zvýhodnění na děti do výpočtu?",
                    Status = PendingQuestionStatus.Open,
                    Scope = PendingQuestionScope.Entity,
                    RelatedEntityId = payrollRecordId
                },
                new PendingQuestionNode
                {
                    Id = "q.2",
                    Text = "Jakým způsobem se řeší roční zúčtování daně?",
                    Status = PendingQuestionStatus.Open,
                    Scope = PendingQuestionScope.Behavior,
                    RelatedEntityId = payrollRecordId,
                    RelatedBehaviorId = "bh.pr-generate"
                }
            ]
        };
    }
}
